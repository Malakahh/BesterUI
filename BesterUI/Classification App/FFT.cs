using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNumerics;
using System.IO;

namespace Classification_App
{
    class FFT
    {
        public static int SAMPLING_WINDOW_LENGTH = 256;

        complex[] rawFFTOutput;

        public double[] FrequencyPowerSampling;

        public Dictionary<string, double> AbsoluteBandPower = new Dictionary<string, double>();
        public Dictionary<string, double> RelativeBandPower = new Dictionary<string, double>();


        //Used to calculate relativeBandPower
        double totalFrequencyPower = 0;

        //Source of amplitude & dB calculations:
        //http://www.silisoftware.com/tools/db.php
        //(See javascript) These might be shaky...
        public static double Amplitude2Decibel(double amplitude)
        {
            return 20 * Math.Log(amplitude) / Math.Log(10);
        }

        public static double Decibel2Amplitude(double dB)
        {
            return Math.Pow(10, dB / 20);
        }

        /// <summary>
        /// Computes FFT for the given sampleset.
        /// </summary>
        /// <param name="Samples">Sampleset on which to compute a FFT</param>
        public FFT(List<double> Samples, List<BandFrequencyDefinition> CustomBands = null)
        {
            using (ILScope.Enter())
            {
                ILInArray<double> inArr = Samples.ToArray();
                ILRetArray<complex> output = ILMath.fft(inArr);
                rawFFTOutput = output.ToArray();
            }
            
            ComputeFrequencyPowerSamples();

            //FrequencyBands
            ComputeAbsoluteBandPower(BandFrequencyDefinition.Delta);
            ComputeAbsoluteBandPower(BandFrequencyDefinition.Theta);
            ComputeAbsoluteBandPower(BandFrequencyDefinition.Alpha);
            ComputeAbsoluteBandPower(BandFrequencyDefinition.Beta);
            ComputeAbsoluteBandPower(BandFrequencyDefinition.Gamma);
            if (CustomBands != null)
            {
                foreach (BandFrequencyDefinition customBand in CustomBands)
                {
                    ComputeAbsoluteBandPower(customBand);
                }
            }
        }
        
        private void ComputeFrequencyPowerSamples()
        {
            
            FrequencyPowerSampling = new double[rawFFTOutput.Length / 2];

            for (int i = 0; i < FrequencyPowerSampling.Length; i++)
            {
                complex c = rawFFTOutput[i];
                double magnitude = Math.Sqrt(c.real * c.real + c.imag * c.imag);
                FrequencyPowerSampling[i] = magnitude / FrequencyPowerSampling.Length;
            }
        }

        private void ComputeAbsoluteBandPower(BandFrequencyDefinition def)
        {
            double bandPower = 0;

            for (int i = def.LowerLimit; i <= def.UpperLimit; i++)
            {
                if (i >= rawFFTOutput.Length)
                {
                    BesterUI.Helpers.Log.LogMessage($"Not enough FFT samples {rawFFTOutput.Length} vs {def.UpperLimit}");
                    break;
                }
                complex c = rawFFTOutput[i];
                double magnitude = Math.Sqrt(c.real * c.real + c.imag * c.imag);
                bandPower += Math.Pow(magnitude, 2);
            }
            //Add to list of frequency bands
            AbsoluteBandPower.Add(def.Label, bandPower);
            totalFrequencyPower += bandPower;
        }


        /// <summary>
        /// This functions should be used to calculate the relative band power, AFTER the absolute band power is calculated
        /// </summary>
        /// <param name="band">The band for which the relative power should be calculated</param>
        private void ComputeRelativeBandPower(BandFrequencyDefinition band)
        {
            if (AbsoluteBandPower.Keys.Contains<string>(band.Label))
            {
                RelativeBandPower.Add(band.Label, AbsoluteBandPower[band.Label] / totalFrequencyPower);
            }
            else
            {
                throw new Exception("The requested band [" + band.Label + "] isn't in the AbsoluteBandPower List please make sure to compute the absolute powers before using relative powers");
            }

        }
    }

    public class BandFrequencyDefinition
    {
        public static BandFrequencyDefinition Delta = new BandFrequencyDefinition(1, 3, "Delta");
        public static BandFrequencyDefinition Theta = new BandFrequencyDefinition(4, 7, "Theta");
        public static BandFrequencyDefinition Alpha = new BandFrequencyDefinition(8, 13, "Alpha");
        public static BandFrequencyDefinition Beta = new BandFrequencyDefinition(14, 30, "Beta");
        public static BandFrequencyDefinition Gamma = new BandFrequencyDefinition(31, 45, "Gamma");

        public static List<BandFrequencyDefinition> preDef = new List<BandFrequencyDefinition>()
        {
            Delta, Theta, Alpha, Beta, Gamma
        };

        public int LowerLimit;
        public int UpperLimit;
        public string Label;

        public BandFrequencyDefinition(int LowerLimit, int UpperLimit, string Label)
        {
            this.LowerLimit = LowerLimit;
            this.UpperLimit = UpperLimit;
            this.Label = Label;
        }
    }
}
