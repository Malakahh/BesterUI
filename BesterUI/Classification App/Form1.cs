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
using LibSVMsharp;
using System.Threading;

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
            {
                //nothing checked
                List<Feature> feats = chklist_Features.CheckedItems.OfType<Feature>().ToList();
                SVMParameter temp = new SVMParameter();

                SVMConfiguration cfg = new SVMConfiguration(temp, feats);
                StdClassifier lol = new StdClassifier(cfg, samData);

                Thread newThread = new Thread(() =>
                {
                    var res = lol.CrossValidate(SAMDataPoint.FeelingModel.Arousal2High, 1);
                    foreach (var resTemp in res)
                    {
                        Log.LogMessage("A Score was: " + resTemp.AverageFScore());
                    }
                });
                newThread.Start();
            }
            else if (!chk_FeatureOptimizationNormal.Checked && chk_ParameterOptimizationNormal.Checked)
            {
                //param opt checked
                List<Feature> feats = chklist_Features.CheckedItems.OfType<Feature>().ToList();

                Thread newThread = new Thread(() =>
                {
                    StdClassifier lol = new StdClassifier("bestClassifier", GenerateSVMParameters(), feats, samData);
                    var res = lol.CrossValidate(SAMDataPoint.FeelingModel.Arousal2High, 1);
                    foreach (var resTemp in res)
                    {
                        Log.LogMessage("A Score was: " + resTemp.AverageFScore());
                    }
                });
                newThread.Start();
            }
            else if (chk_FeatureOptimizationNormal.Checked && !chk_ParameterOptimizationNormal.Checked)
            {

                List<Feature> feats = chklist_Features.CheckedItems.OfType<Feature>().ToList();
                SVMParameter temp = new SVMParameter();

                SVMConfiguration cfg = new SVMConfiguration(temp, feats);

                StdClassifier combinations = new StdClassifier(cfg, samData);
                //Combination of features
                Thread someThread = new Thread(() =>
                {
                    List<PredictionResult> res = new List<PredictionResult>();

                    res = combinations.CrossValidateCombinations(SAMDataPoint.FeelingModel.Arousal2High, 1);

                    foreach (var resTemp in res)
                    {
                        Log.LogMessage("Score was: " + resTemp.AverageFScore());
                    }
                }
                );
                someThread.Start();
            }
            else if (chk_FeatureOptimizationNormal.Checked && chk_ParameterOptimizationNormal.Checked)
            {//both checked

            }
            else
            {
                Log.LogMessage("Something went not right (nor left), just wrong");
            }
        }

        void SaveConfiguration(SVMConfiguration conf)
        {
            if (svmConfs.Contains(conf))
            {
                svmConfs.Add(conf);
            }

            if (!Directory.Exists(currentPath + @"\STD"))
            {
                Directory.CreateDirectory(currentPath + @"\STD");
            }

            File.WriteAllText(currentPath + @"\STD\" + conf.Name + ".svm", conf.Serialize());
        }

        void SaveConfiguration(MetaSVMConfiguration conf)
        {
            if (!Directory.Exists(currentPath + @"\META"))
            {
                Directory.CreateDirectory(currentPath + @"\META");
            }

            File.WriteAllText(currentPath + @"\META\" + conf.Name, conf.Serialize() + ".svm");
        }
        private void btn_LoadData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                LoadData(fbd.SelectedPath);
                DataLoaded();
            }
        }

        void LoadData(string path)
        {
            currentPath = path;
            Log.LogMessage("Selected folder: " + path);
            //load fusion data
            _fd.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat" });
            samData = SAMData.LoadFromPath(path + @"\SAM.json");
            Log.LogMessage("Fusion Data loaded!");

            Log.LogMessage("Applying data to features..");

            FeatureCreator.GSRFeatures.ForEach(x => x.SetData(_fd.gsrData.ToList<DataReading>()));
            FeatureCreator.HRFeatures.ForEach(x => x.SetData(_fd.hrData.ToList<DataReading>()));
            FeatureCreator.EEGFeatures.ForEach(x => x.SetData(_fd.eegData.ToList<DataReading>()));

            Log.LogMessage("Looking for configurations...");

            svmConfs.Clear();
            if (Directory.Exists(path + @"\STD"))
            {
                var files = Directory.GetFiles(path + @"\STD");
                Log.LogMessage("Found STD! Contains " + files.Length + " configurations.");
                foreach (var item in files)
                {
                    svmConfs.Add(SVMConfiguration.Deserialize(File.ReadAllText(item)));
                }

            }

            if (Directory.Exists(path + @"\META"))
            {
                var files = Directory.GetFiles(path + @"\META");
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

        #region [Enable UI Steps]
        private void DataLoaded()
        {
            tabControl1.Enabled = true;
            btn_RunAll.Enabled = true;
            statusLabel.Text = "Data is ready";
        }

        #endregion

        #region [Helper Functions]

        private List<SVMParameter> GenerateSVMParameters()
        {
            List<double> cTypes = new List<double>() { };
            List<double> gammaTypes = new List<double>() { };
            List<SVMKernelType> kernels = new List<SVMKernelType> { SVMKernelType.LINEAR, SVMKernelType.POLY, SVMKernelType.RBF, SVMKernelType.SIGMOID };
            for (int t = -5; t <= 15; t++)
            {
                cTypes.Add(Math.Pow(2, t));
            }
            for (int t = -15; t <= 3; t++)
            {
                gammaTypes.Add(Math.Pow(2, t));
            }
            //Generate SVMParams
            List<SVMParameter> svmParams = new List<SVMParameter>();

            foreach (SVMKernelType kernel in kernels)
            {
                foreach (double c in cTypes)
                {
                    for (int i = 0; (kernel != SVMKernelType.LINEAR) ? i < gammaTypes.Count : i < 1; i++)
                    {
                        SVMParameter t = new SVMParameter();
                        t.Kernel = kernel;
                        t.C = c;
                        t.Gamma = gammaTypes[i];
                        svmParams.Add(t);
                    }
                }
            }
            return svmParams;
        }

        private void AddStdClassifierToLists(StdClassifier standardClassifier)
        {
            chklst_SvmConfigurations.Items.Add(standardClassifier);
            chklst_meta.Items.Add(standardClassifier);
        }
        #endregion

        volatile int gsrProg = 0;
        volatile int gsrTot = 1;
        volatile int hrProg = 0;
        volatile int hrTot = 1;
        volatile int eegProg = 0;
        volatile int eegTot = 1;
        private void btn_RunAll_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                btn_LoadData.Enabled = false;
                var dataFolders = Directory.GetDirectories(fbd.SelectedPath);
                List<SVMParameter> parameters = GenerateSVMParameters();
                int curDat = 1;
                int maxDat = dataFolders.Length;
                foreach (var item in dataFolders)
                {
                    if (File.Exists(item + @"\donno.dk")) continue;

                    LoadData(item);

                    gsrProg = 0;
                    hrProg = 0;
                    eegProg = 0;


                    Thread gsrThread = new Thread(() =>
                    {
                        List<Feature> gsrFeats = FeatureCreator.GSROptimizationFeatures;

                        StdClassifier gsrMachine = new StdClassifier("GSR", parameters, gsrFeats, samData);
                        gsrMachine.UpdateCallback = (cur, max) => { gsrProg = cur; gsrTot = max; };
                        var res = gsrMachine.CrossValidateCombinations(SAMDataPoint.FeelingModel.Arousal2High, 1);
                        SaveBestResult(res, gsrMachine.Name);
                    });

                    Thread hrThread = new Thread(() =>
                    {
                        List<Feature> hrFeats = FeatureCreator.HROptimizationFeatures;

                        StdClassifier hrMachine = new StdClassifier("HR", parameters, hrFeats, samData);
                        hrMachine.UpdateCallback = (cur, max) => { hrProg = cur; hrTot = max; };
                        var res = hrMachine.CrossValidateCombinations(SAMDataPoint.FeelingModel.Arousal2High, 1);
                        SaveBestResult(res, hrMachine.Name);
                    });

                    Thread eegThread = new Thread(() =>
                    {
                        List<Feature> eegFeats = FeatureCreator.EEGOptimizationFeatures;

                        StdClassifier eegMachine = new StdClassifier("EEG", parameters, eegFeats, samData);
                        eegMachine.UpdateCallback = (cur, max) => { eegProg = cur; eegTot = max; };
                        var res = eegMachine.CrossValidateCombinations(SAMDataPoint.FeelingModel.Arousal2High, 1);
                        SaveBestResult(res, eegMachine.Name);
                    });

                    gsrThread.Priority = ThreadPriority.Highest;
                    hrThread.Priority = ThreadPriority.Highest;
                    eegThread.Priority = ThreadPriority.Highest;

                    gsrThread.Start();
                    hrThread.Start();
                    eegThread.Start();

                    while (gsrThread.IsAlive || hrThread.IsAlive || eegThread.IsAlive)
                    {
                        Thread.Sleep(500);
                        double pct = (double)(gsrProg + hrProg + eegProg) * (double)100 / (double)(gsrTot + hrTot + eegTot);
                        Log.LogMessageSameLine(curDat + "/" + maxDat + " | Progress: " + pct.ToString("0.0") + "% - [GSR(" + gsrProg + "/" + gsrTot + ")] - [HR(" + hrProg + "/" + hrTot + ")] - [EEG(" + eegProg + "/" + eegTot + ")]");
                        Application.DoEvents();
                    }

                    using (var temp = File.Create(currentPath + @"\donno.dk")) { }
                    Log.LogMessage("DonnoDK");
                    curDat++;
                }
            }
        }

        void SaveBestResult(List<PredictionResult> res, string prefix)
        {
            double bestF = 0;
            foreach (var resTemp in res)
            {
                bestF = Math.Max(bestF, resTemp.AverageFScore());
            }

            var confs = res.Where(x => x.AverageFScore() == bestF);
            foreach (var conf in confs)
            {
                var c = conf.GenerateConfiguration();
                c.Name = prefix + "_" + c.Name;
                SaveConfiguration(c);
            }
        }
    }
}
