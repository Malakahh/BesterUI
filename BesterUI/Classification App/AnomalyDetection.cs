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

        string[] events = new string[0];
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

                events = File.ReadAllLines(path + @"\SecondTest.dat");

                string[] tmpSevents = File.ReadAllLines(path + @"\sam.dat");
                foreach (string ev in tmpSevents)
                {
                    sEvents.Add(new samEvents(int.Parse(ev.Split(':')[0]), int.Parse(ev.Split(':')[1]), int.Parse(ev.Split(':')[2])));
                }
            }
        }

        private void SlideWindow(SENSOR s)
        {
            List<DataReading> data = new List<DataReading>();
            int stepSize = 0;
            int windowSize = 0;
            if (s == SENSOR.EEG)
            {
                data = _fdAnomaly.eegData.Cast<DataReading>().ToList();
                windowSize = EEG_DURATION;
                stepSize = 100;
            }
            else if (s == SENSOR.GSR)
            {
                data = _fdAnomaly.gsrData.Cast<DataReading>().ToList();
                windowSize = GSR_DURATION;
                stepSize = 100;
            }
            else if (s == SENSOR.HR)
            {
                data = _fdAnomaly.hrData.Cast<DataReading>().ToList();
                windowSize = HR_DURATION;
                stepSize = 100;
            }
            else if (s == SENSOR.FACE)
            {
                data = _fdAnomaly.faceData.Cast<DataReading>().ToList();
                windowSize = FACE_DURATION;
                stepSize = 100;
            }


            List<int> timeStamps = new List<int>();

            for (int i = 0; i < data.Last().timestamp - data.First().timestamp - windowSize; i += stepSize)
            {
                if (s == SENSOR.GSR)
                    GetGSRFeatures(data.Cast<GSRDataReading>().ToList(), i, windowSize);
                else if (s == SENSOR.HR)
                    GetHRFeatures(data.Cast<HRDataReading>().ToList(), i, windowSize);

                timeStamps.Add(i);
            }


            int start = (useRestInTraining.Checked) ? 180000 : 0;
            int trainingEnd = int.Parse(events[2].Split('#')[0]);


            featureVectors[s.ToString()] = featureVectors[s.ToString()].NormalizeFeatureList<double>(Normalize.OneMinusOne).ToList();
            var dataSet = featureVectors[s.ToString()].Zip(timeStamps, (first, second) => { return Tuple.Create(first, second); });

            var trainingSet = dataSet.SkipWhile(x => x.Item2 < start).TakeWhile(x => x.Item2 < trainingEnd);
            var predictionSet = dataSet.SkipWhile(x => x.Item2 < trainingEnd);

            int count = predictionSet.Count();
            int firstPredcition = predictionSet.First().Item2;

            predictions[s.ToString()].AddRange(SVMThisShit(trainingSet, predictionSet));
        }

        private List<int> SVMThisShit(IEnumerable<Tuple<List<double>, int>> trainingSet, IEnumerable<Tuple<List<double>, int>> predictionSet)
        {
            OneClassClassifier occ = new OneClassClassifier(trainingSet.Select(x => x.Item1).ToList());
            SVMParameter svmP = new SVMParameter();
            svmP.Kernel = SVMKernelType.RBF;
            svmP.C = 100;
            svmP.Gamma = 0.01;
            svmP.Nu = 0.01;
            svmP.Type = SVMType.ONE_CLASS;
            occ.CreateModel(svmP);
            List<int> indexes = occ.PredictOutliers(predictionSet.Select(x => x.Item1).ToList());
            return indexes;
        }


        private void GetHRFeatures(List<HRDataReading> data, int i, int windowSize)
        {
            List<double> featureVector = new List<double>();
            List<double> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < i).TakeWhile(x => i + windowSize > (x.timestamp - data.First().timestamp)).Select(x => (double)x.IBI).ToList();
            if (data.Count == 0)
                return;

            featureVector.Add(d.Average());
            featureVector.Add(d.Max());
            featureVector.Add(d.Min());
            double sd = Math.Sqrt(d.Average(x => Math.Pow(x - d.Average(), 2)));
            featureVector.Add(sd);
            featureVectors["HR"].Add(featureVector);

        }

        private void GetFACEFeatures(List<FaceDataReading> data, int i, int windowSize)
        {

        }

        private void GetGSRFeatures(List<GSRDataReading> data, int i, int windowSize)
        {
            List<double> featureVector = new List<double>();
            List<double> d = data.SkipWhile(x => (x.timestamp - data.First().timestamp) < i).TakeWhile(x => i + windowSize > (x.timestamp - data.First().timestamp)).Select(x => (double)x.resistance).ToList();
            if (data.Count == 0)
                return;
            featureVector.Add(d.Average());
            featureVector.Add(d.Max());
            featureVector.Add(d.Min());
            double sd = Math.Sqrt(d.Average(x => Math.Pow(x - d.Average(), 2)));
            featureVector.Add(sd);
            featureVectors["GSR"].Add(featureVector);



        }

        private void btn_getData_Click(object sender, EventArgs e)
        {
            //var data = Extensions.GetDataFromInterval(_fdAnomaly.gsrData.Cast<DataReading>().ToList(), 1000, 2000);

            SlideWindow(SENSOR.GSR);
            SlideWindow(SENSOR.HR);
            var k = predictions;
            var f = featureVectors;

        }
    }
}