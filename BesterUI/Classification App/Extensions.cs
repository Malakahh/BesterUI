using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    public enum Normalize { ZeroOne, OneMinusOne }
    static class Extensions
    {
        /// <summary>
        /// Item1 is the training set, Item2 is the prediction set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="nFold"></param>
        /// <returns>returns null if the collection can't be nfolded</returns>
        public static List<Tuple<SVMProblem, SVMProblem>> GetCrossValidationSets<T>(this IEnumerable<List<double>> original, SAMData samData, SAMDataPoint.FeelingModel feelingsmodel, int nFold, bool UseIAPSRatings = false)
        {
            //TODO: Needs to be tested, can't test before data can be loaded into the program
            List<Tuple<SVMProblem, SVMProblem>> allSets = new List<Tuple<SVMProblem, SVMProblem>>();

            if (original.Count() % nFold != 0)
            {
                return null;
            }

            List<int> counter = new List<int>();
            for (int k = 0; k < original.Count(); k++)
            {
                counter.Add(k);
            }
            List<List<int>> trainIndicies = new List<List<int>>();
            List<List<int>> predictIndicies = new List<List<int>>();
            for (int i = 0; i < original.Count(); i += nFold)
            {
                var temp = counter.Skip(i).Take(nFold).ToList();
                predictIndicies.Add(temp);
                trainIndicies.Add(counter.Except(temp).ToList());
            }

            for (int j = 0; j < original.Count(); j++)
            {
                SVMProblem trainSVMProblem = new SVMProblem();
                SVMProblem predictSVMProblem = new SVMProblem();
                foreach (int trainIndex in trainIndicies[j])
                {
                    SVMNode[] featureVector = new SVMNode[original.ElementAt(trainIndex).Count];
                    for (int w = 0; w < original.ElementAt(trainIndex).Count; w++)
                    {
                        featureVector[w] = new SVMNode(w + 1, original.ElementAt(trainIndex)[w]);
                    }
                    trainSVMProblem.Add(featureVector, samData.dataPoints[trainIndex].ToAVCoordinate(feelingsmodel, UseIAPSRatings));
                }
                foreach (int predictIndex in predictIndicies[j])
                {

                    SVMNode[] featureVector = new SVMNode[original.ElementAt(predictIndex).Count];
                    for (int w = 0; w < original.ElementAt(predictIndex).Count; w++)
                    {
                        featureVector[w] = new SVMNode(w + 1, original.ElementAt(predictIndex)[w]);
                    }
                    predictSVMProblem.Add(featureVector, samData.dataPoints[predictIndex].ToAVCoordinate(feelingsmodel, UseIAPSRatings));
                }

                allSets.Add(new Tuple<SVMProblem, SVMProblem>(trainSVMProblem, predictSVMProblem));
            }
            
            return allSets;
        }

        public static IEnumerable<List<double>> NormalizeFeatureList<T>(this IEnumerable<List<double>> original, Normalize nMethod)
        {
            double maxNormalize = 0;
            double minNormalize = 0;

            switch (nMethod)
            {
                case Normalize.OneMinusOne:
                    maxNormalize = 1;
                    minNormalize = -1;
                    break;
                case Normalize.ZeroOne:
                    maxNormalize = 1;
                    minNormalize = -1;
                    break;
                default:
                    throw new Exception("The chosen normalize case is not valid");
            }

            try
            {
                for (int i = 0; i < original.First().Count; i++)
                {
                    double minValue = original.Min(x => x[i]);
                    double maxValue = original.Max(x => x[i]);

                    for (int j = 0; j < original.Count(); j++)
                    {
                        double temp = (original.ElementAt(j)[i] - minValue) / (maxValue - minValue);
                        //Scale to -1 to 1 (normalized_value *(max-min)+min)
                        original.ElementAt(j)[i] = (temp * (maxNormalize - (minNormalize)) + (minNormalize));
                    }

                }
            }
            catch
            {
                return null;
            }
            return original;
        }
    }
}
