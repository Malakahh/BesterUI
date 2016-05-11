using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class samEvents
    {
        public int timestamp;
        public int valence;
        public int arousal;

        public samEvents(int ts, int v, int a)
        {
            this.timestamp = ts;
            this.valence = v;
            this.arousal = a;
        }
    }
}
