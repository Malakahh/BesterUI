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
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;

using System.Windows.Forms.DataVisualization.Charting;

using OxyPlot;
using OxyPlot.WindowsForms;

namespace Classification_App
{
    public partial class Form1 : Form
    {
        FusionData _fd = new FusionData();
        SAMData samData;
        string currentPath;
        List<SVMConfiguration> svmConfs = new List<SVMConfiguration>();
        List<MetaSVMConfiguration> metaConfs = new List<MetaSVMConfiguration>();

        FusionData fdTest = new FusionData();
        FusionData fdRecall = new FusionData();
        FusionData fdNovelty = new FusionData();

        public Form1()
        {
            InitializeComponent();

            threadBox.Items.AddRange(Enum.GetNames(typeof(ThreadPriority)));
            threadBox.SelectedItem = ThreadPriority.Highest.ToString();

            Log.LogBox = richTextBox1;
            this.FormClosing += Form1_FormClosing;

            chart_TestData.Series.Clear();
            chart_TestData.Series.Add(new Series() { Color = Color.Red, ChartType = SeriesChartType.Line });
            chart_TestData.Series.Add(new Series() { Color = Color.Blue, ChartType = SeriesChartType.Line });

            var area = chart_TestData.ChartAreas.First();
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 1;

            dataInterpreters.Add("GSR Raw", (dat) =>
            {
                List<long> xs = dat.gsrData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.gsrData.Select(x => (double)x.resistance).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("HR Raw", (dat) =>
            {
                List<long> xs = dat.hrData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.hrData.Select(x => (double)x.signal).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("HR IBI", (dat) =>
            {
                List<long> xs = dat.hrData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.hrData.Select(x => (double)x.IBI).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("HR BPM", (dat) =>
            {
                List<long> xs = dat.hrData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.hrData.Select(x => (double)x.BPM).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("EEG F3 Raw", (dat) =>
            {
                List<long> xs = dat.eegData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.eegData.Select(x => (double)x.data["F3"]).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("EEG F4 Raw", (dat) =>
            {
                List<long> xs = dat.eegData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.eegData.Select(x => (double)x.data["F4"]).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("EEG AF3 Raw", (dat) =>
            {
                List<long> xs = dat.eegData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.eegData.Select(x => (double)x.data["AF3"]).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("EEG AF4 Raw", (dat) =>
            {
                List<long> xs = dat.eegData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.eegData.Select(x => (double)x.data["F3"]).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("EEG F3-F4 Raw", (dat) =>
            {
                List<long> xs = dat.eegData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.eegData.Select(x => x.data["F3"] - x.data["F4"]).ToList();
                return Tuple.Create(xs, ys);
            });

            dataInterpreters.Add("EEG AF3-AF4 Raw", (dat) =>
            {
                List<long> xs = dat.eegData.Select(x => x.timestamp).ToList();
                List<double> ys = dat.eegData.Select(x => x.data["AF3"] - x.data["AF4"]).ToList();
                return Tuple.Create(xs, ys);
            });

            foreach (var item in dataInterpreters)
            {
                cmb_PlotDataType.Items.Add(item.Key);
            }

            cmb_PlotDataType.Text = (string)cmb_PlotDataType.Items[0];

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (gsrThread != null)
            {
                gsrThread.Abort();
            }

            if (eegThread != null)
            {
                eegThread.Abort();
            }

            if (faceThread != null)
            {
                faceThread.Abort();
            }
            if (hrThread != null)
            {
                hrThread.Abort();
            }
            if (eh != null)
            {
                eh.Save();
                eh.CloseBooks();
            }
            DataReading.kill = true;
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
            if (!svmConfs.Contains(conf))
            {
                this.Invoke((MethodInvoker)delegate { svmConfs.Add(conf); });
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

            File.WriteAllText(currentPath + @"\META\" + conf.Name + ".svm", conf.Serialize());
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

        Thread CreateMachineThread(string name, List<SVMParameter> pars, List<Feature> feats, SAMDataPoint.FeelingModel feels, Action<int, int> UpdateCallback, bool useControlSAM)
        {
            return new Thread(() =>
            {
                StdClassifier mac = new StdClassifier(name, pars, feats, samData);
                mac.UpdateCallback = UpdateCallback;
                var res = mac.OldCrossValidate(feels, 1, useControlSAM);
                SaveBestResult(res, mac.Name + "_" + feels);
            });
        }

        void SaveBestResult(List<PredictionResult> res, string prefix)
        {
            double bestF = 0;
            foreach (var resTemp in res)
            {
                bestF = Math.Max(bestF, resTemp.GetAverageFScore());
            }

            var confs = res.Where(x => x.GetAverageFScore() == bestF);
            foreach (var conf in confs)
            {
                var c = conf.GenerateConfiguration();
                c.Name = prefix + "_" + c.Name;
                SaveConfiguration(c);
            }
        }

        bool LoadData(string path, FusionData fd)
        {
            currentPath = path;
            Log.LogMessage("Selected folder: " + path);
            //load fusion data
            samData = SAMData.LoadFromPath(path + @"\SAM.json");
            string temp = samData.ShouldSkip();
            if (!(temp == ""))
            {
                Log.LogMessage(temp);
                return false;
            }
            shouldRun = fd.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, samData.startTime);
            //Slicing
            List<SAMDataPoint> throwaway = new List<SAMDataPoint>();
            foreach (SAMDataPoint samD in samData.dataPoints)
            {
                if (FeatureCreator.EEGDataSlice(fd.eegData.ToList<DataReading>(), samD).Count == 0 ||
                    FeatureCreator.GSRDataSlice(fd.gsrData.ToList<DataReading>(), samD).Count == 0 ||
                    FeatureCreator.HRDataSlice(fd.hrData.ToList<DataReading>(), samD).Count == 0 ||
                    FeatureCreator.FaceDataSlice(fd.faceData.ToList<DataReading>(), samD).Count == 0)
                {
                    throwaway.Add(samD);
                }
            }

            if (throwaway.Count > 5)
            {
                return false;
            }

            for (int i = 0; i < throwaway.Count; i++)
            {
                Log.LogMessage("Threw away a sam data point");
                samData.dataPoints.Remove(throwaway[i]);
            }
            if (throwaway.Count > 5 && samData.ShouldSkip() == "")
            {
                Log.LogMessage("Too many data points thrown away (" + throwaway.Count + ")");
                return false;
            }
            Log.LogMessage("Fusion Data loaded!");

            Log.LogMessage("Applying data to features..");


            FeatureCreator.GSRArousalOptimizationFeatures.ForEach(x => x.SetData(fd.gsrData.ToList<DataReading>()));
            FeatureCreator.HRArousalOptimizationFeatures.ForEach(x => x.SetData(fd.hrData.ToList<DataReading>()));
            FeatureCreator.HRValenceOptimizationFeatures.ForEach(x => x.SetData(fd.hrData.ToList<DataReading>()));
            FeatureCreator.EEGArousalOptimizationFeatures.ForEach(x => x.SetData(fd.eegData.ToList<DataReading>()));
            FeatureCreator.EEGValenceOptimizationFeatures.ForEach(x => x.SetData(fd.eegData.ToList<DataReading>()));
            FeatureCreator.FACEArousalOptimizationFeatures.ForEach(x => x.SetData(fd.faceData.ToList<DataReading>()));
            FeatureCreator.FACEValenceOptimizationFeatures.ForEach(x => x.SetData(fd.faceData.ToList<DataReading>()));

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
                foreach (var item in files)
                {
                    metaConfs.Add(MetaSVMConfiguration.Deserialize(File.ReadAllText(item)));
                }
            }

            if (svmConfs.Count == 0 && metaConfs.Count == 0)
            {
                Log.LogMessage("No configurations found, maybe you should run some optimizations on some features.");
            }
            return true;
        }


        #endregion

        #region [Forms Events]
        volatile int gsrProg = 0;
        volatile int gsrTot = 1;
        volatile int hrProg = 0;
        volatile int hrTot = 1;
        volatile int eegProg = 0;
        volatile int eegTot = 1;
        volatile int faceProg = 0;
        volatile int faceTot = 1;


        //Debug purposes
        private bool skipGSR = false;
        private bool skipEEG = false;
        private bool skipFace = false;
        private bool skipHR = false;

        private ThreadPriority threadPrio = ThreadPriority.Normal;

        ExcelHandler eh;
        Thread gsrThread = null;
        Thread hrThread = null;
        Thread eegThread = null;
        Thread faceThread = null;
        Dictionary<string, bool> shouldRun;

        private void btn_RunAll_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                chk_useControlValues.Enabled = false;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Log.LogMessage("Starting Stopwatch");
                List<SAMDataPoint.FeelingModel> feelings = new List<SAMDataPoint.FeelingModel>()
                {
                    SAMDataPoint.FeelingModel.Arousal2High,
                    SAMDataPoint.FeelingModel.Arousal2Low,
                    SAMDataPoint.FeelingModel.Arousal3,
                    SAMDataPoint.FeelingModel.Valence2Low,
                    SAMDataPoint.FeelingModel.Valence2High,
                    SAMDataPoint.FeelingModel.Valence3
                };

                eh = new ExcelHandler(fbd.SelectedPath);
                if (!eh.BooksOpen)
                {
                    Log.LogMessage("Cannot open or write to books");
                    return;
                }

                var dataFolders = Directory.GetDirectories(fbd.SelectedPath);
                List<SVMParameter> parameters = GenerateSVMParameters();

                //Debug param
                /* List<SVMParameter> parameters = new List<SVMParameter> { new SVMParameter() };
                 parameters[0].C = 32;
                 parameters[0].Gamma = 0.25;
                 parameters[0].Kernel = SVMKernelType.SIGMOID;*/


                int curDat = 1;
                int maxDat = dataFolders.Length;

                foreach (var item in dataFolders)
                {
                    if (item.Split('\\').Last() == "Stats")
                    {
                        Log.LogMessage("Stats folder skipping");
                        continue;
                    }
                    DataProgressHandler DPH = new DataProgressHandler(item);
                    if (DPH.AllDone)
                    {
                        Log.LogMessage("Already did " + item + ", skipping..");
                        curDat++;
                        continue;
                    }
                    if (!LoadData(item, _fd))
                    {
                        Log.LogMessage(item.Split('-').Last() + " is not classifiable");
                        continue;
                    }


                    string personName = item.Split('\\').Last();
                    eh.AddPersonToBooks(personName);

                    foreach (var feel in feelings)
                    {
                        statusLabel.Text = "STANDARD: " + curDat + "/" + maxDat + " -> " + feel + " -> " + item.Split('\\').Last();

                        gsrProg = 0;
                        gsrTot = 1;
                        hrProg = 0;
                        hrTot = 1;
                        eegProg = 0;
                        eegTot = 1;
                        faceProg = 0;
                        faceTot = 1;

                        bool gsrWrite = false;
                        bool hrWrite = false;
                        bool eegWrite = false;
                        bool faceWrite = false;

                        if (feel != SAMDataPoint.FeelingModel.Valence2High &&
                            feel != SAMDataPoint.FeelingModel.Valence2Low &&
                            feel != SAMDataPoint.FeelingModel.Valence3 &&
                            !skipGSR &&
                            !DPH.done["GSR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] &&
                            shouldRun["GSR.dat"])
                        {
                            gsrThread = CreateMachineThread("GSR", parameters, FeatureCreator.GSRArousalOptimizationFeatures, feel, (cur, max) => { gsrProg = cur; gsrTot = max; }, chk_useControlValues.Checked);
                            gsrThread.Priority = threadPrio;
                            gsrThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("GSR skipping");
                        }

                        if (!DPH.done["HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] && !skipHR && shouldRun["HR.dat"])
                        {
                            hrThread = CreateMachineThread("HR", parameters,
                                (feel == SAMDataPoint.FeelingModel.Valence2High || feel == SAMDataPoint.FeelingModel.Valence2Low || feel == SAMDataPoint.FeelingModel.Valence3)
                                    ? FeatureCreator.HRValenceOptimizationFeatures : FeatureCreator.HRArousalOptimizationFeatures,
                                feel, (cur, max) => { hrProg = cur; hrTot = max; }, chk_useControlValues.Checked);
                            hrThread.Priority = threadPrio;
                            hrThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("HR skipping");
                        }

                        if (!DPH.done["EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] && !skipEEG && shouldRun["EEG.dat"])
                        {
                            eegThread = CreateMachineThread("EEG", parameters,
                                 (feel == SAMDataPoint.FeelingModel.Valence2High || feel == SAMDataPoint.FeelingModel.Valence2Low || feel == SAMDataPoint.FeelingModel.Valence3)
                                    ? FeatureCreator.EEGValenceOptimizationFeatures : FeatureCreator.EEGArousalOptimizationFeatures,
                                 feel, (cur, max) => { eegProg = cur; eegTot = max; }, chk_useControlValues.Checked);
                            eegThread.Priority = threadPrio;
                            eegThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("EEG skipping");
                        }

                        if (!DPH.done["Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] && !skipFace && shouldRun["KINECT.dat"])
                        {
                            faceThread = CreateMachineThread("FACE", parameters,
                                                             (feel == SAMDataPoint.FeelingModel.Valence2High || feel == SAMDataPoint.FeelingModel.Valence2Low || feel == SAMDataPoint.FeelingModel.Valence3)
                                                                ? FeatureCreator.FACEValenceOptimizationFeatures : FeatureCreator.FACEArousalOptimizationFeatures,
                                                             feel, (cur, max) => { faceProg = cur; faceTot = max; }, chk_useControlValues.Checked);
                            faceThread.Priority = threadPrio;
                            faceThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("Face skipping");
                        }



                        List<SVMConfiguration> confs = new List<SVMConfiguration>();
                        SVMConfiguration gsrConf;
                        SVMConfiguration eegConf;
                        SVMConfiguration hrConf;
                        SVMConfiguration faceConf;

                        while ((gsrThread != null && gsrThread.IsAlive) || (hrThread != null && hrThread.IsAlive) || (eegThread != null && eegThread.IsAlive) || (faceThread != null && faceThread.IsAlive))
                        {
                            Thread.Sleep(1000);
                            eegBar.Value = (int)(((double)eegProg / eegTot) * 100);
                            gsrBar.Value = (int)(((double)gsrProg / gsrTot) * 100);
                            faceBar.Value = (int)(((double)faceProg / faceTot) * 100);
                            hrBar.Value = (int)(((double)hrProg / hrTot) * 100);
                            Application.DoEvents();

                            if (gsrThread != null && !gsrThread.IsAlive && !gsrWrite)
                            {
                                gsrWrite = true;
                                gsrConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("GSR") && x.Name.Contains(feel.ToString()));
                                confs.Add(gsrConf);
                                var gsrMac = new StdClassifier(gsrConf, samData);
                                var gsrRes = gsrMac.OldCrossValidate(feel, 1);
                                Log.LogMessage("Best result for person " + curDat + " GSR " + feel + " is " + gsrRes[0].GetAccuracy());

                                eh.AddDataToPerson(personName, ExcelHandler.Book.GSR, gsrRes.First(), feel);
                                DPH.done["GSR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] = true;
                                DPH.SaveProgress();
                                gsrThread = null;


                            }

                            if (hrThread != null && !hrThread.IsAlive && !hrWrite)
                            {
                                hrWrite = true;
                                hrConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("HR") && x.Name.Contains(feel.ToString()));
                                confs.Add(hrConf);
                                var hrMac = new StdClassifier(hrConf, samData);
                                var hrRes = hrMac.OldCrossValidate(feel, 1);
                                Log.LogMessage("Best result for person " + curDat + " HR " + feel + " is " + hrRes[0].GetAccuracy());

                                eh.AddDataToPerson(personName, ExcelHandler.Book.HR, hrRes.First(), feel);
                                DPH.done["HR" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] = true;
                                DPH.SaveProgress();
                                hrThread = null;
                            }

                            if (eegThread != null && !eegThread.IsAlive && !eegWrite)
                            {
                                eegWrite = true;
                                eegConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("EEG") && x.Name.Contains(feel.ToString()));
                                confs.Add(eegConf);
                                var eegMac = new StdClassifier(eegConf, samData);
                                var eegRes = eegMac.OldCrossValidate(feel, 1);
                                Log.LogMessage("Best result for person " + curDat + " EEG " + feel + " is " + eegRes[0].GetAccuracy());

                                eh.AddDataToPerson(personName, ExcelHandler.Book.EEG, eegRes.First(), feel);
                                DPH.done["EEG" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] = true;
                                DPH.SaveProgress();
                                eegThread = null;
                            }

                            if (faceThread != null && !faceThread.IsAlive && !faceWrite)
                            {
                                faceWrite = true;
                                faceConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("FACE") && x.Name.Contains(feel.ToString()));
                                confs.Add(faceConf);
                                var faceMac = new StdClassifier(faceConf, samData);
                                var faceRes = faceMac.OldCrossValidate(feel, 1);
                                Log.LogMessage("Best result for person " + curDat + " Face " + feel + " is " + faceRes[0].GetAccuracy());

                                eh.AddDataToPerson(personName, ExcelHandler.Book.FACE, faceRes.First(), feel);
                                DPH.done["Face" + Enum.GetName(typeof(SAMDataPoint.FeelingModel), feel)] = true;
                                DPH.SaveProgress();
                                faceThread = null;
                            }
                        }

                        Log.LogMessage("Done with single machine searching.");

                        foreach (var cnf in confs)
                        {
                            Log.LogMessage("Saving " + cnf.Name + "...");
                        }
                        //Write normal results
                        eh.Save();
                        Log.LogMessage("Total time: " + stopwatch.Elapsed + " Current person: " + curDat + " and model " + feel.ToString());


                    }

                    curDat++;
                    eh.Save();
                    Log.LogMessage("DonnoDK");
                }
                eh.CloseBooks();
                Log.LogMessage("Closing books and saving");
                Log.LogMessage("Done in: " + stopwatch.Elapsed);
            }

        }

        List<StdClassifier> ConfigurationsToStds(List<SVMConfiguration> confs)
        {
            return confs.Select((x) =>
            {
                StdClassifier cls = new StdClassifier(x, samData);
                return cls;
            }).ToList();
        }

        #endregion

        volatile int metaProg = 0;
        volatile int metaMax = 1;
        private void btn_metaAll_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                chk_useControlValues.Enabled = false;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Log.LogMessage("Starting Stopwatch");
                List<SAMDataPoint.FeelingModel> feelings = new List<SAMDataPoint.FeelingModel>()
                {
                    SAMDataPoint.FeelingModel.Arousal2High,
                    SAMDataPoint.FeelingModel.Arousal2Low,
                    SAMDataPoint.FeelingModel.Arousal3,
                    SAMDataPoint.FeelingModel.Valence2Low,
                    SAMDataPoint.FeelingModel.Valence2High,
                    SAMDataPoint.FeelingModel.Valence3
                };

                eh = new ExcelHandler(fbd.SelectedPath);
                if (!eh.BooksOpen)
                {
                    Log.LogMessage("Cannot open or write to books");
                    return;
                }


                var dataFolders = Directory.GetDirectories(fbd.SelectedPath);
                List<SVMParameter> parameters = GenerateSVMParameters();

                //List<SVMParameter> parameters = new List<SVMParameter> { new SVMParameter() };


                int curDat = 1;
                int maxDat = dataFolders.Length;

                foreach (var item in dataFolders)
                {
                    if (item.Split('\\').Last() == "Stats")
                    {
                        Log.LogMessage("Stats folder skipping");
                        continue;
                    }
                    DataProgressHandler DPH = new DataProgressHandler(item);
                    if (DPH.MetaDone)
                    {
                        Log.LogMessage("Already did " + item + ", skipping..");
                        curDat++;
                        continue;
                    }

                    string personName = item.Split('\\').Last();
                    eh.AddPersonToBooks(personName);

                    LoadData(item, _fd);
                    foreach (var feel in feelings)
                    {
                        statusLabel.Text = "META: " + curDat + "/" + maxDat + " -> " + feel + " -> " + item.Split('\\').Last();
                        List<SVMConfiguration> confs = new List<SVMConfiguration>();
                        SVMConfiguration gsrConf;
                        SVMConfiguration eegConf;
                        SVMConfiguration hrConf;
                        SVMConfiguration faceConf;

                        gsrConf = svmConfs.OfType<SVMConfiguration>().FirstOrDefault((x) => x.Name.StartsWith("GSR") && x.Name.Contains(feel.ToString()));
                        hrConf = svmConfs.OfType<SVMConfiguration>().FirstOrDefault((x) => x.Name.StartsWith("HR") && x.Name.Contains(feel.ToString()));
                        eegConf = svmConfs.OfType<SVMConfiguration>().FirstOrDefault((x) => x.Name.StartsWith("EEG") && x.Name.Contains(feel.ToString()));
                        faceConf = svmConfs.OfType<SVMConfiguration>().FirstOrDefault((x) => x.Name.StartsWith("FACE") && x.Name.Contains(feel.ToString()));

                        if (gsrConf != null)
                        {
                            confs.Add(gsrConf);
                        }

                        if (eegConf != null)
                        {
                            confs.Add(eegConf);
                        }

                        if (hrConf != null)
                        {
                            confs.Add(hrConf);
                        }

                        if (faceConf != null)
                        {
                            confs.Add(faceConf);
                        }

                        Log.LogMessage("Creating meta machine..");


                        MetaClassifier meta = new MetaClassifier("Stacking", parameters, samData, ConfigurationsToStds(confs));
                        prg_meta.Minimum = 0;


                        meta.UpdateCallback = (cur, max) => { metaProg = cur; metaMax = max; };

                        if (!DPH.done["Voting" + feel])
                        {
                            Thread tVote = new Thread(() =>
                            {
                                var voteRes = meta.DoVoting(feel, 1, chk_useControlValues.Checked);
                                eh.AddDataToPerson(personName, ExcelHandler.Book.Voting, voteRes, feel);
                                Log.LogMessage("Voting on " + feel + " gave " + voteRes.GetAccuracy());
                                DPH.done["Voting" + feel] = true;
                                DPH.SaveProgress();
                            });
                            tVote.Priority = threadPrio;
                            Log.LogMessage("Doing voting");
                            tVote.Start();
                            while (tVote != null && tVote.IsAlive)
                            {
                                Thread.Sleep(500);
                                prg_meta.Maximum = metaMax;
                                prg_meta.Value = metaProg;
                                prg_meta_txt.Text = "Voting: " + metaProg + " / " + metaMax;
                                Application.DoEvents();
                            }
                            eh.Save();
                        }


                        if (!DPH.done["Stacking" + feel])
                        {
                            Thread tStack = new Thread(() =>
                            {
                                var res = meta.DoStacking(feel, 1, chk_useControlValues.Checked);
                                var bestRes = meta.FindBestFScorePrediction(res);
                                eh.AddDataToPerson(personName, ExcelHandler.Book.Stacking, bestRes, feel);
                                Log.LogMessage("Stacking on " + feel + " gave " + bestRes.GetAccuracy());
                                DPH.done["Stacking" + feel] = true;
                                DPH.SaveProgress();
                                meta.Parameters = new List<SVMParameter>() { bestRes.svmParams };
                                SaveConfiguration(meta.GetConfiguration());
                            });
                            tStack.Priority = threadPrio;
                            Log.LogMessage("Doing Stacking");
                            tStack.Start();
                            while (tStack != null && tStack.IsAlive)
                            {
                                Thread.Sleep(500);
                                prg_meta.Maximum = metaMax;
                                prg_meta.Value = metaProg;
                                prg_meta_txt.Text = "Stacking: " + metaProg + " / " + metaMax;
                                Application.DoEvents();
                            }
                            eh.Save();
                        }
                    }

                    curDat++;
                    eh.Save();
                    DPH.SaveProgress();
                }
                eh.CloseBooks();
                Log.LogMessage("Closing books and saving");
                Log.LogMessage("Done in: " + stopwatch.Elapsed);
            }

        }

        private void threadBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            threadPrio = (ThreadPriority)Enum.Parse(typeof(ThreadPriority), threadBox.SelectedItem.ToString());
        }

        private void prg_meta_Click(object sender, EventArgs e)
        {
            Log.LogMessage("Don't click the progress bar pls");
        }

        #region MERGING
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_excel_add_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = true;
            fd.Filter = "xlsx|*.xlsx";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                foreach (var item in fd.FileNames)
                {
                    lst_excel_files.Items.Add(item);
                }
            }
        }

        private void lst_excel_files_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lst_excel_files.SelectedItems.Count > 0)
            {
                List<string> tmp = new List<string>(lst_excel_files.SelectedItems.Cast<string>().ToList());

                foreach (var xlsx in tmp)
                {
                    lst_excel_files.Items.Remove(xlsx);
                }
            }
        }

        private void btn_excel_merge_Click(object sender, EventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = "xlsx|*.xlsx";

            if (sd.ShowDialog() == DialogResult.OK)
            {
                Excel.Application exc = new Excel.Application() { Visible = false };

                //group excel files by name
                Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();
                foreach (var file in lst_excel_files.Items.Cast<string>())
                {
                    string group = file.Split('\\').Last();

                    if (!fileGroups.ContainsKey(group))
                    {
                        fileGroups.Add(group, new List<string>() { file });
                    }
                    else
                    {
                        fileGroups[group].Add(file);
                    }
                }

                //merge each group
                foreach (var item in fileGroups)
                {
                    Log.LogMessage("Merging " + item.Key);
                    Excel.Workbook merged = exc.Workbooks.Add(ExcelHandler.missingValue);
                    ExcelHandler.CreateStandardBookSetup(merged);

                    foreach (var path in item.Value)
                    {
                        Excel.Workbook current = exc.Workbooks.Open(path, ExcelHandler.missingValue,
                                                                false,
                                                                ExcelHandler.missingValue,
                                                                ExcelHandler.missingValue,
                                                                ExcelHandler.missingValue,
                                                                true,
                                                                ExcelHandler.missingValue,
                                                                ExcelHandler.missingValue,
                                                                true,
                                                                ExcelHandler.missingValue,
                                                                ExcelHandler.missingValue,
                                                                ExcelHandler.missingValue);

                        foreach (Excel.Worksheet sheit in current.Sheets)
                        {
                            if (sheit.Name == "First" || sheit.Name == "Last" || sheit.Name == "Overview")
                            {
                                continue;
                            }

                            //look away pls
                            try
                            {
                                var existCheck = merged.Sheets[sheit.Name];
                            }
                            catch
                            {
                                sheit.Copy(merged.Sheets["Last"]);
                            }
                        }

                        current.Close();
                    }

                    string savePath = sd.FileName;
                    savePath = savePath.Replace(savePath.Split('\\').Last(), "");

                    savePath += item.Key;

                    merged.Sheets["Sheet1"].Delete();
                    merged.SaveCopyAs(savePath);
                    merged.Close(false);
                }
                exc.Quit();
                Log.LogMessage("Closing Excel");
            }
        }

        private void btn_Anova_Click(object sender, EventArgs e)
        {
            var files = lst_excel_files.Items.Cast<string>().ToList();
            if (files.Count(x => x.Contains("Stacking")) == 0 ||
                files.Count(x => x.Contains("Voting")) == 0 ||
                files.Count(x => x.Contains("GSR")) == 0 ||
                files.Count(x => x.Contains("HR")) == 0 ||
                files.Count(x => x.Contains("EEG")) == 0 ||
                files.Count(x => x.Contains("FACE")) == 0)
            {
                Log.LogMessage("You must use all data files");
                return;
            }

            SaveFileDialog sd = new SaveFileDialog();
            sd.FileName = "anova.xlsx";
            sd.Filter = "xlsx|*.xlsx";

            //feeling type < test subject <  sensor type < value >>>
            var values = new Dictionary<SAMDataPoint.FeelingModel, Dictionary<string, Dictionary<string, double>>>();
            foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
            {
                values.Add(feel, new Dictionary<string, Dictionary<string, double>>());
            }


            if (sd.ShowDialog() == DialogResult.OK)
            {
                Log.LogMessage("Starting Excel");
                Excel.Application exc = new Excel.Application() { Visible = false };
                List<string> skipped = new List<string>();

                foreach (var path in files)
                {
                    string fileType = path.Split('\\').Last().Split('.').First();
                    Excel.Workbook current = exc.Workbooks.Open(path, ExcelHandler.missingValue,
                                                            false,
                                                            ExcelHandler.missingValue,
                                                            ExcelHandler.missingValue,
                                                            ExcelHandler.missingValue,
                                                            true,
                                                            ExcelHandler.missingValue,
                                                            ExcelHandler.missingValue,
                                                            true,
                                                            ExcelHandler.missingValue,
                                                            ExcelHandler.missingValue,
                                                            ExcelHandler.missingValue);

                    foreach (Excel.Worksheet sheit in current.Sheets)
                    {
                        if (sheit.Name == "First" || sheit.Name == "Last" || sheit.Name == "Overview")
                        {
                            continue;
                        }

                        string tpName = "tp" + sheit.Name.Split(' ').Last();

                        var accuracies = new Dictionary<SAMDataPoint.FeelingModel, double>();
                        foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                        {
                            accuracies.Add(feel, -1);
                        }

                        #region TryValues
                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Arousal2High] = sheit.Cells[2, 3].Value;
                        }
                        catch { };

                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Arousal2Low] = sheit.Cells[15, 3].Value;
                        }
                        catch { };

                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Arousal3] = sheit.Cells[28, 3].Value;
                        }
                        catch { };

                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Valence2High] = sheit.Cells[44, 3].Value;
                        }
                        catch { };
                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Valence2Low] = sheit.Cells[57, 3].Value;
                        }
                        catch { };
                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Valence3] = sheit.Cells[70, 3].Value;
                        }
                        catch { };
                        #endregion

                        foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                        {
                            if (accuracies[feel] == -1)
                            {
                                //if (fileType == "GSR" || !feel.ToString().Contains("Valence"))
                                //{
                                //    skipped.Add(feel + tpName);
                                //}

                                //continue;
                            }

                            if (!values[feel].ContainsKey(tpName))
                            {
                                values[feel].Add(tpName, new Dictionary<string, double>());
                            }

                            if (!values[feel][tpName].ContainsKey(fileType))
                            {
                                values[feel][tpName].Add(fileType, accuracies[feel]);
                            }
                        }
                    }

                    current.Close();
                }


                Dictionary<string, int> columns = new Dictionary<string, int>()
                {
                    ["Stacking"] = 0,
                    ["Voting"] = 1,
                    ["EEG"] = 2,
                    ["HR"] = 3,
                    ["FACE"] = 4,
                    ["GSR"] = 5,
                };
                foreach (var feelValue in values)
                {
                    Excel.Workbook anova = exc.Workbooks.Add(ExcelHandler.missingValue);
                    Dictionary<int, List<double>> anovals = new Dictionary<int, List<double>>();

                    foreach (var testSubject in feelValue.Value)
                    {
                        //if (skipped.Contains(feelValue.Key + testSubject.Key)) continue;
                        bool skip = false;
                        foreach (var sensorType in testSubject.Value)
                        {
                            if (sensorType.Value < 0 && !(sensorType.Key == "GSR" && feelValue.Key.ToString().Contains("Valence")))
                            {
                                skip = true;
                                skipped.Add(testSubject.Key + "_" + feelValue.Key + "_" + sensorType.Key);
                            }
                        }

                        if (skip) continue;

                        foreach (var sensorType in testSubject.Value)
                        {
                            if (!anovals.ContainsKey(columns[sensorType.Key]))
                            {
                                anovals.Add(columns[sensorType.Key], new List<double>());
                            }

                            anovals[columns[sensorType.Key]].Add(sensorType.Value);
                        }
                    }

                    anova.Sheets[1].Cells[1, 1] = "Machine";
                    anova.Sheets[1].Cells[1, 2] = "DataPoint";

                    int currentRow = 2;
                    foreach (var item in anovals)
                    {
                        for (int i = 0; i < item.Value.Count; i++)
                        {
                            anova.Sheets[1].Cells[currentRow, 1] = item.Key;
                            anova.Sheets[1].Cells[currentRow, 2] = item.Value[i];
                            currentRow++;
                        }
                    }

                    anova.SaveCopyAs(sd.FileName.Replace(".xlsx", $"_{feelValue.Key.ToString()}.xlsx"));
                    Log.LogMessage("Done with " + feelValue.Key);
                    anova.Close(false);

                }

                File.WriteAllLines(sd.FileName.Replace(".xlsx", "_skipped.txt"), skipped);


                exc.Quit();
                Log.LogMessage("Closing Excel");
            }
        }

        #endregion

        #region Plotting
        private void btn_ExportPNG_Click(object sender, EventArgs e)
        {
            if (loaded.Count < 2)
            {
                Log.LogMessage("You must load all data before exporting!");
                return;
            }

            int height = 0;
            if (!int.TryParse(txt_height.Text, out height))
            {
                Log.LogMessage("Height must be an integer");
                return;
            }

            int width = 0;
            if (!int.TryParse(txt_width.Text, out width))
            {
                Log.LogMessage("Width must be an integer");
                return;
            }

            int offset = 0;
            if (!int.TryParse(txt_PlotDataOffset.Text, out offset))
            {
                Log.LogMessage("Offset must be an integer");
                return;
            }

            double pointSize = 0;
            if (!double.TryParse(txt_PlotPointSize.Text.Replace(".", ","), out pointSize))
            {
                Log.LogMessage("Point size must be a double");
                return;
            }

            int from = -1;
            if (!int.TryParse(txt_ExportFrom.Text, out from) || txt_ExportFrom.Text == "")
            {
                Log.LogMessage("Export from not valid integer, using 0");
                from = -1;
            }

            int to = -1;
            if (!int.TryParse(txt_ExportTo.Text, out to) || txt_ExportTo.Text == "")
            {
                Log.LogMessage("Export to not valid integer, using max");
                to = -1;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.Filter = "png|*.png";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PngExporter pngify = new PngExporter();
                pngify.Width = width;
                pngify.Height = height;

                var xy = GetXY(from, to);

                var test = xy.Item1;
                var recall = xy.Item2;

                FitPlot(test, recall);

                var model = new PlotModel() { Title = $"Slope:{slope.ToString("0.000")} RSquared:{rsquared.ToString("0.000")}" };
                var dataSeries = new OxyPlot.Series.ScatterSeries() { MarkerType = MarkerType.Circle, MarkerStroke = OxyColors.Red };
                for (int i = 0; i < test.Count; i++)
                {
                    dataSeries.Points.Add(new OxyPlot.Series.ScatterPoint(test[i], recall[i]) { Size = pointSize });
                }

                var fitSeries = new OxyPlot.Series.FunctionSeries((x) => intercept + slope * x, test.Min(), test.Max(), 0.001) { Color = OxyColors.Blue };

                model.Series.Add(dataSeries);
                model.Series.Add(fitSeries);

                model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Left });
                model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom });


                pngify.ExportToFile(model, sfd.FileName);
                Log.LogMessage("Saved " + sfd.FileName + "!");
            }
        }

        private void btn_PlotExportExcel_Click(object sender, EventArgs e)
        {
            if (loaded.Count < 2)
            {
                Log.LogMessage("You must load all data before exporting!");
                return;
            }

            int from = -1;
            if (!int.TryParse(txt_ExportFrom.Text, out from) || txt_ExportFrom.Text == "")
            {
                Log.LogMessage("Export from not valid integer, using 0");
                from = -1;
            }

            int to = -1;
            if (!int.TryParse(txt_ExportTo.Text, out to) || txt_ExportTo.Text == "")
            {
                Log.LogMessage("Export to not valid integer, using max");
                to = -1;
            }

            int offset = 0;
            if (!int.TryParse(txt_PlotDataOffset.Text, out offset))
            {
                Log.LogMessage("Offset must be an integer");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".xlsx";
            sfd.Filter = "xlsx|*.xlsx";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var xy = GetXY(from, to);

                var test = xy.Item1;
                var recall = xy.Item2;

                FitPlot(test, recall);

                Excel.Application exc = new Excel.Application() { Visible = false };

                Excel.Workbook dataBook = exc.Workbooks.Add(ExcelHandler.missingValue);
                //Excel.Workbook dataBook = exc.Workbooks.Open(sfd.FileName, ExcelHandler.missingValue,
                //                                                    false,
                //                                                    ExcelHandler.missingValue,
                //                                                    ExcelHandler.missingValue,
                //                                                    ExcelHandler.missingValue,
                //                                                    true,
                //                                                    ExcelHandler.missingValue,
                //                                                    ExcelHandler.missingValue,
                //                                                    true,
                //                                                    ExcelHandler.missingValue,
                //                                                    ExcelHandler.missingValue,
                //                                                    ExcelHandler.missingValue);

                Excel.Worksheet sheet = dataBook.Sheets["Sheet1"];

                sheet.Cells[1, 1] = "Test";
                sheet.Cells[1, 2] = "Recall";

                Log.LogMessage("Data count: " + test.Count);
                Log.LogMessage("Currently : ");
                var timer = Stopwatch.StartNew();
                for (int i = 0; i < test.Count; i++)
                {
                    sheet.Cells[i + 2, 1] = test[i];
                    sheet.Cells[i + 2, 2] = recall[i];
                    if (timer.ElapsedMilliseconds > 1000)
                    {
                        Log.LogMessageSameLine("Currently: " + i);
                        timer.Restart();
                        Application.DoEvents();
                    }
                }
                Log.LogMessageSameLine("Currently: " + test.Count);
                sheet.Name = "data";

                dataBook.SaveCopyAs(sfd.FileName);
                dataBook.Close(false);
                exc.Quit();

                Log.LogMessage("Saved " + sfd.FileName + "!");
            }
        }

        double intercept = 0;
        double slope = 0;
        double rsquared = 0;
        void FitPlot(List<double> xs, List<double> ys)
        {
            if (xs.Count > ys.Count)
            {
                xs = xs.Take(ys.Count).ToList();
            }
            else if (ys.Count > xs.Count)
            {
                ys = ys.Take(xs.Count).ToList();
            }

            var fit = MathNet.Numerics.Fit.Line(xs.ToArray(), ys.ToArray());
            intercept = fit.Item1;
            slope = fit.Item2;
            rsquared = MathNet.Numerics.GoodnessOfFit.RSquared(xs.Select(x => intercept + slope * x), ys);

            txt_intercept.Text = intercept.ToString("0.000");
            txt_slope.Text = slope.ToString("0.000");
            txt_rsquared.Text = rsquared.ToString("0.000");
        }

        private void btn_PlotLoadTest_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                fdTest.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);
                txt_TestDataName.Text = fbd.SelectedPath.Split('\\').Last();

                var xs = fdTest.gsrData.Select(x => x.timestamp).ToList();
                var ys = fdTest.gsrData.Select(y => (double)y.resistance).ToList();

                LoadDataSeries(xs, ys, 0);
            }
        }

        private void btn_PlotLoadRecall_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                fdRecall.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);
                txt_RecallDataName.Text = fbd.SelectedPath.Split('\\').Last();

                var xs = fdRecall.gsrData.Select(x => x.timestamp).ToList();
                var ys = fdRecall.gsrData.Select(y => (double)y.resistance).ToList();

                LoadDataSeries(xs, ys, 1);
            }
        }

        List<int> loaded = new List<int>();
        void LoadDataSeries(List<long> xs, List<double> ys, int seriesId)
        {
            var ser = chart_TestData.Series[seriesId];
            ser.Points.Clear();

            long first = xs[0];
            for (int i = 0; i < xs.Count; i++)
            {
                xs[i] -= first;
            }

            double max = ys.Max();
            for (int i = 0; i < ys.Count; i++)
            {
                ser.Points.AddXY(xs[i], ys[i] / max);
            }

            scroll_PlotView.Maximum = (int)xs.Last() - chartMsToShow;
            UpdateChart();
            if (!loaded.Contains(seriesId))
            {
                loaded.Add(seriesId);
            }
            if (loaded.Count > 1)
            {
                enableFitting = true;
                var xy = GetXY();
                FitPlot(xy.Item1, xy.Item2);
            }
        }

        bool enableFitting = false;
        int chartMsToShow = 20000;
        void UpdateChart()
        {
            var area = chart_TestData.ChartAreas.First();

            area.AxisX.Minimum = scroll_PlotView.Value;
            area.AxisX.Maximum = scroll_PlotView.Value + chartMsToShow;
        }


        private void scroll_PlotView_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateChart();
        }

        int oldOffset = 0;
        void PlotSetNewOffset(int offset)
        {
            var diff = oldOffset - offset;

            var test = chart_TestData.Series[0];
            var recall = chart_TestData.Series[1];

            for (int i = 0; i < recall.Points.Count; i++)
            {
                recall.Points[i].XValue += diff;
            }

            oldOffset = offset;

            if (enableFitting)
            {
                var xy = GetXY();
                FitPlot(xy.Item1, xy.Item2);
            }
        }

        private void txt_PlotDataOffset_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int offset = 0;
                if (!int.TryParse(txt_PlotDataOffset.Text, out offset))
                {
                    Log.LogMessage("Offset must be an integer");
                    return;
                }

                PlotSetNewOffset(offset);
            }
        }

        Tuple<List<double>, List<double>> GetXY(int from = -1, int to = -1)
        {
            var test = chart_TestData.Series[0].Points.ToList();
            var testMax = test.Max(x => x.XValue);

            var recall = chart_TestData.Series[1].Points.Where(p => p.XValue >= 0 && p.XValue <= testMax).ToList();
            var recallMin = recall.Min(p => p.XValue);
            test = test.Where(p => p.XValue >= recallMin).ToList();

            if (from != -1 && to != -1)
            {
                test = test.Where(p => p.XValue >= from && p.XValue <= to).ToList();
                recall = recall.Where(p => p.XValue >= from && p.XValue <= to).ToList();
            }


            if (test.Count > recall.Count)
            {
                test = test.Take(recall.Count).ToList();
            }
            else if (recall.Count > test.Count)
            {
                recall = recall.Take(test.Count).ToList();
            }


            return Tuple.Create(test.Select(p => p.YValues[0]).ToList(), recall.Select(p => p.YValues[0]).ToList());

        }

        Dictionary<string, Func<FusionData, Tuple<List<long>, List<double>>>> dataInterpreters = new Dictionary<string, Func<FusionData, Tuple<List<long>, List<double>>>>();
        private void cmb_PlotDataType_SelectedValueChanged(object sender, EventArgs e)
        {

            if (cmb_PlotDataType.SelectedItem == null || !dataInterpreters.ContainsKey((string)cmb_PlotDataType.SelectedItem))
            {
                Log.LogMessage("Something wrong was selected in the dropdown menu");
                return;
            }

            if (fdTest.Loaded)
            {
                var newData = dataInterpreters[(string)cmb_PlotDataType.SelectedItem](fdTest);
                LoadDataSeries(newData.Item1, newData.Item2, 0);
            }

            if (fdRecall.Loaded)
            {
                var newData = dataInterpreters[(string)cmb_PlotDataType.SelectedItem](fdRecall);
                LoadDataSeries(newData.Item1, newData.Item2, 1);
            }

            Log.LogMessage($"Switched to new data [{(string)cmb_PlotDataType.SelectedItem}]");
        }

        private void txt_PlotWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var oldValue = chartMsToShow;
                if (!int.TryParse(txt_PlotWindow.Text, out chartMsToShow))
                {
                    Log.LogMessage("View window must be integer");
                }
                else if (chartMsToShow <= 10)
                {
                    Log.LogMessage("View window must be >10");
                    chartMsToShow = oldValue;
                }
                else
                {
                    scroll_PlotView.LargeChange = chartMsToShow / 10;
                    scroll_PlotView.SmallChange = chartMsToShow / 100;
                    UpdateChart();
                }
            }
        }

        bool searching = false;
        private void btn_SearchOffset_Click(object sender, EventArgs e)
        {

            if (!enableFitting)
            {
                Log.LogMessage("You must load all data first");
                return;
            }


            int from = 0;
            int to = 0;
            int stepsize = 0;

            if (!int.TryParse(txt_OffsetFrom.Text, out from))
            {
                Log.LogMessage("Offset from must be an integer");
                return;
            }

            if (!int.TryParse(txt_OffsetTo.Text, out to))
            {
                Log.LogMessage("Offset to must be an integer");
                return;
            }

            if (!int.TryParse(txt_OffsetStep.Text, out stepsize))
            {
                Log.LogMessage("offset stepsize must be integer");
                return;
            }

            txt_PlotDataOffset.Enabled = false;

            Log.LogMessage($"Search for offset between {from} and {to}, stepsize {stepsize}...");

            int slopeIdx = 0;
            int rIdx = 0;
            double bestSlope = double.MaxValue;
            double bestR = 0;
            Log.LogMessage("...");
            searching = true;
            btn_StopSearch.Visible = true;
            for (int offset = from; offset <= to && searching; offset += stepsize)
            {
                txt_PlotDataOffset.Text = offset.ToString();

                PlotSetNewOffset(offset);
                var xy = GetXY();
                FitPlot(xy.Item1, xy.Item2);

                var newSlope = Math.Abs(slope - 1);
                if (newSlope < bestSlope)
                {
                    bestSlope = newSlope;
                    slopeIdx = offset;
                }

                if (rsquared > bestR)
                {
                    bestR = rsquared;
                    rIdx = offset;
                }

                Log.LogMessageSameLine($"At {offset}, best slope: {bestSlope.ToString("0.000")} best rsquared: {bestR.ToString("0.000")}");
                Application.DoEvents();
            }
            //Log.LogMessageSameLine($"At {to}, best slope: {bestSlope.ToString("0.000")} best rsquared: {bestR.ToString("0.000")}");

            Log.LogMessage($"Best slope found at offset {slopeIdx}");
            Log.LogMessage($"Best RSquared found was {bestR.ToString("0.000")} at offset {rIdx}");

            txt_PlotDataOffset.Enabled = true;
            searching = false;
            btn_StopSearch.Visible = false;
        }

        private void btn_StopSearch_Click(object sender, EventArgs e)
        {
            searching = false;
            btn_StopSearch.Visible = false;
        }
        #endregion

        private void btn_CalculateResults_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                FolderBrowserDialog sfd = new FolderBrowserDialog() { Description = "Select folder to load excel sheets from" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Excel.Application exc = new Excel.Application() { Visible = false };

                    List<string> singles = new List<string>()
                    {
                        "EEG",
                        "FACE",
                        "GSR",
                        "HR"
                    };

                    List<string> fusion = new List<string>()
                    {
                        "Stacking",
                        "Voting"
                    };

                    Dictionary<SAMDataPoint.FeelingModel, int> cellOffsets = new Dictionary<SAMDataPoint.FeelingModel, int>()
                    {
                        [SAMDataPoint.FeelingModel.Arousal2High] = 11,
                        [SAMDataPoint.FeelingModel.Arousal2Low] = 24,
                        [SAMDataPoint.FeelingModel.Arousal3] = 40,
                        [SAMDataPoint.FeelingModel.Valence2High] = 53,
                        [SAMDataPoint.FeelingModel.Valence2Low] = 66,
                        [SAMDataPoint.FeelingModel.Valence3] = 82
                    };

                    //test subject[SAMModel[machineType]]
                    //items: C, Gamma, Kernel
                    var configs = new Dictionary<string, Dictionary<SAMDataPoint.FeelingModel, Dictionary<string, Tuple<double, double, int>>>>();


                    foreach (var configBook in singles.Concat(fusion))
                    {
                        Log.LogMessage($"Reading {configBook}...");
                        Excel.Workbook dataBook = exc.Workbooks.Open(sfd.SelectedPath + $"/{configBook}.xlsx", ExcelHandler.missingValue,
                                                                    false,
                                                                    ExcelHandler.missingValue,
                                                                    ExcelHandler.missingValue,
                                                                    ExcelHandler.missingValue,
                                                                    true,
                                                                    ExcelHandler.missingValue,
                                                                    ExcelHandler.missingValue,
                                                                    true,
                                                                    ExcelHandler.missingValue,
                                                                    ExcelHandler.missingValue,
                                                                    ExcelHandler.missingValue);

                        foreach (Excel.Worksheet sheet in dataBook.Sheets)
                        {
                            if (sheet.Name == "Overview" || sheet.Name == "First" || sheet.Name == "Last") continue;

                            if (!configs.ContainsKey(sheet.Name))
                            {
                                configs.Add(sheet.Name, new Dictionary<SAMDataPoint.FeelingModel, Dictionary<string, Tuple<double, double, int>>>());
                            }

                            if (configs[sheet.Name].Count == 0)
                            {
                                foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                                {
                                    configs[sheet.Name].Add(feel, new Dictionary<string, Tuple<double, double, int>>());

                                    singles.ForEach(x => configs[sheet.Name][feel].Add(x, null));
                                    fusion.ForEach(x => configs[sheet.Name][feel].Add(x, null));

                                    if (feel.ToString().Contains("Valence"))
                                    {
                                        configs[sheet.Name][feel].Remove("GSR");
                                    }
                                }
                            }


                            foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                            {
                                if (configBook == "GSR" && feel.ToString().Contains("Valence")) continue;

                                Excel.Range c = (Excel.Range)sheet.Cells[cellOffsets[feel], 3];
                                Excel.Range gamma = (Excel.Range)sheet.Cells[cellOffsets[feel] + 1, 3];
                                Excel.Range kernel = (Excel.Range)sheet.Cells[cellOffsets[feel] + 2, 3];

                                if (c.Value != null && gamma.Value != null && kernel.Value != null)
                                {
                                    configs[sheet.Name][feel][configBook] = Tuple.Create(c.Value, gamma.Value, (int)kernel.Value);
                                }

                            }

                        }

                        dataBook.Close();
                    }

                    List<string> skippedList = new List<string>();
                    foreach (var subject in configs)
                    {
                        foreach (var feel in subject.Value)
                        {
                            foreach (var machine in feel.Value)
                            {
                                if (machine.Value == null)
                                {
                                    skippedList.Add(subject.Key);
                                }
                            }
                        }
                    }

                    foreach (var skipped in skippedList)
                    {
                        configs.Remove(skipped);
                        Log.LogMessage($"Skipped {skipped}");
                    }

                    var resultsList = new Dictionary<string, Dictionary<string, List<int>>>();
                    foreach (string feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                    {
                        resultsList.Add(feel, new Dictionary<string, List<int>>());
                        resultsList.Add("SAM", new Dictionary<string, List<int>>());
                        foreach (var item in singles.Concat(fusion))
                        {
                            resultsList[feel].Add(item, new List<int>());
                        }
                    }

                    foreach (var subject in configs)
                    {
                        if (LoadData(fbd.SelectedPath + $"/{subject.Key}", _fd))
                        {
                            foreach (var feel in subject.Value)
                            {
                                resultsList["SAM"][feel.Key.ToString()].AddRange(samData.dataPoints.Select(x => x.ToAVCoordinate(feel.Key)));

                                foreach (var machine in feel.Value)
                                {
                                    Log.LogMessage($"Calculating {subject.Key} / {feel} / {machine}");

                                    var param = new SVMParameter()
                                    {
                                        C = machine.Value.Item1,
                                        Gamma = machine.Value.Item2,
                                        Kernel = (SVMKernelType)machine.Value.Item3
                                    };

                                    if (singles.Contains(machine.Key))
                                    {
                                        var classifier = new StdClassifier(new SVMConfiguration(param, FeatureCreator.GetFeatures(machine.Key, feel.Key)), samData);

                                        var results = classifier.OldCrossValidate(feel.Key, 1);
                                        foreach (var res in results[0].guesses)
                                        {
                                            resultsList[feel.Key.ToString()][machine.Key].Add(res);
                                        }
                                    }
                                    else
                                    {
                                        //var classifier = new MetaClassifier(machine.Key, param, samData)
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.LogMessage($"Skipped {subject.Key} due to bad data");
                            skippedList.Add(subject.Key);
                        }
                    }

                    Excel.Workbook resultBook = exc.Workbooks.Add(ExcelHandler.missingValue);

                    Excel.Worksheet metaSheet = resultBook.Sheets.Add();
                    metaSheet.Name = "Meta";

                    metaSheet.Cells[1, 1] = "Skipped " + skippedList.Count;
                    for (int i = 0; i < skippedList.Count; i++)
                    {
                        metaSheet.Cells[i + 2, 1] = skippedList[i];
                    }

                    foreach (var feel in Enum.GetNames(typeof(SAMDataPoint.FeelingModel)))
                    {
                        Excel.Worksheet feelSheet = resultBook.Sheets.Add();
                        feelSheet.Name = feel;
                    }

                    exc.Quit();
                }
            }
        }

        #region Data Compare
        private void btn_ExportDataCompare_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();



            if (fbd.ShowDialog() == DialogResult.OK)
            {
                int counterino = 1;
                var dirs = Directory.GetDirectories(fbd.SelectedPath);
                foreach (var dirPath in dirs)
                {
                    if (dirPath == "results") continue;
                    List<string> files = new List<string>()
                    {
                        "EEG.dat",
                        //"GSR.dat",
                        //"HR.dat",
                        //"KINECT.dat"
                    };
                    bool runIndividualTasks = true;

                    files.RemoveAll(f => !File.Exists($"{dirPath}/test/{f}") || !File.Exists($"{dirPath}/recall/{f}"));

                    string subject = dirPath.Split('\\').Last();
                    statusLabel.Text = $"{counterino++} / {dirs.Length}";

                    var metaLines = File.ReadAllLines(dirPath + "/meta.txt");
                    fdTest = new FusionData();
                    fdRecall = new FusionData();
                    var fdTestStatus = fdTest.LoadFromFile(files.Select(f => dirPath + "/test/" + f).ToArray());
                    if (!fdTest.Loaded)
                    {
                        //throw new Exception("I crashed because bad loading of fdTest");
                        Log.LogMessage("ERROR in " + dirPath + " couldn't load test data");
                        continue;
                    }
                    var fdRecallStatus = fdRecall.LoadFromFile(files.Select(x => dirPath + "/recall/" + x).ToArray());
                    if (!fdRecall.Loaded)
                    {
                        //throw new Exception("I crashed because bad loading of fdRecall");
                        Log.LogMessage("ERROR in " + dirPath + " couldn't load recall data");
                        continue;
                    }
                    var testEvents = File.ReadAllLines(dirPath + "/test/SecondTest.dat");
                    //fix waiting period offset til at være første event i testevents
                    int offset = int.Parse(metaLines.ToList().First(x => x.StartsWith("sync")).Split(':', '=').Last());
                    string stimul = metaLines[1].Split('=').Last();
                    string time = metaLines[0].Split('=').Last();

                    int waitPeriodDone = int.Parse(testEvents[0].Split('#')[0]);
                    //int firstTwoTasksDone = int.Parse(testEvents[2].Split('#')[0]);
                    int wholePeriodDone = int.Parse(testEvents[testEvents.Length - 1].Split('#')[0]);
                    //int waitPeriodDone = 180000;

                    List<TaskStartEnd> tasks = new List<TaskStartEnd>();

                    if (!runIndividualTasks)
                    {
                        tasks.Add(new TaskStartEnd(waitPeriodDone, wholePeriodDone, "full"));
                    }
                    else
                    {
                        List<string> taskEvents = testEvents.Where(x => x.Contains("TaskWizard - ")).ToList();
                        int start = waitPeriodDone;
                        for (int s = 0; s < taskEvents.Count; s++)
                        {
                            string[] currentEvent = taskEvents[s].Split(new char[] { '-', ' ', '#' }, StringSplitOptions.RemoveEmptyEntries);
                            int end = int.Parse(currentEvent[0]);
                            tasks.Add(new TaskStartEnd(start, end, currentEvent[currentEvent.Length - 1]));
                            start = end;
                        }
                    }

                    for (int ta = 0; ta < tasks.Count; ta++)
                    {
                        string csvTimePath = "csv/Time " + time + "/" + subject + "_" + tasks[ta].filenameAppend + "_";
                        string csvStimuliPath = "csv/Stimuli " + (stimul == "neu" ? "low" : "high") + "/" + subject + "_" + tasks[ta].filenameAppend + "_";
                        //string csvPath = "csv/" + subject + "_";

                        Directory.CreateDirectory("csv/Time " + time);
                        Directory.CreateDirectory("csv/Stimuli " + (stimul == "neu" ? "low" : "high"));

                        if (files.Contains("GSR.dat"))
                        {

                            Log.LogMessage("Starting GSR");

                            var fdTestGsr = fdTest.gsrData.SkipWhile(x => x.timestamp < tasks[ta].start).TakeWhile(x => x.timestamp < tasks[ta].end).Select(x => Tuple.Create(x.timestamp, (double)x.resistance)).ToList();
                            var fdRecallGsr = fdRecall.gsrData.SkipWhile(x => x.timestamp - offset < tasks[ta].start).TakeWhile(x => x.timestamp - offset < tasks[ta].end).Select(x => Tuple.Create(x.timestamp - offset, (double)x.resistance)).ToList();

                            var gsr = FilterData(
                                    fdTestGsr,
                                    fdRecallGsr
                                );

                            if (gsr.Item2.Count != 0 || gsr.Item3.Count != 0)
                            {
                                var gsrNorm = NormalizeFilterData(gsr);
                                var pearsCorr = MathNet.Numerics.Statistics.Correlation.Pearson(gsrNorm.Item1.GetRange(0, Math.Min(gsrNorm.Item1.Count, gsrNorm.Item2.Count)), gsrNorm.Item2.GetRange(0, Math.Min(gsrNorm.Item1.Count, gsrNorm.Item2.Count)));
                                //var nonTemporal = gsrNorm.Item1.Zip(gsrNorm.Item2, (a, b) => Tuple.Create(a, b)).OrderBy(x => x.Item1);
                                //var nonTempA = nonTemporal.Select(x => x.Item1).ToList();
                                //var nonTempB = nonTemporal.Select(x => x.Item2).ToList();

                                SavePng(csvTimePath + "GSR.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: ) - Red = test, blue = recall", gsrNorm.Item1, gsrNorm.Item2);
                                //SavePngScatter(csvTimePath + "GSR_Scatter.png", $"{subject} (Time: {time}, Stim: {stimul})", gsrNorm.Item1, gsrNorm.Item2);
                                SaveZip(csvTimePath + "GSR.csv", gsrNorm.Item1, gsrNorm.Item2);
                                //SavePng(csvTimePath + "GSR_nonTemporal.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", nonTempA, nonTempB);

                                int t;
                                if (int.TryParse(time, out t) && t != 0)
                                {
                                    SavePng(csvStimuliPath + "GSR.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: ) - Red = test, blue = recall", gsrNorm.Item1, gsrNorm.Item2);
                                    //SavePngScatter(csvStimuliPath + "GSR_Scatter.png", $"{subject} (Time: {time}, Stim: {stimul})", gsrNorm.Item1, gsrNorm.Item2);
                                    SaveZip(csvStimuliPath + "GSR.csv", gsrNorm.Item1, gsrNorm.Item2);

                                }
                            }
                            Log.LogMessage("GSR done, data filtered: " + gsr.Item1.ToString("0.0") + "%");
                        }

                        if (files.Contains("HR.dat"))
                        {

                            Log.LogMessage("Starting HR");
                            var hr = FilterData(
                                fdTest.hrData.SkipWhile(x => x.timestamp < tasks[ta].start).TakeWhile(x => x.timestamp < tasks[ta].end).Select(x => Tuple.Create(x.timestamp, (double)x.BPM)).ToList(),
                                fdRecall.hrData.SkipWhile(x => x.timestamp - offset < tasks[ta].start).TakeWhile(x => x.timestamp - offset < tasks[ta].end).Select(x => Tuple.Create(x.timestamp - offset, (double)x.BPM)).ToList(),
                                20
                                );


                            if (hr.Item2.Count != 0 && hr.Item3.Count != 0)
                            {
                                var hrNorm = NormalizeFilterData(hr);
                                var setA = hrNorm.Item1;//.MedianFilter(25);
                                var setB = hrNorm.Item2;//.MedianFilter(25);
                                var pearsCorr = MathNet.Numerics.Statistics.Correlation.Pearson(setA.GetRange(0, Math.Min(setA.Count, setB.Count)), setB.GetRange(0, Math.Min(setA.Count, setB.Count)));
                                SavePng(csvTimePath + "HR.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", hrNorm.Item1, hrNorm.Item2);
                                //SavePngScatter(csvTimePath + "HR_Scatter.png", $"{subject} (Time: {time}, Stim: {stimul})", hrNorm.Item1, hrNorm.Item2);
                                SaveZip(csvTimePath + "HR.csv", setA, setB);

                                int t;
                                if (int.TryParse(time, out t) && t != 0)
                                {
                                    SavePng(csvStimuliPath + "HR.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", hrNorm.Item1, hrNorm.Item2);
                                    //SavePngScatter(csvStimuliPath + "HR_Scatter.png", $"{subject} (Time: {time}, Stim: {stimul})", hrNorm.Item1, hrNorm.Item2);
                                    SaveZip(csvStimuliPath + "HR.csv", hrNorm.Item1, hrNorm.Item2);
                                    //SavePng(csvStimuliPath + "GSR_nonTemporal.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", nonTempA, nonTempB);
                                }
                            }
                            Log.LogMessage($"HR done, data filtered: {hr.Item1.ToString("0.0")}%");
                        }

                        if (files.Contains("EEG.dat"))
                        {
                            Log.LogMessage("Starting EEG");
                            foreach (var item in Enum.GetNames(typeof(EEGDataReading.ELECTRODE)))
                            {
                                var eeg = FilterData(
                                    fdTest.eegData.SkipWhile(x => x.timestamp < tasks[ta].start).TakeWhile(x => x.timestamp < tasks[ta].end).Select(x => Tuple.Create(x.timestamp, (double)x.data[item])).ToList(),
                                    fdRecall.eegData.SkipWhile(x => x.timestamp - offset < tasks[ta].start).TakeWhile(x => x.timestamp - offset < tasks[ta].end).Select(x => Tuple.Create(x.timestamp - offset, (double)x.data[item])).ToList(),
                                    8
                                    );

                                if (eeg.Item2.Count == 0 || eeg.Item3.Count == 0) continue;

                                /*
                                var eegNorm = NormalizeFilterData(eeg);
                                var setA = eegNorm.Item1.VarianceFilter(64);
                                var setB = eegNorm.Item2.VarianceFilter(64);
                                */

                                var setA = eeg.Item2.VarianceFilter(64).CalculateNormalized();
                                var setB = eeg.Item3.VarianceFilter(64).CalculateNormalized();

                                var min = Math.Min(setA.Count, setB.Count);
                                Log.LogMessage($"{item} done, data filtered: {eeg.Item1.ToString("0.0")}%");
                                var pearsCorr = MathNet.Numerics.Statistics.Correlation.Pearson(setA.GetRange(0, min), setB.GetRange(0, min));
                                SavePng(csvTimePath + "EEG_" + item + ".png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", setA, setB);
                                SaveZip(csvTimePath + "EEG_" + item + ".csv", setA, setB);

                                int t;
                                if (int.TryParse(time, out t) && t != 0)
                                {
                                    SavePng(csvStimuliPath + "EEG_" + item + ".png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", setA, setB);
                                    SaveZip(csvStimuliPath + "EEG_" + item + ".csv", setA, setB);

                                }
                            }
                            Log.LogMessage("EEG done");
                        }

                        if (files.Contains("KINECT.dat"))
                        {
                            Log.LogMessage("Starting Kinect");
                            foreach (Microsoft.Kinect.Face.FaceShapeAnimations item in Enum.GetValues(typeof(Microsoft.Kinect.Face.FaceShapeAnimations)))
                            {
                                if (item == Microsoft.Kinect.Face.FaceShapeAnimations.Count) continue;

                                var kinect = FilterData(
                                    fdTest.faceData.SkipWhile(x => x.timestamp < tasks[ta].start).TakeWhile(x => x.timestamp < tasks[ta].end).Select(x => Tuple.Create(x.timestamp, (double)x.data[item])).ToList(),
                                    fdRecall.faceData.SkipWhile(x => x.timestamp - offset < tasks[ta].start).TakeWhile(x => x.timestamp - offset < tasks[ta].end).Select(x => Tuple.Create(x.timestamp - offset, (double)x.data[item])).ToList(),
                                    34
                                    );

                                if (kinect.Item2.Count == 0 || kinect.Item3.Count == 0) continue;


                                var kiNorm = NormalizeFilterData(kinect);
                                var setA = kiNorm.Item1.MovingAverageFilter(25);
                                var setB = kiNorm.Item2.MovingAverageFilter(25);
                                var min = Math.Min(setA.Count, setB.Count);
                                Log.LogMessage($"{item} done, data filtered: {kinect.Item1.ToString("0.0")}%");

                                var pearsCorr = MathNet.Numerics.Statistics.Correlation.Pearson(setA.GetRange(0, min), setB.GetRange(0, min));
                                SavePng(csvTimePath + "FACE_" + item + ".png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", setA, setB);
                                SaveZip(csvTimePath + "FACE_" + item + ".csv", setA, setB);

                                int t;
                                if (int.TryParse(time, out t) && t != 0)
                                {
                                    SavePng(csvStimuliPath + "FACE_" + item + ".png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", setA, setB);
                                    SaveZip(csvStimuliPath + "FACE_" + item + ".csv", setA, setB);
                                    //SavePng(csvStimuliPath + "GSR_nonTemporal.png", $"{subject} (Time: {time}, Stim: {stimul}, Corr: {pearsCorr.ToString("0.000")}) - Red = test, blue = recall", nonTempA, nonTempB);
                                }
                            }
                            Log.LogMessage("Kinect done");
                        }
                    }







                    //var resA = Pearson(result.Item2, result.Item3);//MathNet.Numerics.Statistics.Correlation.Pearson(result.Item2, result.Item3);
                    //var sigA = Significance(resA, result.Item2.Count);
                    //Log.LogMessage($"Best case pearson correlation: {resA.ToString("0.000")}");

                    //var resB = Pearson(result.Item2, result.Item4);//MathNet.Numerics.Statistics.Correlation.Pearson(result.Item2, result.Item4);
                    //var sigB = Significance(resB, result.Item2.Count);
                    //Log.LogMessage($"Second best case pearson correlation: {resB.ToString("0.000")}");

                    //List<string> toWrite = new List<string>();
                    //toWrite.Add($"[{dirPath.Split('\\').Last()}]");
                    //toWrite.Add($"Data removed={result.Item1}");
                    //toWrite.Add($"Pearson closest={resA}");
                    //toWrite.Add($"Significance={sigA}");
                    //toWrite.Add($"Pearson second closest={resB}");
                    //toWrite.Add($"Significance={sigB}");
                    //toWrite.Add("");
                    //File.AppendAllLines("results.txt", toWrite);
                }

                Log.LogMessage("DonnoDK!");
            }
        }

        static void SavePng(string path, string name, List<double> A, List<double> B, List<Tuple<int, int>> pairings = null)
        {
            PngExporter pngify = new PngExporter();
            pngify.Width = 3200;
            pngify.Height = 1200;

            var model = new PlotModel() { Title = name };

            var aSeries = new OxyPlot.Series.LineSeries() { Color = OxyColors.Blue };
            var bSeries = new OxyPlot.Series.LineSeries() { Color = OxyColors.Red };

            for (int i = 0; i < A.Count; i++)
            {
                aSeries.Points.Add(new OxyPlot.DataPoint(i, A[i]));
            }

            for (int i = 0; i < B.Count; i++)
            {
                bSeries.Points.Add(new OxyPlot.DataPoint(i, B[i]));
            }

            if (pairings != null)
            {
                for (int i = 0; i < pairings.Count; i += 10)
                {
                    var lineSeries = new OxyPlot.Series.LineSeries() { Color = OxyColors.Gray, StrokeThickness = 0.2 };

                    lineSeries.Points.Add(aSeries.Points[pairings[i].Item1]);
                    lineSeries.Points.Add(bSeries.Points[pairings[i].Item2]);

                    model.Series.Add(lineSeries);
                }
            }

            model.Series.Add(aSeries);
            model.Series.Add(bSeries);

            model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Left });
            //model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Minimum = 0, Maximum = 1, Position = OxyPlot.Axes.AxisPosition.Bottom });


            pngify.ExportToFile(model, path);
        }

        static void SavePngScatter(string path, string name, List<double> A, List<double> B, double axisMin = 0, double axisMax = 0)
        {
            PngExporter pngify = new PngExporter();
            pngify.Width = 2000;
            pngify.Height = 2000;

            var model = new PlotModel() { Title = name };

            var scatterSeries = new OxyPlot.Series.ScatterSeries()
            {
                MarkerSize = 0.8f,
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Black
            };


            for (int i = 0; i < A.Count && i < B.Count; i++)
            {
                scatterSeries.Points.Add(new OxyPlot.Series.ScatterPoint(A[i], B[i]));
            }

            model.Series.Add(scatterSeries);
            model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Minimum = axisMin, Maximum = axisMax, Position = OxyPlot.Axes.AxisPosition.Left });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis() { Minimum = axisMin, Maximum = axisMax, Position = OxyPlot.Axes.AxisPosition.Bottom });

            pngify.ExportToFile(model, path);
        }

        static void SaveZip(string path, List<double> A, List<double> B)
        {
            File.WriteAllLines(path, A.Zip(B, (a, b) => (a + ";" + b).Replace(',', '.')));
        }

        public static double Pearson(IEnumerable<double> dataA, IEnumerable<double> dataB)
        {
            int n = 0;
            double r = 0.0;

            double meanA = 0;
            double meanB = 0;
            double varA = 0;
            double varB = 0;

            // WARNING: do not try to "optimize" by summing up products instead of using differences.
            // It would indeed be faster, but numerically much less robust if large mean + low variance.

            using (IEnumerator<double> ieA = dataA.GetEnumerator())
            using (IEnumerator<double> ieB = dataB.GetEnumerator())
            {
                while (ieA.MoveNext())
                {
                    if (!ieB.MoveNext())
                    {
                        //throw new ArgumentOutOfRangeException("dataB", Resources.ArgumentArraysSameLength);
                        throw new NotImplementedException();
                    }

                    double currentA = ieA.Current;
                    double currentB = ieB.Current;

                    double deltaA = currentA - meanA;
                    double scaleDeltaA = deltaA / ++n;

                    double deltaB = currentB - meanB;
                    double scaleDeltaB = deltaB / n;

                    meanA += scaleDeltaA;
                    meanB += scaleDeltaB;

                    varA += scaleDeltaA * deltaA * (n - 1);
                    varB += scaleDeltaB * deltaB * (n - 1);
                    r += (deltaA * deltaB * (n - 1)) / n;
                }

                if (ieB.MoveNext())
                {
                    //throw new ArgumentOutOfRangeException("dataA", Resources.ArgumentArraysSameLength);
                    throw new NotImplementedException();
                }
            }

            double denom = Math.Sqrt(varA * varB);

            if (denom != 0)
                return r / denom;
            else
                return 0;
        }

        Tuple<List<double>, List<double>> NormalizeFilterData(Tuple<double, List<double>, List<double>, List<double>> input)
        {
            var max2 = input.Item2.Max();
            var new2 = input.Item2.Select(x => x / max2).ToList();

            var max3 = input.Item3.Max();
            var new3 = input.Item3.Select(x => x / max3).ToList();

            return Tuple.Create(new2, new3);
        }

        List<Tuple<int, int>> FilterPairing(List<Tuple<long, double>> Ain, List<Tuple<long, double>> Bin)
        {
            int window = 1000;

            List<Tuple<int, int>> pairing = new List<Tuple<int, int>>();


            for (int i = 0; i < Ain.Count; i++)
            {
                int closestId = 0;
                double closestDist = double.MaxValue;

                for (int j = 0; j < Bin.Count && Bin[j].Item1 < Ain[i].Item1 + window; j++)
                {
                    if (Bin[j].Item1 > Ain[i].Item1 - window) continue;

                    var curDist = Math.Abs(Bin[j].Item2 - Ain[i].Item2);
                    if (closestDist > curDist)
                    {
                        closestDist = curDist;
                        closestId = j;
                    }
                }

                pairing.Add(Tuple.Create(i, closestId));
            }

            return pairing;
        }

        List<Tuple<int, int>> FilterSlopePairing(List<Tuple<long, double>> Ain, List<Tuple<long, double>> Bin)
        {
            int window = 1000;

            List<Tuple<int, int>> pairing = new List<Tuple<int, int>>();
            List<double> slopeA = new List<double>(Ain.Count - 1);
            for (int i = 1; i < Ain.Count; i++)
            {
                slopeA.Add((Ain[i].Item2 - Ain[i - 1].Item2) / (Ain[i].Item1 - Ain[i - 1].Item1));
            }

            List<double> slopeB = new List<double>(Bin.Count - 1);
            for (int i = 1; i < Bin.Count; i++)
            {
                slopeB.Add((Bin[i].Item2 - Bin[i - 1].Item2) / (Bin[i].Item1 - Bin[i - 1].Item1));
            }

            for (int i = 0; i < slopeA.Count; i++)
            {
                int closestId = 0;
                double closestDist = double.MaxValue;

                for (int j = 0; j < slopeB.Count && j < slopeA.Count && Bin[j + 1].Item1 < Ain[i + 1].Item1 + window; j++)
                {
                    if (Bin[j + 1].Item1 < Ain[i + 1].Item1 - window) continue;

                    var positionalDiff = Math.Sqrt(Math.Pow(Ain[i + 1].Item1 - Bin[j + 1].Item1, 2) + Math.Pow(Ain[i + 1].Item2 - Bin[j + 1].Item2, 2));

                    var slopeDiff = Math.Abs(slopeA[i] - slopeB[j]) * window * 10;

                    var timeDiff = Ain[i + 1].Item1 - Bin[j + 1].Item1;

                    var curDist = slopeDiff + positionalDiff;
                    if (closestDist > curDist)
                    {
                        closestDist = curDist;
                        closestId = j;
                    }
                }

                pairing.Add(Tuple.Create(i, closestId));
            }

            return pairing;
        }

        bool PairTupleCompare(Tuple<long, double, bool> t1, Tuple<long, double, bool> t2)
        {
            return t1 == null || t1.Item3 == t2.Item3;
        }

        Tuple<double, List<double>, List<double>, List<double>> FilterData(List<Tuple<long, double>> Ain, List<Tuple<long, double>> Bin, int msPerReading = -1)
        {
            List<Tuple<long, double>> A = new List<Tuple<long, double>>(Ain);
            List<Tuple<long, double>> B = new List<Tuple<long, double>>(Bin);

            int removed = 0;


            var together = A.Select(x => Tuple.Create(x.Item1, x.Item2, true)).Concat(B.Select(y => Tuple.Create(y.Item1, y.Item2, false))).OrderBy(x => x.Item1).ToList();
            for (int i = 0; i < together.Count - 2; i++)
            {
                if (PairTupleCompare(together[i], together[i + 1]) && PairTupleCompare(together[i + 1], together[i + 2]))
                {
                    //together.RemoveAt(i + 1);
                    together[i + 1] = null;

                    if (i == 0)
                    {
                        together[0] = null;
                    }

                    if (i + 2 == together.Count - 1)
                    {
                        together[i + 2] = null;
                    }
                }
            }
            A = together.Where(x => x != null && x.Item3).Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            B = together.Where(x => x != null && !x.Item3).Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            removed = together.Count - A.Count - B.Count;


            //for (int i = 0; i < Ain.Count - 2; i++)
            //{
            //    int r = 0;
            //    while (true)
            //    {
            //        if (i +r + 2 < Bin.Count && Ain[i + r + 2].Item1 < Bin[i].Item1)
            //        {
            //            A[i + r + 1] = null;
            //            //A.Remove(Ain[i + r + 1]);
            //            r++;

            //            if (i == 0)
            //            {
            //                //A.Remove(Ain[0]);
            //                A[0] = null;
            //            }

            //        }
            //        else
            //        {
            //            if (i + r + 2 >= Bin.Count)
            //            {
            //                for (int d = i + r + 2; d < A.Count; d++)
            //                {
            //                    A[d] = null;
            //                }
            //            }

            //            break;
            //        }
            //    }
            //}

            //for (int i = 0; i < Bin.Count - 2; i++)
            //{
            //    int r = 0;
            //    while (true)
            //    {
            //        if (i + r + 2 < Ain.Count && Bin[i + r + 2].Item1 < Ain[i].Item1)
            //        {
            //            //B.Remove(Bin[i + r + 1]);
            //            B[i + r + 1] = null;
            //            r++;

            //            if (i == 0)
            //            {
            //                //A.Remove(Ain[0]);
            //                B[0] = null;
            //            }
            //        }
            //        else
            //        {
            //            if (i + r + 2 >= Ain.Count)
            //            {
            //                for (int d = i + r + 2; d < B.Count; d++)
            //                {
            //                    B[d] = null;
            //                }
            //            }

            //            break;
            //        }
            //    }
            //}

            //A.RemoveAll(x => x == null);
            //B.RemoveAll(x => x == null);

            //for (int aItr = 0, bItr = 0; aItr < A.Count - 2 && bItr < B.Count - 2;)
            //{
            //    if (A[aItr + 2].Item1 < B[bItr].Item1)
            //    {
            //        A.Remove(A[aItr + 1]);
            //    }
            //    else
            //    {
            //        aItr++;
            //    }

            //    if (B[bItr + 2].Item1 < A[aItr].Item1)
            //    {
            //        B.Remove(B[bItr + 1]);
            //    }
            //    else
            //    {
            //        bItr++;
            //    }

            //}
            /*
            if (A.Last().Item1 < B.Last().Item1)
            {
                B = B.Where(x => x.Item1 < A.Last().Item1).ToList();
            }
            else
            {
                A = A.Where(x => x.Item1 < B.Last().Item1).ToList();
            }

            if (A.Last().Item1 < B.Last().Item1)
            {
                B = B.Where(x => x.Item1 < A.Last().Item1).ToList();
            }
            else
            {
                A = A.Where(x => x.Item1 < B.Last().Item1).ToList();
            }
            */


            //step 2, do pair pointer analysis thingy
            //List<double> As = new List<double>();
            //List<double> closestB = new List<double>();
            //List<double> secondClosestB = new List<double>();


            //int furthestB = 0;
            //for (int i = 0; i < A.Count; i++)
            //{
            //    long bestDist = int.MaxValue;
            //    int bestDistId = 0;
            //    long secondBestDist = int.MaxValue;
            //    int secondBestDistId = 0;

            //    long prevDist = int.MaxValue;

            //    for (int j = furthestB; j < B.Count; j++)
            //    {
            //        long dist = Math.Abs(A[i].Item1 - B[j].Item1);

            //        if (dist < bestDist)
            //        {
            //            secondBestDist = bestDist;
            //            secondBestDistId = bestDistId;
            //            bestDist = dist;
            //            bestDistId = j;
            //        }
            //        else if (dist < secondBestDist)
            //        {
            //            secondBestDist = dist;
            //            secondBestDistId = j;
            //        }

            //        if (prevDist < dist)
            //        {
            //            break;
            //        }

            //        prevDist = dist;
            //    }

            //    furthestB = Math.Max(0, bestDistId - 1);

            //    As.Add(A[i].Item2);
            //    closestB.Add(B[(int)bestDistId].Item2);
            //    secondClosestB.Add(B[(int)secondBestDistId].Item2);
            //}



            return Tuple.Create((A.Count + B.Count) / (double)(Ain.Count + Bin.Count), A.Select(x => x.Item2).ToList(), B.Select(x => x.Item2).ToList(), new List<double>());

        }

        private void btn_CreateResultTable_Click(object sender, EventArgs e)
        {
            OxyColor EEGColor = OxyColors.Cyan;
            OxyColor EDAColor = OxyColors.LightGreen;
            OxyColor HRColor = OxyColors.Salmon;

            //var tester = new List<double>
            //{
            //    0.01
            //};

            //var tester2 = new List<double>
            //{
            //    0.01, 0.01
            //};

            //var tester3 = new List<double>
            //{
            //    0.01, 0.01, 0.01
            //};

            //MessageBox.Show(tester.FisherCombineP().ToString("0.00000000") + "\n" + tester2.FisherCombineP().ToString("0.00000000") + "\n" + tester3.FisherCombineP().ToString("0.00000000"));

            //var res1 = FisherCompare1(0.4, 10, 0.3, 12);
            //var res2 = FisherCompare2(0.4, 10, 0.3, 12);

            //MessageBox.Show("1)\n" + res1.Item1 + "\n" + res1.Item2 + "\n" + res1.Item3 + "\n\n2)\n" + res2.Item1 + "\n" + res2.Item2 + "\n" + res2.Item3);


            string corrType = "Pearson";
            //string corrType = "Kendall";
            //string corrType = "Spearman";
            double minMilliseconds = 10000;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                //sensor is first string
                var timeTable = new Dictionary<string, Dictionary<int, List<Tuple<double, double>>>>();
                var stimuliTable = new Dictionary<string, Dictionary<string, List<Tuple<double, double>>>>();
                var totalList = new Dictionary<string, List<Tuple<double, double>>>();
                var big5List = new Dictionary<string, List<Dictionary<Big5, int>>>();
                List<string> sensors = new List<string>();
                List<int> times = new List<int>();
                List<string> stimulis = new List<string>();
                List<string> resultFiles = new List<string>();

                foreach (var folder in Directory.GetDirectories(fbd.SelectedPath))
                {
                    if (folder.Contains("Stimuli high") ||
                        folder.Contains("Stimuli low") ||
                        folder.Contains("Time 0") ||
                        folder.Contains("Time 1") ||
                        folder.Contains("Time 2") ||
                        folder.Contains(".git") ||
                        folder.Split('\\').Last() == "3" ||
                        folder.Split('\\').Last() == "6" ||
                        folder.Split('\\').Last() == "13")
                    {
                        continue;
                    }

                    string subject = folder.Split('\\').Last();

                    var metaLines = File.ReadAllLines($"{folder}/meta.txt");
                    var big5 = GetBig5(metaLines);
                    int time = int.Parse(metaLines[0].Split('=').Last());
                    string stimuli = metaLines[1].Split('=').Last();
                    stimuli = stimuli == "neu" ? "low" : "high";
                    if (!times.Contains(time)) times.Add(time);
                    if (!stimulis.Contains(stimuli)) stimulis.Add(stimuli);

                    List<string> foldersToExamine = new List<string>();
                    foldersToExamine.Add(fbd.SelectedPath + "\\Time " + time);

                    if (time > 0)
                    {
                        foldersToExamine.Add(fbd.SelectedPath + "\\Stimuli " + stimuli);
                    }

                    if (!big5List.ContainsKey("time" + time))
                    {
                        big5List.Add("time" + time, new List<Dictionary<Big5, int>>());
                    }

                    if (!big5List.ContainsKey("stim" + stimuli))
                    {
                        big5List.Add("stim" + stimuli, new List<Dictionary<Big5, int>>());
                    }

                    if (!big5List.ContainsKey("total"))
                    {
                        big5List.Add("total", new List<Dictionary<Big5, int>>());
                    }

                    if (!big5List.ContainsKey("corr"))
                    {
                        big5List.Add("corr", new List<Dictionary<Big5, int>>());
                    }

                    if (!big5List.ContainsKey("revCorr"))
                    {
                        big5List.Add("revCorr", new List<Dictionary<Big5, int>>());
                    }

                    big5List["time" + time].Add(big5);
                    if (time != 0)
                    {
                        big5List["stim" + stimuli].Add(big5);
                    }
                    big5List["total"].Add(big5);
                    foreach (var folderToExamine in foldersToExamine)
                    {
                        foreach (var resultFile in Directory.GetFiles(folderToExamine).Where(f => f.Split('\\').Last().StartsWith(subject) && f.Split('\\').Last().EndsWith(".txt")))
                        {
                            if (resultFiles.Contains(resultFile.Split('\\').Last()) || !folderToExamine.Contains("Time") && !foldersToExamine.Contains("Stimuli") || resultFile.Contains("dtw"))
                            {
                                continue;
                            }

                            resultFiles.Add(resultFile.Split('\\').Last());

                            string sensor = new String(resultFile.Split('.').First().SkipWhile(x => x != '_').Skip(1).SkipWhile(x => x != '_').Skip(1).ToArray());

                            if (!sensors.Contains(sensor)) sensors.Add(sensor);

                            var resultLines = File.ReadAllLines(resultFile);
                            string correlationLine = resultLines.First(x => x.Contains("|" + corrType));
                            int corrId = resultLines.ToList().IndexOf(correlationLine);
                            int sigId = corrId + 2;
                            string significanceLine = resultLines[sigId];
                            string N = resultLines[sigId + 2];

                            double highPassThreshold = minMilliseconds / 1000;

                            if (sensor.Contains("EEG"))
                            {
                                highPassThreshold *= 128;
                            }
                            else if (sensor.Contains("GSR"))
                            {
                                highPassThreshold *= 20;
                            }
                            else if (sensor.Contains("HR"))
                            {
                                highPassThreshold *= 1;
                            }

                            string[] Nsplit = N.Split(new char[] { '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (correlationLine.Contains(".a") || significanceLine.Contains(".a") || int.Parse(Nsplit[2]) < highPassThreshold)
                            {
                                if (int.Parse(Nsplit[2]) < highPassThreshold)
                                {
                                    Log.LogMessage("Removing - " + Nsplit[2] + ": " + resultFile);
                                }

                                continue;
                            }

                            int splitIndex = (corrType == "Pearson") ? 3 : 4;

                            double pearsCorrelation = double.Parse(correlationLine.Split(new char[] { '|', '*' }, StringSplitOptions.RemoveEmptyEntries)[splitIndex].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                            double pearsSignificance = double.Parse(significanceLine.Split(new char[] { '|', '*' }, StringSplitOptions.RemoveEmptyEntries)[splitIndex].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);


                            var result = Tuple.Create(pearsCorrelation, pearsSignificance);


                            if (!timeTable.ContainsKey(sensor))
                            {
                                timeTable.Add(sensor, new Dictionary<int, List<Tuple<double, double>>>());
                                stimuliTable.Add(sensor, new Dictionary<string, List<Tuple<double, double>>>());
                                totalList.Add(sensor, new List<Tuple<double, double>>());
                            }
                            if (!timeTable[sensor].ContainsKey(time))
                            {
                                timeTable[sensor].Add(time, new List<Tuple<double, double>>());
                            }
                            if (!stimuliTable[sensor].ContainsKey(stimuli))
                            {
                                stimuliTable[sensor].Add(stimuli, new List<Tuple<double, double>>());
                            }

                            timeTable[sensor][time].Add(result);
                            totalList[sensor].Add(result);
                            if (time != 0)
                            {
                                stimuliTable[sensor][stimuli].Add(result);
                            }

                            if (pearsCorrelation > 0)
                            {
                                big5List["corr"].Add(big5);
                            }
                            else
                            {
                                big5List["revCorr"].Add(big5);

                            }
                        }
                    }
                }

                //done gathering results
                List<string> totalToWrite = new List<string>();
                totalToWrite.Add("Sensor&Avg Corr&Avg Sig. \\\\");
                foreach (var sensor in sensors)
                {
                    double avgCorrelation = totalList[sensor].Average(x => x.Item1);
                    double stdevCorrelation = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(totalList[sensor].Select(x => x.Item1).ToArray());
                    double avgSignificance = totalList[sensor].Average(x => x.Item2);
                    double stdevSignificance = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(totalList[sensor].Select(x => x.Item2).ToArray());

                    totalToWrite.Add($"{sensor}&{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})&{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")}) \\\\");
                }

                Dictionary<Big5, List<string>> big5Anova = new Dictionary<Big5, List<string>>();
                foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                {
                    big5Anova.Add(item, new List<string>());
                    totalToWrite.Add(item + " Mean: " + big5List["total"].Average(x => x[item]).ToString("0.00") + ", SD: " + MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(big5List["total"].Select(x => x[item]).ToArray()).ToString("0.00") + ".");

                    big5List["time0"].ForEach(x => big5Anova[item].Add("0;" + x[item]));
                    big5List["time1"].ForEach(x => big5Anova[item].Add("1;" + x[item]));
                    big5List["time2"].ForEach(x => big5Anova[item].Add("2;" + x[item]));
                    big5List["stimlow"].ForEach(x => big5Anova[item].Add("3;" + x[item]));
                    big5List["stimhigh"].ForEach(x => big5Anova[item].Add("4;" + x[item]));
                }

                foreach (var big5group in big5Anova)
                {
                    File.WriteAllLines(fbd.SelectedPath + "/" + big5group.Key + "_anova.csv", big5group.Value);
                }

                File.WriteAllLines(fbd.SelectedPath + "/" + corrType + "_totals.txt", totalToWrite);

                double width = 1 / (sensors.Count * 1.4);
                double widthTime = 0.3;


                var timeModel = new PlotModel() { Title = $"Time Groups Box Plot" };
                var avgLineSeries = new OxyPlot.Series.LineSeries() { };

                List<OxyColor> colors = new List<OxyColor>()
                {
                };

                int small = sensors.Count / 3;
                int mid = small * 2;
                int stepsize = 255 / small;
                for (int i = 0; i < sensors.Count; i++)
                {
                    byte increaser = (byte)((i % small) * stepsize);
                    byte decreaser = (byte)(255 - increaser);

                    if (i < small)
                    {
                        colors.Add(OxyColor.FromRgb(increaser, decreaser, decreaser));
                    }
                    else if (i < mid)
                    {
                        colors.Add(OxyColor.FromRgb(decreaser, increaser, decreaser));
                    }
                    else
                    {
                        colors.Add(OxyColor.FromRgb(decreaser, decreaser, increaser));
                    }
                }

                List<string> sensorsAdded = new List<string>();

                foreach (var sensor in sensors)
                {
                    List<string> timeAnova = new List<string>();
                    foreach (var time in times)
                    {
                        timeTable[sensor][time].ForEach(x => timeAnova.Add(time + ";" + x.Item1));
                    }
                    File.WriteAllLines(fbd.SelectedPath + "/" + sensor + ".csv", timeAnova);
                }

                Dictionary<string, int[]> significantAmount = new Dictionary<string, int[]>();
                Dictionary<string, int[]> significantAmountMax = new Dictionary<string, int[]>();
                var significantCorr = new Dictionary<string, Tuple<double, double, double, double>[]>();
                foreach (var sensor in sensors)
                {
                    significantAmount.Add(sensor, new int[5]);
                    significantAmountMax.Add(sensor, new int[5]);
                    //significantCorr.Add(sensor, new Tuple<double, double>[5]);
                }

                significantCorr.Add("EEG", new Tuple<double, double, double, double>[5]);
                significantCorr.Add("EDA", new Tuple<double, double, double, double>[5]);
                significantCorr.Add("HR", new Tuple<double, double, double, double>[5]);
                //significantCorr.Add("AVG", new Tuple<double, double, double, double>[5]);

                Action<string, int, List<Tuple<double, double>>> AddCorrelation = (sens, id, correl) =>
                {
                    //old average + sd
                    //significantCorr[sens][id] = Tuple.Create(correl.Average(x => x.Item1), correl.Select(x => x.Item1).STDEV(), correl.Average(x => x.Item2), correl.Select(x => x.Item2).STDEV());

                    //new fisher algorithms
                    significantCorr[sens][id] = Tuple.Create(FisherInverse(correl.Average(x => Fisher(x.Item1))), Math.Round((double)correl.Count), correl.Select(x => x.Item2).FisherCombineP(), Math.Round((double)correl.Count));
                };
                List<string> amountTimeSignificant = new List<string>();

                List<double> timeSignificantPoints = new List<double>();
                var TimeErrorModel = new PlotModel() { Title = $"Time Error Model" };
                var timeErrorSeries = new OxyPlot.Series.ErrorColumnSeries() { };
                TimeErrorModel.Series.Add(timeErrorSeries);
                TimeErrorModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left });
                var axis = new OxyPlot.Axes.CategoryAxis { Position = OxyPlot.Axes.AxisPosition.Bottom };
                axis.Labels.Add("Time 0");
                axis.Labels.Add("Time 1");
                axis.Labels.Add("Time 2");
                TimeErrorModel.Axes.Add(axis);
                TimeErrorModel.Annotations.Add(new OxyPlot.Annotations.LineAnnotation() { Y = 0, Type = OxyPlot.Annotations.LineAnnotationType.Horizontal });

                var TimeErrorModel2 = new PlotModel() { Title = $"Time Error Model" };
                TimeErrorModel2.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left });
                var axis2 = new OxyPlot.Axes.CategoryAxis { Position = OxyPlot.Axes.AxisPosition.Bottom };
                axis2.Labels.Add("Time 0");
                axis2.Labels.Add("Time 1");
                axis2.Labels.Add("Time 2");
                TimeErrorModel2.Axes.Add(axis2);
                List<string> AnovaIndividual = new List<string>();
                List<string> AnovaAvg = new List<string>();
                int anovaIndividualId = 0;
                int anovaAvgId = 0;
                List<string> AnovaIndividualLegend = new List<string>();
                List<string> AnovaAvgLegend = new List<string>();
                foreach (var time in times)
                {
                    int sensorId = 0;
                    List<string> timeToWrite = new List<string>();
                    timeToWrite.Add("Sensor&Avg Corr&Avg Sig. \\\\");

                    List<double> avgs = new List<double>();
                    List<double> sigPoints = new List<double>();

                    var errorSeries = new OxyPlot.Series.ErrorColumnSeries();
                    TimeErrorModel2.Series.Add(errorSeries);

                    var EEGAllCorrelations = new List<Tuple<double, double>>();
                    var GSRAllCorrelations = timeTable["GSR"][time];
                    var HRAllCorrelations = timeTable["HR"][time];
                    foreach (var sensor in sensors)
                    {
                        if (sensor.Contains("EEG"))
                        {
                            EEGAllCorrelations.AddRange(timeTable[sensor][time]);
                        }
                        double avgCorrelation = timeTable[sensor][time].Average(x => x.Item1);
                        double stdevCorrelation = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Select(x => x.Item1).ToArray());
                        double avgSignificance = timeTable[sensor][time].Average(x => x.Item2);
                        double stdevSignificance = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Select(x => x.Item2).ToArray());

                        var orderedAll = timeTable[sensor][time].OrderBy(x => x.Item1).ToList();//timeTable[sensor][time].Where(x => x.Item2 * 100 < (int)5).OrderBy(x => x.Item1).ToList();
                        amountTimeSignificant.Add(time + " & " + sensor + " & " + orderedAll.Count);
                        significantAmount[sensor][time] = orderedAll.Count;
                        significantAmountMax[sensor][time] = timeTable[sensor][time].Count;
                        //significantCorr[sensor][time] = Tuple.Create(orderedAll.Average(x => x.Item1), MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(orderedAll.Select(x => x.Item1).ToArray()));

                        //var boxItem = new OxyPlot.Series.BoxPlotItem(time + sensorId * widthTime - (0.5 * widthTime * sensors.Count), orderedAll[0].Item1, orderedAll[(int)(orderedAll.Count * 0.25)].Item1, orderedAll[orderedAll.Count / 2].Item1, orderedAll[(int)(orderedAll.Count * 0.75)].Item1, orderedAll.Last().Item1);
                        //var boxItem = new OxyPlot.Series.BoxPlotItem(sensorId + time * widthTime - (0.5 * widthTime * (times.Count - 1)), orderedAll[0].Item1, orderedAll[(int)(orderedAll.Count * 0.25)].Item1, orderedAll[orderedAll.Count / 2].Item1, orderedAll[(int)(orderedAll.Count * 0.75)].Item1, orderedAll.Last().Item1);
                        //var boxSeries = new OxyPlot.Series.BoxPlotSeries() { };
                        //boxSeries.BoxWidth = widthTime;
                        //boxSeries.WhiskerWidth = widthTime;
                        //boxSeries.Items.Add(boxItem);
                        //boxSeries.Fill = colors[sensorId];
                        //timeModel.Series.Add(boxSeries);



                        errorSeries.Items.Add(new OxyPlot.Series.ErrorColumnItem(orderedAll.Average(x => x.Item1), MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(orderedAll.Select(x => x.Item1).ToArray()), time) { Color = colors[sensorId] });

                        //if (!sensorsAdded.Contains(sensor))
                        //{
                        //    sensorsAdded.Add(sensor);
                        //    boxSeries.Title = sensor;
                        //}

                        avgs.Add(orderedAll.Average(x => x.Item1));
                        sigPoints.AddRange(orderedAll.Select(x => x.Item1));

                        if (avgSignificance * 100 < (int)5)
                        {
                            timeToWrite.Add($"\\textbf{{{sensor}}}&\\textbf{{{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})}}&\\textbf{{{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")})}} \\\\");
                        }
                        else
                        {
                            timeToWrite.Add($"{sensor}&{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})&{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")}) \\\\");
                        }
                        sensorId++;
                    }

                    double boxWidth = 0.3;

                    //eeg
                    EEGAllCorrelations = EEGAllCorrelations.OrderBy(x => x.Item1).ToList();
                    var EEGSeries = new OxyPlot.Series.BoxPlotSeries() { Fill = EEGColor, BoxWidth = boxWidth, WhiskerWidth = boxWidth };
                    if (time == 0) EEGSeries.Title = "EEG";
                    var EEGItem = CreateBoxItem(EEGAllCorrelations);
                    EEGItem.X = time - EEGSeries.BoxWidth * 1;
                    EEGSeries.Items.Add(EEGItem);
                    timeModel.Series.Add(EEGSeries);

                    AddCorrelation("EEG", time, EEGAllCorrelations);

                    foreach (var cor in EEGAllCorrelations)
                    {
                        AnovaIndividual.Add(anovaIndividualId + ";" + cor.Item1);
                    }
                    AnovaIndividualLegend.Add(anovaIndividualId++ + "=time_" + time + "_EEG");

                    //gsr
                    GSRAllCorrelations = GSRAllCorrelations.OrderBy(x => x.Item1).ToList();
                    var GSRSeries = new OxyPlot.Series.BoxPlotSeries() { Fill = EDAColor, BoxWidth = boxWidth, WhiskerWidth = boxWidth };
                    if (time == 0) GSRSeries.Title = "EDA";
                    var GSRItem = CreateBoxItem(GSRAllCorrelations);
                    GSRItem.X = time;
                    GSRSeries.Items.Add(GSRItem);
                    timeModel.Series.Add(GSRSeries);

                    AddCorrelation("EDA", time, GSRAllCorrelations);

                    foreach (var cor in GSRAllCorrelations)
                    {
                        AnovaIndividual.Add(anovaIndividualId + ";" + cor.Item1);
                    }
                    AnovaIndividualLegend.Add(anovaIndividualId++ + "=time_" + time + "_GSR");

                    //hr
                    HRAllCorrelations = HRAllCorrelations.OrderBy(x => x.Item1).ToList();
                    var HRSeries = new OxyPlot.Series.BoxPlotSeries() { Fill = HRColor, BoxWidth = boxWidth, WhiskerWidth = boxWidth };
                    if (time == 0) HRSeries.Title = "HR";
                    var HRItem = CreateBoxItem(HRAllCorrelations);
                    HRItem.X = time + HRSeries.BoxWidth * 1;
                    HRSeries.Items.Add(HRItem);
                    timeModel.Series.Add(HRSeries);

                    AddCorrelation("HR", time, HRAllCorrelations);

                    foreach (var cor in HRAllCorrelations)
                    {
                        AnovaIndividual.Add(anovaIndividualId + ";" + cor.Item1);
                    }
                    AnovaIndividualLegend.Add(anovaIndividualId++ + "=time_" + time + "_HR");

                    //avg
                    var AVGAllCorrelations = EEGAllCorrelations.Concat(GSRAllCorrelations.Concat(HRAllCorrelations)).ToList();

                    //AddCorrelation("AVG", time, AVGAllCorrelations);

                    foreach (var cor in AVGAllCorrelations)
                    {
                        AnovaAvg.Add(anovaAvgId + ";" + cor.Item1);
                    }
                    AnovaAvgLegend.Add(anovaAvgId++ + "=time_" + time);
                    double totalAvg = AVGAllCorrelations.Average(x => x.Item1);
                    var txtAvg = new OxyPlot.Annotations.TextAnnotation() { TextPosition = new OxyPlot.DataPoint(time, -1), Text = "Avg " + totalAvg.ToString(".000").Replace(",", "."), Stroke = OxyColors.White };
                    timeModel.Annotations.Add(txtAvg);

                    timeErrorSeries.Items.Add(new OxyPlot.Series.ErrorColumnItem(sigPoints.Average(), MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(sigPoints.ToArray()), time));

                    avgLineSeries.Points.Add(new OxyPlot.DataPoint(time, avgs.Average()));
                    File.WriteAllLines(fbd.SelectedPath + "/" + corrType + "_time" + time + ".txt", timeToWrite);
                }
                File.WriteAllLines(fbd.SelectedPath + "/significantTime.tex", amountTimeSignificant);
                timeModel.LegendPlacement = LegendPlacement.Outside;

                timeModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 1, Minimum = -1, Title = "Pearson's r" });
                timeModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 2.5, Minimum = -0.5, MajorStep = 1, Title = "Time", MinorTickSize = 0 });
                //timeModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = sensors.Count - 0.5, Minimum = -0.5, MajorStep = 1, Title = "Sensors", MinorTickSize = 0 });
                //boxModel.Series.Add(avgLineSeries);
                PngExporter pnger = new PngExporter();

                pnger.ExportToFile(timeModel, fbd.SelectedPath + "/timeBox.png");




                pnger.ExportToFile(TimeErrorModel, fbd.SelectedPath + "/errorPlotTest.png");
                pnger.ExportToFile(TimeErrorModel2, fbd.SelectedPath + "/errorPlotTest2.png");

                /*
                //correlation and reverse correlation
                foreach (var time in times)
                {
                    //Correlation
                    List<string> correlationTimeToWrite = new List<string>();
                    correlationTimeToWrite.Add("Sensor&Avg Corr&Avg Sig. \\\\");


                    //Reverse correlation
                    List<string> reverseCorrelationTimeToWrite = new List<string>();
                    reverseCorrelationTimeToWrite.Add("Sensor & Avg Corr & Avg Sig. \\\\");



                    foreach (var sensor in sensors)
                    {

                        double correlationAvgCorrelation = timeTable[sensor][time].Where(x => x.Item1 >= 0).Average(x => x.Item1);
                        double correlationStdevCorrelation = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Where(x => x.Item1 >= 0).Select(x => x.Item1).ToArray());
                        double correlationAvgSignificance = timeTable[sensor][time].Where(x => x.Item1 >= 0).Average(x => x.Item2);
                        double correlationStdevSignificance = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Where(x => x.Item1 >= 0).Select(x => x.Item2).ToArray());

                        double reverseCorrelationAvgCorrelation = timeTable[sensor][time].Where(x => x.Item1 < 0).Average(x => x.Item1);
                        double reverseCorrelationStdevCorrelation = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Where(x => x.Item1 < 0).Select(x => x.Item1).ToArray());
                        double reverseCorrelationAvgSignificance = timeTable[sensor][time].Where(x => x.Item1 < 0).Average(x => x.Item2);
                        double reverseCorrelationStdevSignificance = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Where(x => x.Item1 < 0).Select(x => x.Item2).ToArray());

                        correlationTimeToWrite.Add($"{sensor}&{correlationAvgCorrelation.ToString("0.000")}({correlationStdevCorrelation.ToString("0.000")})&{correlationAvgSignificance.ToString("0.000")}({correlationStdevSignificance.ToString("0.000")}) \\\\");
                        reverseCorrelationTimeToWrite.Add($"{sensor}&{reverseCorrelationAvgCorrelation.ToString("0.000")}({reverseCorrelationStdevCorrelation.ToString("0.000")})&{reverseCorrelationAvgSignificance.ToString("0.000")}({reverseCorrelationStdevSignificance.ToString("0.000")}) \\\\");
                    }

                    foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                    {
                        correlationTimeToWrite.Add(item + " Mean: " + big5List["corr"].Average(x => x[item]).ToString("0.00") + ", SD: " + MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(big5List["corr"].Select(x => x[item]).ToArray()).ToString("0.00") + ".");
                        reverseCorrelationTimeToWrite.Add(item + " Mean: " + big5List["revCorr"].Average(x => x[item]).ToString("0.00") + ", SD: " + MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(big5List["revCorr"].Select(x => x[item]).ToArray()).ToString("0.00") + ".");
                    }

                    File.WriteAllLines(fbd.SelectedPath + "/correlationTime" + time + ".txt", correlationTimeToWrite);
                    File.WriteAllLines(fbd.SelectedPath + "/reverseCorrelationTime" + time + ".txt", reverseCorrelationTimeToWrite);
                }
                */
                var Big5timeBox = new PlotModel() { Title = "Big5 Time Box Plots", LegendPlacement = LegendPlacement.Outside };
                Dictionary<Big5, OxyPlot.Series.BoxPlotSeries> big5timeSeries = new Dictionary<Big5, OxyPlot.Series.BoxPlotSeries>();
                foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                {
                    big5timeSeries.Add(item, new OxyPlot.Series.BoxPlotSeries() { Fill = colors[(int)item * 2], Title = item.ToString(), BoxWidth = 0.1, WhiskerWidth = 0.1 });
                    Big5timeBox.Series.Add(big5timeSeries[item]);
                }

                Big5timeBox.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 50, Minimum = 10, Title = "Score" });
                Big5timeBox.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 2.5, Minimum = -0.5, MajorStep = 1, Title = "Time", MinorTickSize = 0 });

                foreach (var time in times)
                {
                    foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                    {
                        var orderino = big5List["time" + time].Select(x => x[item]).OrderBy(x => x).ToList();
                        big5timeSeries[item].Items.Add(new OxyPlot.Series.BoxPlotItem(time - 0.25 + (int)item * 0.1, orderino[0], orderino[(int)(orderino.Count * 0.25)], orderino[orderino.Count / 2], orderino[(int)(orderino.Count * 0.75)], orderino.Last()));
                    }
                }

                pnger.ExportToFile(Big5timeBox, fbd.SelectedPath + "/timeBoxBig5.png");

                foreach (var time in times)
                {
                    List<string> timeToWrite = new List<string>();
                    timeToWrite.Add("\\begin{table}");
                    timeToWrite.Add("\\centering");
                    timeToWrite.Add("{\\large \\textbf{Time " + time + "}}\\vspace{1pt}");
                    timeToWrite.Add("\\begin{tabular}{ccc}");
                    timeToWrite.Add("\\toprule");
                    timeToWrite.Add("Sensor&Avg Corr&Avg Sig. \\\\");
                    timeToWrite.Add("\\midrule");
                    foreach (var sensor in sensors)
                    {
                        double avgCorrelation = timeTable[sensor][time].Average(x => x.Item1);
                        double stdevCorrelation = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Select(x => x.Item1).ToArray());
                        double avgSignificance = timeTable[sensor][time].Average(x => x.Item2);
                        double stdevSignificance = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(timeTable[sensor][time].Select(x => x.Item2).ToArray());

                        if (avgSignificance < 0.05)
                        {
                            timeToWrite.Add($"\\textbf{{{sensor}}}&\\textbf{{{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})}}&\\textbf{{{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")})}} \\\\");
                        }
                        else
                        {
                            timeToWrite.Add($"{sensor}&{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})&{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")}) \\\\");
                        }

                    }
                    timeToWrite.Add("\\bottomrule");
                    timeToWrite.Add("\\end{tabular}");
                    timeToWrite.Add("\\caption{Results from time " + time + ".");

                    foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                    {
                        timeToWrite.Add(item + " Mean: " + big5List["time" + time].Average(x => x[item]).ToString("0.00") + ", SD: " + MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(big5List["time" + time].Select(x => x[item]).ToArray()).ToString("0.00") + ".");
                    }
                    timeToWrite.Add("}");

                    timeToWrite.Add("\\label{[TABLE] res time" + time + "}");
                    timeToWrite.Add("\\end{table}");

                    File.WriteAllLines(fbd.SelectedPath + "/" + corrType + "_time" + time + ".txt", timeToWrite);
                }

                var stimModel = new PlotModel() { Title = "Stimuli Groups Box Plot" };
                int stimId = 0;
                sensorsAdded.Clear();
                avgLineSeries.Points.Clear();

                var Big5StimBox = new PlotModel() { Title = "Big5 Stimuli Box Plots", LegendPlacement = LegendPlacement.Outside };
                Dictionary<Big5, OxyPlot.Series.BoxPlotSeries> big5Series = new Dictionary<Big5, OxyPlot.Series.BoxPlotSeries>();
                foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                {
                    big5Series.Add(item, new OxyPlot.Series.BoxPlotSeries() { Fill = colors[(int)item * 2], Title = item.ToString(), BoxWidth = 0.1, WhiskerWidth = 0.1 });
                    Big5StimBox.Series.Add(big5Series[item]);
                }

                Big5StimBox.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 50, Minimum = 10, Title = "Score" });
                Big5StimBox.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 1.5, Minimum = -0.5, MajorStep = 1, Title = "Category", MinorTickSize = 0 });
                List<string> amountStimSignificant = new List<string>();
                foreach (var stimuli in stimulis)
                {
                    List<string> stimuliToWrite = new List<string>();
                    stimuliToWrite.Add("\\begin{table}");
                    stimuliToWrite.Add("\\centering");
                    stimuliToWrite.Add("{\\large \\textbf{Stimuli " + stimuli + "}}\\vspace{1pt}");
                    stimuliToWrite.Add("\\begin{tabular}{ccc}");
                    stimuliToWrite.Add("\\toprule");
                    stimuliToWrite.Add("Sensor&Avg Corr&Avg Sig. \\\\");
                    stimuliToWrite.Add("\\midrule");
                    List<double> avgs = new List<double>();
                    int sensorId = 0;

                    var EEGAllCorrelations = new List<Tuple<double, double>>();
                    var GSRAllCorrelations = stimuliTable["GSR"][stimuli];
                    var HRAllCorrelations = stimuliTable["HR"][stimuli];

                    foreach (var sensor in sensors)
                    {
                        if (sensor.Contains("EEG"))
                        {
                            EEGAllCorrelations.AddRange(stimuliTable[sensor][stimuli]);
                        }
                        double avgCorrelation = stimuliTable[sensor][stimuli].Average(x => x.Item1);
                        double stdevCorrelation = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(stimuliTable[sensor][stimuli].Select(x => x.Item1).ToArray());
                        double avgSignificance = stimuliTable[sensor][stimuli].Average(x => x.Item2);
                        double stdevSignificance = MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(stimuliTable[sensor][stimuli].Select(x => x.Item2).ToArray());

                        var orderedAll = stimuliTable[sensor][stimuli].Where(x => x.Item2 * 100 < (int)5).OrderBy(x => x.Item1).ToList();
                        amountStimSignificant.Add(stimuli + " & " + sensor + " & " + orderedAll.Count);
                        significantAmount[sensor][stimuli == "low" ? 3 : 4] = orderedAll.Count;
                        significantAmountMax[sensor][stimuli == "low" ? 3 : 4] = stimuliTable[sensor][stimuli].Count;
                        //significantCorr[sensor][stimuli == "low" ? 3 : 4] = Tuple.Create(orderedAll.Average(x => x.Item1), MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(orderedAll.Select(x => x.Item1).ToArray()));
                        var boxItem = new OxyPlot.Series.BoxPlotItem(((1 + stimId) % 2) + sensorId * width - (0.5 * width * sensors.Count), orderedAll[0].Item1, orderedAll[(int)(orderedAll.Count * 0.25)].Item1, orderedAll[orderedAll.Count / 2].Item1, orderedAll[(int)(orderedAll.Count * 0.75)].Item1, orderedAll.Last().Item1);
                        //var boxItem = new OxyPlot.Series.BoxPlotItem(sensorId + ((1 + stimId) % 2) * widthTime - (0.5 * widthTime), orderedAll[0].Item1, orderedAll[(int)(orderedAll.Count * 0.25)].Item1, orderedAll[orderedAll.Count / 2].Item1, orderedAll[(int)(orderedAll.Count * 0.75)].Item1, orderedAll.Last().Item1);
                        var boxSeries = new OxyPlot.Series.BoxPlotSeries() { };
                        boxSeries.BoxWidth = width;
                        boxSeries.WhiskerWidth = width;
                        //boxSeries.BoxWidth = widthTime;
                        //boxSeries.WhiskerWidth = widthTime;
                        boxSeries.Items.Add(boxItem);
                        boxSeries.Fill = colors[sensorId];
                        //stimModel.Series.Add(boxSeries);
                        avgs.Add(orderedAll.Average(x => x.Item1));

                        if (!sensorsAdded.Contains(sensor))
                        {
                            sensorsAdded.Add(sensor);
                            boxSeries.Title = sensor;
                        }


                        if (avgSignificance < 0.05)
                        {
                            stimuliToWrite.Add($"\\textbf{{{sensor}}}&\\textbf{{{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})}}&\\textbf{{{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")})}} \\\\");
                        }
                        else
                        {
                            stimuliToWrite.Add($"{sensor}&{avgCorrelation.ToString("0.000")}({stdevCorrelation.ToString("0.000")})&{avgSignificance.ToString("0.000")}({stdevSignificance.ToString("0.000")}) \\\\");
                        }
                        sensorId++;
                    }

                    double boxWidth = 0.3;

                    //eeg
                    EEGAllCorrelations = EEGAllCorrelations.OrderBy(x => x.Item1).ToList();
                    var EEGSeries = new OxyPlot.Series.BoxPlotSeries() { Fill = EEGColor, BoxWidth = boxWidth, WhiskerWidth = boxWidth };
                    if (stimuli == "low") EEGSeries.Title = "EEG";
                    var EEGItem = CreateBoxItem(EEGAllCorrelations);
                    EEGItem.X = (stimuli == "low" ? 0 : 1) - EEGSeries.BoxWidth * 1;
                    EEGSeries.Items.Add(EEGItem);
                    stimModel.Series.Add(EEGSeries);

                    AddCorrelation("EEG", stimuli == "low" ? 3 : 4, EEGAllCorrelations);

                    foreach (var cor in EEGAllCorrelations)
                    {
                        AnovaIndividual.Add(anovaIndividualId + ";" + cor.Item1);
                    }
                    AnovaIndividualLegend.Add(anovaIndividualId++ + "=stimuli_" + stimuli + "_EEG");

                    //gsr
                    GSRAllCorrelations = GSRAllCorrelations.OrderBy(x => x.Item1).ToList();
                    var GSRSeries = new OxyPlot.Series.BoxPlotSeries() { Fill = EDAColor, BoxWidth = boxWidth, WhiskerWidth = boxWidth };
                    if (stimuli == "low") GSRSeries.Title = "EDA";
                    var GSRItem = CreateBoxItem(GSRAllCorrelations);
                    GSRItem.X = (stimuli == "low" ? 0 : 1);
                    GSRSeries.Items.Add(GSRItem);
                    stimModel.Series.Add(GSRSeries);

                    AddCorrelation("EDA", stimuli == "low" ? 3 : 4, GSRAllCorrelations);

                    foreach (var cor in GSRAllCorrelations)
                    {
                        AnovaIndividual.Add(anovaIndividualId + ";" + cor.Item1);
                    }
                    AnovaIndividualLegend.Add(anovaIndividualId++ + "=stimuli_" + stimuli + "_GSR");

                    //hr
                    HRAllCorrelations = HRAllCorrelations.OrderBy(x => x.Item1).ToList();
                    var HRSeries = new OxyPlot.Series.BoxPlotSeries() { Fill = HRColor, BoxWidth = boxWidth, WhiskerWidth = boxWidth };
                    if (stimuli == "low") HRSeries.Title = "HR";
                    var HRItem = CreateBoxItem(HRAllCorrelations);
                    HRItem.X = (stimuli == "low" ? 0 : 1) + HRSeries.BoxWidth * 1;
                    HRSeries.Items.Add(HRItem);
                    stimModel.Series.Add(HRSeries);

                    AddCorrelation("HR", stimuli == "low" ? 3 : 4, HRAllCorrelations);

                    foreach (var cor in HRAllCorrelations)
                    {
                        AnovaIndividual.Add(anovaIndividualId + ";" + cor.Item1);
                    }
                    AnovaIndividualLegend.Add(anovaIndividualId++ + "=stimuli_" + stimuli + "_HR");

                    //avg
                    var AVGAllCorrelations = EEGAllCorrelations.Concat(GSRAllCorrelations.Concat(HRAllCorrelations)).ToList();

                    //AddCorrelation("AVG", stimuli == "low" ? 3 : 4, AVGAllCorrelations);

                    foreach (var cor in AVGAllCorrelations)
                    {
                        AnovaAvg.Add(anovaAvgId + ";" + cor.Item1);
                    }
                    AnovaAvgLegend.Add(anovaAvgId++ + "=stimuli_" + stimuli);

                    avgLineSeries.Points.Add(new OxyPlot.DataPoint(0, avgs.Average()));
                    stimuliToWrite.Add("\\bottomrule");
                    stimuliToWrite.Add("\\end{tabular}");
                    stimuliToWrite.Add("\\caption{Results from stimuli " + stimuli + ".");
                    foreach (Big5 item in Enum.GetValues(typeof(Big5)))
                    {
                        stimuliToWrite.Add(item + " Mean: " + big5List["stim" + stimuli].Average(x => x[item]).ToString("0.00") + ", SD: " + MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(big5List["stim" + stimuli].Select(x => x[item]).ToArray()).ToString("0.00") + ".");
                        var orderino = big5List["stim" + stimuli].Select(x => x[item]).OrderBy(x => x).ToList();
                        big5Series[item].Items.Add(new OxyPlot.Series.BoxPlotItem(((1 + stimId) % 2) - 0.25 + (int)item * 0.1, orderino[0], orderino[(int)(orderino.Count * 0.25)], orderino[orderino.Count / 2], orderino[(int)(orderino.Count * 0.75)], orderino.Last()));
                    }
                    stimuliToWrite.Add("}");
                    stimuliToWrite.Add("\\label{[TABLE] res stimuli" + stimuli + "}");
                    stimuliToWrite.Add("\\end{table}");


                    File.WriteAllLines(fbd.SelectedPath + "/" + corrType + "_stimuli_" + stimuli + ".txt", stimuliToWrite);
                    stimId++;
                }
                File.WriteAllLines(fbd.SelectedPath + "/significantStim.tex", amountStimSignificant);
                List<string> sigAmountLines = new List<string>();
                foreach (var sensor in sensors)
                {
                    string linerino = sensor;
                    for (int i = 0; i < 5; i++)
                    {
                        linerino += $" & {significantAmount[sensor][i]}/{significantAmountMax[sensor][i]}";
                    }
                    sigAmountLines.Add(linerino + "\\\\");
                }
                File.WriteAllLines(fbd.SelectedPath + "/significantTable.tex", sigAmountLines);
                //File.WriteAllLines(fbd.SelectedPath + "/significantTable.tex", significantAmount.Select(x => $"{x.Key} & {x.Value[0]} & {x.Value[1]} & {x.Value[2]} & {x.Value[3]} & {x.Value[4]}").ToList());
                File.WriteAllLines(fbd.SelectedPath + "/significantCorrTable.tex", significantCorr.Select(x => $"{x.Key} & {x.Value[0].Item1.ToString(".000")}({x.Value[0].Item2.ToString(".000")}) & {x.Value[1].Item1.ToString(".000")}({x.Value[1].Item2.ToString(".000")}) & {x.Value[2].Item1.ToString(".000")}({x.Value[2].Item2.ToString(".000")}) & {x.Value[3].Item1.ToString(".000")}({x.Value[3].Item2.ToString(".000")}) & {x.Value[4].Item1.ToString(".000")}({x.Value[4].Item2.ToString(".000")}) \\\\"));
                File.WriteAllLines(fbd.SelectedPath + "/significantCorrTableTime.tex", significantCorr.Select(x => $"{x.Key} & {x.Value[0].Item1.ToString(".000")} (SD={x.Value[0].Item2.ToString(".000")}, p={x.Value[0].Item3.ToString(".000000")}) & {x.Value[1].Item1.ToString(".000")} (SD={x.Value[1].Item2.ToString(".000")}, p={x.Value[1].Item3.ToString(".000000")}) & {x.Value[2].Item1.ToString(".000")} (SD={x.Value[2].Item2.ToString(".000")}, p={x.Value[2].Item3.ToString(".000000")}) \\\\"));
                File.WriteAllLines(fbd.SelectedPath + "/significantCorrTableStimuli.tex", significantCorr.Select(x => $"{x.Key} & {x.Value[3].Item1.ToString(".000")} (SD={x.Value[3].Item2.ToString(".000")}, p={x.Value[3].Item3.ToString(".000")}) & {x.Value[4].Item1.ToString(".000")} (SD={x.Value[4].Item2.ToString(".000")}, p={x.Value[4].Item3.ToString(".000")}) \\\\"));


                List<string> timeLines = new List<string>() { "sensor & 0 vs 1 & 1 vs 2 & 0 vs 2" };
                List<string> stimLines = new List<string>() { "sensor & 0 vs Low & Low vs High & 0 vs High" };
                foreach (var item in significantCorr)
                {
                    var z01 = ZCalc(item.Value[0].Item1, Convert.ToInt32(item.Value[0].Item2), item.Value[1].Item1, Convert.ToInt32(item.Value[1].Item2));
                    var z12 = ZCalc(item.Value[1].Item1, Convert.ToInt32(item.Value[1].Item2), item.Value[2].Item1, Convert.ToInt32(item.Value[2].Item2));
                    var z02 = ZCalc(item.Value[0].Item1, Convert.ToInt32(item.Value[0].Item2), item.Value[2].Item1, Convert.ToInt32(item.Value[2].Item2));
                    var p01 = ZtoP(z01);
                    var p12 = ZtoP(z12);
                    var p02 = ZtoP(z02);
                    timeLines.Add($"{item.Key} & z: {z01} | p: {p01} & z: {z12} | p: {p12} & z: {z02} | p: {p02}");

                    var z0Low = ZCalc(item.Value[0].Item1, Convert.ToInt32(item.Value[0].Item2), item.Value[3].Item1, Convert.ToInt32(item.Value[3].Item2));
                    var zLowHigh = ZCalc(item.Value[3].Item1, Convert.ToInt32(item.Value[3].Item2), item.Value[4].Item1, Convert.ToInt32(item.Value[4].Item2));
                    var z0High = ZCalc(item.Value[0].Item1, Convert.ToInt32(item.Value[0].Item2), item.Value[4].Item1, Convert.ToInt32(item.Value[4].Item2));
                    var p0Low = ZtoP(z0Low);
                    var pLowHigh = ZtoP(zLowHigh);
                    var p0High = ZtoP(z0High);
                    stimLines.Add($"{item.Key} & z: {z0Low} | p: {p0Low} & z: {zLowHigh} | p: {pLowHigh} & z: {z0High} | p: {p0High}");
                }

                File.WriteAllLines(fbd.SelectedPath + "/significantCorrCompareTime.tex", timeLines);
                File.WriteAllLines(fbd.SelectedPath + "/significantCorrCompareStimuli.tex", stimLines);


                pnger.ExportToFile(Big5StimBox, fbd.SelectedPath + "/stimBoxBig5.png");

                stimModel.LegendPlacement = LegendPlacement.Outside;

                //index 1 = low
                var stimTxt0 = new OxyPlot.Annotations.TextAnnotation() { TextPosition = new OxyPlot.DataPoint(0, -1), Text = "Avg " + avgLineSeries.Points[1].Y.ToString(".000").Replace(",", "."), Stroke = OxyColors.White };
                //index 0 = high
                var stimTxt1 = new OxyPlot.Annotations.TextAnnotation() { TextPosition = new OxyPlot.DataPoint(1, -1), Text = "Avg " + avgLineSeries.Points[0].Y.ToString(".000").Replace(",", "."), Stroke = OxyColors.White };
                stimModel.Annotations.Add(stimTxt0);
                stimModel.Annotations.Add(stimTxt1);
                stimModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Left, Maximum = 1, Minimum = -1, Title = "Pearson's r" });
                stimModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = 1.5, Minimum = -0.5, MajorStep = 1, Title = "Stimuli", MinorTickSize = 0 });
                //stimModel.Axes.Add(new OxyPlot.Axes.LinearAxis() { Position = OxyPlot.Axes.AxisPosition.Bottom, Maximum = sensors.Count - 0.5, Minimum = -0.5, MajorStep = 1, Title = "Sensors", MinorTickSize = 0 });

                pnger.ExportToFile(stimModel, fbd.SelectedPath + "/stimBox.png");


                File.WriteAllLines(fbd.SelectedPath + "/anovaIndividual.csv", AnovaIndividual);
                File.WriteAllLines(fbd.SelectedPath + "/anovaIndividualLegend.csv", AnovaIndividualLegend);
                File.WriteAllLines(fbd.SelectedPath + "/anovaAvg.csv", AnovaAvg);
                File.WriteAllLines(fbd.SelectedPath + "/anovaAvgLegend.csv", AnovaAvgLegend);

                Log.LogMessage("DonnoDK");
            }
        }

        static OxyPlot.Series.BoxPlotItem CreateBoxItem(List<Tuple<double, double>> orderedAll)
        {
            return new OxyPlot.Series.BoxPlotItem(0, orderedAll[0].Item1, orderedAll[(int)(orderedAll.Count * 0.25)].Item1, orderedAll[orderedAll.Count / 2].Item1, orderedAll[(int)(orderedAll.Count * 0.75)].Item1, orderedAll.Last().Item1);
        }


        #endregion

        private Color Event2Color(string eventName)
        {
            if (eventName.Contains("AddAttachmentButtonClick:") ||
                eventName.Contains("CreateDraft, language changed to") ||
                eventName.Contains("Changed"))
            {
                return Color.FromArgb(255, 255, 255, 100);
            }
            else if (eventName.Contains("RemoveContact clicked") ||
                eventName.Contains("SendDraft error shown") ||
                eventName.Contains("Task: NotResponding"))
            {
                return Color.Red;
            }
            else if (eventName.Contains("Add Contact Button click"))
            {
                return Color.Green;
            }
            else
            {
                return Color.Transparent;
            }
        }


        private void updateChart()
        {
            noveltyChart.Series.Clear();
            for (int i = 0; i < events.Length; i++)
            {
                try
                {
                    noveltyChart.Series.Add(new Series(events[i].Split('#')[1]));
                    noveltyChart.Series[events[i].Split('#')[1]].Points.AddXY(int.Parse(events[i].Split('#')[0]), 1.1);
                    noveltyChart.Series[events[i].Split('#')[1]].Color = Event2Color(events[i].Split('#')[1]);
                    noveltyChart.Series[events[i].Split('#')[1]].IsVisibleInLegend = false;
                }
                catch { }
            }
            int includedSevents = 0;
            if (samEvents.Checked)
            {
                for (int i = 0; i < sEvents.Count; i++)
                {
                    string arVal = (samEventsFilterArousal.Text.Length > 0) ? samEventsFilterArousal.Text : "0";
                    string vaVal = (samEventsFilterValence.Text.Length > 0) ? samEventsFilterValence.Text : "0";

                    if (sEvents[i].valence <= int.Parse(vaVal) && sEvents[i].arousal <= int.Parse(arVal) || !samEventsFilter.Checked)
                    {
                        noveltyChart.Series.Add(new Series("UserSAM" + i));
                        noveltyChart.Series["UserSAM" + i].Points.AddXY(sEvents[i].timestamp, 1.1);
                        noveltyChart.Series["UserSAM" + i].Color = Color.Magenta;
                        noveltyChart.Series["UserSAM" + i].IsVisibleInLegend = false;
                        includedSevents++;
                    }
                }
            }

            Series tmpS = new Series("test");
            noveltyChart.Series.Add(tmpS);
            tmpS.ChartType = SeriesChartType.StackedColumn;
            noveltyChart.Series["test"].IsVisibleInLegend = false;

            foreach (var outlier in timestampsOutliers)
            {
                noveltyChart.Series["test"].Points.AddXY(outlier, 0.5);
            }

            Log.LogMessage("Included SAM Events: " + includedSevents + "/" + sEvents.Count);
        }

        string[] events = new string[0];
        List<samEvents> sEvents = new List<samEvents>();
        List<int> timestampsOutliers = new List<int>();

        private void button1_Click(object sender, EventArgs e)
        {
            noveltyChart.ChartAreas.First().BackColor = Color.DarkGray;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.
                OK)
            {
                string path = fbd.SelectedPath;
                string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];

                fdNovelty.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);
                events = File.ReadAllLines(path + @"\SecondTest.dat");

                string[] tmpSevents = File.ReadAllLines(path + @"\sam.dat");
                foreach (string ev in tmpSevents)
                {
                    sEvents.Add(new samEvents(int.Parse(ev.Split(':')[0]), int.Parse(ev.Split(':')[1]), int.Parse(ev.Split(':')[2])));
                }

            }
            if (events.Length == 0)
            {
                //if events is not assigned with second test data
                return;
            }
            int start = (useRestInTraining.Checked) ? 180000 : 0;
            int trainingEnd = int.Parse(events[2].Split('#')[0]);
            int windowSize = 5000;
            int stepSize = 100;
            int delay = 2000;


            //Split into training & prediction set
            List<List<double>> featureVectors = new List<List<double>>();
            List<int> timeStamps = new List<int>();

            for (int i = 0; i < fdNovelty.gsrData.Last().timestamp - fdNovelty.gsrData.First().timestamp - windowSize; i += stepSize)
            {
                List<double> featureVector = new List<double>();
                List<double> data = fdNovelty.gsrData.SkipWhile(x => (x.timestamp - fdNovelty.gsrData.First().timestamp) < i).TakeWhile(x => i + windowSize > (x.timestamp - fdNovelty.gsrData.First().timestamp)).Select(x => (double)x.resistance).ToList();
                if (data.Count == 0) continue;
                featureVector.Add(data.Average());
                featureVector.Add(data.Max());
                featureVector.Add(data.Min());
                double avg = data.Average();
                double sd = Math.Sqrt(data.Average(x => Math.Pow(x - avg, 2)));
                featureVector.Add(sd);
                featureVectors.Add(featureVector);
                timeStamps.Add(i);
            }

            featureVectors = featureVectors.NormalizeFeatureList<double>(Normalize.OneMinusOne).ToList();
            var dataSet = featureVectors.Zip(timeStamps, (first, second) => { return Tuple.Create(first, second); });

            var trainingSet = dataSet.SkipWhile(x => x.Item2 < start).TakeWhile(x => x.Item2 < trainingEnd);
            var predictionSet = dataSet.SkipWhile(x => x.Item2 < trainingEnd);

            int count = predictionSet.Count();
            int firstPredcition = predictionSet.First().Item2;
            OneClassClassifier occ = new OneClassClassifier(trainingSet.Select(x => x.Item1).ToList());
            SVMParameter svmP = new SVMParameter();
            svmP.Kernel = SVMKernelType.RBF;
            svmP.C = 100;
            svmP.Gamma = 0.01;
            svmP.Nu = 0.01;
            svmP.Type = SVMType.ONE_CLASS;
            occ.CreateModel(svmP);
            /* List<int> indexes = occ.PredictOutliers(predictionSet.Select(x => x.Item1).ToList());

             foreach (int index in indexes)
             {
                 timestampsOutliers.Add(predictionSet.ElementAt(index).Item2 - firstPredcition + 180000 + 4000);
             }
             */

            updateChart();

            int k = 0;

        }

        private void samEvents_CheckedChanged(object sender, EventArgs e)
        {
            updateChart();
        }

        private void samEventsFilter_CheckedChanged(object sender, EventArgs e)
        {
            updateChart();
        }

        private void samEventsFilterArousal_TextChanged(object sender, EventArgs e)
        {
            if (samEventsFilter.Checked)
                updateChart();
        }

        private void samEventsFilterValence_TextChanged(object sender, EventArgs e)
        {
            if (samEventsFilter.Checked)
                updateChart();
        }

        private void btn_anomalyDetection_Click(object sender, EventArgs e)
        {
            AnomalyDetection a = new AnomalyDetection();
            a.ShowDialog(this);
        }

        public enum Big5
        {
            Extraversion = 1,
            Agreeableness,
            Conscientiousness,
            EmotionalStability,
            IntellectImagination
        }

        Dictionary<Big5, int> GetBig5(string[] metaText)
        {
            Dictionary<Big5, int> retVal = new Dictionary<Big5, int>();

            foreach (var item in metaText)
            {
                if (item.Contains("big"))
                {
                    var split = item.Split('=');

                    retVal.Add((Big5)int.Parse(split[0].Replace("big", "")), int.Parse(split[1]));
                }
            }

            return retVal;
        }

        private void btn_CalcSam_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            //if (fbd.ShowDialog() == DialogResult.OK)
            //{
            //    string[] dirs = Directory.GetDirectories(fbd.SelectedPath);
            //    List<string> skipped = File.ReadAllLines(fbd.SelectedPath + "/anova_skipped.txt").Select(x => x.Split(new string[] { "_", "tp" }, StringSplitOptions.RemoveEmptyEntries).First()).Distinct().ToList();

            //    foreach (var dir in dirs)
            //    {
            //        SAMData.LoadFromPath(dir + "/SAMData.json");
            //    }
            //}

            var fbd = new FolderBrowserDialog();
            List<string> groupings = new List<string>()
            {
                "Stimuli high",
                "Stimuli low",
                "Time 0",
                "Time 1",
                "Time 2"
            };

            //grouping -> sensor -> cost
            var dtwCost = new Dictionary<string, Dictionary<string, List<double>>>();
            var dtwBeforeLength = new Dictionary<string, Dictionary<string, List<int>>>();
            var dtwBeforeCost = new Dictionary<string, Dictionary<string, List<double>>>();
            var dtwAfterLength = new Dictionary<string, Dictionary<string, List<int>>>();
            var dtwAfterCost = new Dictionary<string, Dictionary<string, List<double>>>();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                foreach (var grouping in groupings)
                {
                    dtwCost.Add(grouping, new Dictionary<string, List<double>>());
                    dtwBeforeLength.Add(grouping, new Dictionary<string, List<int>>());
                    dtwBeforeCost.Add(grouping, new Dictionary<string, List<double>>());
                    dtwAfterLength.Add(grouping, new Dictionary<string, List<int>>());
                    dtwAfterCost.Add(grouping, new Dictionary<string, List<double>>());

                    var files = Directory.GetFiles($"{fbd.SelectedPath}/{grouping}").Where(x => x.Contains("dtw")).ToList();

                    foreach (var file in files)
                    {
                        string sensor = file.Split('\\').Last().Split('.').First().Split('_')[2];
                        if (!dtwCost[grouping].ContainsKey(sensor))
                        {
                            dtwCost[grouping].Add(sensor, new List<double>());
                            dtwBeforeLength[grouping].Add(sensor, new List<int>());
                            dtwBeforeCost[grouping].Add(sensor, new List<double>());
                            dtwAfterLength[grouping].Add(sensor, new List<int>());
                            dtwAfterCost[grouping].Add(sensor, new List<double>());
                        }

                        var lines = File.ReadAllLines(file);
                        dtwCost[grouping][sensor].Add(lines[0].Split('=').Last().ParseDouble());
                        dtwBeforeLength[grouping][sensor].Add(int.Parse(lines[1].Split('=').Last()));
                        dtwBeforeCost[grouping][sensor].Add(lines[2].Split('=').Last().ParseDouble());
                        dtwAfterLength[grouping][sensor].Add(int.Parse(lines[3].Split('=').Last()));
                        dtwAfterCost[grouping][sensor].Add(lines[4].Split('=').Last().ParseDouble());
                    }
                }

                foreach (var grouping in groupings)
                {
                    List<string> shit = new List<string>();
                    shit.Add("Sensor & Cost & CostRatio1 & CostRatio2");
                    foreach (var sensor in dtwCost[grouping])
                    {
                        //shit.Add($"{sensor.Key} & {dtwCost[grouping][sensor.Key].Average()} (SD={dtwCost[grouping][sensor.Key].STDEV()}) & {dtwBeforeCost[grouping][sensor.Key].Average()} (SD={dtwBeforeCost[grouping][sensor.Key].STDEV()}) & {dtwAfterCost[grouping][sensor.Key].Average()} (SD={dtwAfterCost[grouping][sensor.Key].STDEV()})");
                        shit.Add($"{sensor.Key} & {dtwCost[grouping][sensor.Key].Average()} (SD={dtwCost[grouping][sensor.Key].STDEV()}) & {dtwCost[grouping][sensor.Key].Sum() / dtwBeforeLength[grouping][sensor.Key].Sum()} (SD={dtwBeforeCost[grouping][sensor.Key].STDEV()}) & {dtwCost[grouping][sensor.Key].Sum() / dtwAfterLength[grouping][sensor.Key].Sum()} (SD={dtwAfterCost[grouping][sensor.Key].STDEV()}) & {dtwAfterLength[grouping][sensor.Key].Sum() / (double)dtwBeforeLength[grouping][sensor.Key].Sum()}");
                    }

                    File.WriteAllLines(fbd.SelectedPath + "/dtw_" + grouping + ".tex", shit);
                }

                Log.LogMessage("DonnoDK!");
            }
        }

        private void btn_GenerateScatter_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in Directory.GetFiles(fbd.SelectedPath).Where(x => x.EndsWith(".txt")))
                {
                    using (var f = File.OpenText(file))
                    {
                        List<double> As = new List<double>(2000);
                        List<double> Bs = new List<double>(2000);

                        double min = 1;
                        double max = 0;

                        do
                        {
                            string line = f.ReadLine();
                            if (line != "")
                            {
                                double aVal = double.Parse(line.Split(';')[0].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                                double bVal = double.Parse(line.Split(';')[1].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);

                                min = Math.Min(aVal, Math.Min(bVal, min));
                                max = Math.Max(aVal, Math.Max(bVal, max));

                                As.Add(aVal);
                                Bs.Add(bVal);
                            }
                        }
                        while (!f.EndOfStream);

                        SavePngScatter(file + "_scatter.png", file, As, Bs, min, max);
                        Log.LogMessage("Donno" + file);
                    }
                }
            }
        }

        public static double Fisher(double input)
        {
            return (Math.Log(1 + input) - Math.Log(1 - input)) / 2;
        }

        public static double FisherInverse(double input)
        {
            return (Math.Pow(Math.E, 2 * input) - 1) / (Math.Pow(Math.E, 2 * input) + 1);
        }

        /// <summary>
        /// From http://vassarstats.net/rdiff.html, doesn't work
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="na"></param>
        /// <param name="rb"></param>
        /// <param name="nb"></param>
        /// <returns></returns>
        public static Tuple<double, double, double> FisherCompare1(double ra, int na, double rb, int nb)
        {
            if (ra == 1) ra = 0.999;
            if (rb == 1) rb = 0.999;

            var raplus = ra + 1;
            var raminus = 1 - ra;
            var rbplus = rb + 1;
            var rbminus = 1 - rb;

            var za = (Math.Log(raplus) - Math.Log(raminus)) / 2;
            var zb = (Math.Log(rbplus) - Math.Log(rbminus)) / 2;

            var se = Math.Sqrt((1 / (na - 3)) + (1 / (nb - 3)));
            var z = (za - zb) / se;
            //var z = Math.Round(z * 100) / 100;

            var z2 = Math.Abs(z);

            var p2 = (((((.000005383 * z2 + .0000488906) * z2 + .0000380036) * z2 + .0032776263) * z2 + .0211410061) * z2 + .049867347) * z2 + 1;

            p2 = Math.Pow(p2, -16);
            var p1 = p2 / 2;

            return Tuple.Create(z2, p1, p2);
        }

        /// <summary>
        /// From http://www.quantpsy.org/corrtest/corrtest.htm, doesn't work
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="na"></param>
        /// <param name="rb"></param>
        /// <param name="nb"></param>
        /// <returns></returns>
        public static Tuple<double, double, double> FisherCompare2(double ra, int na, double rb, int nb)
        {
            var x1 = ra + 1;
            var x2 = 1 + -1 * ra;
            var y1 = rb + 1;
            var y2 = 1 + -1 * rb;

            var zz1 = 0.5 * Math.Log(x1) - Math.Log(x2);
            var zz2 = 0.5 * Math.Log(y1) - Math.Log(y2);
            var zz = (zz1 - zz2) / (Math.Sqrt(1 / (na - 3) + 1 / (nb - 3)));

            var chi = new MathNet.Numerics.Distributions.ChiSquared(1);

            var pp2 = chi.CumulativeDistribution(zz * zz);
            var pp1 = pp2 / 2;

            return Tuple.Create(zz, pp1, pp2);
        }

        public static double ZtoP(double z)
        {
            var pstuff = MathNet.Numerics.Integrate.OnClosedInterval(t => 1 / Math.Sqrt(2 * Math.PI) * Math.Pow(Math.E, -(t * t) / 2), -99999, z);
            var twotail = 2 * (1 - pstuff);
            return twotail;
        }

        public static double ZCalc(double corrA, int countA, double corrB, int countB)
        {
            double stuff1 = Fisher(corrA);
            double stuff2 = Fisher(corrB);

            var se = Math.Sqrt((1 / (double)(countA - 3)) + (1 / (double)(countB - 3)));

            return (stuff1 - stuff2) / se;
        }
    }
}

class TaskStartEnd
{
    public int start;
    public int end;
    public string filenameAppend;

    public TaskStartEnd(int start, int end, string filenameAppend)
    {
        this.start = start;
        this.end = end;
        this.filenameAppend = filenameAppend;
    }
}

public static class Extensions
{
    public static double ParseDouble(this string input)
    {
        return double.Parse(input.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double STDEV(this IEnumerable<double> input)
    {
        return input.ToArray().STDEV();
    }

    public static double STDEV(this List<double> input)
    {
        return input.ToArray().STDEV();
    }

    public static double STDEV(this double[] input)
    {
        return MathNet.Numerics.Statistics.ArrayStatistics.PopulationStandardDeviation(input);
    }

    public static double STDEV(this List<int> input)
    {
        return input.Select(x => (double)x).ToArray().STDEV();
    }

    public static double FisherCombineP(this IEnumerable<double> input)
    {
        int pCounter = 0;
        double accum = 0;
        foreach (var p in input)
        {
            pCounter++;
            accum += Math.Log(Math.Max(0.0001, p));
        }
        accum *= -2;
        var chi2cdf = new MathNet.Numerics.Distributions.ChiSquared(pCounter * 2);
        var retVal = 1 - chi2cdf.CumulativeDistribution(accum);

        return retVal;
    }


}