using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
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
                if (FeatureCreator.EEGDataSlice(_fd.eegData.ToList<DataReading>(), samD).Count == 0 ||
                    FeatureCreator.GSRDataSlice(_fd.gsrData.ToList<DataReading>(), samD).Count == 0 ||
                    (FeatureCreator.HRDataSlice(_fd.hrData.ToList<DataReading>(), samD).Count == 0 || FeatureCreator.HRDataSlice(_fd.hrData.ToList<DataReading>(), samD).Cast<HRDataReading>().Count(x => x.isBeat) == 0) ||
                    FeatureCreator.FaceDataSlice(_fd.faceData.ToList<DataReading>(), samD).Count == 0)
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
            svmConfs.Clear();

            Log.LogMessage("Looking for configurations...");

            //if (Directory.Exists(path + @"\STD"))
            //{
            //    var files = Directory.GetFiles(path + @"\STD");
            //    Log.LogMessage("Found STD! Contains " + files.Length + " configurations.");
            //    foreach (var item in files)
            //    {
            //        svmConfs.Add(SVMConfiguration.Deserialize(File.ReadAllText(item)));
            //    }

            //}

            //if (Directory.Exists(path + @"\META"))
            //{
            //    var files = Directory.GetFiles(path + @"\META");
            //    Log.LogMessage("Found META! Contains " + files.Length + " configurations.");
            //    foreach (var item in files)
            //    {
            //        metaConfs.Add(MetaSVMConfiguration.Deserialize(File.ReadAllText(item)));
            //    }
            //}

            //if (svmConfs.Count == 0 && metaConfs.Count == 0)
            //{
            //    Log.LogMessage("No configurations found, maybe you should run some optimizations on some features.");
            //}
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

                    if (!LoadData(item, _fd))
                    {
                        Log.LogMessage(item.Split('-').Last() + " is not classifiable");
                        continue;
                    }

                    string personName = item.Split('\\').Last();
                    eh.AddPersonToBooks(personName);

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

                    foreach (Excel.Worksheet currentSheet in current.Sheets)
                    {
                        if (currentSheet.Name == "First" || currentSheet.Name == "Last" || currentSheet.Name == "Overview")
                        {
                            continue;
                        }

                        string tpName = "tp" + currentSheet.Name.Split(' ').Last();

                        var accuracies = new Dictionary<SAMDataPoint.FeelingModel, double>();
                        foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                        {
                            accuracies.Add(feel, -1);
                        }

                        #region TryValues
                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Arousal2High] = currentSheet.Cells[2, 3].Value;
                        }
                        catch { };

                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Arousal2Low] = currentSheet.Cells[15, 3].Value;
                        }
                        catch { };

                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Arousal3] = currentSheet.Cells[28, 3].Value;
                        }
                        catch { };

                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Valence2High] = currentSheet.Cells[44, 3].Value;
                        }
                        catch { };
                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Valence2Low] = currentSheet.Cells[57, 3].Value;
                        }
                        catch { };
                        try
                        {
                            accuracies[SAMDataPoint.FeelingModel.Valence3] = currentSheet.Cells[70, 3].Value;
                        }
                        catch { };
                        #endregion

                        foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                        {
                            if (accuracies[feel] == -1) continue;

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


                exc.Quit();
                Log.LogMessage("Closing Excel");
            }
        }

        #endregion

        #region Plotting
        private void btn_ExportPNG_Click(object sender, EventArgs e)
        {
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

        void btn_CalculateResults_Click(Object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                FolderBrowserDialog sfd = new FolderBrowserDialog() { Description = "Select folder to load excel sheets from" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    new Thread(() => Threaded_Calc(fbd.SelectedPath, sfd.SelectedPath)).Start();
                }
            }
        }

        private void Threaded_Calc(string fbdPath, string sfdPath)
        {
            fbdPath = @"D:\temp\DataReadings";
            sfdPath = @"D:\temp\Excels";

            Log.LogMessage("Starting huge calc");
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
                [SAMDataPoint.FeelingModel.Arousal2High] = 2,
                [SAMDataPoint.FeelingModel.Arousal2Low] = 15,
                [SAMDataPoint.FeelingModel.Arousal3] = 28,
                [SAMDataPoint.FeelingModel.Valence2High] = 44,
                [SAMDataPoint.FeelingModel.Valence2Low] = 57,
                [SAMDataPoint.FeelingModel.Valence3] = 70
            };

            //test subject[SAMModel[machineType]]
            //feeling, List of persons(name, results for each sensor) 
            Dictionary<SAMDataPoint.FeelingModel, List<Tuple<string, Dictionary<string, double>>>> results = new Dictionary<SAMDataPoint.FeelingModel, List<Tuple<string, Dictionary<string, double>>>>();
            //Add Lists 
            foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
            {
                results.Add(feel, new List<Tuple<string, Dictionary<string, double>>>());
            }

            #region [Data from books]
            foreach (var configBook in singles.Concat(fusion))
            {
                Log.LogMessage($"Reading {configBook}...");
                Excel.Workbook dataBook = exc.Workbooks.Open(sfdPath + $"/{configBook}.xlsx", ExcelHandler.missingValue,
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

                    string newName = sheet.Name.Split(' ').Last();
                    if (configBook == "GSR")
                    {
                        foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                        {
                            if (!feel.ToString().Contains("Valence"))
                            {
                                Excel.Range accuracy = (Excel.Range)sheet.Cells[cellOffsets[feel], 3];
                                results[feel].First(x => x.Item1 == newName).Item2.Add(configBook, (double)accuracy.Value);
                            }
                        }
                    }
                    else
                    {

                        foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                        {
                            Excel.Range accuracy = (Excel.Range)sheet.Cells[cellOffsets[feel], 3];
                            results[feel].First(x => x.Item1 == newName).Item2.Add(configBook, (double)accuracy.Value);

                        }


                    }
                    dataBook.Close();
                }

                #endregion

                #region [Add Naiv results]
                foreach (var feelingModel in results.Keys)
                {
                    foreach (var person in results[feelingModel])
                    {
                        samData = SAMData.LoadFromPath(fbdPath + $"/{person.Item1}" + @"\SAM.json");
                        var sams = samData.dataPoints.Select(x => x.ToAVCoordinate(feelingModel)).ToList();
                        List<int> classes = sams.Distinct().ToList();
                        int maxClassCount = 0;
                        foreach (int i in classes)
                        {
                            if (sams.Count(x => x == i) > maxClassCount)
                            {
                                maxClassCount = sams.Count(x => x == i);
                            }
                        }

                        results[feelingModel].First(x => x.Item1 == person.Item1).Item2.Add("NAIV", (double)(maxClassCount/sams.Count));
                    }
                }
                #endregion

                Log.LogMessage("Creating excel file");
                Excel.Workbook resultBook = exc.Workbooks.Add(ExcelHandler.missingValue);

                Log.LogMessage("Writing results");
                foreach (var feelingResult in results)
                {
                    Log.LogMessage("Writing " + feelingResult.Key);
                    Excel.Worksheet feelSheet = resultBook.Sheets.Add();
                    feelSheet.Name = feelingResult.Key.ToString();
                    int column = 1;
                    foreach (SAMDataPoint.FeelingModel feel in Enum.GetValues(typeof(SAMDataPoint.FeelingModel)))
                    {
                        feelSheet.Cells[1, column++] = feel.ToString();
                    }
                    int row = 1;
                    foreach (var person in feelingResult.Value)
                    {
                        column = 1;
                        if (person.Item2.Values.Count == 0) continue;
                        feelSheet.Cells[row, column++] = person.Item1;
                        foreach (var sensor in person.Item2.Keys)
                        {
                            feelSheet.Cells[row++, column] = person.Item2[sensor];
                        }

                    }

                }
                /*   Excel.Worksheet metaSheet = resultBook.Sheets.Add();
               metaSheet.Name = "Meta";

               metaSheet.Cells[1, 1] = "Skipped " + skippedList.Count;
               for (int i = 0; i < skippedList.Count; i++)
               {
                   metaSheet.Cells[i + 2, 1] = skippedList[i];
               }*/

                Log.LogMessage("Saving...");
                resultBook.SaveCopyAs(sfdPath + "/allAnswers.xlsx");

                resultBook.Close(false);

                Log.LogMessage("Done!");

                exc.Quit();
            }

        }
    }
}
