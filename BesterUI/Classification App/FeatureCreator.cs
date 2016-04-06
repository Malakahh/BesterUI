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



        const int GSR_LATENCY = 2000;
        const int GSR_DURATION = 5000;
        const int HR_LATENCY = 4000;
        const int HR_DURATION = 3000;
        const int EEG_LATENCY = 350;
        const int EEG_DURATION = 710;
        const int FACE_LATENCY = 500;
        const int FACE_DURATION = 500;
        
        const char SEPARATOR = '|';

        static FeatureCreator()
        {
            PopulateEEG();
            PopulateGSR();
            PopulateHR();
            PopulateFACE();

            allFeatures.AddRange(GSRArousalOptimizationFeatures);
            allFeatures.AddRange(HRArousalOptimizationFeatures);
            allFeatures.AddRange(HRValenceOptimizationFeatures);
            allFeatures.AddRange(EEGArousalOptimizationFeatures);
            allFeatures.AddRange(EEGValenceOptimizationFeatures);
            allFeatures.AddRange(FACEArousalOptimizationFeatures);
            allFeatures.AddRange(FACEValenceOptimizationFeatures);
        }


        #region Feature Value Accessors
        public static double GSRValueAccessor(DataReading d)
        {
            return ((GSRDataReading)d).resistance;
        }

        public static double HRValueAccessor(DataReading d)
        {
            return ((HRDataReading)d).BPM;
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

        public static double IBIMean(List<DataReading> data)
        {
            return data.Where(x => ((HRDataReading)x).isBeat || x == data.First()).Average(x => (int)((HRDataReading)x).IBI);
        }

        public static double IBISD(List<DataReading> data)
        {
            double avg = data.Where(x => ((HRDataReading)x).isBeat || x == data.First()).Average(x => (int)((HRDataReading)x).IBI);
            return Math.Sqrt(data.Where(x => ((HRDataReading)x).isBeat || x == data.First()).Average(x => Math.Pow((int)((HRDataReading)x).IBI - avg, 2)));
        }

        public static double HRVMean(List<DataReading> data)
        {
            List<double> hrv = new List<double>();
            HRDataReading lastBeat = (HRDataReading)data.First();
            foreach (HRDataReading d in data)
            {
                if (d.isBeat)
                {
                    if (lastBeat.IBI != null)
                    {
                        hrv.Add((int)d.IBI - (int)lastBeat.IBI);
                        lastBeat = d;
                    }
                    else
                    {
                        lastBeat = d;
                    }
                }
            }
            return hrv.Average(x => x);
        }

        public static double HRVRMSSD(List<DataReading> data)
        {
            List<double> sqauredHRV = new List<double>();
            HRDataReading lastBeat = (HRDataReading)data.First();
            foreach (HRDataReading d in data)
            {
                if (d.isBeat)
                {
                    if (lastBeat.IBI != null)
                    {
                        sqauredHRV.Add(Math.Pow((double)d.IBI - (double)lastBeat.IBI,2));
                        lastBeat = d;
                    }
                    else
                    {
                        lastBeat = d;
                    }
                }
            }
            return Math.Sqrt(sqauredHRV.Average(x => x));
        }

        public static double HRVSD(List<DataReading> data)
        {
            List<double> hrv = new List<double>();
            HRDataReading lastBeat = (HRDataReading)data.First();
            foreach (HRDataReading d in data)
            {
                if (d.isBeat)
                {
                    if (lastBeat.IBI != null)
                    {
                        hrv.Add((int)d.IBI - (int)lastBeat.IBI);
                    }
                    else
                    {
                        lastBeat = d;
                    }
                }
            }
            double avg = hrv.Average(x => x);
            return Math.Sqrt(hrv.Average(x => Math.Pow(x - avg, 2)));
        }

        public static double EEGPSD(List<DataReading> data, BandFrequencyDefinition BFD, Func<DataReading, double> valueAccesor)
        {
            List<FFT> ffts = new List<FFT>();
            for (int i = 0; i < data.Count - FFT.SAMPLING_WINDOW_LENGTH; i++)
            {
                FFT temp = new FFT(data.Skip(i).Take(FFT.SAMPLING_WINDOW_LENGTH).Select(x => valueAccesor(x)).ToList());
                if (double.IsNegativeInfinity(temp.AbsoluteBandPower[BFD.Label]))
                {
                    int j = 0;
                }
                ffts.Add(temp);

            }

            return ffts.Average(x => x.AbsoluteBandPower[BFD.Label]);
        }

        public static double DASM(List<DataReading> data, string band, Func<DataReading, double> valueAccessor1, Func<DataReading, double> valueAccessor2)
        {        
            FFT fft1 = new FFT(data.Select(x => valueAccessor1(x)).ToList());        
            FFT fft2 = new FFT(data.Select(x => valueAccessor2(x)).ToList());        
            return fft1.AbsoluteBandPower[band] - fft2.AbsoluteBandPower[band];        
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
            List<string> names = new List<string>() { "Delta", "Theta", "Alpha","Beta", "Gamma" };

            foreach (string name in names)
            {
                //Arousal 
                EEGArousalOptimizationFeatures.Add(new Feature("EEG DASM AF3-AF4",
                    (data, sam) =>
                    DASM(EEGDataSlice(data, sam), name,
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString())))));

                EEGArousalOptimizationFeatures.Add(new Feature("EEG DASM F3-F4",
                    (data, sam) =>
                    DASM(EEGDataSlice(data, sam), name,
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString())))));

                //Valence
                EEGValenceOptimizationFeatures.Add(new Feature("EEG DASM AF3-AF4",
                    (data, sam) =>
                    DASM(EEGDataSlice(data, sam), name,
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString())))));

                EEGValenceOptimizationFeatures.Add(new Feature("EEG DASM F3-F4",
                    (data, sam) =>
                    DASM(EEGDataSlice(data, sam), name,
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                    (x => EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString())))));

            }

        }

        static void PopulateHR()
        {
            //Arousal
            HRArousalOptimizationFeatures.Add(new Feature("HR Mean IBI", (data, sam) => IBIMean(HRDataSlice(data, sam))));
            HRArousalOptimizationFeatures.Add(new Feature("HR stdev IBI", (data, sam) => IBISD(HRDataSlice(data, sam))));
            HRArousalOptimizationFeatures.Add(new Feature("HRV RMSSD", (data, sam) => HRVRMSSD(HRDataSlice(data, sam))));


            //Valence
            HRValenceOptimizationFeatures.Add(new Feature("HR Mean IBI", (data, sam) => IBIMean(HRDataSlice(data, sam))));
            HRValenceOptimizationFeatures.Add(new Feature("HR stdev IBI", (data, sam) => IBISD(HRDataSlice(data, sam))));
            HRValenceOptimizationFeatures.Add(new Feature("HRV RMSSD", (data, sam) => HRVRMSSD(HRDataSlice(data, sam))));
            HRValenceOptimizationFeatures.Add(new Feature("HR Mean", (data, sam) => Mean(HRDataSlice(data, sam), HRValueAccessor)));
            HRValenceOptimizationFeatures.Add(new Feature("HR Max", (data, sam) => Max(HRDataSlice(data, sam), HRValueAccessor)));
            
        }

        static void PopulateGSR()
        {
            //Arousal
            GSRArousalOptimizationFeatures.Add(new Feature("GSR Mean", (data, sam) => Mean(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRArousalOptimizationFeatures.Add(new Feature("GSR stdev", (data, sam) => StandardDeviation(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRArousalOptimizationFeatures.Add(new Feature("GSR Min", (data, sam) => Min(GSRDataSlice(data, sam), GSRValueAccessor)));
            GSRArousalOptimizationFeatures.Add(new Feature("GSR Max", (data, sam) => Max(GSRDataSlice(data, sam), GSRValueAccessor)));
        }
        private static List<int> meanFaceLeftSide = new List<int> { 5, 13, 15 };
        private static List<int> meanFaceRightSide = new List<int> { 6, 14, 16 };

        private static List<int> sdFaceLeftSide = new List<int> { 5, 13, 15 };
        private static List<int> sdFaceRightSide = new List<int> { 6, 14, 16 };

        static void PopulateFACE()
        {
            //Valence
            for (int i = 0; i < meanFaceLeftSide.Count; i++)
            {
                int l = i;
                FACEValenceOptimizationFeatures.Add(new Feature("Face Mean " + meanFaceLeftSide[l] + " & " + meanFaceRightSide[l], (data, sam) => FaceMean(FaceDataSlice(data, sam),
                      (x => KinectValueAccessor(x, (FaceShapeAnimations)meanFaceLeftSide[l])),
                      (x => KinectValueAccessor(x, (FaceShapeAnimations)meanFaceRightSide[l])))));
            }
            for (int j = 0; j < sdFaceLeftSide.Count; j++)
            {
                int k = j;
                FACEValenceOptimizationFeatures.Add(new Feature("Face SD " + sdFaceLeftSide[k] + " & " + sdFaceRightSide[k],
                    (data, sam) => FaceStandardDeviation(FaceDataSlice(data, sam),
                    (x => KinectValueAccessor(x, (FaceShapeAnimations)sdFaceLeftSide.ElementAt(k))),
                    (x => KinectValueAccessor(x, (FaceShapeAnimations)sdFaceRightSide.ElementAt(k))))));
            }

            //Arousal
            FACEArousalOptimizationFeatures.Add(new Feature("Face Mean 11 & 12 ", (data, sam) => FaceMean(FaceDataSlice(data, sam),
                (x => KinectValueAccessor(x, (FaceShapeAnimations)11)),
                (x => KinectValueAccessor(x, (FaceShapeAnimations)12)))));
            
            FACEArousalOptimizationFeatures.Add(new Feature("Face SD 11 & 12 ",
                (data, sam) => FaceStandardDeviation(FaceDataSlice(data, sam),
                (x => KinectValueAccessor(x, (FaceShapeAnimations)11)),
                (x => KinectValueAccessor(x, (FaceShapeAnimations)12)))));

        }
    }
}
