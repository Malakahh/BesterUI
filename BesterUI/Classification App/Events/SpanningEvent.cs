﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App.Evnt
{
    class SpanningEvent : Events
    {

        public SpanningEvent(int startTimeStamp, int endTimestamp, string eventName, double PercentageToHit): base(startTimeStamp, endTimestamp, eventName)
        {
            this.percentageToHit = PercentageToHit;
        }

        protected override bool CalculateHit()
        {
            return POI.PercentageAreaHit(GetTimestampStart(), GetTimestampEnd()) >= 0;
        }
    }
}
