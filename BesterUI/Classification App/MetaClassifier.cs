﻿using System;
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
        private const string ONLY_ONE_CLASS = "Only one class is present in the full";
        private const string ONLY_ONE_CLASS_IN_TRAINING = "Only one class is present in the trained set";
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

            bool postedOneClassError = false;
            if (answers.Distinct().Count() <= 1)
            {
                int progressCounter = 0;
                List<PredictionResult> predictedResults = new List<PredictionResult>();
                int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                //Calculate scoring results
                double[,] confus = CalculateConfusion(answers.ToList().ConvertAll(x => (double)x).ToArray(), answers, numberOfLabels);
                List<double> pres = CalculatePrecision(confus, numberOfLabels);
                List<double> recall = CalculateRecall(confus, numberOfLabels);
                List<double> fscore = CalculateFScore(pres, recall);
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, new SVMParameter(), new List<Feature>(), answers.ToList(), answers.ToList().ConvertAll(x => (int)x));
                predictedResults.Add(pR);
                progressCounter++;
                Log.LogMessage(ONLY_ONE_CLASS);
                Log.LogMessage("");
                return predictedResults;
            }

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
                try
                {

                    foreach (Tuple<SVMProblem, SVMProblem> tupleProblem in problems)
                    {
                        SVMModel trainingModel = tupleProblem.Item1.Train(SVMpara);
                        if (trainingModel.ClassCount <= 1)
                        {
                            if (!postedOneClassError)
                            {
                                Log.LogMessage(ONLY_ONE_CLASS_IN_TRAINING);
                                postedOneClassError = true;
                            }
                            guesses.AddRange(tupleProblem.Item1.Y.ToList().Take(tupleProblem.Item2.Y.Count()).ToList());
                        }
                        else
                        {
                            guesses.AddRange(tupleProblem.Item2.Predict(trainingModel));
                        }
                    }
                }
                catch (Exception e)
                {
                    for (int i = 0; i < samData.dataPoints.Count; i++)
                    {
                        guesses.Add(-1);
                    }
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

        public PredictionResult DoStackingMachineless(SAMDataPoint.FeelingModel feelingsmodel, int nFold, List<List<double>> machineAnswers, bool useIAPSratings = false)
        {
            List<List<double>> featureList = new List<List<double>>();
            //Create a List of list of answers from each machine
            for (int i = 0; i < samData.dataPoints.Count; i++)
            {
                List<double> featuresToDataPoint = new List<double>();
                foreach (List<double> classifier in machineAnswers)
                {
                    featuresToDataPoint.Add(classifier[i]);
                }
                featureList.Add(featuresToDataPoint);
            }

            //Split into nfold problems
            List<Tuple<SVMProblem, SVMProblem>> problems = featureList.GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings);

            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

            if (answers.Distinct().Count() <= 1)
            {
                return null;
            }

            List<double> guesses = new List<double>();
            //model and predict each nfold
            try
            {

                foreach (Tuple<SVMProblem, SVMProblem> tupleProblem in problems)
                {
                    SVMModel trainingModel = tupleProblem.Item1.Train(Parameters[0]);
                    if (trainingModel.ClassCount <= 1)
                    {
                        return null;
                    }
                    else
                    {
                        guesses.AddRange(tupleProblem.Item2.Predict(trainingModel));
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
            int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
            //Calculate scoring results
            double[,] confus = CalculateConfusion(guesses.ToArray(), answers, numberOfLabels);
            List<double> pres = CalculatePrecision(confus, numberOfLabels);
            List<double> recall = CalculateRecall(confus, numberOfLabels);
            List<double> fscore = CalculateFScore(pres, recall);
            PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, null, new List<Feature> { }, answers.ToList(), guesses.ConvertAll(x => (int)x));

            return pR;
        }

        public PredictionResult DoVoting(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizeFormat = Normalize.OneMinusOne)
        {
            List<List<Tuple<double, int>>> classifiers = new List<List<Tuple<double, int>>>();
            //For each classifier run a crossvalidation and find the best params
            int prg = 0;
            foreach (StdClassifier classifier in standardClassifiers)
            {
                if (UpdateCallback != null)
                {
                    UpdateCallback(prg++, standardClassifiers.Count);
                }
                List<Tuple<double, int>> results = classifier.CrossValidationForVoting(feelingsmodel, useIAPSratings, normalizeFormat);
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

            //Add Answers + weight
            for (int nGuesses = 0; nGuesses < samData.dataPoints.Count; nGuesses++)
            {
                for (int nClassifiers = 0; nClassifiers < classifiers.Count; nClassifiers++)
                {
                    weightedGuesses[nGuesses][classifiers[nClassifiers][nGuesses].Item2] += classifiers[nClassifiers][nGuesses].Item1;
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

        public PredictionResult DoVotingMachineless(SAMDataPoint.FeelingModel feelingsmodel, int nFold, List<List<double>> machineAnswers, bool useIAPSratings = false)
        {
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
                foreach (var predictResult in machineAnswers)
                {
                    double correct = 0;
                    //calculate weights
                    for (int trainingIndex = 0; trainingIndex < trainIndicies[i].Count; trainingIndex++)
                    {
                        if (predictResult[trainIndicies[i][trainingIndex]] == samData.dataPoints[trainIndicies[i][trainingIndex]].ToAVCoordinate(feelingsmodel))
                        {
                            correct++;
                        }
                    }

                    //Add weight from the trainingset to each of the guesses
                    weightedGuesses[i][(int)predictResult[i]] += (correct / trainIndicies.Count);
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
