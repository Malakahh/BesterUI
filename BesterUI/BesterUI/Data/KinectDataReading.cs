using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Face;

namespace BesterUI.Data
{
    public class KinectDataReading : DataReading
    {
        public Dictionary<string, double> data = new Dictionary<string, double>();

        public KinectDataReading(bool beginTimer) : base(beginTimer)
        {

        }

        public KinectDataReading() : this(true)
        {

        }

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
