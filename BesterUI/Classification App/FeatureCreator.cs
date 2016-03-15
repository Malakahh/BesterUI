using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using Microsoft.Kinect.Face;

namespace Classification_App
{
    public static class FeatureCreator
    {
        public static List<Feature> allFeatures = new List<Feature>();

        public static List<Feature> GSRFeatures = new List<Feature>();
        public static List<Feature> GSRArousalOptimizationFeatures = new List<Feature>();

        public static List<Feature> HRFeatures = new List<Feature>();
        public static List<Feature> HRArousalOptimizationFeatures = new List<Feature>();
        public static List<Feature> HRValenceOptimizationFeatures = new List<Feature>();

        public static List<Feature> EEGFeatures = new List<Feature>();
        public static List<Feature> EEGArousalOptimizationFeatures = new List<Feature>();
        public static List<Feature> EEGValenceOptimizationFeatures = new List<Feature>();

        public static List<Feature> FACEFeatures = new List<Feature>();
        public static List<Feature> FACEArousalOptimizationFeatures = new List<Feature>();
        public static List<Feature> FACEValenceOptimizationFeatures = new List<Feature>();



        const int GSR_LATENCY = 3000;
        const int GSR_DURATION = 4000;
        const int HR_LATENCY = 3000;
        const int HR_DURATION = 4000;
        const int EEG_LATENCY = 0;
        const int EEG_DURATION = 3000;
        const int FACE_LATENCY = 0;
        const int FACE_DURATION = 7000;

        const char SEPARATOR = '|';

        static FeatureCreator()
        {
            PopulateEEG();
            PopulateGSR();
            PopulateHR();
            PopulateFACE();

            allFeatures.AddRange(GSRFeatures);
            allFeatures.AddRange(HRFeatures);
            allFeatures.AddRange(EEGFeatures);
            allFeatures.AddRange(FACEFeatures);

            //Arousal Features
            GSRArousalOptimizationFeatures.Add(GSRFeatures.Find(x => x.name.Contains("stdev")));
            GSRArousalOptimizationFeatures.Add(GSRFeatures.Find(x => x.name.Contains("Mean")));
            GSRArousalOptimizationFeatures.Add(GSRFeatures.Find(x => x.name.Contains("Median")));
            GSRArousalOptimizationFeatures.Add(GSRFeatures.Find(x => x.name.Contains("Min")));
            GSRArousalOptimizationFeatures.Add(GSRFeatures.Find(x => x.name.Contains("Max")));
            
            HRArousalOptimizationFeatures.Add(HRFeatures.Find(x => x.name.Contains("stdev")));
            EEGArousalOptimizationFeatures.Add(EEGFeatures.Find(x => x.name.Contains("stdev") && x.name.Contains("F3")));
            //FACEArousalOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("stdev")));

            //Valence Features
            HRValenceOptimizationFeatures.Add(HRFeatures.Find(x => x.name.Contains("stdev")));

            //FACE FEatures
            FACEArousalOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("SD") && x.name.Contains("11")));
            FACEArousalOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("Mean") && x.name.Contains("11")));
            FACEValenceOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("Mean") && x.name.Contains("5")));
            FACEValenceOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("Mean") && x.name.Contains("13")));
            FACEValenceOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("Mean") && x.name.Contains("15")));
            FACEValenceOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("SD") && x.name.Contains("5")));
            FACEValenceOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("SD") && x.name.Contains("13")));
            FACEValenceOptimizationFeatures.Add(FACEFeatures.Find(x => x.name.Contains("SD") && x.name.Contains("5")));
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

        public static double KinectValueAccessor(DataReading d, FaceShapeAnimations FSA)
        {
            return ((FaceDataReading)d).data[FSA];
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

        public static double DASM(List<DataReading> data, string band, Func<DataReading, double> valueAccessor1, Func<DataReading, double> valueAccessor2)
        {
            FFT fft1 = new FFT(data.Select(x => valueAccessor1(x)).ToList());
            FFT fft2 = new FFT(data.Select(x => valueAccessor2(x)).ToList());
            return fft1.AbsoluteBandPower[band] - fft2.AbsoluteBandPower[band];
        }

        public static double FaceMean(List<DataReading> data, Func<DataReading, double> valueAccessor1, Func<DataReading, double> valueAccessor2)
        {
            return (data.Average(x => valueAccessor1(x)) + data.Average(x => valueAccessor2(x))) / 2;
        }

        public static double FaceStandardDeviation(List<DataReading> data, Func<DataReading, double> valueAccessor1, Func<DataReading, double> valueAccessor2)
        {

            double avg1 = Mean(data, valueAccessor1);
            double sd1 = Math.Sqrt(data.Average(x => Math.Pow(valueAccessor1(x) - avg1, 2)));

            double avg2 = Mean(data, valueAccessor2);
            double sd2 = Math.Sqrt(data.Average(x => Math.Pow(valueAccessor2(x) - avg2, 2)));
            return (sd1 + sd2) / 2;
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

        static List<DataReading> FaceDataSlice(List<DataReading> data, SAMDataPoint sam)
        {
            return data.SkipWhile(x => x.timestamp < sam.timeOffset + FACE_LATENCY).TakeWhile(x => x.timestamp < sam.timeOffset + FACE_LATENCY + FACE_DURATION).ToList();
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
          /*  foreach (var electrode in Enum.GetNames(typeof(EEGDataReading.ELECTRODE)))
            {
                Func<DataReading, double> va = (x => EEGValueAccessor(x, electrode));

                EEGFeatures.Add(new Feature("EEG " + electrode + " Mean", (data, sam) => Mean(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Median", (data, sam) => Median(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " stdev", (data, sam) => StandardDeviation(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Max", (data, sam) => Max(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Min", (data, sam) => Min(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " First", (data, sam) => First(EEGDataSlice(data, sam), va)));
                EEGFeatures.Add(new Feature("EEG " + electrode + " Last", (data, sam) => Last(EEGDataSlice(data, sam), va)));
            }*/

            //dasm

            //## START DASM ##
            List<Tuple<EEGDataReading.ELECTRODE, EEGDataReading.ELECTRODE>> dasmPairs = new List<Tuple<EEGDataReading.ELECTRODE, EEGDataReading.ELECTRODE>>();
            dasmPairs.Add(Tuple.Create(EEGDataReading.ELECTRODE.AF3, EEGDataReading.ELECTRODE.AF4));
            dasmPairs.Add(Tuple.Create(EEGDataReading.ELECTRODE.F3, EEGDataReading.ELECTRODE.F4));
            dasmPairs.Add(Tuple.Create(EEGDataReading.ELECTRODE.F7, EEGDataReading.ELECTRODE.F8));
            dasmPairs.Add(Tuple.Create(EEGDataReading.ELECTRODE.T7, EEGDataReading.ELECTRODE.T8));
            dasmPairs.Add(Tuple.Create(EEGDataReading.ELECTRODE.P7, EEGDataReading.ELECTRODE.P8));
            dasmPairs.Add(Tuple.Create(EEGDataReading.ELECTRODE.O1, EEGDataReading.ELECTRODE.O2));

            //Not original dasm12 pair
            // dasmPairs.Add(Tuple.Create(Electrodes.FC5, Electrodes.FC6));
            List<string> bandNames = new List<string> { "Delta", "Theta", "Alpha","Beta", "Gamma"};
            foreach (var pair in dasmPairs)
            {
                EEGDataReading.ELECTRODE a = pair.Item1;
                EEGDataReading.ELECTRODE b = pair.Item2;
                foreach (string bandName in bandNames)
                {
                    if (a != null && b != null)
                    {
                        //Test this
                        EEGFeatures.Add(new Feature("DASM " + a.ToString() + a.ToString().Last(),
                            (data, sam) => {
                                return DASM(EEGDataSlice(data, sam), bandName, 
                                    (x => EEGValueAccessor(x, Enum.GetName(typeof(EEGDataReading.ELECTRODE), a))),
                                    (x => EEGValueAccessor(x, Enum.GetName(typeof(EEGDataReading.ELECTRODE), a))));
                            }
                            ));
                    }
                }
            }
            //## END DASM ##

            //## START INDIVIDUAL ##
            /*foreach (var elec in FusionDataReference.eeg.electrodes)
            {
                features.Add(elec.data[timeIndex].fft.AbsoluteBandPower[BandFrequencyDefinition.Delta.Label]);
                features.Add(elec.data[timeIndex].fft.AbsoluteBandPower[BandFrequencyDefinition.Theta.Label]);
                features.Add(elec.data[timeIndex].fft.AbsoluteBandPower[BandFrequencyDefinition.Alpha.Label]);
                features.Add(elec.data[timeIndex].fft.AbsoluteBandPower[BandFrequencyDefinition.Beta.Label]);
                features.Add(elec.data[timeIndex].fft.AbsoluteBandPower[BandFrequencyDefinition.Gamma.Label]);
            }*/
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
        private static List<int> meanFaceLeftSide = new List<int> { 5, 13, 15, 11 };
        private static List<int> meanFaceRightSide = new List<int> { 6, 14, 16, 12 };

        private static List<int> sdFaceLeftSide = new List<int> { 5, 13, 15, 11 };
        private static List<int> sdFaceRightSide = new List<int> { 6, 14, 16, 12 };

        static void PopulateFACE()
        {
            for (int i = 0; i < meanFaceLeftSide.Count; i++)
            {
                FACEFeatures.Add(new Feature("Face Mean " +meanFaceLeftSide[i]+ " & " + meanFaceRightSide[i], (data, sam) => FaceMean(FaceDataSlice(data, sam),
                    (x => KinectValueAccessor(x, (FaceShapeAnimations)meanFaceLeftSide[i])),
                    (x => KinectValueAccessor(x, (FaceShapeAnimations)meanFaceRightSide[i])))));
            }

            for (int j = 0; j < sdFaceLeftSide.Count; j++)
            {
                FACEFeatures.Add(new Feature("Face SD " + sdFaceLeftSide[j] + " & " + sdFaceRightSide[j],
                    (data, sam) => FaceStandardDeviation(FaceDataSlice(data, sam),
                    (x => KinectValueAccessor(x, (FaceShapeAnimations)sdFaceLeftSide[j])),
                    (x => KinectValueAccessor(x, (FaceShapeAnimations)sdFaceRightSide[j])))));
            }
        }
    }
}
