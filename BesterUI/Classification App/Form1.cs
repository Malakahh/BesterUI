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
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;

namespace Classification_App
{
    public partial class Form1 : Form
    {
        FusionData _fd = new FusionData();
        SAMData samData;
        string currentPath;
        List<SVMConfiguration> svmConfs = new List<SVMConfiguration>();
        List<MetaSVMConfiguration> metaConfs = new List<MetaSVMConfiguration>();


        public Form1()
        {
            InitializeComponent();

            threadBox.Items.AddRange(Enum.GetNames(typeof(ThreadPriority)));
            threadBox.SelectedItem = ThreadPriority.Highest.ToString();

            Log.LogBox = richTextBox1;
            this.FormClosing += Form1_FormClosing;
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

        bool LoadData(string path)
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
            shouldRun = _fd.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, samData.startTime);
            //Slicing
            List<SAMDataPoint> throwaway = new List<SAMDataPoint>();
            foreach (SAMDataPoint samD in samData.dataPoints)
            {
                if (FeatureCreator.EEGDataSlice(_fd.eegData.ToList<DataReading>(), samD).Count == 0 ||
                    FeatureCreator.GSRDataSlice(_fd.gsrData.ToList<DataReading>(), samD).Count == 0 ||
                    FeatureCreator.HRDataSlice(_fd.hrData.ToList<DataReading>(), samD).Count == 0 ||
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


            FeatureCreator.GSRArousalOptimizationFeatures.ForEach(x => x.SetData(_fd.gsrData.ToList<DataReading>()));
            FeatureCreator.HRArousalOptimizationFeatures.ForEach(x => x.SetData(_fd.hrData.ToList<DataReading>()));
            FeatureCreator.HRValenceOptimizationFeatures.ForEach(x => x.SetData(_fd.hrData.ToList<DataReading>()));
            FeatureCreator.EEGArousalOptimizationFeatures.ForEach(x => x.SetData(_fd.eegData.ToList<DataReading>()));
            FeatureCreator.EEGValenceOptimizationFeatures.ForEach(x => x.SetData(_fd.eegData.ToList<DataReading>()));
            FeatureCreator.FACEArousalOptimizationFeatures.ForEach(x => x.SetData(_fd.faceData.ToList<DataReading>()));
            FeatureCreator.FACEValenceOptimizationFeatures.ForEach(x => x.SetData(_fd.faceData.ToList<DataReading>()));

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
                    if (!LoadData(item))
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

                    LoadData(item);
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
        #endregion

    }
}
