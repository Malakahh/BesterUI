using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Classification_App
{
    static class AnomaliSerializer
    {
        #region [Feature Vectors]
        public static Dictionary<SENSOR, List<OneClassFV>> LoadFeatureVectors(string path)
        {
            throw new Exception("Not implemented");
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
                                        + string.Join(",", Array.ConvertAll(fv.Features, x => x.Value.ToString()));
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
        #endregion

    }
}
