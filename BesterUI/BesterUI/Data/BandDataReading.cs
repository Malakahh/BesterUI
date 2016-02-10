using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BesterUI.Data
{
    public class BandDataReading : DataReading
    {
        public enum QUALITY { LOCKED, ACQUIRING }

        public string quality;
        public int heartRate;

        public BandDataReading(bool beginTimer) : base(beginTimer)
        {

        }

        public BandDataReading() : this(true)
        {

        }

        public override void Write()
        {
            DataReading.StaticWrite("Band", this);
        }

        public override void EndWrite()
        {
            DataReading.StaticEndWrite("Band");
        }
    }
}
