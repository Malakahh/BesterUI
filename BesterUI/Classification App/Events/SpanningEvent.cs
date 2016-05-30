using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App.Evnt
{
    class SpanningEvent : Events
    {
        private double percentageToHit;

        public SpanningEvent(int startTimeStamp, int endTimestamp, string eventName, double percentageToHit): base(startTimeStamp, endTimestamp, eventName)
        {
            this.percentageToHit = percentageToHit;
        }

        protected override bool CalculateHit()
        {
            return POI.PercentageAreaHit(endTimestamp, endTimestamp) >= percentageToHit;
        }
    }
}
