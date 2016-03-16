using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using BesterUI.Helpers;

namespace BesterUI.Data
{
    public abstract class DataReading
    {
        public static DateTime? startTime = null;
        public const string dateFormat = "yyyy-MM-dd HH_mm_ss_fff";
        public const string oldDateFormat = "yyyy-MM-dd HH_mm_ss";
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
                dir += @"\" + startTime.Value.ToString(dateFormat);
                isFirst = true;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string fileName = dir + "/" + deviceName + ".dat";

                writers.Add(deviceName, new StreamWriter(fileName));

                dat += deviceName + "|" + startTime.Value.ToString(dateFormat) + "\n";
            }

            dat += obj.timestamp + "#" + obj.Serialize();
            writers[deviceName].Write(((isFirst) ? "" : "\n") + dat);
            writers[deviceName].Flush();
        }

        public abstract string Serialize();

        static volatile bool doneReading = true;
        public static volatile bool kill = false;
        static volatile string fPath;
        static volatile System.Collections.Queue q;
        public static List<T> LoadFromFile<T>(string path, DateTime dT) where T : DataReading, new()
        {
            List<T> retVal = new List<T>();

            fPath = path;
            doneReading = false;
            q = System.Collections.Queue.Synchronized(new System.Collections.Queue());
            Thread readThread = new Thread(new ThreadStart(ReadToQueue));
            readThread.Start();
            bool first = true;

            long size = new FileInfo(path).Length / 1024;
            long progress = 0;
            long next = size / 100;

            TimeSpan offset = new TimeSpan();

            Log.LogMessage("Loading " + typeof(T).Name + ": 0/" + size + " bytes.");
                
            while ((!doneReading || q.Count > 0) && !kill)
            {
                if (q.Count > 0)
                {
                    string curLine = (string)q.Dequeue();

                    if (first)
                    {
                        progress += curLine.Length;
                        var bits = curLine.Split('|');
                        try
                        {
                            startTime = DateTime.ParseExact(bits[1], dateFormat, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch
                        {

                            startTime = DateTime.ParseExact(bits[1], oldDateFormat, System.Globalization.CultureInfo.InvariantCulture);
                            startTime.Value.AddMilliseconds(500);
                        }
                        TimeSpan DifferenceOffset = new TimeSpan(1, 0, 180);
                        offset = startTime.Value.Subtract(dT);
                        offset = offset.Add(DifferenceOffset);
                        first = false;
                    }
                    else
                    {
                        progress += curLine.Length;
                        var datBits = curLine.Split('#');
                        DataReading t = new T();
                        t.timestamp = long.Parse(datBits[0]) - (long)offset.TotalMilliseconds;
                        retVal.Add((T)t.Deserialize(datBits[1]));

                        if ((progress / 1024) > next)
                        {
                            next += size / 100;
                            Log.LogMessageSameLine("Loading " + typeof(T).Name + ": " + (progress / 1024) + "/" + size + " kbytes.");
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }

            return retVal;
        }

        public static void ReadToQueue()
        {
            using (var dat = File.OpenText(fPath))
            {
                string curLine = dat.ReadLine();
                while (!string.IsNullOrEmpty(curLine))
                {
                    if (Log.LogBox != null)
                    {
                        q.Enqueue(curLine);
                        curLine = dat.ReadLine();
                    }
                    else
                    {
                        kill = true;
                        return;
                    }
                }
            }

            doneReading = true;
        }

        protected abstract DataReading Deserialize(string line);

        public abstract void Write();
    }

}
