using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classification_App.Evnt;

namespace Classification_App
{
    class NoveltyResult
    {
        public PointsOfInterest poi;
        public List<Events> events;
        public List<OneClassFV> anomalis;
        public int start;
        public int end;
        public LibSVMsharp.SVMParameter parameter;
        
        private const double HIT_WEIGHT = 1;
        private const double TIME_WEIGHT = 1;
        private bool _scoreIsCalculated = false;
        private double score;
        private bool _flaggedAreaIsCalculated = false;
        private double _area;

        public NoveltyResult(PointsOfInterest poi, List<Events> events, int start, int end, LibSVMsharp.SVMParameter parameter, List<OneClassFV> anomalis)
        {
            this.poi = poi;
            this.events = events.Select(x => x.Copy()).ToList();
            this.start = start;
            this.end = end;
            this.parameter = parameter;
            this.anomalis = anomalis.ToList();
        }

        public double FlaggedAreaSize()
        {
            if (!_flaggedAreaIsCalculated)
            {
                _area = (double)poi.GetFlaggedAreas().Where(x => x.Item2 > start).Sum(x => (x.Item1 < start) ? x.Item2 - start : (x.Item2 - x.Item1));
                _flaggedAreaIsCalculated = true;
                return _area;
            }
            else
            {
                return _area;
            }
        }

        public double CalculateScore()
        {
            if (!_scoreIsCalculated)
            {
                double timeReduction = 1 -  (FlaggedAreaSize()/ (end - start));
                double eventsHit = (double)events.Where(x => x.isHit).Count() / events.Count;
                score = ((TIME_WEIGHT * timeReduction) * (HIT_WEIGHT * eventsHit)) / (HIT_WEIGHT + TIME_WEIGHT);
                _scoreIsCalculated = true;
                return score;
            }
            else
            {
                return score;
            }

        }

        public static double CalculateEarlyScore(PointsOfInterest poiT, List<Events> eventsT, int startT, int endT)
        {
            double timeReduction = 1 - ((double)poiT.GetFlaggedAreas().Where(x=>x.Item2> startT).Sum(x => (x.Item2 - x.Item1)) / (endT - startT));
            double eventsHit = (double)eventsT.Where(x => x.isHit).Count() / eventsT.Count;
            
            return ((TIME_WEIGHT * timeReduction) * (HIT_WEIGHT * eventsHit)) / (HIT_WEIGHT + TIME_WEIGHT);

        }
    }
}
