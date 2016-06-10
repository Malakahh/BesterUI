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
            NoveltyResult temp = new NoveltyResult(votingPOI, evnts, start, end, new LibSVMsharp.SVMParameter(), anomalies);
            temp.events.ForEach(x => x.SetPointOfInterest(votingPOI));
            return temp;
        }

        private void CreateVotingPOI()
        {
            anomalies = new List<OneClassFV>();
            List<Tuple<int, int>> gsr = pois[SENSOR.GSR].GetFlaggedAreas();
            List<Tuple<int, int>> hr = pois[SENSOR.HR].GetFlaggedAreas();
            List<Tuple<int, int>> eeg = new List<Tuple<int, int>>();
            if (pois.Keys.Contains(SENSOR.EEG))
           {
                eeg = pois[SENSOR.EEG].GetFlaggedAreas();
            }
            List<Tuple<int, int>> face = pois[SENSOR.FACE].GetFlaggedAreas();

            Dictionary<string, bool> anomaliPresent = new Dictionary<string, bool>();
            anomaliPresent.Add("gsr", false);
            anomaliPresent.Add("hr", false);
            anomaliPresent.Add("eeg", false);
            anomaliPresent.Add("face", false);
            for (int time = start; time < end; time++)
            {
                if(gsr.Count > 0 && gsr.First().Item1 < time && time < gsr.First().Item2)
                {
                    anomaliPresent["gsr"] = true;
                }
                if (hr.Count > 0 && hr.First().Item1 < time && time < hr.First().Item2)
                {
                    anomaliPresent["hr"] = true;
                }
                if (eeg.Count > 0 && eeg.First().Item1 < time && time < eeg.First().Item2)
                {
                    anomaliPresent["eeg"] = true;
                }
                if (face.Count > 0 && face.First().Item1 < time && time < face.First().Item2)
                {
                    anomaliPresent["face"] = true;
                }
                if (anomaliPresent.Where(x=>x.Value == true).Count() >= voteThreashold)
                {
                    List<double> centers = new List<double>();
                    foreach (var key in anomaliPresent.Keys)
                    {
                        if (anomaliPresent[key] == true)
                        {
                            if (key == "gsr")
                            {
                                centers.Add(((gsr.First().Item2 -gsr.First().Item1)/2) + gsr.First().Item1);
                                gsr.RemoveAt(0);
                            }
                            else if (key == "eeg")
                            {
                                centers.Add(((eeg.First().Item2 - eeg.First().Item1) / 2) + eeg.First().Item1);
                                eeg.RemoveAt(0);
                            }
                            else if (key == "face")
                            {
                                centers.Add(((face.First().Item2 - face.First().Item1) / 2) + face.First().Item1);
                                face.RemoveAt(0);
                            }
                            else
                            {
                                centers.Add(((hr.First().Item2 - hr.First().Item1) / 2) + hr.First().Item1);
                                hr.RemoveAt(0);
                            }
                        }
                    }
                    anomalies.Add(new OneClassFV(null, (int)Math.Round(centers.Average())));

                }
                if (gsr.Count > 0 && time > gsr.First().Item2)
                {
                    gsr.RemoveAt(0);
                }
                if (hr.Count > 0 && time > hr.First().Item2)
                {
                    hr.RemoveAt(0);
                }
                if (eeg.Count > 0 && time > eeg.First().Item2)
                {
                    eeg.RemoveAt(0);
                }
                if (face.Count > 0 && time > face.First().Item2)
                {
                    face.RemoveAt(0);
                }
                //Clean
                anomaliPresent["gsr"] = false;
                anomaliPresent["eeg"] = false;
                anomaliPresent["hr"] = false;
                anomaliPresent["face"] = false;
            }
            votingPOI = new PointsOfInterest(anomalies);
        }
    }
}
