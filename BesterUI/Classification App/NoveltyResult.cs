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

        public NoveltyResult(PointsOfInterest poi, List<Events> events, int start, int end, LibSVMsharp.SVMParameter parameter, List<OneClassFV> anomalis)
        {
            this.poi = poi;
            this.events = events.Select(x => x.Copy()).ToList();
            this.start = start;
            this.end = end;
            this.parameter = parameter;
            this.anomalis = anomalis.ToList();
        }

        public double CalculateScore(double hitWeight, double timeReductionWeight)
        {
            double timeReduction = 1 - ((double)poi.GetFlaggedAreas().Where(x => x.Item1 > start).Sum(x => (x.Item2 - x.Item1)) / (end - start));
            double eventsHit = (double)events.Where(x=>x.isHit).Count()/events.Count;

            return ((timeReductionWeight * timeReduction) + (hitWeight * eventsHit)) / (hitWeight + timeReductionWeight);
        }

        public static double CalculateEarlyScore(PointsOfInterest poiT, List<Events> eventsT, int startT, int endT, double hitWeightT, double timeReductionWeightT)
        {
            double timeReduction = 1 - ((double)poiT.GetFlaggedAreas().Where(x=>x.Item1> startT).Sum(x => (x.Item2 - x.Item1)) / (endT - startT));
            double eventsHit = (double)eventsT.Where(x => x.isHit).Count() / eventsT.Count;

            return ((timeReductionWeightT * timeReduction) + (hitWeightT * eventsHit)) / (hitWeightT + timeReductionWeightT);

        }
    }
}
