using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using System.Windows.Forms;
using System.IO;

namespace BesterUI
{
    public class FusionData
    {
        private List<HRDataReading> hrData = new List<HRDataReading>();
        private List<EEGDataReading> eegData = new List<EEGDataReading>();
        private List<GSRDataReading> gsrData = new List<GSRDataReading>();

        public FusionData()
        {

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

        public void LoadFromFile(string[] fileNames)
        {
            DialogResult res = DialogResult.OK;

            if (hrData.Count != 0 || eegData.Count != 0 || gsrData.Count != 0)
            {
                res = MessageBox.Show("You are about to overwrite unsaved data. Continue?", "WARNING", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }

            if (res == DialogResult.OK)
            {
                string[] correctNames = new string[3] { "GSR.json", "EEG.json", "Band.json", };

                foreach (string file in fileNames)
                {
                    string s = file.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (!correctNames.Contains(s))
                    {
                        continue;
                    }

                    string json = File.ReadAllText(file);

                    switch (s)
                    {
                        case "GSR.json":
                            gsrData = GSRDataReading.LoadFromFile(json);
                            break;
                        case "EEG.json":
                            eegData = EEGDataReading.LoadFromFile(json);
                            break;
                        case "Band.json":
                            hrData = HRDataReading.LoadFromFile(json);
                            break;
                    }
                }
            }
        }

        public void CreateDummyData()
        {
            //EEG
            EEGDataReading test = new EEGDataReading();
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
            test3.EndWrite();

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
            gsr6.EndWrite();


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
            band1.EndWrite();

        }
    }
}
