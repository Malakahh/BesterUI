using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App.Evnt
{
    class InstantEvents : Events
    {
        public InstantEvents(int timestamp, int endTime, string eventName): base(timestamp, endTime, eventName)
        {
        }

        protected override bool CalculateHit()
        {
            return POI.IsPointFlagged(GetTimestampStart());
        }
    }
}
