using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BesterUI.Data
{
    class BandDataReading : DataReading
    {
        public enum QUALITY { LOCKED, ACQUIRING }

        public string quality;
        public int heartRate;

        public BandDataReading()
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
