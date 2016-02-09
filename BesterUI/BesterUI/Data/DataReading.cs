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
    abstract class DataReading
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
            string fileName = ((DateTime)startTime).ToString("yyyy-MM-dd_hh-mm-ss") + "_" + deviceName + ".json";
            string json = "";
            if (!writers.ContainsKey(deviceName))
            {
                writers.Add(deviceName, new StreamWriter(fileName));

                json += "{\"" + deviceName + "\": {" +
                    "\"startTime\":\"" + startTime + "\"," +
                    "\"Data\":[";
            }

            json += new JavaScriptSerializer().Serialize(obj);
            writers[deviceName].Write(json + ",");
            writers[deviceName].Flush();
        }

        public static void StaticEndWrite(string deviceName)
        {
            writers[deviceName].Write("]}}");
            writers[deviceName].Flush();
        }

        public abstract void Write();
        public abstract void EndWrite();
    }
}
