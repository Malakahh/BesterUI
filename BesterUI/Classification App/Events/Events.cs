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
        private bool _POIchanged = true;
        public int startTimestamp;
        public int endTimestamp = 0;
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

        public Events(int startTimeStamp, int endTimestamp, string eventName)
        {
            this.startTimestamp = startTimeStamp;
            this.endTimestamp = endTimestamp;
            this.eventName = eventName;
        }
        public void SetPointOfInterest(PointsOfInterest poi)
        {
            POI = poi;
            _POIchanged = true;
        }

        protected virtual bool CalculateHit()
        {
            return POI.IsPointFlagged(endTimestamp);
        }

        public Events Copy()
        {      
            Events e = new Events(startTimestamp, endTimestamp, eventName);
            e.SetPointOfInterest(POI);
            return e;
        }
    }
}
