using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classification_App.Evnt;

namespace Classification_App
{
    class Voting
    {
        Dictionary<SENSOR, PointsOfInterest> pois;
        List<Events> evnts;
        int start;
        int end;
        PointsOfInterest votingPOI;
        int voteThreashold;
        List<OneClassFV> anomalies;
        public Voting(int Start, int End, Dictionary<SENSOR, PointsOfInterest> Pois, List<Events> Evnts, int VoteThreshold)
        {
            pois = Pois;
            evnts = Evnts.Select(x => x.Copy()).ToList();
            start = Start;
            end = End;
            voteThreashold = VoteThreshold;
            CreateVotingPOI();
        }

        public NoveltyResult GetNoveltyResult()
        {
            return new NoveltyResult(votingPOI, evnts, start, end, new LibSVMsharp.SVMParameter(), anomalies);
        }

        private void CreateVotingPOI()
        {
            anomalies = new List<OneClassFV>();

            for (int time = start; time < end; time++)
            {
                int count = 0;
                foreach (SENSOR key in pois.Keys)
                {
                    if (pois[key].IsPointFlagged(time))
                    {
                        count++;
                        if (count >= voteThreashold)
                        {
                            anomalies.Add(new OneClassFV(null, time));
                            continue;
                        }
                    }
                }
            }
            votingPOI = new PointsOfInterest(anomalies);
        }
    }
}
