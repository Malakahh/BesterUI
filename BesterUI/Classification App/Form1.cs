using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BesterUI.Helpers;
using BesterUI;
using BesterUI.Data;
using System.IO;

namespace Classification_App
{
    public partial class Form1 : Form
    {
        FusionData _fd = new FusionData();
        SAMData samData;
        string currentPath;
        CheckedListBox.ObjectCollection svmConfs;
        CheckedListBox.ObjectCollection metaConfs;
        CheckedListBox.ObjectCollection features;

        public Form1()
        {
            InitializeComponent();

            svmConfs = chklst_SvmConfigurations.Items;
            metaConfs = chklst_meta.Items;
            features = chklist_Features.Items;

            FeatureCreator.allFeatures.ForEach(x => features.Add(x, false));

            Log.LogBox = richTextBox1;
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            if (!chk_FeatureOptimizationNormal.Checked && !chk_ParameterOptimizationNormal.Checked)
            {//nothing checked

            }
            else if (!chk_FeatureOptimizationNormal.Checked && chk_ParameterOptimizationNormal.Checked)
            {//param opt checked

            }
            else if (chk_FeatureOptimizationNormal.Checked && !chk_ParameterOptimizationNormal.Checked)
            {//feature opt checked

            }
            else if (chk_FeatureOptimizationNormal.Checked && chk_ParameterOptimizationNormal.Checked)
            {//both checked
                List<Feature> feats = chklist_Features.CheckedItems.OfType<Feature>().ToList();
                SVMConfiguration cfg = new SVMConfiguration();
                cfg.features = feats;

                StdClassifier lol = new StdClassifier(cfg, samData);
                var res = lol.CrossValidate(SAMDataPoint.FeelingModel.Arousal2High, 1);
                Log.LogMessage("FScore: " + res.First().AverageFScore());
            }
        }

        private void btn_LoadData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                currentPath = fbd.SelectedPath;
                Log.LogMessage("Selected folder: " + fbd.SelectedPath);
                //load fusion data
                _fd.LoadFromFile(new string[] { fbd.SelectedPath + @"\EEG.json", fbd.SelectedPath + @"\GSR.json", fbd.SelectedPath + @"\HR.json" });
                samData = SAMData.LoadFromPath(fbd.SelectedPath + @"\SAM.json");
                Log.LogMessage("Fusion Data loaded!");

                Log.LogMessage("Applying data to features..");

                FeatureCreator.GSRFeatures.ForEach(x => x.SetData(_fd.gsrData.ToList<DataReading>()));
                FeatureCreator.HRFeatures.ForEach(x => x.SetData(_fd.hrData.ToList<DataReading>()));
                FeatureCreator.EEGFeatures.ForEach(x => x.SetData(_fd.eegData.ToList<DataReading>()));

                Log.LogMessage("Looking for configurations...");

                svmConfs.Clear();
                if (Directory.Exists(fbd.SelectedPath + @"\STD"))
                {
                    var files = Directory.GetFiles(fbd.SelectedPath + @"\STD");
                    Log.LogMessage("Found STD! Contains " + files.Length + " configurations.");
                    foreach (var item in files)
                    {
                        svmConfs.Add(SVMConfiguration.Deserialize(File.ReadAllText(item)));
                    }

                }

                if (Directory.Exists(fbd.SelectedPath + @"\META"))
                {
                    var files = Directory.GetFiles(fbd.SelectedPath + @"\META");
                    Log.LogMessage("Found META! Contains " + files.Length + " configurations.");
                    /* same procedure?? */
                    //foreach (var item in files)
                    //{
                    //    svmConfigs.Add(SVMConfiguration.Deserialize(File.ReadAllText(item)));
                    //}
                }

                if (svmConfs.Count == 0 && metaConfs.Count == 0)
                {
                    Log.LogMessage("No configurations found, maybe you should run some optimizations on some features.");
                }
            }
        }
    }
}
