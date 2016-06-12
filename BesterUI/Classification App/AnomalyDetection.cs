using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
using Classification_App.Evnt;
using Microsoft.Kinect.Face;

namespace Classification_App
{
    public enum SENSOR { GSR, EEG, HR, FACE };
    public partial class AnomalyDetection : Form
    {
        public void SetProgressMax(int max)
        {
            if (max > eegProgress.Maximum)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    eegProgress.Maximum = max + 1;
                    hrProgress.Maximum = max + 1;
                    gsrProgress.Maximum = max + 1;
                    faceProgress.Maximum = max + 1;
                });
            }
        }
        public void SetProgress(int value, SENSOR sensor)
        {

            switch (sensor)
            {
                case SENSOR.EEG:
                    this.Invoke((MethodInvoker)delegate
                    {
                        eegProgress.Value = value;
                    });
                    break;
                case SENSOR.HR:
                    this.Invoke((MethodInvoker)delegate
                    {
                        hrProgress.Value = value;
                    });
                    break;
                case SENSOR.GSR:
                    this.Invoke((MethodInvoker)delegate
                    {
                        gsrProgress.Value = value;
                    });
                    break;
                case SENSOR.FACE:
                    this.Invoke((MethodInvoker)delegate
                    {
                        faceProgress.Value = value;
                    });
                    break;
            }
        }

        FusionData _fdAnomaly = new FusionData();

        const double WINDOW_FACTOR = 1.0;
        // MS

        /**
           GSR Window 2-7s
           */
        const int GSR_DELAY = 2000;
        const int GSR_DURATION = (int)(5000 * WINDOW_FACTOR);

        /**
            EEG Window 0.350-1.060s
            */
        const int EEG_DELAY = 350;
        const int EEG_DURATION = (int)(710 * WINDOW_FACTOR);

        /**
            Face Window 0.5-1s
            */
        const int FACE_DELAY = 500;
        const int FACE_DURATION = (int)(500 * WINDOW_FACTOR);

        /**
            HR Window 4-7s
            */
        const int HR_DELAY = 4000;
        const int HR_DURATION = (int)(3000 * WINDOW_FACTOR);

        const int STEP_SIZE = 300;


        ConcurrentDictionary<SENSOR, List<OneClassFV>> featureVectors = new ConcurrentDictionary<SENSOR, List<OneClassFV>>();
        Dictionary<SENSOR, List<OneClassFV>> predictions = new Dictionary<SENSOR, List<OneClassFV>>();

        List<Events> events = new List<Events>();
        List<samEvents> sEvents = new List<samEvents>();
        Stopwatch sw = new Stopwatch();

        public AnomalyDetection()
        {
            InitializeComponent();
            foreach (var k in Enum.GetValues(typeof(SENSOR)))
            {
                featureVectors.TryAdd((SENSOR)k, new List<OneClassFV>());
                predictions.Add((SENSOR)k, new List<OneClassFV>());
            }
        }


        string path = "";
        private async void btn_loadData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {

                path = fbd.SelectedPath;
                string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];
                sw.Start();
                Log.LogMessage($"Loading Data");
                _fdAnomaly.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);

                Log.LogMessage($"Getting Events: {sw.Elapsed}");
                string[] tmpevents = File.ReadAllLines(path + @"\SecondTest.dat");
                LoadEvents(tmpevents);


                /*string[] tmpSevents = File.ReadAllLines(path + @"\sam.dat");


                foreach (string ev in tmpSevents)
                {
                    sEvents.Add(new samEvents(int.Parse(ev.Split(':')[0]), int.Parse(ev.Split(':')[1]), int.Parse(ev.Split(':')[2])));
                }*/

                Task gsrThread = Task.Run(() => CreateGSRFeatures(_fdAnomaly.gsrData));
                Task eegThread = Task.Run(() => CreateEEGFeatures(_fdAnomaly.eegData.ToList<DataReading>()));
                Task hrThread = Task.Run(() => CreateHRFeatures(_fdAnomaly.hrData));
                Task faceThread = Task.Run(() => CreateFACEFeatures(_fdAnomaly.faceData.ToList<DataReading>()));
                List<Task> threads = new List<Task>();
                threads.Add(gsrThread);
                threads.Add(eegThread);
                threads.Add(hrThread);
                threads.Add(faceThread);
                await Task.WhenAll(threads.ToArray());

                Log.LogMessage("");


                Log.LogMessage($"Normalizing GSR Feature: {sw.Elapsed}");
                featureVectors[SENSOR.GSR] = featureVectors[SENSOR.GSR].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                Log.LogMessage($"Normalizing EEG Feature: {sw.Elapsed}");
                featureVectors[SENSOR.EEG] = featureVectors[SENSOR.EEG].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                Log.LogMessage($"Normalizing HR Feature: {sw.Elapsed}");
                featureVectors[SENSOR.HR] = featureVectors[SENSOR.HR].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                Log.LogMessage($"Normalizing FACE Feature: {sw.Elapsed}");
                featureVectors[SENSOR.FACE] = featureVectors[SENSOR.FACE].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                Log.LogMessage("Saving Feature Vectors");
                AnomaliSerializer.SaveFeatureVectors(featureVectors, path);

                SetupMachines();
                Log.LogMessage("Done setting up machines");
                btn_getData.Enabled = true;

            }
        }

        private void CreateHRFeatures(List<HRDataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (HR_DELAY + HR_DURATION); time += STEP_SIZE)
            {
                SVMNode[] featureVector = new SVMNode[3];
                List<HRDataReading> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < time + HR_DELAY).TakeWhile(x => time + HR_DURATION + HR_DELAY > (x.timestamp - data.First().timestamp)).Where(x => x.isBeat).ToList();
                if (d.Count == 0)
                {
                    continue;
                }
                featureVector[0] = new SVMNode(1, d.Select(x => (double)x.IBI).Average());
                double sd = Math.Sqrt(d.Average(x => Math.Pow((double)x.IBI - featureVector[0].Value, 2)));
                featureVector[1] = new SVMNode(2, sd);
                featureVector[2] = new SVMNode(3, FeatureCreator.HRVRMSSD(d.ToList<DataReading>()));
                featureVectors[SENSOR.HR].Add(new OneClassFV(featureVector, time));
            }
            Log.LogMessage($"Calculation HR Feature Done: {sw.Elapsed}");
        }

        private void CreateEEGFeatures(List<DataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (EEG_DELAY + EEG_DURATION); time += STEP_SIZE)
            {
                SVMNode[] featureVector = new SVMNode[10];
                List<DataReading> slice = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < EEG_DELAY + time).TakeWhile(x => time + EEG_DELAY + EEG_DURATION > (x.timestamp - data.First().timestamp)).ToList();
                List<string> names = new List<string>() { "Delta", "Theta", "Alpha", "Beta", "Gamma" };
                if (slice.Count == 0)
                {
                    continue;
                }
                int counter = 0;
                foreach (string name in names)
                {
                    //Arousal 
                    featureVector[counter] = new SVMNode(counter + 1, FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));
                    counter++;
                    featureVector[counter] = new SVMNode(counter + 1, FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString()))));
                    counter++;
                }
                featureVectors[SENSOR.EEG].Add(new OneClassFV(featureVector, time));
            }

            Log.LogMessage($"Calculation EEG Feature: {sw.Elapsed}");
        }


        private void CreateFACEFeatures(List<DataReading> data)
        {
            List<int> leftSide = new List<int>() { 5, 13, 15 };
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (FACE_DELAY + FACE_DURATION); time += STEP_SIZE)
            {
                List<DataReading> dataSlice = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < FACE_DELAY + time).TakeWhile(x => time + FACE_DELAY + FACE_DURATION > (x.timestamp - data.First().timestamp)).ToList();
                if (dataSlice.Count == 0)
                {
                    continue;
                }

                SVMNode[] featureVector = new SVMNode[4];
                double average12 = dataSlice.Average(d => ((FaceDataReading)d).data[FaceShapeAnimations.RighteyeClosed]);
                double average11 = dataSlice.Average(d => ((FaceDataReading)d).data[FaceShapeAnimations.LefteyeClosed]);

                double sd12 = Math.Sqrt(dataSlice.Average(x => Math.Pow(((FaceDataReading)x).data[FaceShapeAnimations.RighteyeClosed] - average12, 2)));
                double sd11 = Math.Sqrt(dataSlice.Average(x => Math.Pow(((FaceDataReading)x).data[FaceShapeAnimations.LefteyeClosed] - average11, 2)));
                featureVector[0] = new SVMNode(1, average11);
                featureVector[1] = new SVMNode(2, average12);
                featureVector[2] = new SVMNode(3, sd11);
                featureVector[3] = new SVMNode(4, sd12);

                featureVectors[SENSOR.FACE].Add(new OneClassFV(featureVector, time));
            }

            Log.LogMessage($"Calculation FACE Feature: {sw.Elapsed}");
        }

        private void CreateGSRFeatures(List<GSRDataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (GSR_DELAY + GSR_DURATION); time += STEP_SIZE)
            {
                SVMNode[] featureVector = new SVMNode[4];
                List<double> slice = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < GSR_DELAY + time).TakeWhile(x => time + GSR_DELAY + GSR_DURATION > (x.timestamp - data.First().timestamp)).Select(x => (double)x.resistance).ToList();
                if (slice.Count == 0)
                {
                    continue;
                }
                featureVector[0] = new SVMNode(1, slice.Average());
                double sd = Math.Sqrt(slice.Average(x => Math.Pow(x - slice.Average(), 2)));
                featureVector[1] = new SVMNode(2, sd);
                featureVector[2] = new SVMNode(3, slice.Max());
                featureVector[3] = new SVMNode(4, slice.Min());
                featureVectors[SENSOR.GSR].Add(new OneClassFV(featureVector, time));
            }

            Log.LogMessage($"Calculation GSR Feature: {sw.Elapsed}");
        }

        private List<OneClassFV> PredictSlice(SENSOR machine, List<OneClassFV> data)
        {
            return machines[machine].PredictOutliers(data);
        }


        private IEnumerable<Tuple<List<double>, int>> GetTrainingData(SENSOR machine, int start, int trainingEnd)
        {

            //Split into training & prediction set
            List<List<double>> featureVectors = new List<List<double>>();
            List<int> timeStamps = new List<int>();

            if (machine == SENSOR.GSR)
            {
                int stepSize = 100;
                for (int i = 0; i < _fdAnomaly.gsrData.Last().timestamp - _fdAnomaly.gsrData.First().timestamp - GSR_DURATION + GSR_DELAY; i += stepSize)
                {
                    List<double> featureVector = new List<double>();
                    List<double> data = _fdAnomaly.gsrData.SkipWhile(x => (x.timestamp - _fdAnomaly.gsrData.First().timestamp) < i + GSR_DELAY).TakeWhile(x => i + GSR_DURATION > (x.timestamp - _fdAnomaly.gsrData.First().timestamp)).Select(x => (double)x.resistance).ToList();
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
            }
            featureVectors = featureVectors.NormalizeFeatureList<double>(Normalize.OneMinusOne).ToList();
            var dataSet = featureVectors.Zip(timeStamps, (first, second) => { return Tuple.Create(first, second); });

            var trainingSet = dataSet.SkipWhile(x => x.Item2 < start).TakeWhile(x => x.Item2 < trainingEnd);

            return trainingSet;
        }

        private void SetupMachines()
        {
            machines.Clear();
            int trainingStart = (useRestInTraining.Checked) ? 180000 : 0;
            int trainingEnd = events[2].GetTimestampEnd();
            foreach (SENSOR sensor in Enum.GetValues(typeof(SENSOR)))
            {
                CreateSVM(sensor, featureVectors[sensor].TakeWhile(x => x.TimeStamp <= trainingEnd).ToList());
            }
        }

        Dictionary<SENSOR, OneClassClassifier> machines = new Dictionary<SENSOR, OneClassClassifier>();

        private void CreateSVM(SENSOR machine, List<OneClassFV> trainingSet)
        {

            var data = trainingSet.Select(x => x.Features).ToList();

            OneClassClassifier occ = new OneClassClassifier(data);
            SVMParameter svmP = new SVMParameter();
            svmP.Kernel = SVMKernelType.RBF;
            svmP.C = 100;
            svmP.Gamma = 0.01;
            svmP.Nu = 0.01;
            svmP.Type = SVMType.ONE_CLASS;
            occ.CreateModel(svmP);

            machines.Add(machine, occ);
        }

        List<Tuple<int, int, int>> offsets = new List<Tuple<int, int, int>>();

        private List<OneClassFV> FindNearestTimeStamp(List<OneClassFV> data, int timestamp)
        {
            List<OneClassFV> returnData = new List<OneClassFV>();
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].TimeStamp == timestamp)
                {
                    returnData.Add(data[i]);
                }
                else if (data[i].TimeStamp > timestamp)
                {
                    if (timestamp - data[i - 1].TimeStamp < Math.Abs(timestamp - data[i].TimeStamp))
                    {
                        offsets.Add(new Tuple<int, int, int>(timestamp - data[i - 1].TimeStamp, Math.Abs(timestamp - data[i].TimeStamp), Math.Abs(timestamp - data[i - 1].TimeStamp)));
                        returnData.Add(data[i - 1]);
                        break;
                    }
                    else
                    {
                        offsets.Add(new Tuple<int, int, int>(timestamp - data[i - 1].TimeStamp, Math.Abs(timestamp - data[i].TimeStamp), Math.Abs(timestamp - data[i].TimeStamp)));
                        returnData.Add(data[i]);
                        break;
                    }

                }
            }

            return returnData;
        }


        private void GroupByEvent()
        {
            //We have no event for "pre-resting", this holds all outliers predicted in that period.
            /*  Events rest_event = new Events(0, "Resting Period");

              foreach (SENSOR s in Enum.GetValues(typeof(SENSOR)))
              {
                  for (int i = 0; i < events.Count; i++)
                  {
                      foreach (OneClassFV outlier in predictions[s])
                      {
                          if (i != events.Count - 1)
                          {
                              if (outlier.TimeStamp < events[i].timestamp && i == 0)
                              {
                                  //The outlier is prior any events, should be added to the rest period.
                                  rest_event.AddOutlier(outlier);
                                  events.Add(rest_event);

                              }
                              else if (outlier.TimeStamp >= events[i].timestamp && outlier.TimeStamp < events[i + 1].timestamp)
                              {
                                  events[i].AddOutlier(outlier);
                              }
                              else
                              {
                                  var wrong = outlier;
                              }
                          }
                          else
                          {
                              if (outlier.TimeStamp >= events[i].timestamp)
                              {
                                  events[i].AddOutlier(outlier);
                              }
                              else
                              {
                                  var wrong = outlier;
                              }
                          }
                      }
                  }
              }*/
        }

        private void btn_getData_Click(object sender, EventArgs e)
        {
            Dictionary<SENSOR, List<OneClassFV>> anomali = new Dictionary<SENSOR, List<OneClassFV>>();
            Dictionary<SENSOR, List<Events>> eventResult = new Dictionary<SENSOR, List<Events>>();
            List<OneClassFV> outliersFromSam = new List<OneClassFV>();
            Dictionary<SENSOR, PointsOfInterest> dPointsOfInterest = new Dictionary<SENSOR, PointsOfInterest>();
            sw.Restart();
            foreach (SENSOR key in featureVectors.Keys)
            {
                Log.LogMessage($"Predicting {key}: {sw.Elapsed}");
                anomali.Add(key, PredictSlice(key, featureVectors[key]));
                dPointsOfInterest.Add(key, new PointsOfInterest(anomali[key]));
            }
            foreach (SENSOR key in dPointsOfInterest.Keys)
            {
                List<Events> tempEvents = new List<Events>();
                foreach (Events p in events)
                {
                    p.SetPointOfInterest(dPointsOfInterest[key]);
                    tempEvents.Add(p.Copy());
                }
                eventResult.Add(key, tempEvents);
            }
            AnomaliSerializer.SaveAnomalis(anomali, path, STEP_SIZE);
            AnomaliSerializer.SaveEvents(eventResult, path);
            AnomaliSerializer.SavePointsOfInterest(dPointsOfInterest, path);
            Log.LogMessage("Done saving Anomalis, Events, and POIs");
        }

        private void load_data_from_files_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {

                path = fbd.SelectedPath;
                string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];
                sw.Start();
                Log.LogMessage($"Loading Data");
                featureVectors = AnomaliSerializer.LoadFeatureVectors(path);
                string[] tmpevents = File.ReadAllLines(path + @"\SecondTest.dat");
                LoadEvents(tmpevents);
                SetupMachines();
                Log.LogMessage("Done setting up machines");
                btn_getData.Enabled = true;

            }
        }

        public void LoadEvents(string[] tmpevents)
        {
            events.Clear();
            List<string> eventList = tmpevents.ToList();
            #region [Instant]
            //Add Attachment
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("AddAttachmentButtonClick:"))
                {
                    //+2000 for delay for the failed message shown
                    events.Add(new Events(int.Parse(temp[0]) + 2000, int.Parse(temp[0]) + 2000, temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            //Send draft
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("SendDraft error shown"))
                {
                    events.Add(new Events(int.Parse(temp[0]), int.Parse(temp[0]), temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            //Not responding
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("NotResponding"))
                {
                    events.Add(new Events(int.Parse(temp[0]), int.Parse(temp[0]), temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            //Remove contact
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("RemoveContact clicked"))
                {
                    events.Add(new Events(int.Parse(temp[0]), int.Parse(temp[0]), temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            #endregion
            #region [Non-instant]
            //Add contact
            int firstContactClick = 0;
            int lastContactClick = 0;
            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("Add Contact Button click: 1"))
                {
                    firstContactClick = int.Parse(temp[0]);
                    eventList.RemoveAt(i);
                    break;
                }
                else if (temp[1].Contains("Add Contact Button click: 2"))
                {
                    firstContactClick = int.Parse(temp[0]);
                    eventList.RemoveAt(i);
                    continue;
                }
                else if (temp[1].Contains("Add Contact Button click: 3"))
                {
                    lastContactClick = int.Parse(temp[0]);
                    eventList.RemoveAt(i);
                    continue;
                }
            }
            events.Add(new Events(firstContactClick, lastContactClick, "Send Draft", 0.5));

            //Caret Movement
            int firstCaretMoved = 0;
            int lastCaretMoved = 0;
            bool lastSet = true;
            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("Caret Moved"))
                {
                    if (lastSet)
                    {
                        lastCaretMoved = int.Parse(temp[0]);
                        eventList.RemoveAt(i);
                        lastSet = false;
                        continue;
                    }
                    else
                    {
                        firstCaretMoved = int.Parse(temp[0]);
                        eventList.RemoveAt(i);
                        continue;
                    }

                }
                else if (temp[1].Contains("BogusMessage: Text Changed"))
                {
                    eventList.RemoveAt(i);
                }
            }
            events.Add(new Events(firstCaretMoved, lastCaretMoved, "CaretMoved", 0.20));

            //Language
            int langugeChanged = 0;
            int languageTaskDone = 0;
            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("CreateDraft, language changed to: US") || temp[1].Contains("CreateDraft, language changed to: Amerikansk"))
                {
                    langugeChanged = int.Parse(temp[0]);
                    eventList.RemoveAt(i);
                    continue;
                }
                else if (temp[1].Contains("TaskWizard - BtnCompleteClicked - CreateDraft") || temp[1].Contains("TaskWizard - BtnIncompleteClicked - CreateDraft"))
                {
                    languageTaskDone = int.Parse(temp[0]);
                    eventList.RemoveAt(i);
                }
            }
            events.Add(new Events(langugeChanged, languageTaskDone, "Language Changed", 0.25));


            #endregion
        }

        private async void runAllButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                foreach (string folder in Directory.GetDirectories(fbd.SelectedPath))
                {
                    if (folder.Contains("Stats"))
                    {
                        continue;
                    }
                    featureVectors.Clear();
                    foreach (var k in Enum.GetValues(typeof(SENSOR)))
                    {
                        featureVectors.TryAdd((SENSOR)k, new List<OneClassFV>());
                    }
                    path = folder;
                    path += "/test";
                    string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];
                    sw.Start();
                    Log.LogMessage($"Loading Data");
                    var KeepRunning = _fdAnomaly.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);
                    foreach (var k in KeepRunning.Keys)
                    {
                        if (KeepRunning[k] == false)
                        {
                            Log.LogMessage($"Skipping: " + testSubjectId + " due to missing " + k.ToString());
                            continue;
                        }
                    }

                    Log.LogMessage($"Getting Events: {sw.Elapsed}");
                    string[] tmpevents;
                    try
                    {
                        tmpevents = File.ReadAllLines(path + @"\SecondTest.dat");
                    }
                    catch (Exception ex)
                    {
                        Log.LogMessage($"Skipping {testSubjectId} due to {ex.Message}");
                        continue;
                    }
                    LoadEvents(tmpevents);

                    Task gsrThread = Task.Run(() => CreateGSRFeatures(_fdAnomaly.gsrData));
                    Task eegThread = Task.Run(() => CreateEEGFeatures(_fdAnomaly.eegData.ToList<DataReading>()));
                    Task hrThread = Task.Run(() => CreateHRFeatures(_fdAnomaly.hrData));
                    Task faceThread = Task.Run(() => CreateFACEFeatures(_fdAnomaly.faceData.ToList<DataReading>()));
                    List<Task> threads = new List<Task>();
                    threads.Add(gsrThread);
                    threads.Add(eegThread);
                    threads.Add(hrThread);
                    threads.Add(faceThread);
                    await Task.WhenAll(threads.ToArray());

                    Log.LogMessage("");


                    Log.LogMessage($"Normalizing GSR Feature: {sw.Elapsed}");
                    featureVectors[SENSOR.GSR] = featureVectors[SENSOR.GSR].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                    Log.LogMessage($"Normalizing EEG Feature: {sw.Elapsed}");
                    featureVectors[SENSOR.EEG] = featureVectors[SENSOR.EEG].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                    Log.LogMessage($"Normalizing HR Feature: {sw.Elapsed}");
                    featureVectors[SENSOR.HR] = featureVectors[SENSOR.HR].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                    Log.LogMessage($"Normalizing FACE Feature: {sw.Elapsed}");
                    featureVectors[SENSOR.FACE] = featureVectors[SENSOR.FACE].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();

                    Log.LogMessage("Saving Feature Vectors");
                    AnomaliSerializer.SaveFeatureVectors(featureVectors, path);
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };
            int counter = 1;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                sw.Start();
                NoveltyExcel excel = new NoveltyExcel(fbd.SelectedPath, "result");
                var dic = Directory.GetDirectories(fbd.SelectedPath);
                int total = dic.Count(x => !x.Contains("Stats"));
                foreach (string folder in dic)
                {
                    statusLabel.Text = $"{counter}/{total}";
                    if (folder.Contains("Stats"))
                    {
                        continue;
                    }
                    path = folder + "/test";
                    string testSubjectId = folder.Split('\\').Last();
                    excel.AddPersonToBooks(testSubjectId);
                    Log.LogMessage($"Loading Data: " + testSubjectId);
                    featureVectors = AnomaliSerializer.LoadFeatureVectors(path);
                    string[] tmpevents = File.ReadAllLines(path + "/SecondTest.dat");
                    int start = int.Parse(tmpevents.ToList().Find(x => x.Contains("ReplyToMail")).Split('#')[0]);
                    int end = int.Parse(tmpevents.Last().Split('#')[0]);
                    LoadEvents(tmpevents);
                    Dictionary<SENSOR, Tuple<NoveltyResult, NoveltyResult>> predictionResults = new Dictionary<SENSOR, Tuple<NoveltyResult, NoveltyResult>>();
                    //Do gridsearch       
                    Task<Tuple<NoveltyResult,NoveltyResult>> gsrThread = Task.Run(() => DoNoveltyDetection(SENSOR.GSR, start, end));
                    int gsrId = gsrThread.Id;
                    Task<Tuple<NoveltyResult, NoveltyResult>> eegThread = Task.Run(() => DoNoveltyDetection(SENSOR.EEG, start, end));
                    int eegId = eegThread.Id;
                    Task<Tuple<NoveltyResult, NoveltyResult>> hrThread = Task.Run(() => DoNoveltyDetection(SENSOR.HR, start, end));
                    int hrId = hrThread.Id;
                    Task<Tuple<NoveltyResult, NoveltyResult>> faceThread = Task.Run(() => DoNoveltyDetection(SENSOR.FACE, start, end));
                    int faceId = faceThread.Id;
                    List<Task<Tuple<NoveltyResult, NoveltyResult>>> threads = new List<Task<Tuple<NoveltyResult, NoveltyResult>>>();
                    threads.Add(gsrThread);
                    threads.Add(eegThread);
                    threads.Add(hrThread);
                    threads.Add(faceThread);
                    await Task.WhenAll(threads);
                    foreach (Task<Tuple<NoveltyResult, NoveltyResult>> t in threads)
                    {
                        if (t.Id == gsrId)
                        {
                            predictionResults.Add(SENSOR.GSR, t.Result);
                            continue;
                        }
                        else if (t.Id == eegId)
                        {
                            predictionResults.Add(SENSOR.EEG, t.Result);
                            continue;
                        }
                        else if (t.Id == faceId)
                        {
                            predictionResults.Add(SENSOR.FACE, t.Result);
                            continue;
                        }
                        else if (t.Id == hrId)
                        {
                            predictionResults.Add(SENSOR.HR, t.Result);
                            continue;
                        }
                        else
                        {
                            Log.LogMessage("Could not match thread ID");
                        }

                    }
                    foreach (var key in predictionResults.Keys)
                    {
                        List<string> allData = new List<string>();
                        string hitPath = path + "/hit";
                        if (!Directory.Exists(hitPath))
                        {
                            Directory.CreateDirectory(hitPath);
                        }
                        NoveltyResult hitResult =predictionResults[key].Item1;
                        AnomaliSerializer.SavePathAnomalis(hitResult.anomalis, hitPath, STEP_SIZE, key.ToString());
                        AnomaliSerializer.SavePathEvents(hitResult.events, hitPath, key.ToString());
                        AnomaliSerializer.SavePathPointsOfInterest(hitResult.poi, hitPath, key.ToString());
                        AnomaliSerializer.SavePathPointsOfInterest(hitResult.poi, hitPath, key.ToString());

                        string areaPath = path + "/area";
                        if (!Directory.Exists(hitPath))
                        {
                            Directory.CreateDirectory(hitPath);
                        }
                        NoveltyResult areaResult = predictionResults[key].Item2;
                        AnomaliSerializer.SavePathAnomalis(areaResult.anomalis, areaPath, STEP_SIZE, key.ToString());
                        AnomaliSerializer.SavePathEvents(areaResult.events, areaPath, key.ToString());
                        AnomaliSerializer.SavePathPointsOfInterest(areaResult.poi, areaPath, key.ToString());
                        AnomaliSerializer.SavePathPointsOfInterest(areaResult.poi, areaPath, key.ToString());

                        allData.Add("HitResult");
                        allData.Add("HScore:"+hitResult.CalculateHitScore().ToString());
                        allData.Add("eventHits:" + hitResult.CalculateHitResult().eventHits.ToString());
                        allData.Add("totalEvents" + hitResult.CalculateHitResult().eventsTotal.ToString());
                        allData.Add("hits" + hitResult.CalculateHitResult().hits.ToString());
                        allData.Add("misses" + hitResult.CalculateHitResult().misses.ToString());
                        allData.Add("CScore" + hitResult.CalculateCoveredScore().ToString());
                        allData.Add("FlaggedSizeAre" + hitResult.FlaggedAreaSize().ToString());
                        allData.Add("NormalArea" + hitResult.CalculateTotalNormalArea().ToString());
                        allData.Add(" ");
                        allData.Add("CoveredResult");
                        allData.Add("HScore:" + areaResult.CalculateHitScore().ToString());
                        allData.Add("eventHits:" + areaResult.CalculateHitResult().eventHits.ToString());
                        allData.Add("totalEvents" + areaResult.CalculateHitResult().eventsTotal.ToString());
                        allData.Add("hits" + areaResult.CalculateHitResult().hits.ToString());
                        allData.Add("misses" + areaResult.CalculateHitResult().misses.ToString());
                        allData.Add("CScore" + areaResult.CalculateCoveredScore().ToString());
                        allData.Add("FlaggedSizeAre" + areaResult.FlaggedAreaSize().ToString());
                        allData.Add("NormalArea" + areaResult.CalculateTotalNormalArea().ToString());

                        Log.LogMessage($"Person {testSubjectId} done");

                    }
                    counter++;
                }
                excel.CloseHandler();
                statusLabel.Text = "donno.dk";
            }

        }
       // private int numberOfTasks = 10;
        private async Task<Tuple<NoveltyResult, NoveltyResult>> DoNoveltyDetection(SENSOR sensor, int start, int end)
        {
            string sensorPath = path + "/" + sensor.ToString();
            var data = featureVectors[sensor].Select(x => x.Features).ToList();
            ConcurrentStack<SVMParameter> svmParams = new ConcurrentStack<SVMParameter>();
            //Debug purpose
          /*  for (int i = 0; i < 10; i++)
            {
                SVMParameter s = new SVMParameter();
                s.C = 100;
                s.Gamma = 0.01;
                s.Kernel = SVMKernelType.RBF;
                s.Type = SVMType.ONE_CLASS;
                s.Nu = 0.01;
                svmParams.Push(s);
            }*/
           svmParams.PushRange(GenerateOneClassSVMParameters().ToArray());
            SetProgressMax(svmParams.Count + 1);
            NoveltyResult besthitResult = null;
            NoveltyResult bestCoveredResult = null;
            Mutex bestResultMu = new Mutex(false, sensor.ToString());
            int count = 1;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < threadMAX.Value; i++)
            {
                Task task = Task.Run(() => PredictionThread(ref count, sensor, start, end, ref svmParams, data, svmParams.Count, ref besthitResult, ref bestCoveredResult, bestResultMu));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);

            //Hits
           /*  List<int> integers = Enumerable.Range(1, 100).ToList();
            //List<int> integers = new List<int>() { 1, 100 };
            List <double> nus = integers.Select(x => ((double)x) / 100).ToList();
            foreach (double d in nus)
            {
                SVMParameter para = new SVMParameter();
                para.Gamma = besthitResult.parameter.Gamma;
                para.Nu = d;
                para.Kernel = besthitResult.parameter.Kernel;
                svmParams.Push(para);
            }
            count = 1;
            SetProgressMax(svmParams.Count + 1);
            List<Task> nuHit = new List<Task>();
            NoveltyResult bestHitnu = null;
            NoveltyResult bestCovnu = null;
            ConcurrentBag<string> hitNus = new ConcurrentBag<string>();
            for (int i = 0; i < threadMAX.Value; i++)
            {
                Task task = Task.Run(() => PredictionNuThread(ref count, sensor, start, end, ref svmParams, data, svmParams.Count, ref bestHitnu, ref bestCovnu, bestResultMu, ref hitNus));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);

            File.WriteAllLines(path + $"/{sensor.ToString()}HitNu.txt", hitNus.ToList());

            count = 1;
            //Hits
            foreach (double d in nus)
            {
                SVMParameter para = new SVMParameter();
                para.Gamma = bestCoveredResult.parameter.Gamma;
                para.Nu = d;
                para.Kernel = bestCoveredResult.parameter.Kernel;
                svmParams.Push(para);
            }
            SetProgressMax(svmParams.Count + 1);
            List<Task> nuCov = new List<Task>();
            ConcurrentBag<string> covNu = new ConcurrentBag<string>();
            for (int i = 0; i < threadMAX.Value; i++)
            {
                Task task = Task.Run(() => PredictionNuThread(ref count, sensor, start, end, ref svmParams, data, svmParams.Count, ref bestHitnu, ref bestCovnu, bestResultMu, ref covNu));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);

            File.WriteAllLines(path + $"/{sensor.ToString()}CovNu.txt", covNu.ToList());

            */
            Log.LogMessage($"best resulter on: {sensor.ToString()} - {besthitResult.CalculateHitScore()}");
            bestResultMu.Dispose();
            return Tuple.Create(besthitResult, bestCoveredResult);
        }
        private void PredictionThread(ref int count, SENSOR sensor, int start, int end, ref ConcurrentStack<SVMParameter> svmParams, List<SVMNode[]> data, int svmCount, ref NoveltyResult bestHitResult, ref NoveltyResult bestCoveredResult, Mutex mutex)
        {
            OneClassClassifier occ = new OneClassClassifier(data);
            List<OneClassFV> anomali = new List<OneClassFV>();
            List<Events> eventResult = new List<Events>();
            List<OneClassFV> outliersFromSam = new List<OneClassFV>();
            int maxCount = svmCount;
            string sensorPath = path + "/" + sensor.ToString();
            foreach (Events p in events)
            {
                var evt = p.Copy();
                eventResult.Add(evt);
            }
            while (!svmParams.IsEmpty)
            {
                SVMParameter svmParam = null;
                svmParams.TryPop(out svmParam);
                if (svmParam == null)
                {
                    break;
                }
                anomali = new List<OneClassFV>();
                occ.CreateModel(svmParam);
                anomali.AddRange(occ.PredictOutliers(featureVectors[sensor].Where(x => start < x.TimeStamp && x.TimeStamp < end).ToList()));
                PointsOfInterest dPointsOfInterest = new PointsOfInterest(anomali);

                foreach (Events evt in eventResult)
                {
                    evt.SetPointOfInterest(dPointsOfInterest);
                }

                NoveltyResult tempResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali);
                /*NoveltyResult.ConfusionMatrix cfm = tempResult.CalculateConfusionMatrix();
                decimal tpr = ((decimal)cfm.TruePostive) / ((decimal)cfm.TruePostive + cfm.FalseNegative);
                decimal fpr = 1 - ((decimal)cfm.TrueNegative / ((decimal)cfm.TrueNegative + cfm.FalsePostive));
                */
                mutex.WaitOne();
                if (bestHitResult == null)
                {
                    bestHitResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali);
                    Log.LogMessage(bestHitResult.CalculateHitScore().ToString());
                }
                else if (tempResult.CalculateHitScore() > bestHitResult.CalculateHitScore())
                {
                    //bestResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali); ;
                    bestHitResult = tempResult;
                    Log.LogMessage(bestHitResult.CalculateHitScore().ToString() + " with param ");
                    Log.LogMessage("C:" + bestHitResult.parameter.C + " Gamma" + bestHitResult.parameter.Gamma
                        + " Kernel " + bestHitResult.parameter.Kernel + " Nu:" + bestHitResult.parameter.Nu
                        + " POI's:"+bestHitResult.poi.GetFlaggedAreas().Count.ToString());
                }

                if (bestCoveredResult == null)
                {
                    bestCoveredResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali);
                    Log.LogMessage(bestHitResult.CalculateHitScore().ToString());
                }
                else if (tempResult.CalculateCoveredScore() > bestCoveredResult.CalculateCoveredScore())
                {
                    //bestResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali); ;
                    bestHitResult = tempResult;
                    Log.LogMessage(bestHitResult.CalculateHitScore().ToString() + " with param ");
                    Log.LogMessage("C:" + bestHitResult.parameter.C + " Gamma" + bestHitResult.parameter.Gamma
                        + " Kernel " + bestHitResult.parameter.Kernel + " Nu:" + bestHitResult.parameter.Nu
                        + " POI's:" + bestHitResult.poi.GetFlaggedAreas().Count.ToString());
                }

                count++;
                SetProgress(count, sensor);
                mutex.ReleaseMutex();
            }
            Log.LogMessage(sensor + " done!");
        }

        private void PredictionNuThread(ref int count, SENSOR sensor, int start, int end, ref ConcurrentStack<SVMParameter> svmParams, List<SVMNode[]> data, int svmCount, ref NoveltyResult bestHitResult, ref NoveltyResult bestCoveredResult, Mutex mutex, ref ConcurrentBag<string> nuResults)
        {
            OneClassClassifier occ = new OneClassClassifier(data);
            List<OneClassFV> anomali = new List<OneClassFV>();
            List<Events> eventResult = new List<Events>();
            List<OneClassFV> outliersFromSam = new List<OneClassFV>();
            int maxCount = svmCount;
            string sensorPath = path + "/" + sensor.ToString();
            foreach (Events p in events)
            {
                var evt = p.Copy();
                eventResult.Add(evt);
            }
            while (!svmParams.IsEmpty)
            {
                SVMParameter svmParam = null;
                svmParams.TryPop(out svmParam);
                if (svmParam == null)
                {
                    break;
                }
                anomali = new List<OneClassFV>();
                occ.CreateModel(svmParam);
                anomali.AddRange(occ.PredictOutliers(featureVectors[sensor].Where(x => start < x.TimeStamp && x.TimeStamp < end).ToList()));
                PointsOfInterest dPointsOfInterest = new PointsOfInterest(anomali);

                foreach (Events evt in eventResult)
                {
                    evt.SetPointOfInterest(dPointsOfInterest);
                }

                NoveltyResult tempResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali);
                /*NoveltyResult.ConfusionMatrix cfm = tempResult.CalculateConfusionMatrix();
                decimal tpr = ((decimal)cfm.TruePostive) / ((decimal)cfm.TruePostive + cfm.FalseNegative);
                decimal fpr = 1 - ((decimal)cfm.TrueNegative / ((decimal)cfm.TrueNegative + cfm.FalsePostive));
                */
                mutex.WaitOne();
                double temp = tempResult.FlaggedAreaSize();

                double temp2 = tempResult.CalculateTotalNormalArea();
                double areaCovered = ((double)tempResult.FlaggedAreaSize() / tempResult.CalculateTotalNormalArea() > 1) ? 1 : tempResult.FlaggedAreaSize() / (double)tempResult.CalculateTotalNormalArea();
                nuResults.Add($"{tempResult.parameter.Nu.ToString()}:"
                   + $"{tempResult.CalculateHitResult().eventHits/ (double)tempResult.CalculateHitResult().eventsTotal};" 
                   + $"{tempResult.CalculateHitResult().hits / ((double)tempResult.CalculateHitResult().misses+ tempResult.CalculateHitResult().hits)};"
                   + $"{areaCovered}");

                count++;
                SetProgress(count, sensor);
                mutex.ReleaseMutex();
            }
            Log.LogMessage(sensor + " done!");
        }


        private List<SVMParameter> GenerateOneClassSVMParameters()
        {
            List<double> gammaTypes = new List<double>() { };
            List<SVMKernelType> kernels = new List<SVMKernelType> { SVMKernelType.RBF, SVMKernelType.SIGMOID };

            for (int t = -14; t <= 2; t += 1)
            {
                gammaTypes.Add(Math.Pow(2, t));
            }
            //Generate SVMParams
            List<SVMParameter> svmParams = new List<SVMParameter>();
            foreach (SVMKernelType kernel in kernels)
            {
                for (int i = 0; i < gammaTypes.Count; i++)
                {
                    SVMParameter t = new SVMParameter();
                    t.Kernel = kernel;
                    t.Nu = 0.25;
                    t.Gamma = gammaTypes[i];
                    svmParams.Add(t);
                }
            }
            return svmParams;
        }

        const string COVERED = "Covered";
        const string PRES = "Precision";
        int VoteCounter = 1;
        int VoteCountMax = 1;
        private async void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                sw.Start();
                VoteCountMax = Directory.GetDirectories(fbd.SelectedPath).Count();
                //  NoveltyExcel excel = new NoveltyExcel(fbd.SelectedPath, "voting");
                foreach (string folder in Directory.GetDirectories(fbd.SelectedPath))
                {
                    statusLabel.Text = $"{VoteCounter}/{VoteCountMax}";
                    if (folder.Contains("Stats"))
                    {
                        continue;
                    }

                    path = folder + "/test";
                    string testSubjectId = folder.Split('\\').Last();
                    //excel.AddPersonToVotingBook(testSubjectId);
                    Log.LogMessage($"Loading Data: " + testSubjectId);
                    string[] tmpevents = File.ReadAllLines(path + "/SecondTest.dat");
                    int start = int.Parse(tmpevents.ToList().Find(x => x.Contains("ReplyToMail")).Split('#')[0]);
                    int end = int.Parse(tmpevents.Last().Split('#')[0]);
                    featureVectors = AnomaliSerializer.LoadFeatureVectors(path);

                    /*
                    //COVERED
                    //Load data and do scoring to find best parameter from Locked nu
                    double gsrNUCov = 0.45;
                    double eegNUCov = 0.3;
                    double hrNUCov = 0.35;
                    double faceNUCov = 0.35;
                    List<string> voteCov = new List<string>();

                    statusLabel.Text = $"{VoteCounter}/{VoteCountMax} (Covered)";
                    featureVectors = AnomaliSerializer.LoadFeatureVectors(path);
                    LoadEvents(tmpevents);
                    Dictionary<SENSOR, NoveltyResult> predictionResults = new Dictionary<SENSOR, NoveltyResult>();
                    //Do gridsearch       
                    Task<NoveltyResult> gsrThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.GSR, start, end, gsrNUCov, COVERED));
                    int gsrId = gsrThread.Id;
                    Task<NoveltyResult> eegThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.EEG, start, end, eegNUCov, COVERED));
                    int eegId = eegThread.Id;
                    Task<NoveltyResult> hrThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.HR, start, end, hrNUCov, COVERED));
                    int hrId = hrThread.Id;
                    Task<NoveltyResult> faceThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.FACE, start, end, faceNUCov, COVERED));
                    int faceId = faceThread.Id;
                    List<Task<NoveltyResult>> threads = new List<Task<NoveltyResult>>();
                    threads.Add(gsrThread);
                    threads.Add(eegThread);
                    threads.Add(hrThread);
                    threads.Add(faceThread);
                    await Task.WhenAll(threads);
                    foreach (Task<NoveltyResult> t in threads)
                    {
                        if (t.Id == gsrId)
                        {
                            predictionResults.Add(SENSOR.GSR, t.Result);
                            continue;
                        }
                        else if (t.Id == eegId)
                        {
                            predictionResults.Add(SENSOR.EEG, t.Result);
                            continue;
                        }
                        else if (t.Id == faceId)
                        {
                            predictionResults.Add(SENSOR.FACE, t.Result);
                            continue;
                        }
                        else if (t.Id == hrId)
                        {
                            predictionResults.Add(SENSOR.HR, t.Result);
                            continue;
                        }
                        else
                        {
                            Log.LogMessage("Could not match thread ID");
                        }

                    }


                    Dictionary<SENSOR, PointsOfInterest> pois = new Dictionary<SENSOR, PointsOfInterest>();
                    foreach (var key in predictionResults.Keys)
                    {
                        pois.Add(key, predictionResults[key].poi);
                    }
                    LoadEvents(tmpevents);
                    string covVotingPath = path + "/Cov";
                    Dictionary<int, NoveltyResult> results = new Dictionary<int, NoveltyResult>();
                    for (int i = 1; i <= 4; i++)
                    {
                        Voting vote = new Voting(start, end, pois, events, i);
                        NoveltyResult noveltyResult = vote.GetNoveltyResult();
                        noveltyResult.CalculateCoveredScore();
                        Log.LogMessage($"agreement for Person {testSubjectId}: " + i + " -" +noveltyResult.CalculateHitScore().ToString());
                        results.Add(i, noveltyResult);
                        if (!Directory.Exists(covVotingPath))
                        {
                            Directory.CreateDirectory(covVotingPath);
                        }
                        AnomaliSerializer.SaveVotingAnomalis(noveltyResult.anomalis, covVotingPath, STEP_SIZE, i.ToString());
                        AnomaliSerializer.SaveVotingEvents(noveltyResult.events, covVotingPath, i.ToString());
                        AnomaliSerializer.SaveVotingPointsOfInterest(noveltyResult.poi, covVotingPath, i.ToString());
                        double areaCovered = ((double)noveltyResult.FlaggedAreaSize() / noveltyResult.CalculateTotalNormalArea() > 1) ? 1 : noveltyResult.FlaggedAreaSize() / (double)noveltyResult.CalculateTotalNormalArea();
                           
                        voteCov.Add($"{i}:"
                                  + $"{noveltyResult.CalculateHitResult().eventHits / (double)noveltyResult.CalculateHitResult().eventsTotal};"
                                  + $"{noveltyResult.CalculateHitResult().hits / ((double)noveltyResult.CalculateHitResult().misses + noveltyResult.CalculateHitResult().hits)};"
                                  + $"{areaCovered}");
                    }
                    File.WriteAllLines(path + "/VotingCov.txt", voteCov);
                    */

                    //Pres
                    //Load data and do scoring to find best parameter from Locked nu
                    double gsrNUPres = 0.09;
                    double eegNUPres = 0.01;
                    double hrNUPres = 0.05;
                    double faceNUPres = 0.01;
                    List<string> votePres = new List<string>();

                    statusLabel.Text = $"{VoteCounter}/{VoteCountMax} (Pres)";
                    LoadEvents(tmpevents);
                    Dictionary<SENSOR, NoveltyResult> predictionPresResults = new Dictionary<SENSOR, NoveltyResult>();
                    //Do gridsearch       
                    Task<NoveltyResult> gsrPresThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.GSR, start, end, gsrNUPres, COVERED));
                    int gsrId = gsrPresThread.Id;
                    Task<NoveltyResult> eegPresThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.EEG, start, end, eegNUPres, COVERED));
                    int eegId = eegPresThread.Id;
                    Task<NoveltyResult> hrPresThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.HR, start, end, hrNUPres, COVERED));
                    int hrId = hrPresThread.Id;
                    Task<NoveltyResult> facePresThread = Task.Run(() => DoVotingNoveltyDetection(SENSOR.FACE, start, end, faceNUPres, COVERED));
                    int faceId = facePresThread.Id;
                    List<Task<NoveltyResult>> presThreads = new List<Task<NoveltyResult>>();
                    presThreads.Add(gsrPresThread);
                    presThreads.Add(eegPresThread);
                    presThreads.Add(hrPresThread);
                    presThreads.Add(facePresThread);
                    await Task.WhenAll(presThreads);
                    foreach (Task<NoveltyResult> t in presThreads)
                    {
                        if (t.Id == gsrId)
                        {
                            predictionPresResults.Add(SENSOR.GSR, t.Result);
                            continue;
                        }
                        else if (t.Id == eegId)
                        {
                            predictionPresResults.Add(SENSOR.EEG, t.Result);
                            continue;
                        }
                        else if (t.Id == faceId)
                        {
                            predictionPresResults.Add(SENSOR.FACE, t.Result);
                            continue;
                        }
                        else if (t.Id == hrId)
                        {
                            predictionPresResults.Add(SENSOR.HR, t.Result);
                            continue;
                        }
                        else
                        {
                            Log.LogMessage("Could not match thread ID");
                        }

                    }


                    Dictionary<SENSOR, PointsOfInterest> poisPres = new Dictionary<SENSOR, PointsOfInterest>();
                    foreach (var key in predictionPresResults.Keys)
                    {
                        poisPres.Add(key, predictionPresResults[key].poi);
                    }
                    LoadEvents(tmpevents);
                    string presVotingPath = path + "/Pres";
                    Dictionary<int, NoveltyResult> presVotingResult = new Dictionary<int, NoveltyResult>();
                    for (int i = 1; i <= 4; i++)
                    {
                        Voting vote = new Voting(start, end, poisPres, events, i);
                        NoveltyResult noveltyResult = vote.GetNoveltyResult();
                        noveltyResult.CalculateHitResult();
                        Log.LogMessage($"agreement for Person {testSubjectId}: " + i + " -" + noveltyResult.CalculateHitScore().ToString());
                        presVotingResult.Add(i, noveltyResult);
                        if (!Directory.Exists(presVotingPath))
                        {
                            Directory.CreateDirectory(presVotingPath);
                        }
                        AnomaliSerializer.SaveVotingAnomalis(noveltyResult.anomalis, presVotingPath, STEP_SIZE, i.ToString());
                        AnomaliSerializer.SaveVotingEvents(noveltyResult.events, presVotingPath, i.ToString());
                        AnomaliSerializer.SaveVotingPointsOfInterest(noveltyResult.poi, presVotingPath, i.ToString());
                        double areaCovered = ((double)noveltyResult.FlaggedAreaSize() / noveltyResult.CalculateTotalNormalArea() > 1) ? 1 : noveltyResult.FlaggedAreaSize() / (double)noveltyResult.CalculateTotalNormalArea();

                        votePres.Add($"{i}:"
                                  + $"{noveltyResult.CalculateHitResult().eventHits / (double)noveltyResult.CalculateHitResult().eventsTotal};"
                                  + $"{noveltyResult.CalculateHitResult().hits / ((double)noveltyResult.CalculateHitResult().misses + noveltyResult.CalculateHitResult().hits)};"
                                  + $"{areaCovered}");
                    }
                    File.WriteAllLines(path + "/PresVoting.txt", votePres);
                }
                // excel.CloseHandler();
                statusLabel.Text = "Donno.dk";
                Log.LogMessage("Done!");
            }
        }

        private List<SVMParameter> GenerateVotingSVMParameters(double nu)
        {
            List<double> gammaTypes = new List<double>() { };
            List<SVMKernelType> kernels = new List<SVMKernelType> { SVMKernelType.RBF, SVMKernelType.SIGMOID };

            for (int t = -14; t <= 2; t += 1)
            {
                gammaTypes.Add(Math.Pow(2, t));
            }
            //Generate SVMParams
            List<SVMParameter> svmParams = new List<SVMParameter>();
            foreach (SVMKernelType kernel in kernels)
            {
                for (int i = 0; i < gammaTypes.Count; i++)
                {
                    SVMParameter t = new SVMParameter();
                    t.Kernel = kernel;
                    t.Nu = nu;
                    t.Gamma = gammaTypes[i];
                    svmParams.Add(t);
                }
            }
            return svmParams;
        }

        private async Task<NoveltyResult> DoVotingNoveltyDetection(SENSOR sensor, int start, int end, double nu, string type)
        {
            string sensorPath = path + "/" + sensor.ToString();
            var data = featureVectors[sensor].Select(x => x.Features).ToList();
            ConcurrentStack<SVMParameter> svmParams = new ConcurrentStack<SVMParameter>();
            svmParams.PushRange(GenerateVotingSVMParameters(nu).ToArray());
            SetProgressMax(svmParams.Count + 1);
            NoveltyResult bestResult = null;
            Mutex bestResultMu = new Mutex(false, sensor.ToString());
            int count = 1;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < threadMAX.Value; i++)
            {
                Task task = Task.Run(() => PredictionVoteThread(ref count, sensor, start, end, ref svmParams, data, svmParams.Count, ref bestResult, bestResultMu, type));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
            
            bestResultMu.Dispose();
            return bestResult;
        }

        private void PredictionVoteThread(ref int count, SENSOR sensor, int start, int end, ref ConcurrentStack<SVMParameter> svmParams, List<SVMNode[]> data, int svmCount, ref NoveltyResult bestResult, Mutex mutex, string type)
        {
            OneClassClassifier occ = new OneClassClassifier(data);
            List<OneClassFV> anomali = new List<OneClassFV>();
            List<Events> eventResult = new List<Events>();
            List<OneClassFV> outliersFromSam = new List<OneClassFV>();
            int maxCount = svmCount;
            string sensorPath = path + "/" + sensor.ToString();
            foreach (Events p in events)
            {
                var evt = p.Copy();
                eventResult.Add(evt);
            }
            while (!svmParams.IsEmpty)
            {
                SVMParameter svmParam = null;
                svmParams.TryPop(out svmParam);
                if (svmParam == null)
                {
                    break;
                }
                anomali = new List<OneClassFV>();
                occ.CreateModel(svmParam);
                anomali.AddRange(occ.PredictOutliers(featureVectors[sensor].Where(x => start < x.TimeStamp && x.TimeStamp < end).ToList()));
                PointsOfInterest dPointsOfInterest = new PointsOfInterest(anomali);

                foreach (Events evt in eventResult)
                {
                    evt.SetPointOfInterest(dPointsOfInterest);
                }

                NoveltyResult tempResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali);
                mutex.WaitOne();
                if (bestResult == null)
                {
                    bestResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, svmParam, anomali);
                }
                else if ((type==COVERED) ? tempResult.CalculateCoveredScore() > bestResult.CalculateCoveredScore() 
                                         : tempResult.CalculateHitScore() > bestResult.CalculateHitScore())
                {
                    bestResult = tempResult;
                }
                count++;
                SetProgress(count, sensor);
                mutex.ReleaseMutex();
            }
            Log.LogMessage(sensor + " done!");
        }
    }
}