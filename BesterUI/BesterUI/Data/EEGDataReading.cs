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
