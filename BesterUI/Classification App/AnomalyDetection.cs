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


        Dictionary<string, List<List<double>>> featureVectors = new Dictionary<string, List<List<double>>>();
        Dictionary<string, List<int>> predictions = new Dictionary<string, List<int>>();

        List<Events> events = new List<Events>();
        List<samEvents> sEvents = new List<samEvents>();

        public AnomalyDetection()
        {
            InitializeComponent();
            featureVectors.Add("GSR", new List<List<double>>());
            featureVectors.Add("EEG", new List<List<double>>());
            featureVectors.Add("FACE", new List<List<double>>());
            featureVectors.Add("HR", new List<List<double>>());

            predictions.Add("GSR", new List<int>());
            predictions.Add("EEG", new List<int>());
            predictions.Add("FACE", new List<int>());
            predictions.Add("HR", new List<int>());

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

        private void GetHRFeatures(List<HRDataReading> data, int i)
        {
            List<double> featureVector = new List<double>();
            List<double> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < i + HR_DELAY).TakeWhile(x => i + HR_DURATION > (x.timestamp - data.First().timestamp)).Select(x => (double)x.IBI).ToList();
            if (d.Count == 0)
                return;

            featureVector.Add(d.Average());
            featureVector.Add(d.Max());
            featureVector.Add(d.Min());
            double sd = Math.Sqrt(d.Average(x => Math.Pow(x - d.Average(), 2)));
            featureVector.Add(sd);
            featureVectors["HR"].Add(featureVector);

        }

        private void GetFACEFeatures(List<DataReading> data, int i, int windowSize)
        {
            List<double> featureVector = new List<double>();
            List<int> leftSide = new List<int>() { 5, 13, 15 };

            foreach (int fsa in leftSide)
            {
                double mean = FeatureCreator.FaceMean(data,
                (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                double sd = FeatureCreator.FaceStandardDeviation(data,
                     (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                     (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                double max = FeatureCreator.FaceMax(data,
                     (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                     (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                double min = FeatureCreator.FaceMin(data,
                     (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa)),
                     (x => FeatureCreator.KinectValueAccessor(x, (FaceShapeAnimations)fsa + 1)));

                featureVector.Add(mean);
                featureVector.Add(sd);
                featureVector.Add(max);
                featureVector.Add(min);
            }

            featureVectors["FACE"].Add(featureVector);



        }

        private List<List<double>> CreateGSRFeatures(List<GSRDataReading> data)
        {
            List<double> featureVector = new List<double>();
            List<double> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < GSR_DELAY).TakeWhile(x => GSR_DURATION > (x.timestamp - data.First().timestamp)).Select(x => (double)x.resistance).ToList();
            if (d.Count == 0)
                return new List<List<double>>();
            featureVector.Add(d.Average());
            featureVector.Add(d.Max());
            featureVector.Add(d.Min());
            double sd = Math.Sqrt(d.Average(x => Math.Pow(x - d.Average(), 2)));
            featureVector.Add(sd);
            featureVectors["GSR"].Clear();
            featureVectors["GSR"].Add(featureVector);
            return featureVectors["GSR"];
        }

        private List<int> PredictSlice(SENSOR machine, List<List<double>> data)
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

            for (int i = 2; i < events.Count; i++)
            {
                List<GSRDataReading> gsrData = Extensions.GetDataFromInterval(_fdAnomaly.gsrData.Cast<DataReading>().ToList(), events[i].timestamp, SENSOR.GSR).Cast<GSRDataReading>().ToList();
                predictions["GSR"].AddRange(PredictSlice(SENSOR.GSR, CreateGSRFeatures(gsrData).ToList()));
            }

            var x = predictions;
            //SlideWindow(SENSOR.GSR);
            //SlideWindow(SENSOR.HR);
            //SlideWindow(SENSOR.FACE);


        }


    }
}