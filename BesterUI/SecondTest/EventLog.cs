using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SecondTest
{
    class EventLog
    {
        public static Stopwatch Timer;
        public static DateTime? StartTime;
        public static string DateTimeFormat;

        static string physDataPath = "PhysData";
        static StreamWriter writer;

        public static void LogEvent(string eventName)
        {
            string data = Timer.ElapsedMilliseconds + "#" + eventName + "\n";
            writer.Write(data);
            writer.Flush();
        }

        private static void OpenWriter()
        {
            CloseWriter();

            string dir = physDataPath;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            dir += @"\" + StartTime.Value.ToString(DateTimeFormat);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            dir += @"\SecondTest.dat";

            writer = new StreamWriter(dir);
        }

        public static void CloseWriter()
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }
    }
}
