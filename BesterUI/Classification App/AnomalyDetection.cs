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
using Microsoft.Kinect.Face;

namespace Classification_App
{
    enum SENSOR { GSR, EEG, HR, FACE };
    public partial class AnomalyDetection : Form
    {

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

        const int STEP_SIZE = 100;


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

        private async void btn_loadData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {

                string path = fbd.SelectedPath;
                string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];
                sw.Start();
                Log.LogMessage($"Loading Data");
                _fdAnomaly.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);

                Log.LogMessage($"Getting Events: {sw.Elapsed}");
                string[] tmpevents = File.ReadAllLines(path + @"\SecondTest.dat");
                foreach (string ev in tmpevents)
                {
                    string[] split = ev.Split('#');
                    events.Add(new Events(int.Parse(split[0]), split[1]));
                }


                /*string[] tmpSevents = File.ReadAllLines(path + @"\sam.dat");


                foreach (string ev in tmpSevents)
                {
                    sEvents.Add(new samEvents(int.Parse(ev.Split(':')[0]), int.Parse(ev.Split(':')[1]), int.Parse(ev.Split(':')[2])));
                }*/

                Task gsrThread = Task.Run(() => CreateGSRFeatures(_fdAnomaly.gsrData));
                Task eegThread = Task.Run(() => CreateEEGFeatures(_fdAnomaly.eegData.ToList<DataReading>()));
                Task hrThread = Task.Run(() => CreateHRFeatures(_fdAnomaly.hrData));
                Task faceThread = Task.Run(()=> CreateFACEFeatures(_fdAnomaly.faceData.ToList<DataReading>()));
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


                SetupMachines();
                Log.LogMessage("Done setting up machines");

            }
        }

        private void CreateHRFeatures(List<HRDataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (HR_DELAY + HR_DURATION); time += STEP_SIZE)
            {
                SVMNode[] featureVector = new SVMNode[6];
                List<HRDataReading> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < time + HR_DELAY).TakeWhile(x => time + HR_DURATION + HR_DELAY > (x.timestamp - data.First().timestamp)).Where(x=>x.isBeat).ToList();
                if (d.Count == 0)
                {
                    continue;
                }
                featureVector[0] = new SVMNode(1,d.Select(x=>(double)x.IBI).Average());
                double sd = Math.Sqrt(d.Average(x => Math.Pow((double)x.IBI - featureVector[0].Value, 2)));
                featureVector[1] = new SVMNode(2,sd);
                featureVector[2] = new SVMNode(3,d.Select(x => (double)x.BPM).Average());
                featureVector[3] = new SVMNode(4,FeatureCreator.HRVRMSSD(d.ToList<DataReading>()));
                featureVector[4] = new SVMNode(5,d.Max(x=>x.BPM));
                featureVector[5] = new SVMNode(6,d.Min(x => x.BPM));
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
                    featureVector[0] = new SVMNode(1,FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));

                    featureVector[1] = new SVMNode(2,FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString()))));

                    //Valence
                    featureVector[2] = new SVMNode(3,FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));

                    featureVector[3] = new SVMNode(4,FeatureCreator.DASM(slice, name,
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

                    featureVector[0] = new SVMNode(1,mean);
                    featureVector[1] = new SVMNode(2,sd);
                    featureVector[2] = new SVMNode(3,max);
                    featureVector[3] = new SVMNode(4,min);
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

            int trainingStart = (useRestInTraining.Checked) ? 180000 : 0;
            int trainingEnd = events[2].timestamp;
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
            Events rest_event = new Events(0, "Resting Period");

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
            }
        }


        private void btn_getData_Click(object sender, EventArgs e)
        {
            List<OneClassFV> anomali = new List<OneClassFV>();
            List<OneClassFV> outliersFromSam = new List<OneClassFV>();
            sw.Restart();
            /*  foreach (Events ev in events)
              {
                  var preds = PredictSlice(SENSOR.GSR, FindNearestTimeStamp(featureVectors[SENSOR.GSR], ev.timestamp));
                  anomali.AddRange(preds);
              }

              foreach (samEvents sev in sEvents)
              {
                  var preds = PredictSlice(SENSOR.GSR, FindNearestTimeStamp(featureVectors[SENSOR.GSR], sev.timestamp));
                  outliersFromSam.AddRange(preds);
              }*/
            foreach (SENSOR key in featureVectors.Keys)
            {

                Log.LogMessage($"Predicting {key}: {sw.Elapsed}");
                anomali = PredictSlice(key, featureVectors[key]);
            }
            

            var x = anomali;
        }


    }
}