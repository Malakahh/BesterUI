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
using System.IO;

namespace Classification_App
{
    public partial class Form1 : Form
    {
        FusionData _fd = new FusionData();
        SAMData samData;
        List<SVMConfiguration> svmConfigs = new List<SVMConfiguration>();
        string currentPath;

        public Form1()
        {
            InitializeComponent();

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

                Log.LogMessage("Looking for configurations...");

                svmConfigs.Clear();
                if (Directory.Exists(fbd.SelectedPath + @"\STD"))
                {
                    var files = Directory.GetFiles(fbd.SelectedPath + @"\STD");
                    Log.LogMessage("Found STD! Contains " + files.Length + " configurations.");
                    foreach (var item in files)
                    {
                        svmConfigs.Add(SVMConfiguration.Deserialize(File.ReadAllText(item)));
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

            }
        }
    }
}
