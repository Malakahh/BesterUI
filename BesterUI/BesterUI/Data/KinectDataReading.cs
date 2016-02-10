using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BesterUI.Data
{
    class KinectDataReading : DataReading
    {

        public override void Write()
        {
            DataReading.StaticWrite("Kinect", this);
        }

        public override void EndWrite()
        {
            DataReading.StaticEndWrite("Kinect");
        }
    }
}
