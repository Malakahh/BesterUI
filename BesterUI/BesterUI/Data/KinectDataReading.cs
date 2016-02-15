using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Face;

namespace BesterUI.Data
{
    public class KinectDataReading : DataReading
    {
        public Dictionary<string, double> data = new Dictionary<string, double>();

        public KinectDataReading(bool startReading) : base(startReading)
        {

        }

        public KinectDataReading() : this(true)
        { }

        public override void Write()
        {
            DataReading.StaticWrite("Kinect", this);
        }

        public override void EndWrite()
        {
            DataReading.StaticEndWrite("Kinect");
        }

        public static List<KinectDataReading> LoadFromFile(string json)
        {
            //Timestamp
            string[] commaSeparated = json.Split(new string[] { ",", "{" }, StringSplitOptions.RemoveEmptyEntries);
            string startTimeString = commaSeparated.First(s => s.Contains("startTime"));
            startTimeString = startTimeString.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[3];
            DateTime loadedStartTime;
            DateTime.TryParse(startTimeString, out loadedStartTime);

            List<KinectDataReading> list = new List<KinectDataReading>();
            string[] data = json.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            string[] readings = data[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string r in readings)
            {
                KinectDataReading kinect = new KinectDataReading(false);
                kinect.loadedStartTime = loadedStartTime;

                string[] stats = r.Split(new string[] { ",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < stats.Length - 1; i++)
                {
                    string[] si = stats[i].Split(new string[] { ":", "\"" }, StringSplitOptions.RemoveEmptyEntries);
                    kinect.data.Add(si[0], double.Parse(si[1]));
                }

                string s = stats[stats.Length - 1].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
                kinect.timestamp = long.Parse(s);

                list.Add(kinect);
            }

            return list;
        }
    }
}
