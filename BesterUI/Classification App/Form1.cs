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
            if (!svmConfs.Contains(conf))
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

        private void AddStdClassifierToLists(StdClassifier standardClassifier)
        {
            chklst_SvmConfigurations.Items.Add(standardClassifier);
            chklst_meta.Items.Add(standardClassifier);
        }

        Thread CreateMachineThread(string name, List<SVMParameter> pars, List<Feature> feats, SAMDataPoint.FeelingModel feels, Action<int, int> UpdateCallback)
        {
            return new Thread(() =>
            {
                StdClassifier mac = new StdClassifier(name, pars, feats, samData);
                mac.UpdateCallback = UpdateCallback;
                var res = mac.CrossValidateCombinations(feels, 1);
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
                foreach (var item in files)
                {
                    metaConfs.Add(MetaSVMConfiguration.Deserialize(File.ReadAllText(item)));
                }
            }

            if (svmConfs.Count == 0 && metaConfs.Count == 0)
            {
                Log.LogMessage("No configurations found, maybe you should run some optimizations on some features.");
            }
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


        //TEMP!
        private PredictionResult predicResults;

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
                        predicResults = resTemp;
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
                        predicResults = resTemp;
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
                        Log.LogMessage("Score was: " + resTemp.GetAverageFScore());
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
                LoadData(fbd.SelectedPath);
                DataLoaded();
            }
        }

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
                PredictionResult result = votingMeta.DoVoting(SAMDataPoint.FeelingModel.Arousal2High, 1);
                Log.LogMessage(result.GetAverageFScore());

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
                Log.LogMessage(result[0].GetAverageFScore());
            }
            if (boostingCB.Checked)
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
                Log.LogMessage(result.GetAverageFScore());
            }
        }

        volatile int gsrProg = 0;
        volatile int gsrTot = 1;
        volatile int hrProg = 0;
        volatile int hrTot = 1;
        volatile int eegProg = 0;
        volatile int eegTot = 1;
        volatile int faceProg = 0;
        volatile int faceTot = 1;
        volatile int stackProg = 0;
        volatile int stackTot = 1;
        private void btn_RunAll_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Log.LogMessage("Starting Stopwatch");
                List<SAMDataPoint.FeelingModel> feelings = new List<SAMDataPoint.FeelingModel>()
                {
                    SAMDataPoint.FeelingModel.Arousal2High,
                    SAMDataPoint.FeelingModel.Arousal3,
                    SAMDataPoint.FeelingModel.Valence2Low,
                    SAMDataPoint.FeelingModel.Valence3
                };

                ExcelHandler eh = new ExcelHandler(fbd.SelectedPath);
                if (!eh.BooksOpen)
                {
                    Log.LogMessage("Cannot open or write to books");
                    return;
                }

                btn_LoadData.Enabled = false;
                var dataFolders = Directory.GetDirectories(fbd.SelectedPath);
                List<SVMParameter> parameters = GenerateSVMParameters();

                int curDat = 1;
                int maxDat = dataFolders.Length;

                foreach (var item in dataFolders)
                {
                    if (File.Exists(item + @"\donno.dk"))
                    {
                        Log.LogMessage("Already did " + item + ", skipping..");
                        continue;
                    }
                    else if (item.Split('\\').Last() == "Stats")
                    {
                        Log.LogMessage("Stats folder skipping");
                        continue;
                    }
                    string personName = item.Split('\\').Last();
                    eh.AddPersonToBooks(personName);

                    LoadData(item);
                    foreach (var feel in feelings)
                    {
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

                        Thread gsrThread = null;
                        if (!File.Exists(currentPath + @"\gsr" + feel + ".donnodk") ||
                            feel == SAMDataPoint.FeelingModel.Valence2High ||
                            feel == SAMDataPoint.FeelingModel.Valence2Low ||
                            feel == SAMDataPoint.FeelingModel.Valence3)
                        {
                            gsrThread = CreateMachineThread("GSR", parameters, FeatureCreator.GSRArousalOptimizationFeatures, feel, (cur, max) => { gsrProg = cur; gsrTot = max; });
                            gsrThread.Priority = ThreadPriority.Highest;
                            gsrThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("GSR done skipping (because it's already done or searching on valence");
                        }

                        Thread hrThread = null;
                        if (!File.Exists(currentPath + @"\hr" + feel + ".donnodk"))
                        {
                            hrThread = CreateMachineThread("HR", parameters,
                                (feel == SAMDataPoint.FeelingModel.Valence2High || feel == SAMDataPoint.FeelingModel.Valence2Low || feel == SAMDataPoint.FeelingModel.Valence3)
                                    ? FeatureCreator.HRValenceOptimizationFeatures : FeatureCreator.HRArousalOptimizationFeatures,
                                feel, (cur, max) => { hrProg = cur; hrTot = max; });
                            hrThread.Priority = ThreadPriority.Highest;
                            hrThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("HR done already, skipping");
                        }

                        Thread eegThread = null;
                        if (!File.Exists(currentPath + @"\eeg" + feel + ".donnodk"))
                        {
                            eegThread = CreateMachineThread("EEG", parameters,
                                 (feel == SAMDataPoint.FeelingModel.Valence2High || feel == SAMDataPoint.FeelingModel.Valence2Low || feel == SAMDataPoint.FeelingModel.Valence3)
                                    ? FeatureCreator.EEGValenceOptimizationFeatures : FeatureCreator.EEGArousalOptimizationFeatures,
                                 feel, (cur, max) => { eegProg = cur; eegTot = max; });
                            eegThread.Priority = ThreadPriority.Highest;
                            eegThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("EEG done already, skipping");
                        }

                        Thread faceThread = null;
                        if (!File.Exists(currentPath + @"\face" + feel + ".donnodk"))
                        {
                            faceThread = CreateMachineThread("FACE", parameters,
                                                             (feel == SAMDataPoint.FeelingModel.Valence2High || feel == SAMDataPoint.FeelingModel.Valence2Low || feel == SAMDataPoint.FeelingModel.Valence3)
                                                                ? FeatureCreator.FACEValenceOptimizationFeatures : FeatureCreator.FACEArousalOptimizationFeatures,
                                                             feel, (cur, max) => { faceProg = cur; faceTot = max; });
                            faceThread.Priority = ThreadPriority.Highest;
                            faceThread.Start();
                        }
                        else
                        {
                            Log.LogMessage("FACE done already, skipping");
                        }


                        while ((gsrThread != null && gsrThread.IsAlive) || (hrThread != null && hrThread.IsAlive) || (eegThread != null && eegThread.IsAlive) || (faceThread != null && faceThread.IsAlive))
                        {
                            Thread.Sleep(500);
                            double pct = (double)(gsrProg + hrProg + eegProg + faceProg) * (double)100 / (double)(gsrTot + hrTot + eegTot + faceTot);
                            Log.LogMessageSameLine(feel + " -> " + curDat + "/" + maxDat + " | Progress: " + pct.ToString("0.0") + "% - [GSR(" + gsrProg + "/" + gsrTot + ")] - [HR(" + hrProg + "/" + hrTot + ")] - [EEG(" + eegProg + "/" + eegTot + ")] - [FACE(" + faceProg + "/" + faceTot + ")]");
                            Application.DoEvents();

                            if (gsrThread != null && !gsrThread.IsAlive && !gsrWrite)
                            {
                                gsrWrite = true;
                                using (var temp = File.Create(currentPath + @"\gsr" + feel + ".donnodk")) { }
                            }

                            if (hrThread != null && !hrThread.IsAlive && !hrWrite)
                            {
                                hrWrite = true;
                                using (var temp = File.Create(currentPath + @"\hr" + feel + ".donnodk")) { }
                            }

                            if (eegThread != null && !eegThread.IsAlive && !eegWrite)
                            {
                                eegWrite = true;
                                using (var temp = File.Create(currentPath + @"\eeg" + feel + ".donnodk")) { }
                            }

                            if (faceThread != null && !faceThread.IsAlive && !faceWrite)
                            {
                                faceWrite = true;
                                using (var temp = File.Create(currentPath + @"\face" + feel + ".donnodk")) { }
                            }
                        }

                        Log.LogMessage("Done with single machine searching.");


                        List<SVMConfiguration> confs = new List<SVMConfiguration>();
                        SVMConfiguration gsrConf;
                        SVMConfiguration eegConf;
                        SVMConfiguration hrConf;
                        SVMConfiguration faceConf;

                        if (gsrWrite)
                        {
                            gsrConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("GSR") && x.Name.Contains(feel.ToString()));
                            confs.Add(gsrConf);
                        }
                        if (eegWrite)
                        {
                            eegConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("HR") && x.Name.Contains(feel.ToString()));
                            confs.Add(eegConf);
                        }
                        if (hrWrite)
                        {
                            hrConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("EEG") && x.Name.Contains(feel.ToString()));
                            confs.Add(hrConf);
                        }
                        if (faceWrite)
                        {
                            faceConf = svmConfs.OfType<SVMConfiguration>().First((x) => x.Name.StartsWith("FACE") && x.Name.Contains(feel.ToString()));
                            confs.Add(faceConf);
                        }

                        Log.LogMessage("Creating meta machine..");
                        foreach (var cnf in confs)
                        {
                            Log.LogMessage("Using " + cnf.Name + "...");
                        }

                        //Write normal results
                        var gsrMac = new StdClassifier(confs[0], samData);
                        var gsrRes = gsrMac.CrossValidate(feel, 1);
                        eh.AddDataToPerson(personName, ExcelHandler.Book.GSR, gsrRes.First(), feel);

                        var hrMac = new StdClassifier(confs[1], samData);
                        var hrRes = hrMac.CrossValidate(feel, 1);
                        eh.AddDataToPerson(personName, ExcelHandler.Book.HR, hrRes.First(), feel);

                        var eegMac = new StdClassifier(confs[2], samData);
                        var eegRes = eegMac.CrossValidate(feel, 1);
                        eh.AddDataToPerson(personName, ExcelHandler.Book.EEG, eegRes.First(), feel);

                        var faceMac = new StdClassifier(confs[2], samData);
                        var faceRes = faceMac.CrossValidate(feel, 1);
                        eh.AddDataToPerson(personName, ExcelHandler.Book.FACE, faceRes.First(), feel);

                        eh.Save();
                        Log.LogMessage("Doing Stacking");
                        stackProg = 0;
                        stackTot = 1;
                        MetaClassifier meta = new MetaClassifier("Stacking", parameters, samData, ConfigurationsToStds(confs));
                        //meta.UpdateCallback = (cur, max) => { stackProg = cur; stackTot = max; };
                        var res = meta.DoStacking(feel, 1);
                        var bestRes = meta.FindBestFScorePrediction(res);

                        eh.AddDataToPerson(personName, ExcelHandler.Book.Stacking, bestRes, feel);

                        meta.Parameters = new List<SVMParameter>() { bestRes.svmParams };

                        SaveConfiguration(meta.GetConfiguration());

                        Log.LogMessage("Doing voting");
                        var voteRes = meta.DoVoting(feel, 1);
                        eh.AddDataToPerson(personName, ExcelHandler.Book.Voting, voteRes, feel);
                        Log.LogMessage("Doing boosting");
                        meta.boostingOrder = new List<int>() { 0, 1, 2 };
                        var boostRes = meta.DoBoosting(feel, 1);
                        eh.AddDataToPerson(personName, ExcelHandler.Book.Boosting, boostRes, feel);
                    }

                    curDat++;
                    eh.Save();
                    using (var temp = File.Create(currentPath + @"\donno.dk")) { }
                    Log.LogMessage("DonnoDK");
                }
                eh.CloseBooks();
                Log.LogMessage("Closing books and saving");
                Log.LogMessage("Done in: " + stopwatch.Elapsed);
            }

            btn_LoadData.Enabled = true;

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

    }
}
