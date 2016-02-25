using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    public class PredictionResult
    {
        public double[,] confusionMatrix { get; private set; }
        public List<double> recalls { get; private set; }
        public List<double> precisions { get; private set; }
        public List<double> fscores { get; private set; }
        public SVMParameter svmParams { get; private set; }
        public List<Feature> features { get; private set; }
        public List<int> guesses = new List<int>();
        public List<int> answers = new List<int>();

        public PredictionResult(double[,] ConfusionMatrix, List<double> Recalls, List<double> Precisions, List<double> Fscores, SVMParameter SVMParam, List<Feature> Features, List<int> Answers, List<int> Guesses)
        {
            confusionMatrix = ConfusionMatrix;
            recalls = Recalls;
            precisions = Precisions;
            fscores = Fscores;
            svmParams = SVMParam;
            features = Features;
            guesses = Guesses;
            answers = Answers;
        }

        public double AverageFScore()
        {
            double total = 0;
            for (int i = 0; i < fscores.Count; i++)
            {
                if (!double.IsNaN(fscores[i]))
                {
                    total += fscores[i] * answers.Count(x => x == i);
                }
            }
            total /= answers.Count;
            return total;
        }

        public SVMConfiguration GenerateConfiguration()
        {
            SVMConfiguration conf = new SVMConfiguration(svmParams, features);
            conf.Name = "SVM_" + svmParams.C + "_" + svmParams.Gamma + "_F" + features.Count;
            return conf;
        }

        //TODO: Make some printing functions
    }
}
