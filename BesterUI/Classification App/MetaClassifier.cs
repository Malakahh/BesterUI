using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Core;
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;
using BesterUI.Helpers;

namespace Classification_App
{
    class MetaClassifier : Classifier
    {
        public Action<int, int> UpdateCallback;
        private List<StdClassifier> standardClassifiers = new List<StdClassifier>();
        public List<int> boostingOrder = new List<int>();

        public MetaClassifier(string Name, List<SVMParameter> Parameters, SAMData SamData, List<StdClassifier> Classifiers) : base(Name, Parameters, SamData)
        {
            standardClassifiers = Classifiers;
        }

        public MetaClassifier(string Name, SVMParameter Parameter, SAMData SamData, List<StdClassifier> Classifiers) : base(Name, Parameter, SamData)
        {
            standardClassifiers = Classifiers;
        }


        public List<PredictionResult> DoStacking(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizeFormat = Normalize.OneMinusOne)
        {
            List<PredictionResult> classifiers = new List<PredictionResult>();
            //For each classifier run a crossvalidation and find the best params
            foreach (StdClassifier classifier in standardClassifiers)
            {
                List<PredictionResult> results = classifier.OldCrossValidate(feelingsmodel, 1, useIAPSratings, normalizeFormat);
                classifiers.Add(results.OrderBy(x => x.GetAverageFScore()).First());
            }


            List<List<double>> featureList = new List<List<double>>();
            //Create a List of list of answers from each machine
            for (int i = 0; i < samData.dataPoints.Count; i++)
            {
                List<double> featuresToDataPoint = new List<double>();
                foreach (PredictionResult classifier in classifiers)
                {
                    featuresToDataPoint.Add(classifier.guesses[i]);
                }
                featureList.Add(featuresToDataPoint);
            }
            //Split into nfold problems
            List<Tuple<SVMProblem, SVMProblem>> problems = featureList.GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings);

            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

            List<PredictionResult> finalResults = new List<PredictionResult>();

            //Run for each parameter setting
            int cnt = 1;
            foreach (SVMParameter SVMpara in Parameters)
            {
                if (UpdateCallback != null)
                {
                    UpdateCallback(cnt++, Parameters.Count);
                }
                List<double> guesses = new List<double>();
                //model and predict each nfold
                foreach (Tuple<SVMProblem, SVMProblem> tupleProblem in problems)
                {
                    guesses.AddRange(tupleProblem.Item2.Predict(tupleProblem.Item1.Train(SVMpara)));
                }
                int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                //Calculate scoring results
                double[,] confus = CalculateConfusion(guesses.ToArray(), answers, numberOfLabels);
                List<double> pres = CalculatePrecision(confus, numberOfLabels);
                List<double> recall = CalculateRecall(confus, numberOfLabels);
                List<double> fscore = CalculateFScore(pres, recall);
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, new List<Feature> { }, answers.ToList(), guesses.ConvertAll(x => (int)x));
                finalResults.Add(pR);
            }
            return finalResults;
        }

        public PredictionResult DoVoting(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizeFormat = Normalize.OneMinusOne)
        {
            List<PredictionResult> classifiers = new List<PredictionResult>();
            //For each classifier run a crossvalidation and find the best params
            int prg = 0;
            foreach (StdClassifier classifier in standardClassifiers)
            {
                if (UpdateCallback != null)
                {
                    UpdateCallback(prg++, standardClassifiers.Count);
                }
                List<PredictionResult> results = classifier.OldCrossValidate(feelingsmodel, 1, useIAPSratings, normalizeFormat);
                classifiers.Add(results.OrderBy(x => x.GetAverageFScore()).First());
            }
            if (UpdateCallback != null)
            {
                UpdateCallback(standardClassifiers.Count, standardClassifiers.Count);
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
                        if (predictResult.guesses[trainIndicies[i][trainingIndex]] == samData.dataPoints[trainIndicies[i][trainingIndex]].ToAVCoordinate(feelingsmodel))
                        {
                            correct++;
                        }
                    }

                    //Add weight from the trainingset to each of the guesses
                    weightedGuesses[i][predictResult.guesses[i]] += (correct / trainIndicies.Count);
                }
            }

            //Calculate final answers
            List<double> guesses = new List<double>();

            foreach (Dictionary<int, double> answer in weightedGuesses)
            {
                int tempKey = -1;
                double tempMax = -1;
                foreach (int key in answer.Keys)
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
            return new PredictionResult(confus, recall, pres, fscore, new SVMParameter(), new List<Feature> { }, answers.ToList(), guesses.ConvertAll(x => (int)x));

        }


        public PredictionResult DoBoosting(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizeFormat = Normalize.OneMinusOne)
        {
            if (boostingOrder.Count != standardClassifiers.Count)
            {
                //if boosting order and standardClassifier is not the same size an out of bounds is invetatible 
                Log.LogMessage("The Boosting order list is not the same as the number of classifiers, I'm giving you a null");
                return null;
            }

            PredictionResult prevResult = null;
            for (int i = 0; i < boostingOrder.Count; i++)
            {
                if (i == 0)
                {
                    prevResult = FindBestFScorePrediction(standardClassifiers[boostingOrder[i]].CrossValidate(feelingsmodel, useIAPSratings, normalizeFormat));
                }
                else
                {
                    prevResult = FindBestFScorePrediction(standardClassifiers[boostingOrder[i]].CrossValidateWithBoosting(feelingsmodel, nFold, prevResult.guesses.ConvertAll(x => (double)x).ToArray(), useIAPSratings, normalizeFormat));
                }
            }
            return prevResult;
        }

        #region [Helper Functions]
        public PredictionResult FindBestFScorePrediction(List<PredictionResult> results)
        {
            double maxScore = -1;
            PredictionResult bestResult = null;
            foreach (PredictionResult result in results)
            {
                if (result.GetAverageFScore() > maxScore)
                {
                    bestResult = result;
                    maxScore = result.GetAverageFScore();
                }
            }
            return bestResult;
        }

        public MetaSVMConfiguration GetConfiguration()
        {
            MetaSVMConfiguration retVal = new MetaSVMConfiguration();

            retVal.parameter = Parameters[0];
            retVal.Name = Name;
            retVal.stds = standardClassifiers.Select(x => x.GetConfiguration()).ToList();

            return retVal;
        }
        #endregion

    }
}
