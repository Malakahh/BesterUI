using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class Events
    {

        public int timestamp;
        public string eventName;

        public Events(int ts, string en)
        {
            this.timestamp = ts;
            this.eventName = en;
        }

    }
}
