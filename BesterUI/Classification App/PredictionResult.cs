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
        List<double> presions;
        List<double> fscores;
        public SVMParameter svmParams;

        public PredictionResult()
        {
            
        }
    }
}
