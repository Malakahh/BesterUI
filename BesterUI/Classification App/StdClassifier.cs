using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Extensions;
using LibSVMsharp.Core;
using LibSVMsharp.Helpers;
using BesterUI.Helpers;
using System.Threading;
using System.Runtime.InteropServices;


namespace Classification_App
{
    class StdClassifier : Classifier
    {
        private const string ONLY_ONE_CLASS = "Only one class is present in the full";
        private const string ONLY_ONE_CLASS_IN_TRAINING = "Only one class is present in the trained set";
        public Action<int, int> UpdateCallback;
        public List<Feature> features = new List<Feature>();

        #region [Constructors]
        public StdClassifier(string Name, List<SVMParameter> Parameters, List<Feature> Features, SAMData samData) : base(Name, Parameters, samData)
        {
            features = new List<Feature>(Features);
        }

        public StdClassifier(string Name, SVMParameter Parameter, List<Feature> Features, SAMData samData) : base(Name, Parameter, samData)
        {
            features = new List<Feature>(Features);
        }

        public StdClassifier(SVMConfiguration conf, SAMData samData) : base(conf.Name, conf.GetParameter(), samData)
        {
            features = conf.GetFeautres();
        }
        #endregion

        #region [SVM Functionalities]
        /// <summary>
        /// Run crossvalidation for each combination of the features for this machine
        /// </summary>
        /// <param name="feelingsmodel"></param>
        /// <param name="nFold"></param>
        /// <param name="useIAPSratings"></param>
        public List<PredictionResult> CrossValidateCombinations(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizationType = Normalize.OneMinusOne)
        {
            List<List<bool>> combinations = CalculateCombinations(new List<bool>() { }, features.Count);

            //Get different combination of problems
            List<Tuple<List<Tuple<SVMProblem, SVMProblem>>, List<Feature>>> featureCombinationProblems = new List<Tuple<List<Tuple<SVMProblem, SVMProblem>>, List<Feature>>>();

            for (int i = 0; i < combinations.Count; i++)
            {
                List<Feature> tempFeatures = new List<Feature>();
                for (int j = 0; j < combinations[i].Count; j++)
                {
                    // For each feature combination save the different problems for crossvalidation

                    if (combinations[i][j] == true)
                    {
                        tempFeatures.Add(features[j]);
                    }

                }
                featureCombinationProblems.Add
                                        (
                                            new Tuple<List<Tuple<SVMProblem, SVMProblem>>, List<Feature>>
                                            (
                                                GetFeatureValues(tempFeatures, samData).NormalizeFeatureList<double>(normalizationType).GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings),
                                                tempFeatures
                                            )
                                         );
            }

            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();
            int progressCounter = 0;
            List<PredictionResult> predictionResults = new List<PredictionResult>();
            foreach (SVMParameter SVMpara in Parameters)
            {
                //For each feature setup 
                for (int n = 0; n < featureCombinationProblems.Count; n++)
                {
                    if (UpdateCallback != null)
                    {
                        UpdateCallback(progressCounter, Parameters.Count * featureCombinationProblems.Count);
                    }
                    //PrintProgress(progressCounter, featureCombinationProblems.Count);
                    List<double> guesses = new List<double>();
                    //model and predict each nfold 
                    foreach (var tupleProblem in featureCombinationProblems[n].Item1)
                    {
                        guesses.AddRange(tupleProblem.Item2.Predict(tupleProblem.Item1.Train(SVMpara)));
                    }
                    int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                    //Calculate scoring results
                    double[,] confus = CalculateConfusion(guesses.ToArray(), answers, numberOfLabels);
                    List<double> pres = CalculatePrecision(confus, numberOfLabels);
                    List<double> recall = CalculateRecall(confus, numberOfLabels);
                    List<double> fscore = CalculateFScore(pres, recall);
                    PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, featureCombinationProblems[n].Item2, answers.ToList(), guesses.ConvertAll(x => (int)x));
                    predictionResults.Add(pR);
                    progressCounter++;
                }

            }
            if (UpdateCallback != null)
            {
                UpdateCallback(progressCounter, Parameters.Count * featureCombinationProblems.Count);
            }

            return predictionResults;
        }

        /// <summary>
        /// Run crossvalidation for the feature setup for this machine
        /// </summary>
        /// <param name="feelingsmodel"></param>
        /// <param name="nFold"></param>
        /// <param name="useIAPSratings"></param>
        public List<PredictionResult> CrossValidate(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizationType = Normalize.OneMinusOne)
        {
            List<PredictionResult> predictedResults = new List<PredictionResult>();
            //Split into crossvalidation parts
            List<Tuple<SVMProblem, SVMProblem>> problems = GetFeatureValues(features, samData).NormalizeFeatureList<double>(normalizationType).GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings);
            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();
            int progressCounter = 0;
            if(answers.Distinct().Count() <= 1)
            {
                int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                //Calculate scoring results
                double[,] confus = CalculateConfusion(answers.ToList().ConvertAll(x=>(double)x).ToArray(), answers, numberOfLabels);
                List<double> pres = CalculatePrecision(confus, numberOfLabels);
                List<double> recall = CalculateRecall(confus, numberOfLabels);
                List<double> fscore = CalculateFScore(pres, recall);
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, new SVMParameter(), features, answers.ToList(), answers.ToList().ConvertAll(x => (int)x));
                predictedResults.Add(pR);
                progressCounter++;
                Log.LogMessage(ONLY_ONE_CLASS);
                Log.LogMessage("");
                PrintProgress(progressCounter, 1);
                return predictedResults;
            }
            foreach (SVMParameter SVMpara in Parameters)
            {
                PrintProgress(progressCounter, 1);
                List<double> guesses = new List<double>();
                //model and predict each nfold
                try
                {
                    foreach (Tuple<SVMProblem, SVMProblem> tupleProblem in problems)
                    {
                        SVMModel trainingModel = tupleProblem.Item1.Train(SVMpara);
                        if (trainingModel.ClassCount <= 1)
                        {
                            Log.LogMessage(ONLY_ONE_CLASS_IN_TRAINING);
                            Log.LogMessage("");
                            guesses.AddRange(tupleProblem.Item1.Y.ToList().Take(tupleProblem.Item2.Y.Count()).ToList());
                        }
                        else
                        {
                            double[] d = tupleProblem.Item2.Predict(trainingModel);
                            guesses.AddRange(d);
                        }
                    }
                }
                catch(Exception e)
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
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, features, answers.ToList(), guesses.ConvertAll(x => (int)x));
                predictedResults.Add(pR);
                progressCounter++;

                PrintProgress(progressCounter, 1);
            }

            return predictedResults;
        }


        public List<PredictionResult> CrossValidateWithBoosting(SAMDataPoint.FeelingModel feelingsmodel, int nFold, double[] answersFromPrevious, bool useIAPSratings = false, Normalize normalizationType = Normalize.OneMinusOne)
        {
            List<PredictionResult> predictedResults = new List<PredictionResult>();

            List<List<double>> tempFeatuers = GetFeatureValues(features, samData);
            if (answersFromPrevious.Length != tempFeatuers.Count)
            {
                //answers from previous is not the same size as current feature list, e.g. something is wrong
                Log.LogMessage("The number of guessses from previous machine is the same as number of datapoints in this");
                return null;
            }
            //Split into crossvalidation parts
            List<List<double>> tempFeatures = tempFeatuers.NormalizeFeatureList<double>(normalizationType).ToList();
            for (int i = 0; i < tempFeatuers.Count; i++)
            {
                tempFeatuers[i].Add(answersFromPrevious[i]);
            }
            List<Tuple<SVMProblem, SVMProblem>> problems = tempFeatuers.GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings);


            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

            foreach (SVMParameter SVMpara in Parameters)
            {
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
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, features, answers.ToList(), guesses.ConvertAll(x => (int)x).ToList());
                predictedResults.Add(pR);
            }
            return predictedResults;
        }

        #endregion

        #region [Helper Functions]
        /// <summary>
        /// Get the values for each features
        /// </summary>
        /// <param name="Features"></param>
        /// <returns>A List for each datapoint which contains the feature values for that datapoint</returns>
        private List<List<double>> GetFeatureValues(List<Feature> Features, SAMData samd)
        {
            List<List<double>> temp = new List<List<double>>();
            for (int j = 0; j < samd.dataPoints.Count; j++)
            {
                temp.Add(new List<double>());
            }
            foreach (Feature f in Features)
            {
                List<double> values = f.GetAllValues(samd);
                for (int i = 0; i < values.Count; i++)
                {
                    temp[i].Add(values[i]);
                }

            }
            return temp;
        }

        private void PrintProgress(int progress, int combinations)
        {
            Log.LogMessageSameLine("Done (" + progress + "/" + Parameters.Count * combinations + ")");
        }

        public override string ToString()
        {
            return Name;
        }

        public SVMConfiguration GetConfiguration()
        {
            SVMConfiguration retVal = new SVMConfiguration(Parameters[0], features);
            retVal.Name = Name;
            return retVal;
        }
        #endregion


    }
}
