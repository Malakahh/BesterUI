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
using BesterUI.DataCollectors;
using System.Threading;
using BesterUI.Data;

namespace BesterUI
{
    public partial class CollectForm : Form
    {
        FusionData fusionData = new FusionData();

        //Collectors
        EEGCollector eegCollect;
        GSRCollector gsrCollect;
        HRCollector hrCollect;

        bool collectingData = false;

        bool[] requiredDevices = { true, true, true }; // Required devices for collecting data, EEG, GSR, HR

        bool EEGDeviceReady = false;
        bool GSRDeviceReady = false;
        bool HRDeviceReady = false;

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
        }

        private void EEGReady()
        {
            EEGDeviceReady = true;
            eegReady.BackColor = Color.Green;
        }

        private void SensorsPending()
        {
            eegReady.BackColor = Color.Yellow;
            gsrReady.BackColor = Color.Yellow;
            hrReady.BackColor = Color.Yellow;
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
            if (!collectingData)
            {
                DataReading.ResetTimers();
                /*
                    Required devices to start collecting data.
                */
                if (requiredDevices[0] == EEGDeviceReady &&
                    requiredDevices[1] == GSRDeviceReady &&
                    requiredDevices[2] == HRDeviceReady)
                {
                    eegCollect.StartCollect();
                    gsrCollect.StartCollecting();
                    hrCollect.StartCollecting();
                    button2.Text = "STOP COLLECTING";
                    collectingData = true;
                    collectingDataPanel.BackColor = Color.Green;
                }
                else
                {
                    Log.LogMessage("ERROR: Device requirements not filled - Requirements are: ");
                    Log.LogMessage("EEG: " + requiredDevices[0].ToString());
                    Log.LogMessage("GSR: " + requiredDevices[1].ToString());
                    Log.LogMessage("HR: " + requiredDevices[2].ToString());
                }
            }
            else
            {
                eegCollect.StopCollect();
                gsrCollect.StopCollecting();
                hrCollect.StopCollecting();
                button2.Text = "START COLLECTING";
                collectingData = true;
                collectingDataPanel.BackColor = Color.Red;
            }

        }
    }
}
