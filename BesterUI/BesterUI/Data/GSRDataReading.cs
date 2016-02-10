using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

namespace BesterUI.Data
{
    public class GSRDataReading : DataReading
    {
        public int resistance;

        public GSRDataReading() : base()
        {

        }

        public override void Write()
        {
            DataReading.StaticWrite("GSR", this);
        }

        public override void EndWrite()
        {
            DataReading.StaticEndWrite("GSR");
        }
    }
}
