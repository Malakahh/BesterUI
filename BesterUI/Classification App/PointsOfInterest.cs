using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class PointsOfInterest
    {
        private const int ANOMALI_WIDTH = 2500;
        private List<Tuple<int, int>> flaggedAreas = new List<Tuple<int, int>>();
        private List<int> flags = new List<int>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Flags">Flags has to be sorted for the constructor to work correctly</param>
        public PointsOfInterest(List<int> Flags)
        {
            flags = Flags;
            for (int i = 0; i < Flags.Count; i++)
            {
                flaggedAreas.Add(Tuple.Create(Flags[i] - ANOMALI_WIDTH, Flags[i] + ANOMALI_WIDTH));
                /*
                if (flaggedAreas.Count == 0)
                {
                    flaggedAreas.Add(Tuple.Create(flags[i] - ANOMALI_WIDTH, flags[i] + ANOMALI_WIDTH));
                }
                else
                {
                    Tuple<int, int> latestFlaggedArea = flaggedAreas.Last();
                    if (flags[i] > latestFlaggedArea.Item1 + ANOMALI_WIDTH
                        && flags[i] < latestFlaggedArea.Item2 - ANOMALI_WIDTH)
                    {
                        continue;
                    }
                    else if (flags[i] > latestFlaggedArea.Item1 + ANOMALI_WIDTH
                        && flags[i] < latestFlaggedArea.Item1 - ANOMALI_WIDTH)
                    {
                        flaggedAreas[flaggedAreas.Count - 1] = Tuple.Create(flags[i] - ANOMALI_WIDTH, latestFlaggedArea.Item2);
                    }
                    else if (flags[i] < latestFlaggedArea.Item2 + ANOMALI_WIDTH
                        && flags[i] > latestFlaggedArea.Item2 - ANOMALI_WIDTH)
                    {
                        flaggedAreas[flaggedAreas.Count - 1] = Tuple.Create(latestFlaggedArea.Item1, flags[i] + ANOMALI_WIDTH);
                    }
                    else
                    {
                        flaggedAreas.Add(Tuple.Create(flags[i] - ANOMALI_WIDTH, flags[i] + ANOMALI_WIDTH));
                    }
                }*/
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Flags">Flags has to be sorted for the constructor to work correctly</param>
        public PointsOfInterest(List<OneClassFV> Flags)
        {
            flags = Flags.Select(x => x.TimeStamp).ToList();
            for(int i = 0; i < Flags.Count; i++)
            {
                flaggedAreas.Add(Tuple.Create(Flags[i].TimeStamp - ANOMALI_WIDTH, Flags[i].TimeStamp + ANOMALI_WIDTH));

                /* if (flaggedAreas.Count == 0)
                 {
                     flaggedAreas.Add(Tuple.Create(flags[i].TimeStamp - ANOMALI_WIDTH, flags[i].TimeStamp + ANOMALI_WIDTH));
                 }
                 else
                 {
                     Tuple<int, int> latestFlaggedArea = flaggedAreas.Last();
                     if (flags[i].TimeStamp > latestFlaggedArea.Item1 + ANOMALI_WIDTH
                         && flags[i].TimeStamp < latestFlaggedArea.Item2 - ANOMALI_WIDTH)
                     {
                         continue;
                     }
                     else if (flags[i].TimeStamp > latestFlaggedArea.Item1 + ANOMALI_WIDTH
                         && flags[i].TimeStamp < latestFlaggedArea.Item1 - ANOMALI_WIDTH)
                     {
                         flaggedAreas[flaggedAreas.Count-1] = Tuple.Create(flags[i].TimeStamp - ANOMALI_WIDTH, latestFlaggedArea.Item2);
                     }
                     else if (flags[i].TimeStamp < latestFlaggedArea.Item2 + ANOMALI_WIDTH
                         && flags[i].TimeStamp > latestFlaggedArea.Item2 - ANOMALI_WIDTH)
                     {
                         flaggedAreas[flaggedAreas.Count - 1] = Tuple.Create(latestFlaggedArea.Item1, flags[i].TimeStamp + ANOMALI_WIDTH);
                     }
                     else
                     {
                         flaggedAreas.Add(Tuple.Create(flags[i].TimeStamp - ANOMALI_WIDTH, flags[i].TimeStamp + ANOMALI_WIDTH));
                     }
                 }*/
            }
        }

        public List<Tuple<int, int>> GetFlaggedAreas()
        {
            return flaggedAreas.ToList();
        }

        public List<Tuple<int, int>> GetCoveredAreas()
        {
            List<Tuple<int, int>> cAreas = new List<Tuple<int, int>>();
             for (int i = 0; i < flags.Count; i++)
            {
                cAreas.Add(Tuple.Create(flags[i] - ANOMALI_WIDTH, flags[i] + ANOMALI_WIDTH));
                
                if (cAreas.Count == 0)
                {
                    cAreas.Add(Tuple.Create(flags[i] - ANOMALI_WIDTH, flags[i] + ANOMALI_WIDTH));
                }
                else
                {
                    Tuple<int, int> latestFlaggedArea = cAreas.Last();
                    if (flags[i] > latestFlaggedArea.Item1 + ANOMALI_WIDTH
                        && flags[i] < latestFlaggedArea.Item2 - ANOMALI_WIDTH)
                    {
                        continue;
                    }
                    else if (flags[i] > latestFlaggedArea.Item1 + ANOMALI_WIDTH
                        && flags[i] < latestFlaggedArea.Item1 - ANOMALI_WIDTH)
                    {
                        cAreas[cAreas.Count - 1] = Tuple.Create(flags[i] - ANOMALI_WIDTH, latestFlaggedArea.Item2);
                    }
                    else if (flags[i] < latestFlaggedArea.Item2 + ANOMALI_WIDTH
                        && flags[i] > latestFlaggedArea.Item2 - ANOMALI_WIDTH)
                    {
                        cAreas[cAreas.Count - 1] = Tuple.Create(latestFlaggedArea.Item1, flags[i] + ANOMALI_WIDTH);
                    }
                    else
                    {
                        cAreas.Add(Tuple.Create(flags[i] - ANOMALI_WIDTH, flags[i] + ANOMALI_WIDTH));
                    }
                }
            }
            return cAreas;
        }

        public bool IsPointFlagged(int timePoint)
        {
            foreach (Tuple<int,int> area in flaggedAreas)
            {
                if(area.Item1 < timePoint && timePoint < area.Item2)
                {
                    return true;
                }
            }
            return false;
        }

        public double PercentageAreaHit(int startTime, int endTime)
        {
            decimal hitPercentage = 0;
            foreach (Tuple<int, int> area in flaggedAreas)
            {
                if (area.Item1 < startTime && endTime < area.Item2)
                {
                    return 1;
                }
                else if (area.Item1 >= startTime && area.Item2 >= endTime && area.Item1 <= endTime)
                {
                    hitPercentage += (endTime - (decimal)area.Item1) / (area.Item2 - (decimal)area.Item1);
                }
                else if (area.Item1 <= startTime && area.Item2 <= endTime && area.Item2 >= startTime)
                {
                    hitPercentage += (area.Item2 - (decimal)startTime) / (area.Item2 - (decimal)area.Item1);
                }
                else if (startTime  <= area.Item1 && area.Item2 <= endTime)
                {
                    hitPercentage += (endTime - (decimal)startTime) / (area.Item2 - (decimal)area.Item1);
                }
            }
            return (double)hitPercentage;
        }
    }

}
