using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using LibSVMsharp.Extensions;
using LibSVMsharp.Core;
using LibSVMsharp.Helpers;

namespace Classification_App
{
    class StdClassifier : Classifier
    {
        public override List<SVMParameter> Parameters { get; set; }
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

        public StdClassifier(SVMConfiguration conf, SAMData samData) : base(conf.Name, conf.parameters, samData)
        {
            features = conf.features;
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
                for (int j = 0; j < combinations[i].Count; j++)
                {
                    // For each feature combination save the different problems for crossvalidation
                    List<Feature> tempFeatures = new List<Feature>();
                    if (combinations[i][j] == true)
                    {
                        tempFeatures.Add(features[j]);
                    }
                    featureCombinationProblems.
                        Add
                        (
                            new Tuple<List<Tuple<SVMProblem, SVMProblem>>, List<Feature>>
                            (
                                GetFeatureValues(tempFeatures, samData).NormalizeFeatureList<double>(normalizationType).GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings),
                                tempFeatures
                            )
                        );
                }
            }

            //Get correct results
            int[] answers = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();
            List<PredictionResult> predictionResults = new List<PredictionResult>();
            foreach (SVMParameter SVMpara in Parameters)
            {
                List<double> guesses = new List<double>();
                //For each feature setup 
                for (int n = 0; n < featureCombinationProblems.Count; n++)
                {
                    //model and predict each nfold 
                    foreach (var tupleProblem in featureCombinationProblems[n].Item1)
                    {
                        guesses.AddRange(tupleProblem.Item1.Predict(tupleProblem.Item2.Train(SVMpara)));
                    }
                    int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                    //Calculate scoring results
                    double[,] confus = CalculateConfusion(guesses.ToArray(), answers, numberOfLabels);
                    List<double> pres = CalculatePrecision(confus, numberOfLabels);
                    List<double> recall = CalculateRecall(confus, numberOfLabels);
                    List<double> fscore = CalculateFScore(pres, recall);
                    PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, featureCombinationProblems[n].Item2, answers.ToList(), guesses.Cast<int>().ToList());
                    //TODO: do something with the result
                }

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
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, features, answers.ToList(), guesses.Cast<int>().ToList());
                //TODO: Do something with the result
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
            foreach (Feature f in Features)
            {
                List<double> values = f.GetAllValues(samd);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i == 0)
                    {
                        temp.Add(new List<double>());
                    }
                    temp[i].Add(values[i]);
                }
            }
            return temp;
        }

        public override void PrintResults()
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
