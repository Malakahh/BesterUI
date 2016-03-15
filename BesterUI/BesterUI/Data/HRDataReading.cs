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
        public int? IBI;
        public int BPM;
        public bool isBeat;

        public HRDataReading(bool startReading) : base(startReading)
        {

        }

        public HRDataReading() : this(false)
        { }

        public override void Write()
        {
            DataReading.StaticWrite("HR", this);
        }

        public override string Serialize()
        {
            return signal + "|" + IBI + "|" + BPM + "|" + isBeat;
        }

        protected override DataReading Deserialize(string line)
        {
            var bits = line.Split('|');

            signal = int.Parse(bits[0]);
            IBI = int.Parse(bits[1]);
            BPM = int.Parse(bits[2]);
            isBeat = bool.Parse(bits[3]);

            return this;
        }
    }
}
