using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using System.Windows.Forms;
using System.IO;
using BesterUI.Helpers;
using System.Drawing;

namespace BesterUI
{
    public class FusionData
    {
        public List<HRDataReading> hrData = new List<HRDataReading>();
        public List<EEGDataReading> eegData = new List<EEGDataReading>();
        public List<GSRDataReading> gsrData = new List<GSRDataReading>();
        public List<FaceDataReading> faceData = new List<FaceDataReading>();

        public FusionData()
        {

        }

        public void Reset()
        {
            hrData.Clear();
            eegData.Clear();
            gsrData.Clear();
            Log.LogMessage("Data cleared.");
        }

        public void LoadData()
        {
            OpenFileDialog fb = new OpenFileDialog();
            fb.Multiselect = true;
            DialogResult res = fb.ShowDialog();

            if (res == DialogResult.OK)
            {
                //Commented out because it has 0 references
                //this.LoadFromFile(fb.FileNames);
            }
        }

        public void ExportData()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult res = dialog.ShowDialog();

            if (res != DialogResult.OK)
            {
                return;
            }

            if (hrData.Count <= 0 || eegData.Count <= 0 || gsrData.Count <= 0)
            {
                Log.LogMessage("No data to save..!");
                return;
            }

            foreach (HRDataReading r in hrData)
            {
                DataReading.StaticWrite("HR", r, dialog.SelectedPath);
            }

            foreach (EEGDataReading r in eegData)
            {
                DataReading.StaticWrite("EEG", r, dialog.SelectedPath);
            }

            foreach (GSRDataReading r in gsrData)
            {
                DataReading.StaticWrite("GSR", r, dialog.SelectedPath);
            }

            Reset();
        }

        public void AddFaceData(FaceDataReading data)
        {
            data.Write();
            faceData.Add(data);
        }

        public void AddHRData(HRDataReading data)
        {
            data.Write();
            hrData.Add(data);
        }

        public void AddEEGData(EEGDataReading data)
        {
            data.Write();
            eegData.Add(data);
        }

        public void AddGSRData(GSRDataReading data)
        {
            data.Write();
            gsrData.Add(data);
        }


        private const int MINIMUM_GSR_FILE_SIZE = 250;
        private const int MINIMUM_FACE_FILE_SIZE = 7000;
        private const int MINIMUM_HR_FILE_SIZE = 1000;
        private const int MINIMUM_EEG_FILE_SIZE = 45000;
        private const int EEG_FILTER_MIN_VALUE = 700;

        public Dictionary<string, bool> LoadFromFile(string[] filesToLoad, DateTime dT, bool checkSize = true)
        {
            Dictionary<string, bool> shouldRun = new Dictionary<string, bool>();
            foreach (string file in filesToLoad)
            {
                string s = file.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();
                double size;
                try
                {
                    size = new FileInfo(file).Length / 1024;
                }
                catch
                {
                    size = 0;
                }
                switch (s)
                {
                    case "GSR.dat":
                        if (!checkSize || size > MINIMUM_GSR_FILE_SIZE && File.Exists(file))
                        {
                            Log.LogMessage("Loading GSR data");
                            gsrData = GSRMedianFilter(DataReading.LoadFromFile<GSRDataReading>(file, dT), 15);
                            shouldRun.Add(s, true);
                        }
                        else
                        {
                            Log.LogMessage("File size for the GSR.dat was not big enough might be faulty");
                            shouldRun.Add(s, false);
                        }
                        break;
                    case "EEG.dat":
                        if (!checkSize || size > MINIMUM_EEG_FILE_SIZE && File.Exists(file))
                        {
                            Log.LogMessage("Loading EEG data");
                            eegData = EEGFilter(DataReading.LoadFromFile<EEGDataReading>(file, dT), EEG_FILTER_MIN_VALUE);
                            shouldRun.Add(s, true);
                        }
                        else
                        {
                            Log.LogMessage("File size for the EEG.dat was not big enough might be faulty");
                            shouldRun.Add(s, false);
                        }
                        break;

                    case "HR.dat":
                        if (!checkSize || size > MINIMUM_HR_FILE_SIZE && File.Exists(file))
                        {
                            Log.LogMessage("Loading HR data");
                            hrData = DataReading.LoadFromFile<HRDataReading>(file, dT);
                            shouldRun.Add(s, true);
                        }
                        else
                        {
                            Log.LogMessage("File size for the HR.dat was not big enough might be faulty");
                            shouldRun.Add(s, false);
                        }
                        break;
                    case "KINECT.dat":
                        if (!checkSize || size > MINIMUM_FACE_FILE_SIZE && File.Exists(file))
                        {
                            Log.LogMessage("Loading Face data");
                            faceData = DataReading.LoadFromFile<FaceDataReading>(file, dT);
                            shouldRun.Add(s, true);
                        }
                        else
                        {
                            Log.LogMessage("File size for the Kinect.dat was not big enough might be faulty");
                            shouldRun.Add(s, false);
                        }
                        break;
                    default:
                        throw new Exception("Sorry don't recognize the file name");
                }
            }
            return shouldRun;
        }

        public static List<EEGDataReading> EEGFilter(List<EEGDataReading> data, double lowerLimit)
        {
            Log.LogMessage("Removing artifacts from EEG data");
            return data.Where(x => x.data.Values.Min(y => y) > lowerLimit).ToList();
        }


        public static List<GSRDataReading> GSRMedianFilter(List<GSRDataReading> data, int windowSize)
        {
            Log.LogMessage("Doing median filter on GSR data");
            List<GSRDataReading> newValues = new List<GSRDataReading>();
            GSRDataReading LastValue = null;
            for (int i = 0; i < data.Count - windowSize; i++)
            {
                List<GSRDataReading> tempValues = data.Skip(i).Take(windowSize).ToList();
                tempValues = tempValues.OrderBy(x => x.resistance).ToList();
                if (LastValue != null || tempValues.ElementAt((int)Math.Round((double)windowSize / 2)) != LastValue)
                {
                    newValues.Add(tempValues.ElementAt((int)Math.Round((double)windowSize / 2)));
                    LastValue = tempValues.ElementAt((int)Math.Round((double)windowSize / 2));
                }
            }

            return newValues;

        }

        public void CreateDummyData()
        {
            //EEG
            EEGDataReading test = new EEGDataReading();
            test.data.Add("stuff", 2.1243);
            test.data.Add("stuff2", 3.1243);
            //  test.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 1.1);
            //  test.data.Add(EEGDataReading.ELECTRODE.AF4.GetName(), 2.1);
            test.Write();
            eegData.Add(test);

            EEGDataReading test2 = new EEGDataReading();
            //  test2.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 1.2);
            //  test2.data.Add(EEGDataReading.ELECTRODE.AF4.GetName(), 2.2);
            test2.Write();
            eegData.Add(test2);

            EEGDataReading test3 = new EEGDataReading();
            //  test3.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 13337.0);
            test3.Write();
            eegData.Add(test3);


            //GSR
            for (int i = 0; i < 200; i++)
            {
                GSRDataReading gsr = new GSRDataReading();
                gsr.resistance = 4;
                gsr.Write();
                gsrData.Add(gsr);
            }

            GSRDataReading gsr6 = new GSRDataReading();
            gsr6.resistance = 66666;
            gsr6.Write();
            gsrData.Add(gsr6);



            //Band
            for (int i = 0; i < 200; i++)
            {
                HRDataReading band = new HRDataReading();
                band.signal = 5;
                band.isBeat = false;
                band.Write();
                hrData.Add(band);
            }

            HRDataReading band1 = new HRDataReading();
            band1.signal = 1337;
            band1.isBeat = true;
            band1.Write();
            hrData.Add(band1);


        }

        public void ExportGRF(string inpath = "")
        {
            if (inpath == "") inpath = DataReading.GetWritePath();
            var events = File.ReadAllLines(inpath + @"\SecondTest.dat");
            string path = inpath + @"\Graph.grf";

            int TextLabelCount = 0;
            int FuncCount = 1;
            int PointSeriesCount = 0;
            int ShadeCount = 0;
            int RelationCount = 0;
            int OleObjectCount = 0;

            double xMin = double.MaxValue;
            double xMax = double.MinValue;
            double yMin = double.MaxValue;
            double yMax = double.MinValue;

            Func<Color, string> c2s = (color) =>
            {
                return "0x00" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            };

            Func<string, Color, int, int, string> AddShade = (label, color, from, to) =>
            {
                ShadeCount++;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"[Shade" + ShadeCount + "]");
                sb.AppendLine(@"LegendText = " + label);
                sb.AppendLine(@"ShadeStyle = 1");
                sb.AppendLine(@"BrushStyle = 0");
                sb.AppendLine(@"Color = " + c2s(color));
                sb.AppendLine(@"FuncNo = 1");
                sb.AppendLine(@"sMin = " + from);
                sb.AppendLine(@"sMax = " + to);
                sb.AppendLine(@"sMin2 = " + from);
                sb.AppendLine(@"sMax2 = " + to);
                sb.AppendLine(@"MarkBorder = 0");
                sb.AppendLine();

                return sb.ToString();
            };

            Func<string, Color, List<double>, List<double>, string> AddPointSeries = (label, color, xs, ys) =>
            {
                StringBuilder sb = new StringBuilder();
                PointSeriesCount++;
                sb.AppendLine(@"[PointSeries" + PointSeriesCount + "]");
                sb.AppendLine(@"FillColor = " + c2s(color));
                sb.AppendLine(@"LineColor = " + c2s(color));
                sb.AppendLine(@"Size = 0");
                sb.AppendLine(@"Style = 0");
                sb.AppendLine(@"LineStyle = 0");
                sb.AppendLine(@"LabelPosition = 1");
                sb.AppendLine(@"PointCount = " + xs.Count);

                sb.Append("Points = ");
                for (int pointId = 0; pointId < xs.Count; pointId++)
                {
                    sb.Append(xs[pointId] + "," + ys[pointId] + ";");
                }
                sb.AppendLine();
                sb.AppendLine(@"LegendText = " + label);
                sb.AppendLine();

                xMin = Math.Min(xMin, xs.Min());
                xMax = Math.Max(xMax, xs.Max());
                yMin = Math.Min(yMin, ys.Min());
                yMax = Math.Max(yMax, ys.Max());

                return sb.ToString();
            };

            if (!Directory.Exists(DataReading.GetWritePath()))
            {
                Directory.CreateDirectory(DataReading.GetWritePath());
            }

            List<string> shades = new List<string>();
            for (int eventId = 0; eventId < events.Length - 2; eventId++)
            {
                string[] evnt = events[eventId].Split('#');
                if (evnt[1].Contains("Bogus")) continue;

                shades.Add(AddShade(evnt[1], e2c(evnt[1]), int.Parse(evnt[0]), int.Parse(events[eventId + 1].Split('#')[0])));
            }


            //actual data
            List<string> pointSeries = new List<string>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            #region GSR
            //gsrData.ForEach(gsr => { x.Add(gsr.timestamp - gsrData[0].timestamp); y.Add(gsr.resistance); });
            #endregion
            #region HR
            hrData = hrData.Where(dat => dat.signal < 2000).ToList();
            //hrData.ForEach(hr => { x.Add(hr.timestamp - hrData[0].timestamp); y.Add(hr.signal); });
            hrData.ForEach(hr => { x.Add(hr.timestamp - hrData[0].timestamp); y.Add(hr.BPM); });
            //hrData.ForEach(hr => { x.Add(hr.timestamp - hrData[0].timestamp); y.Add(hr.IBI.Value); });
            #endregion
            #region EEG
            //eegData.ForEach(eeg => { x.Add(eeg.timestamp - eegData[0].timestamp); y.Add(eeg.data[EEGDataReading.ELECTRODE.AF3.ToString()] - eeg.data[EEGDataReading.ELECTRODE.AF4.ToString()]); });
            //eegData.ForEach(eeg => { x.Add(eeg.timestamp - eegData[0].timestamp); y.Add(eeg.data[EEGDataReading.ELECTRODE.F3.ToString()] - eeg.data[EEGDataReading.ELECTRODE.F4.ToString()]); });
            #endregion
            #region Kinect
            //nothing to see here, move along
            #endregion

            pointSeries.Add(AddPointSeries("Data", Color.Black, x, y));

            using (var f = File.CreateText(path))
            {
                f.WriteLine(@"[Graph]");
                f.WriteLine(@"Version = 4.4.2.543");
                f.WriteLine(@"MinVersion = 2.5");
                f.WriteLine(@"OS = Windows NT 6.2");
                f.WriteLine();

                f.WriteLine(@"[Func1]");
                f.WriteLine(@"FuncType = 0");
                f.WriteLine(@"y = " + yMax);
                f.WriteLine(@"Color = clNone");
                f.WriteLine(@"Size = 0");

                f.WriteLine();

                //shades go here
                foreach (var shade in shades)
                {
                    f.Write(shade);
                }
                //end shades

                //data series go here
                foreach (var ps in pointSeries)
                {
                    f.Write(ps);
                }
                //end data series

                f.WriteLine(@"[Axes]");
                f.WriteLine(@"xMin = " + xMin);
                f.WriteLine(@"xMax = " + xMax);
                f.WriteLine(@"xTickUnit = 1");
                f.WriteLine(@"xGridUnit = 1");
                f.WriteLine(@"xAxisCross = " + yMin);
                f.WriteLine(@"yMin = " + yMin);
                f.WriteLine(@"yMax = " + yMax);
                f.WriteLine(@"yTickUnit = 2");
                f.WriteLine(@"yGridUnit = 2");
                f.WriteLine(@"AxesColor = clBlack");
                f.WriteLine(@"GridColor = 0x00FF9999");
                f.WriteLine(@"ShowLegend = 0");
                f.WriteLine(@"Radian = 1");
                f.WriteLine(@"LegendPlacement = 0");
                f.WriteLine(@"LegendPos = 0,0");

                f.WriteLine();

                f.WriteLine(@"[Data]");
                f.WriteLine(@"TextLabelCount = " + TextLabelCount);
                f.WriteLine(@"FuncCount = " + FuncCount);
                f.WriteLine(@"PointSeriesCount = " + PointSeriesCount);
                f.WriteLine(@"ShadeCount = " + ShadeCount);
                f.WriteLine(@"RelationCount = " + RelationCount);
                f.WriteLine(@"OleObjectCount = " + OleObjectCount);
            }

            Log.LogMessage("DonnoDK");
        }

        //event 2 color
        Color e2c(string evnt)
        {
            if (evnt == "TaskWizard - BtnCompleteClicked") return Color.Green;
            if (evnt == "TaskWizard - BtnIncompleteClicked") return Color.Red;
            if (evnt == "SendDraft error shown") return Color.Purple;
            if (evnt == "CreateDraft, language changed to: US") return Color.Purple;
            if (evnt == "AddAttachmentButtonClick: 1") return Color.Turquoise;
            if (evnt == "AddAttachmentButtonClick: 2") return Color.Teal;
            if (evnt == "AddAttachmentButtonClick: 3") return Color.Blue;
            if (evnt == "AddAttachment complete") return Color.BlueViolet;
            if (evnt == "Add Contact Button click: 1") return Color.PaleVioletRed;
            if (evnt == "Add Contact Button click: 2") return Color.MediumVioletRed;
            if (evnt == "Add Contact Button click: 3") return Color.IndianRed;
            if (evnt == "AddContact complete") return Color.HotPink;
            if (evnt == "RemoveContact clicked") return Color.GreenYellow;

            return Color.Black;
        }
    }
}
