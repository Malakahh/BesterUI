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
        private List<List<double>> features;
        public override List<SVMParameter> Parameters { get; set; }

        public StdClassifier(string Name, List<SVMParameter> Parameters, List<List<double>> Features, SAMData samData) : base(Name, Parameters, samData)
        {
            Features.ForEach((x) =>
            {
                features.Add(x);
            });
        }

        public StdClassifier(string Name, SVMParameter Parameter, List<List<double>> Features, SAMData samData) : base(Name, Parameter, samData)
        {
            Features.ForEach((x) =>
            {
                features.Add(x);
            });
        }

        public void CrossValidateCombinations()
        {
            if (features != null || features.Count > 0)
            {
                CalculateCombinations(new List<bool>() { }, features[0].Count);
            }
        }

        public void CrossValidate(SAMDataPoint.FeelingModel feelingsmodel, int nfold, bool useIAPSratings = false)
        {
            //Split into crossvalidation parts
            List<Tuple<SVMProblem, SVMProblem>> problems = features.GetCrossValidationSets<double>(samData, feelingsmodel, nfold, useIAPSratings);
            //Get correct results
            int[] correct = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingsmodel, useIAPSratings)).ToArray();

            foreach (SVMParameter SVMpara in Parameters)
            {
                List<double> guesses = new List<double>();
                //model and predict each nfold
                foreach(Tuple<SVMProblem, SVMProblem> tupleProblem in problems)
                {
                    guesses.AddRange(tupleProblem.Item1.Predict(tupleProblem.Item2.Train(SVMpara)));
                }
                int numberOfLabels = SAMData.GetNumberOfLabels(feelingsmodel);
                //Calculate result
                double[,]confus = CalculateConfusion(guesses.ToArray(), correct, numberOfLabels);
                List<double> pres = CalculatePrecision(confus, numberOfLabels);
                List<double> recall = CalculateRecall(confus, numberOfLabels);
                List<double> fscore = CalculateFScore(pres, recall);

            }    
            //TODO: Find out what to do with the results
       
        }

        public override void PrintResults()
        {
            throw new NotImplementedException();
        }
        
    }
}
