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

        string[] fileNames = new string[3] { "GSR.dat", "EEG.dat", "HR.dat", }; //If you change this, remember to change LoadFromFile as well


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
                this.LoadFromFile(fb.FileNames);
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

        public void LoadFromFile(string[] filesToLoad)
        {
            DialogResult res = DialogResult.OK;

            if (hrData.Count != 0 || eegData.Count != 0 || gsrData.Count != 0)
            {
                res = MessageBox.Show("You are about to overwrite unsaved data. Continue?", "WARNING", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }

            if (res == DialogResult.OK)
            {
                string[] correctNames = new string[3] { "GSR.dat", "EEG.dat", "HR.dat", };


                foreach (string file in filesToLoad)
                {
                    string s = file.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (!fileNames.Contains(s))
                    {
                        continue;
                    }

                    switch (s)
                    {
                        case "GSR.dat":
                            Log.LogMessage("Loading GSR data");
                            gsrData = DataReading.LoadFromFile<GSRDataReading>(file);
                            break;
                        case "EEG.dat":
                            Log.LogMessage("Loading EEG data");
                            eegData = DataReading.LoadFromFile<EEGDataReading>(file);
                            break;
                        case "HR.dat":
                            Log.LogMessage("Loading HR data");
                            hrData = DataReading.LoadFromFile<HRDataReading>(file);
                            break;
                    }
                }
            }
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
