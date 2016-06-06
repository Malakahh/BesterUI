using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App.Evnt
{
    class Events
    {
        protected PointsOfInterest POI;
        public double percentageToHit = 1;
        private bool _POIchanged = true;
        private int startTimestamp;
        private int endTimestamp = 0;
        public string eventName;
        private bool _isHit;
        public bool isHit
        {
            get
            {
                if (_POIchanged)
                {
                    _isHit = CalculateHit();
                    _POIchanged = false;
                    return _isHit;
                }
                else
                {
                    return _isHit;
                }
            }
        }

        public Events(int StartTimeStamp, int EndTimestamp, string EventName)
        {
            this.startTimestamp = StartTimeStamp;
            this.endTimestamp = EndTimestamp;
            this.eventName = EventName;
        }
        public Events(int StartTimeStamp, int EndTimestamp, string EventName, double PercentageHit)
        {
            this.startTimestamp = StartTimeStamp;
            this.endTimestamp = EndTimestamp;
            this.eventName = EventName;
            this.percentageToHit = PercentageHit;
        }

        public void SetPointOfInterest(PointsOfInterest poi)
        {
            POI = poi;
            _POIchanged = true;
        }

        public int GetTimestampStart()
        {
            return startTimestamp - 500;
        }

        public int GetTimestampEnd()
        {
            return endTimestamp + 500;
        }


        protected virtual bool CalculateHit()
        {
            return POI.PercentageAreaHit(GetTimestampStart(), GetTimestampEnd()) > 0;
        }

        public Events Copy()
        {      
            Events e = new Events(startTimestamp, endTimestamp, eventName, percentageToHit);
            e.SetPointOfInterest(POI);
            return e;
        }
    }
}
