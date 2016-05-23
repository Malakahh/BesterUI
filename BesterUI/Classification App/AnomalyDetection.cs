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


        Dictionary<SENSOR, List<OneClassFV>> featureVectors = new Dictionary<SENSOR, List<OneClassFV>>();
        Dictionary<SENSOR, List<int>> predictions = new Dictionary<SENSOR, List<int>>();

        List<Events> events = new List<Events>();
        List<samEvents> sEvents = new List<samEvents>();

        public AnomalyDetection()
        {
            InitializeComponent();
            foreach (var k in Enum.GetValues(typeof(SENSOR)))
            {
                featureVectors.Add((SENSOR)k, new List<OneClassFV>());
                predictions.Add((SENSOR)k, new List<int>());
            }
        }

        private void btn_loadData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select folder to load test subjects from" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {

                string path = fbd.SelectedPath;
                string testSubjectId = path.Split('\\')[path.Split('\\').Length - 2];

                _fdAnomaly.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);

                string[] tmpevents = File.ReadAllLines(path + @"\SecondTest.dat");
                foreach (string ev in tmpevents)
                {
                    string[] split = ev.Split('#');
                    events.Add(new Events(int.Parse(split[0]), split[1]));
                }


                string[] tmpSevents = File.ReadAllLines(path + @"\sam.dat");


                foreach (string ev in tmpSevents)
                {
                    sEvents.Add(new samEvents(int.Parse(ev.Split(':')[0]), int.Parse(ev.Split(':')[1]), int.Parse(ev.Split(':')[2])));
                }
                
                SetupMachines();
                CreateGSRFeatures(_fdAnomaly.gsrData);
                CreateEEGFeatures(_fdAnomaly.eegData.ToList<DataReading>());
                CreateHRFeatures(_fdAnomaly.hrData);
                CreateFACEFeatures(_fdAnomaly.faceData.ToList<DataReading>());
                foreach (var k in featureVectors.Keys)
                {
                    featureVectors[k]= featureVectors[k].NormalizeFeatureVectorList(Normalize.ZeroOne).ToList();
                }
            }
        }

        private void SlideWindow(SENSOR s)
        {

            /* List<int> timeStamps = new List<int>();

             var predictionSet = dataSet.SkipWhile(x => x.Item2 < trainingEnd);

             int count = predictionSet.Count();
             int firstPredcition = predictionSet.First().Item2;

             predictions[s.ToString()].AddRange(SVMThisShit(trainingSet, predictionSet));
             */
        }

        private void CreateHRFeatures(List<HRDataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (HR_DELAY + HR_DURATION); time += STEP_SIZE)
            {
                List<double> featureVector = new List<double>();
                List<double> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < time + HR_DELAY).TakeWhile(x => time + HR_DURATION + HR_DELAY > (x.timestamp - data.First().timestamp)).Select(x => (double)x.IBI).ToList();
                if (d.Count == 0)
                {
                    continue;
                }

                featureVector.Add(d.Average());
                featureVector.Add(d.Max());
                featureVector.Add(d.Min());
                double sd = Math.Sqrt(d.Average(x => Math.Pow(x - d.Average(), 2)));
                featureVector.Add(sd);
                featureVectors[SENSOR.HR].Add(new OneClassFV(featureVector, time));
            }
        }

        private void CreateEEGFeatures(List<DataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (EEG_DELAY + EEG_DURATION); time += STEP_SIZE)
            {
                List<double> featureVector = new List<double>();
                List<DataReading> slice = data.SkipWhile(x => time + EEG_DELAY > x.timestamp).TakeWhile(x => time + EEG_DELAY + EEG_DURATION > x.timestamp).ToList();
                List<string> names = new List<string>() { "Delta", "Theta", "Alpha", "Beta", "Gamma" };
                if (slice.Count == 0)
                {
                    continue;
                }
                foreach (string name in names)
                {
                    //Arousal 
                    featureVector.Add(FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));

                    featureVector.Add(FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString()))));

                    //Valence
                    featureVector.Add(FeatureCreator.DASM(slice, name,
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF3.ToString())),
                        (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.AF4.ToString()))));

                    featureVector.Add(FeatureCreator.DASM(slice, name,
                       (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F3.ToString())),
                       (x => FeatureCreator.EEGValueAccessor(x, EEGDataReading.ELECTRODE.F4.ToString()))));

                }
                featureVectors[SENSOR.EEG].Add(new OneClassFV(featureVector, time));
            }
        }


        private void CreateFACEFeatures(List<DataReading> data)
        {
            List<int> leftSide = new List<int>() { 5, 13, 15 };
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (FACE_DELAY + FACE_DURATION); time += STEP_SIZE)
            {
                List<DataReading> dataSlice = data.SkipWhile(x => time + FACE_DELAY > x.timestamp).TakeWhile(x => time + FACE_DELAY + FACE_DURATION > x.timestamp).ToList();
                List<double> featureVector = new List<double>();
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

                    featureVector.Add(mean);
                    featureVector.Add(sd);
                    featureVector.Add(max);
                    featureVector.Add(min);
                }

                featureVectors[SENSOR.FACE].Add(new OneClassFV(featureVector, time));
            }
        }

        private void CreateGSRFeatures(List<GSRDataReading> data)
        {
            for (int time = 0; time < data.Last().timestamp - data.First().timestamp - (GSR_DELAY + GSR_DURATION); time += STEP_SIZE)
            {
                List<double> featureVector = new List<double>();
                List<double> slice = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < GSR_DELAY + time).TakeWhile(x => time + GSR_DELAY + GSR_DURATION > (x.timestamp - data.First().timestamp)).Select(x => (double)x.resistance).ToList();
                if (slice.Count == 0)
                {
                    continue;
                }
                featureVector.Add(slice.Average());
                featureVector.Add(slice.Max());
                featureVector.Add(slice.Min());
                double sd = Math.Sqrt(slice.Average(x => Math.Pow(x - slice.Average(), 2)));
                featureVector.Add(sd);
                featureVectors[SENSOR.GSR].Add(new OneClassFV(featureVector, time));
            }
        }   

        private List<OneClassFV> PredictSlice(SENSOR machine, List<OneClassFV> data)
        {
            return machines[machine.ToString()].PredictOutliers(data);
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

            List<List<double>> data = new List<List<double>>();
            int amount = 0;
            for (int i = 0; i < 10000; i++)
            {
                List<double> tmp = new List<double>();
                for (int k = 0; k < 4; k++)
                {
                    Random r = new Random();
                    Random rr = new Random();

                    if (rr.Next(1, 100) > 80 && amount > 11)
                    {
                        tmp.Add(80);
                        amount++;
                    }
                    else
                    {
                        tmp.Add(r.Next(1, 3));
                    }

                }

                data.Add(tmp);
            }

            //CreateSVM(SENSOR.GSR, GetTrainingData(SENSOR.GSR, trainingStart, trainingEnd));
            CreateSVM(SENSOR.GSR, data);

            //CreateSVM(SENSOR.EEG);
            //CreateSVM(SENSOR.FACE);
            //CreateSVM(SENSOR.HR);
        }
        Dictionary<string, OneClassClassifier> machines = new Dictionary<string, OneClassClassifier>();

        private void CreateSVM(SENSOR machine, List<List<double>> trainingSet)
        {

            OneClassClassifier occ = new OneClassClassifier(trainingSet);
            SVMParameter svmP = new SVMParameter();
            svmP.Kernel = SVMKernelType.RBF;
            svmP.C = 100;
            svmP.Gamma = 0.01;
            svmP.Nu = 0.01;
            svmP.Type = SVMType.ONE_CLASS;
            occ.CreateModel(svmP);

            machines.Add(machine.ToString(), occ);
        }

        //PREDICT
        /*private List<int> SVMThisShit(IEnumerable<Tuple<List<double>, int>> trainingSet, IEnumerable<Tuple<List<double>, int>> predictionSet)
        {
            List<int> indexes = occ.PredictOutliers(predictionSet.Select(x => x.Item1).ToList());
            return indexes;
        }*/

        private void btn_getData_Click(object sender, EventArgs e)
        {
            //SlideWindow(SENSOR.GSR);
            //SlideWindow(SENSOR.HR);
            //SlideWindow(SENSOR.FACE);

        }


    }
}