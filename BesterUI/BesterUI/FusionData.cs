using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using System.Windows.Forms;
using System.IO;
using BesterUI.Helpers;

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

        public Dictionary<string, bool> LoadFromFile(string[] filesToLoad, DateTime dT)
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
                        if (size > MINIMUM_GSR_FILE_SIZE && File.Exists(file))
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
                        if (size > MINIMUM_EEG_FILE_SIZE && File.Exists(file))
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
                        if (size > MINIMUM_HR_FILE_SIZE && File.Exists(file))
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
                        if (size > MINIMUM_FACE_FILE_SIZE && File.Exists(file))
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
                if (LastValue != null|| tempValues.ElementAt((int)Math.Round((double)windowSize / 2)) != LastValue)
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
    }
}
