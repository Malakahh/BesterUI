using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

namespace BesterUI.Data
{
    public class EEGDataReading : DataReading
    {
        public Dictionary<string, double> data = new Dictionary<string, double>();

        public enum ELECTRODE
        {
            A1,
            F9, FT9, T9, TP9, P9,
            AF7, F7, FT7, T7, TP7, P7, PO7,
            F5, FC5, C5, CP5, P5,
            AF3, F3, FC3, C3, CP3, P3, PO3,
            FP1, F1, FC1, C1, CP1, P1, O1,
            NZ, FPZ, AFZ, FZ, FCZ, CZ, CPZ, PZ, POZ, OZ, IZ,
            FP2, F2, FC2, C2, CP2, P2, O2,
            AF4, F4, FC4, C4, CP4, P4, PO4,
            AF6, FC6, C6, CP6, P6,
            AF8, F8, FT8, T8, TP8, P8,
            F10, FT10, T10, TP10, P10,
            A2
        }

        /*
        public EEGDataReading(string json) : base(json)
        {
            string[] data = json.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            string[] readings = data[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string r in readings)
            {
                string[] stats = r.Split(new string[] { ",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stat in stats)
                {
                    string s = stat.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    


                }
            }
        }
        */

        public EEGDataReading(bool startReading) : base(startReading)
        {

        }

        public EEGDataReading() : this(true)
        { }

        public override void Write()
        {
            DataReading.StaticWrite("EEG", this);
        }

        public override void EndWrite()
        {
            DataReading.StaticEndWrite("EEG");
        }

        public static List<EEGDataReading> LoadFromFile(string json)
        {
            //Timestamp
            string[] commaSeparated = json.Split(new string[] { ",", "{" }, StringSplitOptions.RemoveEmptyEntries);
            string startTimeString = commaSeparated.First(s => s.Contains("startTime"));
            startTimeString = startTimeString.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[3];
            DateTime loadedStartTime;
            DateTime.TryParse(startTimeString, out loadedStartTime);

            List<EEGDataReading> list = new List<EEGDataReading>();
            string[] data = json.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            string[] readings = data[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string r in readings)
            {
                EEGDataReading eeg = new EEGDataReading(false);
                eeg.loadedStartTime = loadedStartTime;

                string[] stats = r.Split(new string[] { ",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < stats.Length - 1; i++)
                {
                    string[] si = stats[i].Split(new string[] { ":", "\"" }, StringSplitOptions.RemoveEmptyEntries);
                    eeg.data.Add(si[0], double.Parse(si[1]));
                }
                
                string s = stats[stats.Length - 1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                eeg.timestamp = long.Parse(s);

                list.Add(eeg);
            }

            return list;
        }
    }
}
