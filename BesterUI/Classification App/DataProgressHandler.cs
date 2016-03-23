using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Classification_App
{
    class DataProgressHandler
    {
        public Dictionary<string, bool> done = new Dictionary<string, bool>();
        public bool AllDone = false;
        public bool MetaDone = false;
        private string Path = "";

        public DataProgressHandler(string path)
        {
            Path = path;
            string[] files = Directory.GetFiles(path);
            if (!files.Contains(path + "\\progress.file"))
            {
                GenerateProgressFile(path);
            }
            string[] progressFile = File.ReadAllLines(path + @"/progress.file");
            foreach (string s in progressFile)
            {
                string[] fraction = s.Split(':');

                if (fraction[0] != "AllDone")
                {
                    if (fraction[0] != "MetaDone")
                    {
                        done.Add(fraction[0], bool.Parse(fraction[1]));
                    }
                    else
                    {
                        if (bool.Parse(fraction[1]))
                        {
                            MetaDone = true;
                        }
                        else
                        {
                            MetaDone = false;
                        }
                    }
                }
                else
                {
                    if (bool.Parse(fraction[1]))
                    {
                        AllDone = true;
                    }
                    else
                    {
                        AllDone = false;
                    }
                }
            }

            //if the file does not contain information about meta (ie. old save format)
            if (done.Keys.Count(x => x.Contains("Voting")) == 0)
            {
                //convert to new type
                AddMissingMeta();
            }
        }

        private void GenerateProgressFile(string path)
        {
            List<string> lines = new List<string>();
            lines.Add("AllDone:false");
            lines.Add("MetaDone:false");
            //GSR
            lines.Add("GSR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2High) + ":false");
            lines.Add("GSR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2Low) + ":false");
            lines.Add("GSR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal3) + ":false");

            //EEG
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2High) + ":false");
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2Low) + ":false");
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal3) + ":false");
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence2High) + ":false");
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence2Low) + ":false");
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence3) + ":false");
            //Kiect
            lines.Add("Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2High) + ":false");
            lines.Add("Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2Low) + ":false");
            lines.Add("Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal3) + ":false");
            lines.Add("Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence2High) + ":false");
            lines.Add("Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence2Low) + ":false");
            lines.Add("Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence3) + ":false");
            //HR 
            lines.Add("HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2High) + ":false");
            lines.Add("HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2Low) + ":false");
            lines.Add("HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal3) + ":false");
            lines.Add("HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence2High) + ":false");
            lines.Add("HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence2Low) + ":false");
            lines.Add("HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Valence3) + ":false");

            //meta
            foreach (var item in Enum.GetNames(typeof(SAMDataPoint.FeelingModel)))
            {
                lines.Add("Voting" + item + ":false");
            }

            foreach (var item in Enum.GetNames(typeof(SAMDataPoint.FeelingModel)))
            {
                lines.Add("Stacking" + item + ":false");
            }

            File.WriteAllLines(path + @"/progress.file", lines);
        }

        public void SaveProgress()
        {
            List<string> lines = new List<string>();

            lines.Add("AllDone:" + AllDone);
            lines.Add("MetaDone:" + MetaDone);

            foreach (string s in done.Keys)
            {
                lines.Add(s + ":" + done[s]);
            }

            File.WriteAllLines(Path + @"/progress.file", lines);

        }

        public void AddMissingMeta()
        {
            foreach (var item in Enum.GetNames(typeof(SAMDataPoint.FeelingModel)))
            {
                done.Add("Voting" + item, false);
            }

            foreach (var item in Enum.GetNames(typeof(SAMDataPoint.FeelingModel)))
            {
                done.Add("Stacking" + item, false);
            }
        }
    }

}
