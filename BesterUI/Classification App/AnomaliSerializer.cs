using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Classification_App.Evnt;

namespace Classification_App
{
    static class AnomaliSerializer
    {
        #region [Feature Vectors]
        public static ConcurrentDictionary<SENSOR, List<OneClassFV>> LoadFeatureVectors(string path)
        {
            ConcurrentDictionary<SENSOR, List<OneClassFV>> featureVectors = new ConcurrentDictionary<SENSOR, List<OneClassFV>>();

            if (!Directory.Exists(path + "/FeatureVectors"))
            {
                return null;
            }
            foreach (SENSOR key in Enum.GetValues(typeof(SENSOR)))
            {
                string[] data = File.ReadAllLines(path + "/FeatureVectors/" + key.ToString() + ".txt");

                List<OneClassFV> featureVector = new List<OneClassFV>();
                for (int i = 1; i < data.Length; i++)
                {
                    string[] firstSplit = data[i].Split(':');
                    int time = int.Parse(firstSplit[0]);
                    string[] secondSplit = firstSplit[1].Split(';');
                    List<double> values = new List<double>();
                    foreach (string s in secondSplit)
                    {

                        values.Add(double.Parse(s.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture));
                    }

                    LibSVMsharp.SVMNode[] node = new LibSVMsharp.SVMNode[values.Count];
                    for(int j = 0; j < values.Count; j++)
                    {
                        node[j] = new LibSVMsharp.SVMNode(j + 1, values[j]);
                    }
                    featureVector.Add(new OneClassFV(node, time));
                }
                featureVectors.TryAdd(key, featureVector);
            }
            return featureVectors;
        }

        public static void SaveFeatureVectors(ConcurrentDictionary<SENSOR, List<OneClassFV>> featureVectors, string path)
        {
            if (!Directory.Exists(path + "/FeatureVectors"))
            {
                Directory.CreateDirectory(path + "/FeatureVectors");
            }
            foreach (SENSOR key in featureVectors.Keys)
            {
                List<string> data = new List<string>();
                data.Add($"{key} -  {featureVectors[key][1].TimeStamp - featureVectors[key][0].TimeStamp}");
                
                foreach (OneClassFV fv in featureVectors[key])
                {
                    string tempVector = fv.TimeStamp.ToString()
                                        + ":"
                                        + string.Join(";", Array.ConvertAll(fv.Features, x => x.Value.ToString()));
                    data.Add(tempVector);
                }
                File.WriteAllLines(path + "/FeatureVectors/" + key.ToString() + ".txt", data);
            }
        }
        #endregion

        #region [Results]
        public static void SaveAnomalis(Dictionary<SENSOR, List<OneClassFV>> anomalis, string path, int stepSize)
        {
            if (!Directory.Exists(path + "/Anomalis"))
            {
                Directory.CreateDirectory(path + "/Anomalis");
            }
            foreach (SENSOR key in anomalis.Keys)
            {
                List<string> data = new List<string>();
                data.Add($"{key} -  {stepSize}");

                foreach (OneClassFV fv in anomalis[key])
                {
                    data.Add(fv.TimeStamp.ToString());
                }
                File.WriteAllLines(path + "/Anomalis/" + key.ToString() + ".txt", data);
            }
        }

        public static void SavePointsOfInterest(Dictionary<SENSOR, PointsOfInterest> POIs, string path)
        {
            if (!Directory.Exists(path + "/POI"))
            {
                Directory.CreateDirectory(path + "/POI");
            }
            foreach (SENSOR key in POIs.Keys)
            {
                List<string> data = new List<string>();
                var tempAreas = POIs[key].GetFlaggedAreas();
                foreach (Tuple<int, int> area in tempAreas)
                {
                    data.Add($"{area.Item1}, {area.Item2}");
                }
                File.WriteAllLines(path + "/POI/" + key.ToString() + ".txt", data);
            }
        }

        public static void SaveEvents(Dictionary<SENSOR, List<Events>> events, string path)
        {
            if (!Directory.Exists(path + "/Events"))
            {
                Directory.CreateDirectory(path + "/Events");
            }
            foreach (SENSOR key in events.Keys)
            {
                List<string> data = new List<string>();

                foreach (Events ev in events[key])
                {
                    data.Add($"{ev.eventName}, {ev.isHit}," 
                        + ((ev.GetTimestampEnd() == 0) ? 
                            $"{ ev.GetTimestampStart().ToString()} ; { ev.GetTimestampStart().ToString()}" :
                            $"{ ev.GetTimestampStart().ToString()} + { ev.GetTimestampEnd().ToString()}"
                            ));
                }
                File.WriteAllLines(path + "/Events/" + key.ToString() + ".txt", data);
            }
        }
        #endregion

        #region [Load Results]
        public static Dictionary<SENSOR, PointsOfInterest> LoadPointOfInterest(string path)
        {
            Dictionary<SENSOR, PointsOfInterest> pois = new Dictionary<SENSOR, PointsOfInterest>();

            if (!Directory.Exists(path + "/POI"))
            {
                return null;
            }
            foreach (SENSOR key in Enum.GetValues(typeof(SENSOR)))
            {
                string[] data = File.ReadAllLines(path + "/Anomalis/" + key.ToString() + ".txt");
                List<int> anoma = new List<int>();
                for (int i = 1; i < data.Length; i++)
                {
                    anoma.Add(int.Parse(data[i]));
                }
                PointsOfInterest currentPoi = new PointsOfInterest(anoma);
                pois.Add(key, currentPoi);
            }
            return pois;
        }
        #endregion
    }
}
