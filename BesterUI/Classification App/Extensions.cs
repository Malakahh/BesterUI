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
        public static List<Tuple<SVMProblem, SVMProblem>> GetCrossValidationSets<T>(this IEnumerable<List<double>> original,  int nFold)
        {
            List<Tuple<SVMProblem, SVMProblem>> allSets = new List<Tuple<SVMProblem, SVMProblem>>();

            if (original.Count() % nFold != 0)
            {
                return null;
            }



            for (int i = 0; i < original.Count(); i += nFold)
            {
                List<List<double>> trainList = original.Except(original.Skip(i).Take(nFold)).ToList();
                SVMProblem trainingProblem = new SVMProblem();

                for (int j = 0; j < trainList[0].Count; j++)
                {
                    SVMProblem problem = new SVMProblem();

                    for (int i = 0; i < samDataPoint.Count; i++)
                    {
                        SVMNode[] featureVector = new SVMNode[machineFeatures[j].features.Count];
                        for (int w = 0; w < machineFeatures[j].features.Count; w++)
                        {
                            featureVector[w] = new SVMNode(w + 1, machineFeatures[j].features[w].func(samDataPoint[i]));
                        }
                        problem.Add(featureVector, samDataPoint[i].ToAVCoordinate(feelingModel));
                    }
                }

                List<List<double>> predictList = original.Except(trainList).ToList();
                SVMProblem predictionProblem = new SVMProblem();




                allSets.Add(new Tuple<SVMProblem, SVMProblem>(trainingProblem, predictionProblem));
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
