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
namespace BesterUI
{
    public partial class CollectForm : Form
    {
        FusionData fusionData = new FusionData();


        //Collectors
        EEGCollector eegCollect;
        GSRCollector gsrCollect;
        HRCollector hrCollect;

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


            /*
                Heart rate initiation
            */
            hrCollect = new HRCollector(fusionData);
            if (hrCollect.Connect())
            {
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
                if(port != hrCollect.MyPort())
                {
                    Log.LogMessage("Trying to bind GSR to port: " + port);
                    gsrCollect = new GSRCollector(port, fusionData);
                    if (gsrCollect.TestPort()) { 
                        connected = true;
                        gsrPort.Text = "(" + port + ")";
                        Log.LogMessageSameLine("Trying to bind GSR to port: " + port + " - SUCCES");
                    } else
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
            }
                
        }
    }
}
