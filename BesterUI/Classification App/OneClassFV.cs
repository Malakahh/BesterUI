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
        public LibSVMsharp.SVMNode[] Features
        {
            get;
            set;
        }

        public OneClassFV(LibSVMsharp.SVMNode[] features, int timeStamp)
        {
            Features = features;
            TimeStamp = timeStamp;
        }
    }
}
