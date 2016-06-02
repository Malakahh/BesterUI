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

        public void SetProgress(int value, SENSOR sensor, int max)
        {
            
            switch (sensor)
            {
                case SENSOR.EEG:
                    this.Invoke((MethodInvoker)delegate {
                        eegProgress.Maximum = max;
                        eegProgress.Value = value;
                    });
                    break;
                case SENSOR.HR:
                    this.Invoke((MethodInvoker)delegate {
                        hrProgress.Value = value;
                        hrProgress.Maximum = max;
                    });
                    break;
                case SENSOR.GSR:
                    this.Invoke((MethodInvoker)delegate {
                        gsrProgress.Value = value;
                        gsrProgress.Maximum = max;
                    });
                    break;
                case SENSOR.FACE:
                    this.Invoke((MethodInvoker)delegate {
                        faceProgress.Value = value;
                        faceProgress.Maximum = max;
                    });
                    break;
            }
        }

        FusionData _fdAnomaly = new FusionData();

        // MS

        /**
           GSR Window 2-7s
           */
        const int GSR_DELAY = 2000;
        const int GSR_DURATION = 5000;

        /**
            EEG Window 0.350-1.060s
            */
        const int EEG_DELAY = 350;
        const int EEG_DURATION = 710;

        /**
            Face Window 0.5-1s
            */
        const int FACE_DELAY = 500;
        const int FACE_DURATION = 500;

        /**
            HR Window 4-7s
            */
        const int HR_DELAY = 4000;
        const int HR_DURATION = 3000;

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
                SVMNode[] featureVector = new SVMNode[6];
                List<HRDataReading> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < time + HR_DELAY).TakeWhile(x => time + HR_DURATION + HR_DELAY > (x.timestamp - data.First().timestamp)).Where(x => x.isBeat).ToList();
                if (d.Count == 0)
                {
                    continue;
                }
                featureVector[0] = new SVMNode(1, d.Select(x => (double)x.IBI).Average());
                double sd = Math.Sqrt(d.Average(x => Math.Pow((double)x.IBI - featureVector[0].Value, 2)));
                featureVector[1] = new SVMNode(2, sd);
                featureVector[2] = new SVMNode(3, d.Select(x => (double)x.BPM).Average());
                featureVector[3] = new SVMNode(4, FeatureCreator.HRVRMSSD(d.ToList<DataReading>()));
                featureVector[4] = new SVMNode(5, d.Max(x => x.BPM));
                featureVector[5] = new SVMNode(6, d.Min(x => x.BPM));
                featureVectors[SENSOR.HR].Add(new OneClassFV(featureVector, time));
            }
            Log.LogMessage($"Calculation HR Feature Done: {sw.Elapsed}");
        }

        private void CreateEEGFeatures(List<DataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (EEG_DELAY + EEG_DURATION); time += STEP_SIZE)
            {
                SVMNode[] featureVector = new SVMNode[4];
                List<DataReading> slice = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < EEG_DELAY + time).TakeWhile(x => time + EEG_DELAY + EEG_DURATION > (x.timestamp - data.First().timestamp)).ToList();
                List<string> names = new List<string>() { "Delta", "Theta", "Alpha", "Beta", "Gamma" };
                if (slice.Count == 0)
                {
                    continue;
                }
                foreach (string name in names)
                {
                    //Arousal 
                    featureVector[0] = new SVMNode(1, FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));

                    featureVector[1] = new SVMNode(2, FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString()))));

                    //Valence
                    featureVector[2] = new SVMNode(3, FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));

                    featureVector[3] = new SVMNode(4, FeatureCreator.DASM(slice, name,
                       (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                       (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString()))));

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
                    continue;

                SVMNode[] featureVector = new SVMNode[4];
                foreach (int fsa in leftSide)
                {
                    double mean = FeatureCreator.FaceMean(dataSlice,
                    (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                    (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                    double sd = FeatureCreator.FaceStandardDeviation(dataSlice,
                         (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                         (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                    double max = FeatureCreator.FaceMax(dataSlice,
                         (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                         (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                    double min = FeatureCreator.FaceMin(dataSlice,
                         (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                         (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                    featureVector[0] = new SVMNode(1, mean);
                    featureVector[1] = new SVMNode(2, sd);
                    featureVector[2] = new SVMNode(3, max);
                    featureVector[3] = new SVMNode(4, min);
                }

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
                featureVector[1] = new SVMNode(2, slice.Max());
                featureVector[2] = new SVMNode(3, slice.Min());
                double sd = Math.Sqrt(slice.Average(x => Math.Pow(x - slice.Average(), 2)));
                featureVector[3] = new SVMNode(4, sd);
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
            int trainingEnd = events[2].endTimestamp;
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
                    events.Add(new InstantEvents(int.Parse(temp[0]) + 2000, int.Parse(temp[0]) + 2000, temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            //Send draft
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("SendDraft error shown"))
                {
                    events.Add(new InstantEvents(int.Parse(temp[0]), int.Parse(temp[0]), temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            //Not responding
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("NotResponding"))
                {
                    events.Add(new InstantEvents(int.Parse(temp[0]), int.Parse(temp[0]), temp[1]));
                    eventList.RemoveAt(i);
                }
            }
            //Remove contact
            for (int i = 0; i < eventList.Count; i++)
            {
                string[] temp = eventList[i].Split('#');
                if (temp[1].Contains("RemoveContact clicked"))
                {
                    events.Add(new InstantEvents(int.Parse(temp[0]), int.Parse(temp[0]), temp[1]));
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
            events.Add(new SpanningEvent(firstContactClick, lastContactClick, "Send Draft", 0.5));

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
            events.Add(new SpanningEvent(firstCaretMoved, lastCaretMoved, "CaretMoved", 0.20));

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
            events.Add(new SpanningEvent(langugeChanged, languageTaskDone, "Language Changed", 0.25));


            #endregion
        }

        private async void runAllButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                foreach (string folder in Directory.GetDirectories(fbd.SelectedPath))
                {
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

                    SetupMachines();
                    Log.LogMessage($"Done setting up machines for {testSubjectId}");
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                sw.Start();
                foreach (string folder in Directory.GetDirectories(fbd.SelectedPath))
                {
                    path = folder + "/test";
                    string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];
                    Log.LogMessage($"Loading Data: " + testSubjectId);
                    featureVectors = AnomaliSerializer.LoadFeatureVectors(path );
                    string[] tmpevents = File.ReadAllLines(path + "/SecondTest.dat");
                    int start = int.Parse(tmpevents.ToList().Find(x => x.Contains("ReplyToMail")).Split('#')[0]);
                    int end = int.Parse(tmpevents.Last().Split('#')[0]);
                    LoadEvents(tmpevents);
                    Dictionary<SENSOR, NoveltyResult> predictionResults = new Dictionary<SENSOR, NoveltyResult>();
                    //Do gridsearch       
                    Task gsrThread = Task.Run(() => predictionResults.Add(SENSOR.GSR, DoNoveltyDetection(SENSOR.GSR, start, end)));
                    Task eegThread = Task.Run(() => predictionResults.Add(SENSOR.EEG, DoNoveltyDetection(SENSOR.EEG, start, end)));
                    Task hrThread = Task.Run(() => predictionResults.Add(SENSOR.HR, DoNoveltyDetection(SENSOR.HR, start, end)));
                    Task faceThread = Task.Run(() => predictionResults.Add(SENSOR.FACE, DoNoveltyDetection(SENSOR.FACE, start, end)));
                    List<Task> threads = new List<Task>();
                    threads.Add(gsrThread);
                    threads.Add(eegThread);
                    threads.Add(hrThread);
                    threads.Add(faceThread);
                    await Task.WhenAll(threads.ToArray());
                    Dictionary<SENSOR, List<OneClassFV>> anomalis = new Dictionary<SENSOR, List<OneClassFV>>();
                    Dictionary<SENSOR, List<Events>> eventResult = new Dictionary<SENSOR, List<Evnt.Events>>();
                    Dictionary<SENSOR, PointsOfInterest> dPointsOfInterest = new Dictionary<SENSOR, PointsOfInterest>();
                    foreach (var key in predictionResults.Keys)
                    {
                        anomalis.Add(key, predictionResults[key].anomalis);
                        eventResult.Add(key, predictionResults[key].events);
                        dPointsOfInterest.Add(key, predictionResults[key].poi);
                        Log.LogMessage($"Person done in {sw.Elapsed}, best {predictionResults[key].CalculateScore()}");

                    }
                    AnomaliSerializer.SaveAnomalis(anomalis, path, STEP_SIZE);
                    AnomaliSerializer.SaveEvents(eventResult, path);
                    AnomaliSerializer.SavePointsOfInterest(dPointsOfInterest, path);
                }
            }
        }

        private NoveltyResult DoNoveltyDetection(SENSOR sensor, int start, int end)
        {
            var data = featureVectors[sensor].Select(x => x.Features).ToList();
            OneClassClassifier occ = new OneClassClassifier(data);
            List<SVMParameter> svmParams = GenerateOneClassSVMParameters();
            NoveltyResult bestResult = null;
            int count = 1;
            List<OneClassFV> anomali = new List<OneClassFV>();
            List<Events> eventResult = new List<Events>();
            List<OneClassFV> outliersFromSam = new List<OneClassFV>();
            foreach (Events p in events)
            {
                var evt = p.Copy();
                eventResult.Add(evt);
            }
            foreach (SVMParameter param in svmParams)
            {
                anomali = new List<OneClassFV>();
                SetProgress(count, sensor, svmParams.Count+1);
                occ.CreateModel(param);
                anomali.AddRange(occ.PredictOutliers(featureVectors[sensor]));
                PointsOfInterest dPointsOfInterest = new PointsOfInterest(anomali);

                foreach (Events evt in eventResult)
                {
                    evt.SetPointOfInterest(dPointsOfInterest);
                }

                if (bestResult == null)
                {
                    bestResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, param, anomali);
                    Log.LogMessage(bestResult.CalculateScore().ToString());
                }
                else if (NoveltyResult.CalculateEarlyScore(dPointsOfInterest, eventResult, start, end) > bestResult.CalculateScore())
                {
                    bestResult = new NoveltyResult(dPointsOfInterest, eventResult, start, end, param, anomali); ;
                    Log.LogMessage(bestResult.CalculateScore().ToString() + " with param ");
                    Log.LogMessage("C:" + bestResult.parameter.C + " Gamma" + bestResult.parameter.Gamma
                        + " Kernel " + bestResult.parameter.Kernel + " Nu:" + bestResult.parameter.Nu);
                }
                count++;
                double tt = bestResult.CalculateScore();
            }
            return bestResult;
        }

        private List<SVMParameter> GenerateOneClassSVMParameters()
        {
            List<double> cTypes = new List<double>() { };
            List<double> gammaTypes = new List<double>() { };
            List<SVMKernelType> kernels = new List<SVMKernelType> { SVMKernelType.RBF, SVMKernelType.SIGMOID };
            for (int t = -4; t <= 12; t+=1)
            {
                cTypes.Add(Math.Pow(2, t));
            }
            for (int t = -14; t <= 2; t += 1)
            {
                gammaTypes.Add(Math.Pow(2, t));
            }
            //Generate SVMParams
            List<SVMParameter> svmParams = new List<SVMParameter>();
            foreach (SVMKernelType kernel in kernels)
            {
                foreach (double c in cTypes)
                {
                        for (int i = 0;i < gammaTypes.Count ; i++)
                        {
                            SVMParameter t = new SVMParameter();
                            t.Kernel = kernel;
                            t.C = c;
                            t.Nu = 0.05;
                            t.Gamma = gammaTypes[i];
                            svmParams.Add(t);
                        }
                }
            }
            return svmParams;
        }
    }
}