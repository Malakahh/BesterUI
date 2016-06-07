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
            List<Tuple<int, int>> gsr = pois[SENSOR.GSR].GetFlaggedAreas();
            List<Tuple<int, int>> hr = pois[SENSOR.HR].GetFlaggedAreas();
            List<Tuple<int, int>> eeg = pois[SENSOR.EEG].GetFlaggedAreas();
            List<Tuple<int, int>> face = pois[SENSOR.FACE].GetFlaggedAreas();

            Dictionary<string, bool> anomaliPresent = new Dictionary<string, bool>();
            anomaliPresent.Add("gsr", false);
            anomaliPresent.Add("hr", false);
            anomaliPresent.Add("eeg", false);
            anomaliPresent.Add("face", false);
            for (int time = start; time < end; time++)
            {
                if(gsr.First().Item1 < time && time < gsr.First().Item2)
                {
                    anomaliPresent["gsr"] = true;
                }
                if (hr.First().Item1 < time && time < hr.First().Item2)
                {
                    anomaliPresent["hr"] = true;
                }
                if (eeg.First().Item1 < time && time < eeg.First().Item2)
                {
                    anomaliPresent["eeg"] = true;
                }
                if (face.First().Item1 < time && time < face.First().Item2)
                {
                    anomaliPresent["face"] = true;
                }
                if (anomaliPresent.Where(x=>x.Value == true).Count() >= voteThreashold)
                {
                    foreach (var key in anomaliPresent.Keys)
                    {
                        if (anomaliPresent[key] == true)
                        {
                            if (key == "gsr")
                            {
                                gsr.RemoveAt(0);
                            }
                            else if (key == "eeg")
                            {
                                eeg.RemoveAt(0);
                            }
                            else if (key == "face")
                            {
                                face.RemoveAt(0);
                            }
                            else
                            {
                                hr.RemoveAt(0);
                            }
                        }
                    }

                }
                if (time > gsr.First().Item2)
                {
                    gsr.RemoveAt(0);
                }
                if (time > hr.First().Item2)
                {
                    hr.RemoveAt(0);
                }
                if (time > eeg.First().Item2)
                {
                    eeg.RemoveAt(0);
                }
                if (time > face.First().Item2)
                {
                    face.RemoveAt(0);
                }
            }
            votingPOI = new PointsOfInterest(anomalies);
        }
    }
}
