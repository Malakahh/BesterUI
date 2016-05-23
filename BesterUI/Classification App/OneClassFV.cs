using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class OneClassFV
    {
        public int TimeStamp
        {
            get;
            set;
        }
        public List<double> Features
        {
            get;
            set;
        }

        public OneClassFV(List<double> features, int timeStamp)
        {
            Features = features;
            TimeStamp = timeStamp;
        }
    }
}
