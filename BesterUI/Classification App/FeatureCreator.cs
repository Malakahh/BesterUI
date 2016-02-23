using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;

namespace Classification_App
{
    public static class FeatureCreator
    {
        public static List<Feature> allFeatures = new List<Feature>();
        public static List<Feature> GSRFeatures = new List<Feature>();
        public static List<Feature> HRFeatures = new List<Feature>();
        public static List<Feature> EEGFeatures = new List<Feature>();


        const int GSR_LATENCY = 3000;
        const int GSR_DURATION = 4000;
        const int HR_LATENCY = 3000;
        const int HR_DURATION = 4000;
        const int EEG_LATENCY = 0;
        const int EEG_DURATION = 3000;

        const char SEPARATOR = '|';

        static FeatureCreator()
        {
            PopulateEEG();
            PopulateGSR();
            PopulateHR();

            allFeatures.AddRange(GSRFeatures);
            allFeatures.AddRange(HRFeatures);
            allFeatures.AddRange(EEGFeatures);
        }

        #region Feature Value Accessors
        public static double GSRValueAccessor(DataReading d)
        {
            return ((GSRDataReading)d).resistance;
        }

        public static double HRValueAccessor(DataReading d)
        {
            return ((HRDataReading)d).signal;
        }

        public static double EEGValueAccessor(DataReading d, string electrode)
        {
            return ((EEGDataReading)d).data[electrode];
        }
        #endregion
        #region Stats
        public static double Mean(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            return data.Average(x => valueAccessor(x));
        }

        public static double Median(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            data.OrderBy(x => valueAccessor(x));
            return data.Count % 2 == 0 ? 0.5 * (valueAccessor(data[data.Count / 2]) + valueAccessor(data[data.Count / 2 + 1])) : valueAccessor(data[data.Count / 2]);
        }

        public static double StandardDeviation(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            double avg = Mean(data, valueAccessor);
            return Math.Sqrt(data.Average(x => Math.Pow(valueAccessor(x) - avg, 2)));
        }

        public static double Min(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            return data.Min(x => valueAccessor(x));
        }

        public static double Max(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            return data.Max(x => valueAccessor(x));
        }

        public static double First(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            return valueAccessor(data.First());
        }

        public static double Last(List<DataReading> data, Func<DataReading, double> valueAccessor)
        {
            return valueAccessor(data.Last());
        }
        #endregion
        #region DataSlicing
        static List<DataReading> EEGDataSlice(List<DataReading> data, SAMDataPoint sam)
        {
            return data.SkipWhile(x => x.timestamp < sam.timeOffset + EEG_LATENCY).TakeWhile(x => x.timestamp < sam.timeOffset + EEG_LATENCY + EEG_DURATION).ToList();
        }

        static List<DataReading> GSRDataSlice(List<DataReading> data, SAMDataPoint sam)
        {
            return data.SkipWhile(x => x.timestamp < sam.timeOffset + GSR_LATENCY).TakeWhile(x => x.timestamp < sam.timeOffset + GSR_LATENCY + GSR_DURATION).ToList();
        }

        static List<DataReading> HRDataSlice(List<DataReading> data, SAMDataPoint sam)
        {
            return data.SkipWhile(x => x.timestamp < sam.timeOffset + HR_LATENCY).TakeWhile(x => x.timestamp < sam.timeOffset + HR_LATENCY + HR_DURATION).ToList();
        }
        #endregion

        public static string GetStringFromFeatures(List<Feature> feats)
        {
            StringBuilder sb = new StringBuilder(feats[0].name);

            for (int i = 1; i < feats.Count; i++)
            {
                sb.Append(SEPARATOR + feats[i].name);
            }

            return sb.ToString();
        }

        public static List<Feature> GetFeaturesFromString(string input)
        {
            List<string> featNames = input.Split(SEPARATOR).ToList();

            return allFeatures.Where(x => featNames.Contains(x.name)).ToList();
        }

        static void PopulateEEG()
        {
            foreach (var electrode in Enum.GetNames(typeof(EEGDataReading.ELECTRODE)))
            {
                Func<DataReading, double> va = (x => EEGValueAccessor(x, electrode));

                EEGFeatures.Add(new Feature("EEG " + electrode + " Mean", (data, sam) => Mean(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Median", (data, sam) => Median(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " stdev", (data, sam) => StandardDeviation(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Max", (data, sam) => Max(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Min", (data, sam) => Min(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " First", (data, sam) => First(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Last", (data, sam) => Last(EEGDataSlice(data, sam), va)));
            }

            //dasm
        }

        static void PopulateHR()
        {
            HRFeatures.Add(new Feature("HR Mean", (data, sam) => Mean(HRDataSlice(data, sam), HRValueAccessor)));
            HRFeatures.Add(new Feature("HR Median", (data, sam) => Median(HRDataSlice(data, sam), HRValueAccessor)));
            HRFeatures.Add(new Feature("HR stdev", (data, sam) => StandardDeviation(HRDataSlice(data, sam), HRValueAccessor)));
            HRFeatures.Add(new Feature("HR Max", (data, sam) => Max(HRDataSlice(data, sam), HRValueAccessor)));
            HRFeatures.Add(new Feature("HR Min", (data, sam) => Min(HRDataSlice(data, sam), HRValueAccessor)));
            HRFeatures.Add(new Feature("HR First", (data, sam) => First(HRDataSlice(data, sam), HRValueAccessor)));
            HRFeatures.Add(new Feature("HR Last", (data, sam) => Last(HRDataSlice(data, sam), HRValueAccessor)));
        }



        static void PopulateGSR()
        {
            GSRFeatures.Add(new Feature("GSR Mean", (data, sam) => Mean(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRFeatures.Add(new Feature("GSR Median", (data, sam) => Median(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRFeatures.Add(new Feature("GSR stdev", (data, sam) => StandardDeviation(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRFeatures.Add(new Feature("GSR Max", (data, sam) => Max(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRFeatures.Add(new Feature("GSR Min", (data, sam) => Min(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRFeatures.Add(new Feature("GSR First", (data, sam) => First(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRFeatures.Add(new Feature("GSR Last", (data, sam) => Last(GSRDataSlice(data, sam), GSRValueAccessor)));
        }
    }
}
