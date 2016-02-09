using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace BesterUI.Data
{
    abstract class DataReading
    {
        public static DateTime? startTime = null;
        static Stopwatch stopWatch;

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

        public abstract void Write();
        public abstract void EndWrite();
    }
}
