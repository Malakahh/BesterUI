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
            string[] progressFile = File.ReadAllLines(path);
            foreach(string s in progressFile)
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
            lines.Add("GSR:false");
            lines.Add("EEG:false");
            lines.Add("Face:false");
            lines.Add("HR:false");
            File.WriteAllLines(path + @"/progress.file", lines);
        }

        private void SaveProgress()
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
