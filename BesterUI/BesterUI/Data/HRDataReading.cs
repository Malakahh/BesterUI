using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BesterUI.Data
{
    public class HRDataReading : DataReading
    {
        public int signal;
        public bool isBeat;

        public HRDataReading(bool startReading) : base(startReading)
        {

        }

        public HRDataReading() : this(true)
        { }

        public override void Write()
        {
            DataReading.StaticWrite("HR", this);
        }

        public override void EndWrite()
        {
            DataReading.StaticEndWrite("HR");
        }

        public static List<HRDataReading> LoadFromFile(string json)
        {
            //Timestamp
            string[] commaSeparated = json.Split(new string[] { ",", "{" }, StringSplitOptions.RemoveEmptyEntries);
            string startTimeString = commaSeparated.First(s => s.Contains("startTime"));
            startTimeString = startTimeString.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[3];
            DateTime loadedStartTime;
            DateTime.TryParse(startTimeString, out loadedStartTime);

            List<HRDataReading> list = new List<HRDataReading>();
            string[] data = json.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            string[] readings = data[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string r in readings)
            {
                HRDataReading band = new HRDataReading(false);
                band.loadedStartTime = loadedStartTime;

                string[] stats = r.Split(new string[] { ",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

                band.isBeat = stats[0].Split(new string[] { ":", "\"" }, StringSplitOptions.RemoveEmptyEntries)[1] == "true";

                string s1 = stats[1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                band.signal = int.Parse(s1);

                string s2 = stats[2].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                band.timestamp = long.Parse(s2);

                list.Add(band);
            }

            return list;
        }
    }
}
