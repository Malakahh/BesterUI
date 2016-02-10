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
        public abstract List<SVMParameter> Parameters { get; set; }

        public Classifier(string Name, List<SVMParameter> Parameters)
        {
            this.Name = Name;
            this.Parameters = Parameters;
        }

        public Classifier(string Name, SVMParameter Parameter)
        {
            this.Name = Name;
            this.Parameters = new List<SVMParameter>() { Parameter };
        }

        //public abstract void CrossValidate(List<SVMParameter> Parameters, bool RunCombinations);


        #region [Helper Functions]
        abstract public void PrintResults();
        #endregion

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

        public static List<string> CalculateRecall(double[,] confusionMatrix, int labelCount)
        {
            List<string> recalls = new List<string>();
            for (int re = 0; re < labelCount; re++)
            {
                double totalPredict = 0;
                for (int we = 0; we < labelCount; we++)
                {
                    totalPredict += confusionMatrix[re, we];
                }
                double recall = (double)confusionMatrix[re, re] / totalPredict;
                recalls.Add(recall.ToString());
            }
            return recalls;
        }

        public static List<string> CalculatePrecision(double[,] confusionMatrix, int labelCount)
        {
            List<string> precisions = new List<string>();

            for (int qw = 0; qw < labelCount; qw++)
            {
                double totalClassPredicted = 0;
                for (int we = 0; we < labelCount; we++)
                {
                    totalClassPredicted += confusionMatrix[we, qw];
                }
                double pres = (double)confusionMatrix[qw, qw] / totalClassPredicted;
                precisions.Add(pres.ToString());
            }
            return precisions;
        }

        public static List<double> CalculateFScore(List<string> precision, List<string> recall, double betaValue)
        {
            List<double> scores = new List<double>();

            for (int i = 0; i < precision.Count; i++)
            {
                try
                {
                    scores.Add((1 + Math.Pow(betaValue, 2)) * (double.Parse(precision[i]) * double.Parse(recall[i])) / (double.Parse(precision[i]) + double.Parse(recall[i])));
                }
                catch
                {
                    scores.Add(0);
                }
            }
            return scores;
        }
        #endregion

    }
}
