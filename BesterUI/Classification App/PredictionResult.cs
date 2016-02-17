using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    class PredictionResult
    {
        double[,] confusionMatrix;
        List<double> recalls;
        List<double> precisions;
        List<double> fscores;
        public SVMParameter svmParams;
        public List<Feature> features;
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

        //TODO: Make some printing functions
    }
}
