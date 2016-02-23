﻿using System;
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


        #region [Enable UI Steps]
        private void DataLoaded()
        {
            tabControl1.Enabled = true;
            btn_RunAll.Enabled = true;
            statusLabel.Text = "Data is ready";
        }

        #endregion

        #region [Helper Functions]

        void SaveConfiguration(SVMConfiguration conf)
        {
            if (!Directory.Exists(currentPath + @"\STD"))
            {
                Directory.CreateDirectory(currentPath + @"\STD");
            }

            File.WriteAllText(currentPath + @"\STD\" + conf.Name, conf.Serialize());
        }

        void SaveConfiguration(MetaSVMConfiguration conf)
        {
            if (!Directory.Exists(currentPath + @"\META"))
            {
                Directory.CreateDirectory(currentPath + @"\META");
            }

            File.WriteAllText(currentPath + @"\META\" + conf.Name, conf.Serialize());
        }

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

        #region [Forms Events]
        private void addMachineBtn_Click(object sender, EventArgs e)
        {
            List<Feature> feats = chklist_Features.CheckedItems.OfType<Feature>().ToList();

            //TODO: Find a way to load the ccorrect parameters in
            SVMParameter temp = new SVMParameter();

            SVMConfiguration cfg = new SVMConfiguration(temp, feats);
            StdClassifier theSVM = new StdClassifier(cfg, samData);
            AddStdClassifierToLists(theSVM);

        }

        //Run normal classifier
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

        private void btn_LoadData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                currentPath = fbd.SelectedPath;
                Log.LogMessage("Selected folder: " + fbd.SelectedPath);
                //load fusion data
                _fd.LoadFromFile(new string[] { fbd.SelectedPath + @"\EEG.dat", fbd.SelectedPath + @"\GSR.dat", fbd.SelectedPath + @"\HR.dat" });
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
            DataLoaded();
        }
        #endregion

        private void metaRunBtn_Click(object sender, EventArgs e)
        {
            if (votingCB.Checked)
            {
                List<StdClassifier> classifiers = new List<StdClassifier>();
                foreach (StdClassifier classifier in chklst_meta.CheckedItems)
                {
                    classifiers.Add(classifier);
                }
                MetaClassifier votingMeta = new MetaClassifier("Voting", new SVMParameter(), samData, classifiers);
                PredictionResult result  = votingMeta.DoVoting(SAMDataPoint.FeelingModel.Arousal2High, 1);
                Log.LogMessage(result.AverageFScore());
               
            }
            if (stackingCB.Checked)
            {
                List<StdClassifier> classifiers = new List<StdClassifier>();
                foreach (StdClassifier classifier in chklst_meta.CheckedItems)
                {
                    classifiers.Add(classifier);
                }
                MetaClassifier stackingMeta = new MetaClassifier("Voting", new SVMParameter(), samData, classifiers);
                List<PredictionResult> result = stackingMeta.DoStacking(SAMDataPoint.FeelingModel.Arousal2High, 1);
                Log.LogMessage(result[0].AverageFScore());
            }
            if(boostingCB.Checked)
            {

                List<StdClassifier> classifiers = new List<StdClassifier>();
                foreach (StdClassifier classifier in chklst_meta.CheckedItems)
                {
                    classifiers.Add(classifier);
                }
                MetaClassifier boostingMeta = new MetaClassifier("Voting", new SVMParameter(), samData, classifiers);

                for (int i = 0; i < chklst_meta.CheckedItems.Count; i++)
                {
                    boostingMeta.boostingOrder.Add(i);
                }

                PredictionResult result = boostingMeta.DoBoosting(SAMDataPoint.FeelingModel.Arousal2High, 1);
                Log.LogMessage(result.AverageFScore());
            }
        }
    }
}
