using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BesterUI
{
    class SAnalysis
    {

        public static BoxPlot BoxPlot(List<Tuple<double, double>> data)
        {
            return new BoxPlot(data.Select(x => x.Item2).ToList());
        }
    }

    class BoxPlot
    {
        public double median;
        public double maxValue;
        public double minValue;
        public double firstQuartile;
        public double thirdQuartile;
        public double lowerOuterFence;
        public double lowerInnerFence;
        public double upperOuterFence;
        public double upperInnerFence;

        public BoxPlot(List<double> data)
        {
            List<double> orderedList = data.OrderBy(x => x).ToList();
            //Min & Max
            minValue = orderedList.Min(x => x);
            maxValue = orderedList.Max(x => x);

            //Median
            if (data.Count % 2 != 0)
            {
                median = (orderedList[(int)(data.Count / 2)] + orderedList[(int)Math.Round((double)data.Count / 2)])/2;
            }
            else
            {
                median = orderedList.ElementAt((int)(data.Count / 2));
            }
            //First Quartile
            List<double> firstQList = orderedList.Where(x => x < median).ToList();
            if (firstQList.Count % 2 != 0)
            {
                firstQuartile = (firstQList[(int)(firstQList.Count / 2)] + firstQList[(int)Math.Round((double)firstQList.Count / 2)])/2;
            }
            else
            {
                firstQuartile = firstQList.ElementAt((int)(firstQList.Count / 2));
            }

            //Second Quartile
            List<double> thirdQList = orderedList.Where(x => x > median).ToList();
            if (thirdQList.Count % 2 != 0)
            {
                thirdQuartile = (thirdQList[(int)(thirdQList.Count / 2)] + thirdQList[(int)Math.Round((double)thirdQList.Count / 2)])/2;
            }
            else
            {
                thirdQuartile = thirdQList.ElementAt((int)(thirdQList.Count / 2));
            }

            double interquartileRange = Math.Abs(firstQuartile - thirdQuartile);
            //outerfences
            lowerInnerFence = firstQuartile - (interquartileRange * 1.5);
            lowerOuterFence = firstQuartile - (interquartileRange * 3);

            //innerfences
            upperInnerFence = thirdQuartile + (interquartileRange * 1.5);
            upperOuterFence = thirdQuartile + (interquartileRange * 3);
        }
    }
}
