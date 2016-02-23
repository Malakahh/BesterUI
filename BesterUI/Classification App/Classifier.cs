using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp.Core;
using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;
using LibSVMsharp;

namespace Classification_App
{
    abstract class Classifier
    {
        public string Name { get; private set; }
        public List<SVMParameter> Parameters { get; set; }
        public readonly SAMData samData;

        public Classifier(string Name, List<SVMParameter> Parameters, SAMData samData)
        {
            this.Name = Name;
            this.Parameters = new List<SVMParameter>();
            Parameters.ForEach((x) =>
            {
                this.Parameters.Add(x);
            });
            this.samData = samData;
        }

        public Classifier(string Name, SVMParameter Parameter, SAMData samData)
        {
            this.Name = Name;
            this.Parameters = new List<SVMParameter>() { Parameter };
            this.samData = samData;
        }

        #region [Scoring Functions]
        public static double[,] CalculateConfusion(double[] answers, int[] correct, int labelCount)
        {
            double[,] confusionMatrix = new double[labelCount, labelCount];
            for (int i = 0; i < answers.Length; i++)
            {
                int y = correct[i];
                int v = (int)answers[i];

                confusionMatrix[y, v]++;
            }
            return confusionMatrix;
        }

        public static List<double> CalculateRecall(double[,] confusionMatrix, int labelCount)
        {
            List<double> recalls = new List<double>();
            for (int re = 0; re < labelCount; re++)
            {
                double totalPredict = 0;
                for (int we = 0; we < labelCount; we++)
                {
                    totalPredict += confusionMatrix[re, we];
                }
                double recall = (double)confusionMatrix[re, re] / totalPredict;
                recalls.Add(recall);
            }
            return recalls;
        }

        public static List<double> CalculatePrecision(double[,] confusionMatrix, int labelCount)
        {
            List<double> precisions = new List<double>();

            for (int qw = 0; qw < labelCount; qw++)
            {
                double totalClassPredicted = 0;
                for (int we = 0; we < labelCount; we++)
                {
                    totalClassPredicted += confusionMatrix[we, qw];
                }
                double pres = (double)confusionMatrix[qw, qw] / totalClassPredicted;
                precisions.Add(pres);
            }
            return precisions;
        }

        public static List<double> CalculateFScore(List<double> precision, List<double> recall, double betaValue = 1)
        {
            List<double> scores = new List<double>();

            for (int i = 0; i < precision.Count; i++)
            {
                try
                {
                    scores.Add((1 + Math.Pow(betaValue, 2)) * (precision[i] * recall[i]) / (precision[i] + recall[i]));
                }
                catch
                {
                    scores.Add(0);
                }
            }
            return scores;
        }

        public static List<List<bool>> CalculateCombinations(List<bool> currentForm, int numberOfFeautres)
        {
            List<List<bool>> theList = new List<List<bool>>();
            bool[] form1 = new bool[currentForm.Count + 1];
            currentForm.CopyTo(form1);
            form1[currentForm.Count] = true;

            bool[] form2 = new bool[currentForm.Count + 1];
            currentForm.CopyTo(form2);
            form2[currentForm.Count] = false;

            if (form1.Length == numberOfFeautres)
            {
                theList.Add(form1.ToList());
                if (form2.ToArray().Count(x => x == false) != numberOfFeautres)
                {
                    theList.Add(form2.ToList());
                }
                return theList;
            }
            List<List<bool>> otemp = CalculateCombinations(form1.ToList(), numberOfFeautres);
            theList.AddRange(otemp.GetRange(0, otemp.Count));

            List<List<bool>> ztemp = CalculateCombinations(form2.ToList(), numberOfFeautres);
            theList.AddRange(ztemp.GetRange(0, ztemp.Count));
            return theList;
        }
        #endregion

    }
}
