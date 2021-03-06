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
using BesterUI.DataCollectors;
using System.Threading;
using BesterUI.Data;
using System.IO;
using SecondTest;

namespace BesterUI
{
    public partial class CollectForm : Form
    {
        FusionData fusionData = new FusionData();

        //Collectors
        EEGCollector eegCollect;
        GSRCollector gsrCollect;
        HRCollector hrCollect;
        FaceCollector faceCollect;

        bool collectingData = false;

        bool EEGDeviceReady = false;
        bool GSRDeviceReady = false;
        bool HRDeviceReady = false;
        bool FACEDeviceReady = false;

        public CollectForm()
        {
            InitializeComponent();
            Log.LogBox = logTextBox;
        }

        private void btnVerifySensors_Click(object sender, EventArgs e)
        {
            SensorsPending();

            Application.DoEvents();



            /*
                EEG Initiation
            */
            eegCollect = new EEGCollector(fusionData);
            eegCollect.Connect();
            eegCollect.DeviceReady += EEGReady; // Event that fires if a user is added to the EEG i.e. the headset is ready for use
            eegCollect.FindUsers();

            /*
                Heart rate initiation
            */
            hrCollect = new HRCollector(fusionData);
            if (hrCollect.Connect())
            {
                HRDeviceReady = true;
                hrReady.BackColor = Color.Green;
                hrPort.Text = "(" + hrCollect.MyPort() + ")";
            }
            else
            {
                hrReady.BackColor = Color.Red;
            }

            /*
                GSR Initiation
            */
            InitiateGSR();


            //kinect time
            InitiateFACE();
        }

        private void EEGReady()
        {
            EEGDeviceReady = true;
            eegReady.BackColor = Color.Green;
        }

        private void FACEReady()
        {

            FACEDeviceReady = true;
            faceReady.BackColor = Color.Green;
        }

        private void SensorsPending()
        {
            eegReady.BackColor = Color.Yellow;
            gsrReady.BackColor = Color.Yellow;
            hrReady.BackColor = Color.Yellow;
            faceReady.BackColor = Color.Yellow;
        }

        private void InitiateFACE()
        {
            Log.LogMessage("Kinect starting");
            faceCollect = new FaceCollector(fusionData);
            faceCollect.OnAskIfCaptured += UpdateFACEDisplayState;
            faceCollect.PrepareSensor();
        }

        private void UpdateFACEDisplayState(bool front, bool right, bool left, bool tilt, bool complete)
        {
            rdyLookForward.BackColor = (front) ? Color.Green : Color.Yellow;
            rdyLookRight.BackColor = (right) ? Color.Green : Color.Yellow;
            rdyLookLeft.BackColor = (left) ? Color.Green : Color.Yellow;
            rdyLookUp.BackColor = (tilt) ? Color.Green : Color.Yellow;

            if (complete)
                FACEReady();
        }

        private void InitiateGSR()
        {
            bool connected = false;
            foreach (string port in COMHandler.Ports())
            {
                if (port != hrCollect.MyPort())
                {
                    Log.LogMessage("Trying to bind GSR to port: " + port);
                    gsrCollect = new GSRCollector(port, fusionData);
                    if (gsrCollect.TestPort())
                    {
                        connected = true;
                        gsrPort.Text = "(" + port + ")";
                        Log.LogMessageSameLine("Trying to bind GSR to port: " + port + " - SUCCES");
                    }
                    else
                    {
                        Log.LogMessageSameLine("Trying to bind GSR to port: " + port + " - FAILED");
                    }
                }
            }

            if (!connected)
            {
                Log.LogMessage("GSR not found");
                gsrReady.BackColor = Color.Red;
            }
            else
            {
                gsrReady.BackColor = Color.Green;
                GSRDeviceReady = true;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
            if (runSecondTestChkBox.Checked)
            {
                EEGDataReading r = new EEGDataReading(true);
                RestingForm rf = new RestingForm();
                rf.Show();
                return;
            }
            */

            if (!collectingData)
            {
                fusionData.Reset();
                DataReading.ResetTimers();

                //StartTestFromCollector();
                eegCollect.StartCollect();
                gsrCollect.StartCollecting();
                hrCollect.StartCollecting();
                faceCollect.CollectData = true;
                button2.Text = "STOP COLLECTING";
                collectingData = true;
                collectingDataPanel.BackColor = Color.Green;

                if (runSecondTestChkBox.Checked)
                {
                    RestingForm rf = new RestingForm();
                    rf.Show();
                }
            }
            else
            {
                eegCollect.StopCollect();
                gsrCollect.StopCollecting();
                hrCollect.StopCollecting();
                faceCollect.CollectData = false;
                button2.Text = "START COLLECTING";
                collectingData = true;
                collectingDataPanel.BackColor = Color.Red;

                if (runSecondTestChkBox.Checked)
                {
                    fusionData.ExportGRF();
                }

                //DeleteStartFile();
            }
        }



        private void exportBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                //string txt = File.ReadLines(path + @"\GSR.dat").First().Split('|')[1];
                //DateTime dt = DateTime.ParseExact(txt, "yyyy-MM-dd HH_mm_ss_fff", System.Globalization.CultureInfo.InvariantCulture);
                fusionData.LoadFromFile(new string[] { path + @"\EEG.dat", path + @"\GSR.dat", path + @"\HR.dat", path + @"\KINECT.dat" }, DateTime.Now, false);

                fusionData.ExportGRF(path);
            }


        }

        private void StartTestFromCollector()
        {
            var f = File.Create(@"D:\wamp\www\data-visualization-dat9\public\test\colrdy.txt");
            f.Close();
        }

        private void DeleteStartFile()
        {
            File.Delete(@"D:\wamp\www\data-visualization-dat9\public\test\colrdy.txt");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HRDataReading hrD = new HRDataReading(true);
            hrD.isBeat = true;
            hrD.Write();


        }
    }
}
