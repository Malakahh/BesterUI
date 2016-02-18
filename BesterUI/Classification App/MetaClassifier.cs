using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Core;
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;

namespace Classification_App
{
    class MetaClassifier : Classifier
    {
        private List<StdClassifier> standardClassifiers = new List<StdClassifier>();


        public MetaClassifier(string Name, List<SVMParameter> Parameters, SAMData SamData) : base(Name, Parameters, SamData)
        {
        }

        public MetaClassifier(string Name, SVMParameter Parameter, SAMData SamData) : base(Name, Parameter, SamData)
        {
        }


        public List<PredictionResult> DoStacking(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings, Normalize normalizeFormat)
        {
            List<PredictionResult> classifiers = new List<PredictionResult>();
            //For each classifier run a crossvalidation and find the best params
            foreach (StdClassifier classifier in standardClassifiers)
            {
                List<PredictionResult> results = classifier.CrossValidate(feelingsmodel, nFold, useIAPSratings, normalizeFormat);
                classifiers.Add(results.OrderBy(x => x.AverageFScore()).First());
            }


            List<List<double>> featureList = new List<List<double>>();
            //Create a List of list of answers from each machine
            for (int i = 0; i < samData.dataPoints.Count; i++)
            {
                List<double> featuresToDataPoint = new List<double>();
                foreach (PredictionResult classifier in classifiers)
                {
                    featuresToDataPoint.Add(classifier.answers[i]);
                }
            }
            //Split into nfold problems
            List<Tuple<SVMProblem, SVMProblem>> problems = featureList.GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings);

            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

            List<PredictionResult> finalResults = new List<PredictionResult>();

            //Run for each parameter setting
            foreach (SVMParameter SVMpara in Parameters)
            {
                List<double> guesses = new List<double>();
                //model and predict each nfold
                foreach (Tuple<SVMProblem, SVMProblem> tupleProblem in problems)
                {
                    guesses.AddRange(tupleProblem.Item1.Predict(tupleProblem.Item2.Train(SVMpara)));
                }
                int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                //Calculate scoring results
                double[,] confus = CalculateConfusion(guesses.ToArray(), answers, numberOfLabels);
                List<double> pres = CalculatePrecision(confus, numberOfLabels);
                List<double> recall = CalculateRecall(confus, numberOfLabels);
                List<double> fscore = CalculateFScore(pres, recall);
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, new List<Feature> { }, answers.ToList(), guesses.Cast<int>().ToList());
                finalResults.Add(pR);
            }
            return finalResults;
        }

        public PredictionResult DoVoting(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings, Normalize normalizeFormat)
        {
            List<PredictionResult> classifiers = new List<PredictionResult>();
            //For each classifier run a crossvalidation and find the best params
            foreach (StdClassifier classifier in standardClassifiers)
            {
                List<PredictionResult> results = classifier.CrossValidate(feelingsmodel, nFold, useIAPSratings, normalizeFormat);
                classifiers.Add(results.OrderBy(x => x.AverageFScore()).First());
            }
            int labelCount = SAMData.GetNumberOfLabels(feelingsmodel);

            //Full List of indicies
            List<int> counter = new List<int>();
            for (int k = 0; k < samData.dataPoints.Count(); k++)
            {
                counter.Add(k);
            }
            //Divide indicies into correct nfold
            List<List<int>> trainIndicies = new List<List<int>>();
            List<List<int>> predictIndicies = new List<List<int>>();
            for (int i = 0; i < samData.dataPoints.Count(); i += nFold)
            {
                var temp = counter.Skip(i).Take(nFold).ToList();
                predictIndicies.Add(temp);
                trainIndicies.Add(counter.Except(temp).ToList());
            }


            List<Dictionary<int, double>> weightedGuesses = new List<Dictionary<int, double>>();
            //Fill up weightedGuesses List
            for (int nGuesses = 0; nGuesses < samData.dataPoints.Count; nGuesses++)
            {
                Dictionary<int, double> tempGuess = new Dictionary<int, double>();
                for (int indexClass = 0; indexClass < labelCount; indexClass++)
                {
                    tempGuess.Add(indexClass, 0);
                }
                weightedGuesses.Add(tempGuess);
            }

            //Split classifiers
            for (int i = 0; i < trainIndicies.Count; i++)
            {
                foreach (PredictionResult predictResult in classifiers)
                {
                    double correct = 0;
                    //calculate weights
                    for (int trainingIndex = 0; trainingIndex < trainIndicies[i].Count; trainingIndex++)
                    {
                        if (predictResult.answers[trainingIndex] == samData.dataPoints[trainingIndex].ToAVCoordinate(feelingsmodel))
                        {
                            correct++;
                        }
                    }

                    //Add weight from the trainingset to each of the guesses
                    for (int predictionIndex = 0; predictionIndex < predictIndicies[i].Count; predictionIndex++)
                    {
                        weightedGuesses[predictionIndex][predictResult.answers[predictionIndex]] += (correct / trainIndicies.Count);
                    }
                }
            }

            //Calculate final answers
            List<double> guesses = new List<double>();

            foreach (Dictionary<int, double> answer in weightedGuesses)
            {
                int tempKey = -1;
                double tempMax = -1;
                foreach(int key in answer.Keys)
                {
                    if (answer[key] > tempMax)
                    {
                        tempKey = key;
                        tempMax = answer[key];
                    }
                }
                guesses.Add(tempKey);
            }


            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();
            int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);


            //Calculate scoring results
            double[,] confus = CalculateConfusion(guesses.ToArray(), answers, numberOfLabels);
            List<double> pres = CalculatePrecision(confus, numberOfLabels);
            List<double> recall = CalculateRecall(confus, numberOfLabels);
            List<double> fscore = CalculateFScore(pres, recall);
            return new PredictionResult(confus, recall, pres, fscore, new SVMParameter(), new List<Feature> { }, answers.ToList(), guesses.Cast<int>().ToList());

        }


        public void DoBoosting()
        {

        }
        
    }
}
