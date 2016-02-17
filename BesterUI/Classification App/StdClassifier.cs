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
            Features.ForEach(x => features.Add(x));
        }

        public StdClassifier(string Name, SVMParameter Parameter, List<Feature> Features, SAMData samData) : base(Name, Parameter, samData)
        {
            Features.ForEach(x => features.Add(x));
        }
        #endregion

        #region [SVM Functionalities]
        /// <summary>
        /// Run crossvalidation for each combination of the features for this machine
        /// </summary>
        /// <param name="feelingsmodel"></param>
        /// <param name="nFold"></param>
        /// <param name="useIAPSratings"></param>
        public void CrossValidateCombinations(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizationType = Normalize.OneMinusOne)
        {
            List<List<bool>> combinations = CalculateCombinations(new List<bool>() { }, features.Count);

            //Get different combination of problems
            List<List<Tuple<SVMProblem, SVMProblem>>> featureCombinationProblems = new List<List<Tuple<SVMProblem, SVMProblem>>>();
            for (int i = 0; i < combinations.Count; i++)
            {
                for (int j = 0; j < combinations[i].Count; j++)
                {
                    List<Feature> tempFeatures = new List<Feature>();
                    if (combinations[i][j] == true)
                    {
                        tempFeatures.Add(features[j]);
                    }
                    featureCombinationProblems.Add(GetFeatureValues(tempFeatures).NormalizeFeatureList<double>(normalizationType).GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings));
                }
            }

            //Get correct results
            int[] correct = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

            foreach (SVMParameter SVMpara in Parameters)
            {
                List<double> guesses = new List<double>();
                //For each feature setup 
                for (int n = 0; n < featureCombinationProblems.Count; n++)
                {      
                    //model and predict each nfold 
                    foreach (Tuple<SVMProblem, SVMProblem> tupleProblem in featureCombinationProblems[n])
                    {
                        guesses.AddRange(tupleProblem.Item1.Predict(tupleProblem.Item2.Train(SVMpara)));
                    }
                    int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                    //Calculate scoring results
                    double[,] confus = CalculateConfusion(guesses.ToArray(), correct, numberOfLabels);
                    List<double> pres = CalculatePrecision(confus, numberOfLabels);
                    List<double> recall = CalculateRecall(confus, numberOfLabels);
                    List<double> fscore = CalculateFScore(pres, recall);
                    PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, features);
                    //TODO: do something with the result
                }

            }
        }

        /// <summary>
        /// Run crossvalidation for the feature setup for this machine
        /// </summary>
        /// <param name="feelingsmodel"></param>
        /// <param name="nFold"></param>
        /// <param name="useIAPSratings"></param>
        public void CrossValidate(SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool useIAPSratings = false, Normalize normalizationType = Normalize.OneMinusOne)
        {
            //Split into crossvalidation parts
            List<Tuple<SVMProblem, SVMProblem>> problems = GetFeatureValues(features).NormalizeFeatureList<double>(normalizationType).GetCrossValidationSets<double>(samData, feelingsmodel, nFold, useIAPSratings);
            //Get correct results
            int[] correct = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

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
                double[,] confus = CalculateConfusion(guesses.ToArray(), correct, numberOfLabels);
                List<double> pres = CalculatePrecision(confus, numberOfLabels);
                List<double> recall = CalculateRecall(confus, numberOfLabels);
                List<double> fscore = CalculateFScore(pres, recall);
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, features);
                //TODO: Do something with the result
            }
        }
        #endregion

        #region [Helper Functions]
        /// <summary>
        /// Get the values for each features
        /// </summary>
        /// <param name="Features"></param>
        /// <returns>A List for each datapoint which contains the feature values for that datapoint</returns>
        private List<List<double>> GetFeatureValues(List<Feature> Features)
        {
            List<List<double>> temp = new List<List<double>>();
            foreach (Feature f in Features)
            {
                List<double> values = f.GetAllValues();
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
