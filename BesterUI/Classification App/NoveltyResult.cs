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
        private bool _calculatedConfusionMatrix = false;
        private ConfusionMatrix _confusionMatrix;

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

        public ConfusionMatrix CalculateConfusionMatrix()
        {
            if (!_calculatedConfusionMatrix)
            {
                int TruePostive = 0;
                int FalsePostive = 0;
                int FalseNegative = 0;
                int TrueNegative = 0;
                //Positives
                List<Events> tempEvents = events.Select(x => x.Copy()).ToList();
                List<Tuple<int, int>> tempPoi = poi.GetFlaggedAreas();
                /*  foreach (Events ev in tempEvents)
                  {
                      if (ev.isHit)
                      {
                          TruePostive++;
                      }
                      else
                      {
                          FalsePostive++;
                      }
                  }*/
                foreach (Tuple<int, int> pointOfInterest in tempPoi)
                {
                    for (int iterator = pointOfInterest.Item1; iterator <= pointOfInterest.Item2; iterator++)
                    {
                        if (iterator < start)
                        {
                            continue;
                        }
                        foreach (Events ev in events)
                        {
                            if (ev.GetTimestampEnd() < iterator)
                            {
                                continue;
                            }
                            else if (ev.GetTimestampStart() < iterator && iterator < ev.GetTimestampEnd())
                            {
                                TruePostive++;
                                continue;
                            }
                        }
                        FalsePostive++;
                    }
                }


                tempEvents = events.Select(x => x.Copy()).ToList();
                tempPoi = poi.GetFlaggedAreas();
                //Negatives
                for (int time = start; time < end; time += 1)
                {
                    if (tempPoi.Count != 0 && tempEvents.Count != 0)
                    {
                        if (!(tempPoi.First().Item1 <= time && time <= tempPoi.First().Item2))
                        {
                            int eventTimeStampStart = tempEvents.First().GetTimestampStart();
                            int eventTimestampEnd = tempEvents.First().GetTimestampEnd();
                            if (eventTimeStampStart < time && time < eventTimestampEnd)
                            {
                                FalseNegative++;
                            }
                            else
                            {
                                TrueNegative++;
                            }
                        }
                        if (time + 1 > tempPoi.First().Item2)
                        {
                            tempPoi.RemoveAt(0);
                        }
                        if (time + 1 > tempEvents.First().GetTimestampEnd())
                        {
                            tempEvents.RemoveAt(0);
                        }
                    }
                    else if (tempEvents.Count != 0 && tempPoi.Count == 0)
                    {
                        if (!(tempEvents.First().GetTimestampStart() <= time && time <= tempEvents.First().GetTimestampEnd()))
                        {
                            TrueNegative++;
                        }
                        else
                        {
                            FalseNegative++;
                        }

                        if (time + 1 >= tempEvents.First().GetTimestampEnd())
                        {
                            tempEvents.RemoveAt(0);
                        }
                    }
                    else if (tempEvents.Count == 0 && tempPoi.Count != 0)
                    {
                        if (tempPoi.First().Item1 <= time && time <= tempPoi.First().Item2)
                        {
                            FalseNegative++;
                        }
                        else
                        {
                            TrueNegative++;
                        }

                        if (time + 1 >= tempPoi.First().Item2)
                        {
                            tempPoi.RemoveAt(0);
                        }
                    }
                    else
                    {
                        TrueNegative++;
                    }
                }
                _confusionMatrix = new ConfusionMatrix(TruePostive, FalsePostive, TrueNegative, FalseNegative);
                _calculatedConfusionMatrix = true;
                return _confusionMatrix;
            }
            else
            {
                return _confusionMatrix;
            }
        }

        public class ConfusionMatrix
        {
            public int TruePostive { get; set; }
            public int FalsePostive { get; set; }
            public int FalseNegative { get; set; }
            public int TrueNegative { get; set; }

            public double CalculateTruePositiveRate()
            {
                return (TruePostive) / (TruePostive + FalseNegative);
            }

            public double CalculateFalsePositiveRate()
            {
                return (TruePostive) / (TruePostive + FalseNegative);
            }

            public ConfusionMatrix(int TPositive, int FPositive, int TNegative, int FNegative)
            {
                TruePostive = TPositive;
                FalsePostive = FPositive;
                FalseNegative = FNegative;
                TrueNegative = TNegative;
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
                ConfusionMatrix conf = CalculateConfusionMatrix();
                int hits = 0;
                foreach (Events ev in events)
                {
                    if (ev.isHit)
                    {
                        hits++;
                    }
                }
                score = (double)(((hits / (decimal)events.Count) * ((2 * (conf.TruePostive / ((decimal)conf.TruePostive + conf.FalsePostive)))) * (conf.TruePostive / ((decimal)conf.TruePostive + conf.FalseNegative))) / 2);
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
