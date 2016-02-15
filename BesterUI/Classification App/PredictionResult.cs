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

        public PredictionResult(double[,] ConfusionMatrix, List<double> Recalls, List<double> Precisions, List<double> Fscores, SVMParameter SVMParam, List<Feature> Features)
        {
            confusionMatrix = ConfusionMatrix;
            recalls = Recalls;
            precisions = Precisions;
            fscores = Fscores;
            svmParams = SVMParam;
            features = Features;

        }

        //TODO: Make some printing functions
    }
}
