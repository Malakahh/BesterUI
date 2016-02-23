using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;

namespace BesterUI.Data
{
    public abstract class DataReading
    {
        public static DateTime? startTime = null;
        public const string dateFormat = "yyyy-MM-dd hh:mm:ss";
        static Stopwatch stopWatch;
        static Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();

        public long timestamp;
        [ScriptIgnore]
        public DateTime loadedStartTime;

        public DataReading(bool startReadings)
        {
            if (startReadings)
            {
                if (startTime == null || stopWatch == null)
                {
                    startTime = DateTime.UtcNow;
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }

                timestamp = stopWatch.ElapsedMilliseconds;
            }
        }

        public static void ResetTimers()
        {
            startTime = null;
            stopWatch = null;
        }


        public static void StaticWrite(string deviceName, DataReading obj, string dir = "PhysData")
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string dat = "";

            bool isFirst = false;
            if (!writers.ContainsKey(deviceName))
            {
                isFirst = true;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string fileName = dir + "/" + deviceName + ".dat";

                writers.Add(deviceName, new StreamWriter(fileName));

                dat += deviceName + "|" + startTime.Value.ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture) + "\n";
            }

            dat += obj.timestamp + "#" + obj.Serialize();
            writers[deviceName].Write(((isFirst) ? "" : "\n") + dat);
            writers[deviceName].Flush();
        }

        public abstract string Serialize();

        public static List<T> LoadFromFile<T>(string path) where T : DataReading, new()
        {
            List<T> retVal = new List<T>();
            using (var dat = File.OpenText(path))
            {
                string curLine = dat.ReadLine();
                var bits = curLine.Split('|');
                startTime = DateTime.ParseExact(bits[1], dateFormat, System.Globalization.CultureInfo.InvariantCulture);

                curLine = dat.ReadLine();
                while (!string.IsNullOrEmpty(curLine))
                {
                    var datBits = curLine.Split('#');
                    DataReading t = new T();
                    t.timestamp = long.Parse(datBits[0]);
                    retVal.Add((T)t.Deserialize(datBits[1]));
                    curLine = dat.ReadLine();
                }
            }

            return retVal;
        }

        protected abstract DataReading Deserialize(string line);

        public abstract void Write();
    }

}
