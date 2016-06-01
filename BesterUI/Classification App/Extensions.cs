using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
using BesterUI.Data;
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

            //Full List of indicies
            List<int> counter = new List<int>();
            for (int k = 0; k < original.Count(); k++)
            {
                counter.Add(k);
            }
            //Divide indicies into correct nfold
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
                //Create training problem
                SVMProblem trainSVMProblem = new SVMProblem();
                //Foreach training index, add features to the problem
                foreach (int trainIndex in trainIndicies[j])
                {
                    SVMNode[] featureVector = new SVMNode[original.ElementAt(trainIndex).Count];
                    for (int w = 0; w < original.ElementAt(trainIndex).Count; w++)
                    {
                        featureVector[w] = new SVMNode(w + 1, original.ElementAt(trainIndex)[w]);
                    }
                    trainSVMProblem.Add(featureVector, samData.dataPoints[trainIndex].ToAVCoordinate(feelingsmodel, UseIAPSRatings));
                }


                //Create predict problem
                SVMProblem predictSVMProblem = new SVMProblem();
                //Foreach predict index, add features to the problem
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

        public static List<DataReading> GetDataFromInterval(List<DataReading> original, int start, SENSOR machine)
        {
            int offset = 0;
            int duration = 0;

            if (machine == SENSOR.GSR)
            {
                offset = 2000;
                duration = 5000;

            }

            List<DataReading> splicedData = new List<DataReading>();

            splicedData.AddRange(
                original.Where(
                    x => x.timestamp >= (start + original.First().timestamp + offset) &&
                    x.timestamp <= (start + original.First().timestamp + duration
                    )
                )
            );

            return splicedData;
        }


        public static SVMProblem CreateCompleteProblem(this IEnumerable<List<double>> original, SAMData sam, SAMDataPoint.FeelingModel feelingModel)
        {
            SVMProblem completeProblem = new SVMProblem();
            for (int i = 0; i < original.Count(); i++)
            {
                SVMNode[] nodeSet = new SVMNode[original.ElementAt(i).Count];
                for (int j = 0; j < original.ElementAt(i).Count; j++)
                {
                    SVMNode currentNode = new SVMNode();
                    currentNode.Index = j + 1;
                    currentNode.Value = original.ElementAt(i)[j];
                    nodeSet[j] = currentNode;
                }
                completeProblem.Add(nodeSet, sam.dataPoints[i].ToAVCoordinate(feelingModel));
            }

            return completeProblem;
        }

        public static SVMProblem CreateCompleteProblemOneClass(this IEnumerable<List<double>> original)
        {
            SVMProblem completeProblem = new SVMProblem();
            for (int i = 0; i < original.Count(); i++)
            {
                SVMNode[] nodeSet = new SVMNode[original.ElementAt(i).Count];
                for (int j = 0; j < original.ElementAt(i).Count; j++)
                {
                    SVMNode currentNode = new SVMNode();
                    currentNode.Index = j + 1;
                    currentNode.Value = original.ElementAt(i)[j];
                    nodeSet[j] = currentNode;
                }
                completeProblem.Add(nodeSet, 1);
            }

            return completeProblem;
        }

        public static List<SVMNode[]> CreateNodesFromData(this IEnumerable<List<double>> original)
        {
            List<SVMNode[]> svmNodeList = new List<SVMNode[]>();
            for (int i = 0; i < original.Count(); i++)
            {
                SVMNode[] nodeSet = new SVMNode[original.ElementAt(i).Count];
                for (int j = 0; j < original.ElementAt(i).Count; j++)
                {
                    SVMNode currentNode = new SVMNode();
                    currentNode.Index = j + 1;
                    currentNode.Value = original.ElementAt(i)[j];
                    nodeSet[j] = currentNode;
                }
                svmNodeList.Add(nodeSet);
            }

            return svmNodeList;
        }

        public static IEnumerable<List<double>> NormalizeFeatureList<T>(this IEnumerable<List<double>> original, Normalize nMethod)
        {
            var tempCopy = original.ToList();
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
                        tempCopy.ElementAt(j)[i] = (temp * (maxNormalize - (minNormalize)) + (minNormalize));
                    }

                }
            }
            catch
            {

                return null;
            }
            return tempCopy;
        }

        public static IEnumerable<OneClassFV> NormalizeFeatureVectorList(this IEnumerable<OneClassFV> original, Normalize nMethod)
        {
            var tempCopy = original.ToList();
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
                for (int i = 0; i < original.First().Features.Length; i++)
                {
                    double minValue = original.Min(x => x.Features[i].Value);
                    double maxValue = original.Max(x => x.Features[i].Value);

                    for (int j = 0; j < original.Count(); j++)
                    {
                        double temp = (original.ElementAt(j).Features[i].Value - minValue) / (maxValue - minValue);
                        //Scale to -1 to 1 (normalized_value *(max-min)+min)
                        tempCopy.ElementAt(j).Features[i].Value = (temp * (maxNormalize - (minNormalize)) + (minNormalize));
                    }
                        
                }
            }
            catch (Exception e)
            {

                return null;
            }
            return tempCopy;
        }

        public static List<double> MedianFilter(this List<double> input, int windowSize)
        {
            List<double> newValues = new List<double>();

            for (int i = 0; i < input.Count - windowSize; i++)
            {
                List<double> tempValues = new List<double>();

                for (int j = 0; j < windowSize; j++)
                {
                    tempValues.Add(input[i + j]);
                }

                newValues.Add(tempValues.ElementAt((int)Math.Round((double)windowSize / 2)));
            }

            return newValues;
        }

        public static List<double> MovingAverageFilter(this List<double> input, int windowSize)
        {
            List<double> newValues = new List<double>();

            for (int i = 0; i < input.Count - windowSize; i++)
            {
                List<double> tempValues = new List<double>();

                for (int j = 0; j < windowSize; j++)
                {
                    tempValues.Add(input[i + j]);
                }

                newValues.Add(tempValues.Average());
            }

            return newValues;
        }

        public static List<double> AveragePointReductionFilter(this List<double> input, int windowSize)
        {
            List<double> newValues = new List<double>();

            for (int i = 0; i < input.Count - windowSize; i += windowSize)
            {
                List<double> tempValues = new List<double>();

                for (int j = 0; j < windowSize; j++)
                {
                    tempValues.Add(input[i + j]);
                }

                newValues.Add(tempValues.Average());
                //newValues.Add(tempValues.ElementAt((int)Math.Round((double)windowSize / 2)));
            }

            return newValues;
        }

        public static List<double> VarianceFilter(this List<double> input, int windowSize)
        {
            List<double> newValues = new List<double>();

            for (int i = 0; i < input.Count - windowSize; i++)
            {
                List<double> tempValues = new List<double>();

                for (int j = 0; j < windowSize; j++)
                {
                    tempValues.Add(input[i + j]);
                }

                double mean = tempValues.Average();
                newValues.Add(tempValues.Average(x => Math.Pow(x - mean, 2)));
            }

            return newValues;
        }
    }
}
