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

        public static void StaticWrite(string deviceName, object obj)
        {
            string dir = "PhysData";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string readingDir = dir + "/" + ((DateTime)startTime).ToString("yyyy-MM-dd_hh.mm.ss");
            StaticWrite(deviceName, obj, readingDir);
        }

        public static void StaticWrite(string deviceName, object obj, string path)
        {
            string json = "";
            bool isFirst = false;
            if (!writers.ContainsKey(deviceName))
            {
                isFirst = true;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = path + "/" + deviceName + ".json";
                writers.Add(deviceName, new StreamWriter(fileName));

                json += "{\n\"" + deviceName + "\": {\n" +
                    "\"startTime\":\"" + startTime + "\",\n" +
                    "\"Data\":[\n";
            }

            json += new JavaScriptSerializer().Serialize(obj);
            writers[deviceName].Write(((isFirst) ? "" : ",\n") + json);
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
