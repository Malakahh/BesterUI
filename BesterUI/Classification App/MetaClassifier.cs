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
                classifiers.Add(results.OrderBy(x=> x.AverageFScore()).First());
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
                PredictionResult pR = new PredictionResult(confus, recall, pres, fscore, SVMpara, new List<Feature>{ }, answers.ToList(), guesses.Cast<int>().ToList());
                finalResults.Add(pR);
            }
            return finalResults;
        }

        public void DoVoting()
        {
        }

        public void DoBoosting()
        {
        }


        public override List<SVMParameter> Parameters
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void PrintResults()
        {
            throw new NotImplementedException();
        }
    }
}
