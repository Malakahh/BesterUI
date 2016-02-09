using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FFT.DataTypes
{
    public class SAMData
    {
        public DateTime startTime;
        public DateTime endTime;

        public List<SAMDataPoint> dataPoints = new List<SAMDataPoint>();

        public static SAMData LoadFromPath(string path)
        {
            SAMData data = new SAMData();

            try
            {
                System.Web.Script.Serialization.JavaScriptSerializer hej = new System.Web.Script.Serialization.JavaScriptSerializer();
                string jsonTxt = File.ReadAllText(path);
                hej.MaxJsonLength = int.MaxValue;
                var JObj = hej.Deserialize<dynamic>(jsonTxt);

                data.startTime = DateTimeFromUnixTime((long)JObj["startTime"]);
                data.endTime = DateTimeFromUnixTime((long)JObj["endTime"]);

                int i = 0;
                while (true)
                {
                    try
                    {
                        data.dataPoints.Add(new SAMDataPoint(
                            JObj["data"][i]["time_image_shown"],
                            JObj["data"][i]["time_clicked_next"],
                            int.Parse(JObj["data"][i]["arousal"]),
                            int.Parse(JObj["data"][i]["valence"]),
                            (double)JObj["data"][i]["control_arousal"],
                            (double)JObj["data"][i]["control_valence"],
                            JObj["data"][i]["image_type"]
                        ));

                        i++;
                    }
                    catch (Exception innerE)
                    {
                        if (i == 0)
                        {
                            //Log.LogMessage(innerE.Message);
                        }
                        break;
                    }
                }


            }
            catch (Exception e)
            {
                //Log.LogMessage("[ERROR] SAM data is corrupt!" + "\n" + e.Message);
            }

            return data;
        }

        public static DateTime DateTimeFromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }
    }

    public class SAMDataPoint
    {
        public readonly int timeOffset;
        public readonly int timeOffsetSAM;
        public readonly int timeOffsetClick;
        public readonly int arousal;
        public readonly double ctrlArousal;
        public readonly int valence;
        public readonly double ctrlValence;
        public readonly string imageType;

        public SAMDataPoint(int timeOffset, int timeOffsetClick, int arousal, int valence, double ctrlArousal, double ctrlValence, string imageType)
        {
            this.timeOffset = timeOffset + 60000;
            this.timeOffsetSAM = timeOffset + 63000;
            this.timeOffsetClick = timeOffsetClick + 60000;
            this.arousal = arousal;
            this.ctrlArousal = ctrlArousal;
            this.valence = valence;
            this.ctrlValence = ctrlValence;
            this.imageType = imageType;
        }

        public int ToAVCoordinate(EEGClass classType, bool useControlValues = false)
        {
            int valenceToUse = useControlValues ? (int)(ctrlValence - 0.5) : valence - 1;
            int arousalToUse = useControlValues ? (int)(ctrlArousal - 0.5) : arousal - 1;

            switch (classType)
            {
                case EEGClass.Valence9:
                    return valenceToUse;
                case EEGClass.Valence3:
                    return valenceToUse < 3 ? 0 : (valenceToUse < 4 ? 1 : 2);
                case EEGClass.Valence2Low:
                    return valenceToUse < 4 ? 0 : 1;
                case EEGClass.Valence2High:
                    return valenceToUse < 5 ? 0 : 1;
                case EEGClass.Arousal9:
                    return arousalToUse;
                case EEGClass.Arousal3:
                    return arousalToUse < 3 ? 0 : (arousalToUse < 6 ? 1 : 2);
                case EEGClass.Arousal2Low:
                    return arousalToUse < 4 ? 0 : 1;
                case EEGClass.Arousal2High:
                    return arousalToUse < 5 ? 0 : 1;
                case EEGClass.ValenceArousal9:
                    return (valenceToUse) * 9 + arousalToUse;
                case EEGClass.ValenceArousal3:
                    int val3 = valenceToUse < 3 ? 0 : (valenceToUse < 6 ? 1 : 2);
                    int aro3 = arousalToUse < 3 ? 0 : (arousalToUse < 6 ? 1 : 2);
                    return val3 * 3 + aro3;
                default:
                    {
                        //Log.LogMessage("wat");
                        return -1;
                    }
            }
        }

        public enum EEGClass
        {
            Valence9,
            Valence3,
            Valence2Low,
            Valence2High,
            Arousal9,
            Arousal3,
            Arousal2Low,
            Arousal2High,
            ValenceArousal9,
            ValenceArousal3
        }
    }

}
