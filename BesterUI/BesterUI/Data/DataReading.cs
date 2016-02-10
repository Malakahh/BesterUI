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
        static Stopwatch stopWatch;
        static Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();

        public long timestamp;

        public DataReading()
        {
            if (startTime == null)
            {
                startTime = DateTime.Now;
            }

            if (stopWatch == null)
            {
                stopWatch = new Stopwatch();
                stopWatch.Start();
            }

            timestamp = stopWatch.ElapsedMilliseconds;
        }

        public static void StaticWrite(string deviceName, object obj)
        {
            string dir = "PhysData";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = "";
            if (!writers.ContainsKey(deviceName))
            {
                string readingDir = dir + "/" + ((DateTime)startTime).ToString("yyyy-MM-dd_hh-mm-ss");
                if (!Directory.Exists(readingDir))
                {
                    Directory.CreateDirectory(readingDir);
                }

                string fileName = readingDir + "/" + deviceName + ".json";
                writers.Add(deviceName, new StreamWriter(fileName));

                json += "{\n\"" + deviceName + "\": {\n" +
                    "\"startTime\":\"" + startTime + "\",\n" +
                    "\"Data\":[\n";
            }

            json += new JavaScriptSerializer().Serialize(obj);
            writers[deviceName].Write(json + ",\n");
            writers[deviceName].Flush();
        }

        public static void StaticEndWrite(string deviceName)
        {
            writers[deviceName].Write("]}}");
            writers[deviceName].Flush();
            writers[deviceName].Dispose();
            writers.Remove(deviceName);
        }

        public abstract void Write();
        public abstract void EndWrite();
    }
}
