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
        private bool _calculatedHitResult = false;
        private HitResults _hitResult;

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

        public HitResults CalculateHitResult()
        {
            if (!_calculatedHitResult)
            {
                int Hits = 0;
                int EventHits = 0;
                int EventTotal = events.Count;
                int Misses = 0;
                foreach (Events ev in events)
                {
                    if (ev.isHit)
                    {
                        EventHits++;
                    }
                }
                foreach (var pointOfIn in poi.GetFlaggedAreas())
                {
                    if (pointOfIn.Item1 > start && pointOfIn.Item2 < end)
                    {
                        bool hitted = false;
                        foreach (var ev in events)
                        {
                            if (pointOfIn.Item1 < ev.GetTimestampStart() && ev.GetTimestampEnd() < pointOfIn.Item2)
                            {
                                Hits++;
                                hitted = true;
                                break;
                            }
                            else if (pointOfIn.Item1 >= ev.GetTimestampStart() && pointOfIn.Item2 >= ev.GetTimestampEnd() && pointOfIn.Item1 <= ev.GetTimestampEnd())
                            {
                                Hits++;
                                hitted = true;
                                break;
                            }
                            else if (pointOfIn.Item1 <= ev.GetTimestampStart() && pointOfIn.Item2 <= ev.GetTimestampEnd() && pointOfIn.Item2 >= ev.GetTimestampStart())
                            {
                                Hits++;
                                hitted = true;
                                break;
                            }
                            else if (ev.GetTimestampStart() <= pointOfIn.Item1 && pointOfIn.Item2 <= ev.GetTimestampEnd())
                            {
                                Hits++;
                                hitted = true;
                                break;
                            }
                        }
                        if (hitted == false)
                        {
                            Misses++;
                        }
                    }
                    _calculatedHitResult = true;
                }
                _hitResult = new HitResults(EventHits, Hits, Misses, EventTotal);
                return _hitResult;
            }
            else
            {
                return _hitResult;
            }
        }

        public class HitResults
        {
            public int eventHits { get; set; }
            public int hits { get; set; }
            public int misses { get; set; }
            public int eventsTotal { get; set; }
            
            public HitResults(int EventHits, int Hits, int Misses, int EventsTotal)
            {
                eventHits = EventHits;
                hits = Hits;
                misses = Misses;
                eventsTotal = EventsTotal;
            }
        }

        public double CalculateScore()
        {
            if (!_scoreIsCalculated)
            {
                /*double timeReduction = 1 -  (FlaggedAreaSize()/ (end - start));
                 double eventsHit = (double)events.Where(x => x.isHit).Count() / events.Count;
                 score = ((TIME_WEIGHT * timeReduction) * (HIT_WEIGHT * eventsHit)) / (HIT_WEIGHT + TIME_WEIGHT);
                 _scoreIsCalculated = true;
                 return score;*/
                HitResults hitResult = CalculateHitResult();

                score = (2*(hitResult.hits / ((double)hitResult.misses + hitResult.hits))  //Precision       
                        * (hitResult.eventHits / ((double)hitResult.eventsTotal))) //recall
                          / ((hitResult.hits / ((double)hitResult.misses + hitResult.hits))  //Precision       
                        + (hitResult.eventHits / ((double)hitResult.eventsTotal)));//recall
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
