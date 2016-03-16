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
        private string Path = "";

        public DataProgressHandler(string path)
        {
            Path = path;
            string[] files = Directory.GetFiles(path);
            if (!files.Contains(path+"\\progress.file"))
            {
                GenerateProgressFile(path);
            }
            string[] progressFile = File.ReadAllLines(path + @"/progress.file");
            foreach (string s in progressFile)
            {
                string[] fraction = s.Split(':');

                if (fraction[0] != "AllDone")
                {
                    done.Add(fraction[0], bool.Parse(fraction[1]));
                }
                else
                {
                    if (bool.Parse(fraction[1]))
                    {
                        AllDone = true;
                        break;
                    }
                    else
                    {
                        AllDone = false;
                    }
                }
            }

        }

        private void GenerateProgressFile(string path)
        {
            List<string> lines = new List<string>();
            lines.Add("AllDone:false");
            //GSR
            lines.Add("GSR"+Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2High)+ ":false");
            lines.Add("GSR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2Low) + ":false");
            lines.Add("GSR"+Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal3)+":false");

            //EEG
            lines.Add("EEG"+Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2High)+":false");
            lines.Add("EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal2Low) + ":false");
            lines.Add("EEG"+ Enum.GetName(typeof(SAMDataPoint.FeelingModel), SAMDataPoint.FeelingModel.Arousal3) + ":false");
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
            File.WriteAllLines(path + @"/progress.file", lines);
        }

        public void SaveProgress()
        {
            List<string> lines = new List<string>();
            if (done.Values.All(x => x))
            {
                lines.Add("AllDone:true");
            }
            else
            {
                lines.Add("AllDone:false");
            }

            foreach (string s in done.Keys)
            {
                lines.Add(s + ":" + done[s]);
            }

            File.WriteAllLines(Path + @"/progress.file", lines);

        }
    }
    
}
