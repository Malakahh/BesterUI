using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

namespace BesterUI.Data
{
    public class GSRDataReading : DataReading
    {
        public int resistance;

        public GSRDataReading(bool startReadings) : base(startReadings)
        {

        }

        public GSRDataReading() : this(false)
        { }

        public override void Write()
        {
            DataReading.StaticWrite("GSR", this);
        }

        public static List<GSRDataReading> LoadFromFile(string json)
        {
            //Timestamp
            string[] commaSeparated = json.Split(new string[] { ",", "{" }, StringSplitOptions.RemoveEmptyEntries);
            string startTimeString = commaSeparated.First(s => s.Contains("startTime"));
            startTimeString = startTimeString.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[3];
            DateTime loadedStartTime;
            DateTime.TryParse(startTimeString, out loadedStartTime);

            List<GSRDataReading> list = new List<GSRDataReading>();
            string[] data = json.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            string[] readings = data[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string r in readings)
            {
                GSRDataReading gsr = new GSRDataReading(false);
                gsr.loadedStartTime = loadedStartTime;

                string[] stats = r.Split(new string[] { ",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

                string s0 = stats[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                gsr.resistance = int.Parse(s0);

                string s1 = stats[1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                gsr.timestamp = long.Parse(s1);

                list.Add(gsr);
            }

            return list;
        }

        public override string Serialize()
        {
            return resistance.ToString();
        }

        protected override DataReading Deserialize(string line)
        {
            resistance = int.Parse(line);
            return this;
        }
    }
}
