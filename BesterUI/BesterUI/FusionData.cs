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

        public bool Loaded
        {
            get { return hrData.Count != 0 || eegData.Count != 0 || gsrData.Count != 0 || faceData.Count != 0; }
        }


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

        public Dictionary<string, bool> LoadFromFile(string[] filesToLoad)
        {
            return LoadFromFile(filesToLoad, DateTime.MinValue, false);
        }

        public Dictionary<string, bool> LoadFromFile(string[] filesToLoad, DateTime dT, bool checkSize = true)
        {
            Dictionary<string, bool> shouldRun = new Dictionary<string, bool>();
            foreach (string file in filesToLoad)
            {
                string s = file.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries).Last();
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
                            //gsrData = GSRMedianFilter(DataReading.LoadFromFile<GSRDataReading>(file, dT), 25);
                            //gsrData = GSRSTDEVFilter(DataReading.LoadFromFile<GSRDataReading>(file, dT));
                            gsrData = GSRMoveAvgFilter(DataReading.LoadFromFile<GSRDataReading>(file, dT), 25);
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
                            hrData = hrData.Where(x => x.signal < 2000).ToList();
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
            for (int i = 0; i < data.Count - windowSize; i++)
            {
                List<GSRDataReading> tempValues = data.Skip(i).Take(windowSize).OrderBy(x => x.resistance).ToList();
                newValues.Add(tempValues.ElementAt((int)Math.Round((double)windowSize / 2)));
            }
            newValues = newValues.Distinct().ToList();
            newValues = newValues.OrderBy(x => x.timestamp).ToList();
            return newValues;

        }

        public static List<GSRDataReading> GSRSTDEVFilter(List<GSRDataReading> data)
        {
            Log.LogMessage("Doing stdev filter on GSR data");

            double avg = data.Average(x => x.resistance);
            double stdev = Math.Sqrt(data.Average(x => Math.Pow((x.resistance) - avg, 2)));

            int stdMult = 3;

            return data.Where(x => x.resistance >= avg - stdev * stdMult && x.resistance <= avg + stdev * stdMult).ToList();
        }

        public static List<GSRDataReading> GSRMoveAvgFilter(List<GSRDataReading> data, int windowSize)
        {
            Log.LogMessage("Doing moving average filter on GSR data");
            List<GSRDataReading> newValues = new List<GSRDataReading>();
            for (int i = 0; i < data.Count - windowSize; i++)
            {
                List<GSRDataReading> tempValues = data.Skip(i).Take(windowSize).OrderBy(x => x.resistance).ToList();
                newValues.Add(new GSRDataReading(false) { resistance = (int)tempValues.Average(x => x.resistance), timestamp = tempValues[0].timestamp });
            }
            //newValues = newValues.Distinct().ToList();
            newValues = newValues.OrderBy(x => x.timestamp).ToList();
            return newValues;
        }

        public void ExportGRF(string inpath = "")
        {
            if (inpath == "") inpath = Directory.GetCurrentDirectory() + DataReading.GetWritePath();
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
                //This order Graph
                return "0x00" + color.B.ToString("X2") + color.G.ToString("X2") + color.R.ToString("X2");
            };

            Func<string, Color, double, double, string> AddShade = (label, color, from, to) =>
            {
                from = from / 1000;
                to = to / 1000;

                ShadeCount++;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"[Shade" + ShadeCount + "]");
                sb.AppendLine(@"LegendText = " + label);
                sb.AppendLine(@"ShadeStyle = 1");
                sb.AppendLine(@"BrushStyle = 0");
                sb.AppendLine(@"Color = " + c2s(color));
                sb.AppendLine(@"FuncNo = 1");
                sb.AppendLine(@"sMin = " + from.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine(@"sMax = " + to.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine(@"sMin2 = " + from.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine(@"sMax2 = " + to.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine(@"MarkBorder = 0");
                sb.AppendLine();

                return sb.ToString();
            };

            Func<string, Color, List<double>, List<double>, string> AddPointSeries = (label, color, xs, ys) =>
            {
                xs = xs.Select(curX => curX / 1000).ToList();
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
                    sb.Append(xs[pointId].ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + ys[pointId].ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
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

            List<string> pointSeries = new List<string>();
            List<string> shades = new List<string>();
            double lastTime = 0;
            for (int eventId = 0; eventId < events.Length; eventId++)
            {
                string[] evnt = events[eventId].Split('#');

                if (evnt[1].Contains("Bogus"))
                {
                    continue;
                }
                shades.Add(AddShade(evnt[1], e2c(evnt[1]), lastTime, int.Parse(evnt[0])));
                lastTime = int.Parse(evnt[0]);

                pointSeries.Add(AddPointSeries("Splitter", Color.Black, new List<double>() { int.Parse(events[eventId].Split('#')[0]), int.Parse(events[eventId].Split('#')[0]) },
                    new List<double>() { 0, 10000 }));
            }
            List<Tuple<double, string>> timestamps = new List<Tuple<double, string>>();
            List<Tuple<double, string>> timestampsProspects = new List<Tuple<double, string>>();
            //actual data
            #region GSR
            gsrData = gsrData.OrderBy(n => n.timestamp).ToList();
            // gsrData.ForEach(gsr => { x.Add(gsr.timestamp - gsrData[0].timestamp); y.Add(gsr.resistance)});
            List<Tuple<long, double>> datGSR = new List<Tuple<long, double>>();
            foreach (GSRDataReading g in gsrData)
            {
                datGSR.Add(new Tuple<long, double>(g.timestamp, g.resistance));
            }

            var gsrResult = GetInterrestingTimeStamp(datGSR, 40, MinMaxDifference, int.Parse(events[2].Split('#')[0]), true, 4000);
            timestamps.AddRange(gsrResult.Item1.Select(x => Tuple.Create(x, "GSR")));
            timestampsProspects.AddRange(gsrResult.Item2.Select(x => Tuple.Create(x, "GSR")));
            //gsrData.ForEach(signal => { x.Add(signal.timestamp - hrData[0].timestamp); y.Add(signal.resistance); });
            //gsrData.ForEach(signal => { x.Add(signal.timestamp - hrData[0].timestamp); y.Add(signal.resistance); });

            #endregion
            #region HR
            List<Tuple<long, double>> datHR = new List<Tuple<long, double>>();
            foreach (HRDataReading h in hrData)
            {
                datHR.Add(new Tuple<long, double>(h.timestamp, (double)h.IBI));
            }

            var hrResult = GetInterrestingTimeStamp(datGSR, 90, Variance, int.Parse(events[2].Split('#')[0]), true, 4000);
            timestamps.AddRange(hrResult.Item1.Select(x => Tuple.Create(x, "HR")));
            timestampsProspects.AddRange(hrResult.Item2.Select(x => Tuple.Create(x, "HR")));

            #endregion
            #region EEG
            List<Tuple<long, double>> datEEG = new List<Tuple<long, double>>();


            #endregion
            #region Kinect
            List<Tuple<long, double>> datKinect = new List<Tuple<long, double>>();
            foreach (FaceDataReading f in faceData)
            {
                datKinect.Add(Tuple.Create(f.timestamp, (double)(f.data[Microsoft.Kinect.Face.FaceShapeAnimations.RighteyebrowLowerer]
                    + f.data[Microsoft.Kinect.Face.FaceShapeAnimations.LefteyebrowLowerer]) / 2));
            }
            var kinectResult = GetInterrestingTimeStamp(datKinect, 15, Variance, int.Parse(events[2].Split('#')[0]), true, 500);
            timestamps.AddRange(kinectResult.Item1.Select(x => Tuple.Create(x, "FACE1")));
            timestampsProspects.AddRange(kinectResult.Item2.Select(x => Tuple.Create(x, "FACE1")));

            List<Tuple<long, double>> datKinect2 = new List<Tuple<long, double>>();
            foreach (FaceDataReading f in faceData)
            {
                datKinect2.Add(Tuple.Create(f.timestamp, (double)(f.data[Microsoft.Kinect.Face.FaceShapeAnimations.LowerlipDepressorRight]
                    + f.data[Microsoft.Kinect.Face.FaceShapeAnimations.LowerlipDepressorLeft]) / 2));
            }
            var kinectResult2 = GetInterrestingTimeStamp(datKinect2, 15, Variance, int.Parse(events[2].Split('#')[0]), true, 500);
            timestamps.AddRange(kinectResult2.Item1.Select(x => Tuple.Create(x, "FACE2")));
            timestampsProspects.AddRange(kinectResult2.Item2.Select(x => Tuple.Create(x, "FACE2")));
            #endregion

            #region Create interest Graph
            List<double> xValues = new List<double>();
            List<double> yValues = new List<double>();
            double outlierWeight = 15;
            double suspectedWeight = 5;
            double windowSize = 1000;
            int tsCount = timestamps.Count;
            int tspCount = timestampsProspects.Count;
            timestamps = timestamps.OrderBy(x => x).ToList();
            timestampsProspects = timestampsProspects.OrderBy(x => x).ToList();
            int lastIndex = 0;
            int prospectsLastIndex = 0;
            for (double i = 0; i < int.Parse(events.Last().Split('#')[0]) - windowSize; i += 10)
            {
                //Find start index
                lastIndex = tsCount - timestamps.Skip(lastIndex).SkipWhile(x => i > x.Item1).Count();
                prospectsLastIndex = tspCount - timestampsProspects.Skip(prospectsLastIndex).SkipWhile(x => i > x.Item1).Count();

                List<string> inThisWindow = new List<string>();

                xValues.Add(i);
                double value = 0;
                var temp = timestamps.Skip(lastIndex).ToList();
                int counter = 0;
                while (true)
                {
                    if (temp.Count > counter && temp[counter].Item1 - i < windowSize)
                    {
                        if (!inThisWindow.Contains(temp[counter].Item2))
                        {
                            inThisWindow.Add(temp[counter].Item2);
                            value += outlierWeight;
                        }

                    }
                    else
                    {
                        break;
                    }

                    counter++;
                }
                var tempPros = timestampsProspects.Skip(prospectsLastIndex).ToList();
                int prosCounter = 0;
                while (true)
                {
                    if (tempPros.Count > prosCounter && tempPros[prosCounter].Item1 - i < windowSize)
                    {
                        if (!inThisWindow.Contains(tempPros[prosCounter].Item2))
                        {
                            inThisWindow.Add(tempPros[prosCounter].Item2);
                            value += suspectedWeight;
                        }

                    }
                    else
                    {
                        break;
                    }

                    prosCounter++;
                }
                yValues.Add(value);
            }

            #endregion

            pointSeries.Add(AddPointSeries("InterrestingPoints", Color.Magenta, xValues, yValues));

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
        private Color e2c(string evnt)
        {
            if (evnt.Contains("TaskWizard - BtnCompleteClicked")) return Color.Green;
            if (evnt.Contains("TaskWizard - BtnIncompleteClicked")) return Color.Red;
            if (evnt.Contains("SendDraft error shown")) return Color.Purple;
            if (evnt.Contains("CreateDraft, language changed to: US")) return Color.Purple;
            if (evnt.Contains("AddAttachmentButtonClick: 1")) return Color.Turquoise;
            if (evnt.Contains("AddAttachmentButtonClick: 2")) return Color.Teal;
            if (evnt.Contains("AddAttachmentButtonClick: 3")) return Color.Blue;
            if (evnt.Contains("AddAttachment complete")) return Color.BlueViolet;
            if (evnt.Contains("Add Contact Button click: 1")) return Color.PaleVioletRed;
            if (evnt.Contains("Add Contact Button click: 2")) return Color.MediumVioletRed;
            if (evnt.Contains("Add Contact Button click: 3")) return Color.IndianRed;
            if (evnt.Contains("AddContact complete")) return Color.HotPink;
            if (evnt.Contains("RemoveContact clicked")) return Color.GreenYellow;
            return Color.DarkMagenta;
        }

        public double Variance(List<double> data)
        {
            double avg = data.Average();
            return Math.Sqrt(data.Average(x => Math.Pow(x - avg, 2)));
        }

        public double MinMaxDifference(List<double> data)
        {
            return Math.Abs(data.Max() - data.Min());
        }


        private Tuple<List<double>, List<double>> GetInterrestingTimeStamp(List<Tuple<long, double>> dataList, int windowSize, Func<List<double>, double> Calculator, int calibrationTime, bool withRestPeriod, int offset = 0)
        {
            int restTimer = (withRestPeriod) ? 180000 : 0;
            dataList = dataList.OrderBy(x => x.Item1).ToList();
            long startTime = dataList.First().Item1;
            List<Tuple<double, double>> data = new List<Tuple<double, double>>();
            for (int i = 0; i < dataList.Count - windowSize; i++)
            {
                List<Tuple<long, double>> datReadings = dataList.Skip(i).Take(windowSize).ToList();
                double result = Calculator(datReadings.Select(x => x.Item2).ToList());
                data.Add(new Tuple<double, double>(datReadings.Select(x => Convert.ToDouble(x.Item1)).Average() - startTime, result));
            }

            List<double> timestamps = new List<double>();
            List<double> timestampsProspects = new List<double>();

            BoxPlot bp = SAnalysis.BoxPlot(data.Where(o => restTimer <= o.Item1 && o.Item1 <= calibrationTime).ToList());
            List<Tuple<double, double>> dat = data.Where(o => bp.upperOuterFence < o.Item2 || bp.lowerOuterFence > o.Item2).ToList();
            timestamps.AddRange(dat.Select(o => o.Item1).ToList());
            List<Tuple<double, double>> datPros = data.Where((o => (bp.upperInnerFence < o.Item2 && o.Item2 < bp.upperOuterFence) || (bp.lowerOuterFence < o.Item2 && o.Item2 < bp.lowerInnerFence))).Where(o => o.Item1 > calibrationTime).ToList();
            timestampsProspects.AddRange(datPros.Select(o => o.Item1).ToList());
            timestamps.ForEach(x => x -= offset);
            timestampsProspects.ForEach(x => x -= offset);
            return new Tuple<List<double>, List<double>>(timestamps, timestampsProspects);
        }
    }

}
