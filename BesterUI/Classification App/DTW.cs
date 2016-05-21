using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class DTW
    {
        public static double[,] CalcDTW(List<double> dataset1, List<double> dataset2, int windowSize)
        {
            double[,] dtw = new double[dataset1.Count, dataset2.Count];
            windowSize = Math.Max(windowSize, Math.Abs(dataset1.Count - dataset2.Count));

            for (int i = 0; i < dataset1.Count; i++)
            {
                for (int j = 0; j < dataset2.Count; j++)
                {
                    dtw[i, j] = Double.MaxValue;
                }
            }

            dtw[0, 0] = 0;

            for (int i = 1; i < dataset1.Count; i++)
            {
                for (int j = Math.Max(1, i - windowSize); j < Math.Min(dataset2.Count, i + windowSize); j++)
                {
                    double cost = distance(dataset1[i], dataset2[j]);
                    dtw[i, j] = cost + Math.Min(dtw[i - 1, j],          //Insertion
                                                Math.Min(
                                                dtw[i, j - 1],      //Deletion
                                                dtw[i - 1, j - 1]   //Match
                    ));
                }
            }

            return dtw;
        }

        private static double distance(double x, double y)
        {
            return Math.Abs(x - y);
        }
    }
}
