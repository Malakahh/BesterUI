using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using Emotiv;
using System.Threading.Tasks;
using BesterUI.Data;
using BesterUI.Helpers;

namespace BesterUI.DataCollectors
{
    class EEGCollector
    {
        private EmoEngine eegEngine;
        private Thread dataCollectThread;
        int userID = -1;
        private FusionData fd;

        public Action DeviceReady;

        
        public EEGCollector(FusionData fusionData)
        {
            eegEngine = EmoEngine.Instance;
            fd = fusionData;
        }

        public void Connect()
        {
            eegEngine.UserAdded += EegEngine_UserAdded;
            eegEngine.Connect();
        }

        volatile bool shouldCollectData = true;
        private void Collect()
        {
            int iterations = 0;
            int dataNotReceived = 0;
            Log.LogMessage("EEG data collection started");
            Dictionary<EdkDll.EE_DataChannel_t, double[]> input;
            while (shouldCollectData)
            {
                // Handle any waiting events
                eegEngine.ProcessEvents();
                
                if (userID == -1)
                {
                    iterations++;
                    Log.LogMessageSameLine("No users found! number of times tried: " + iterations);
                    Thread.Sleep(100);
                    continue;
                }
                
                input = eegEngine.GetData((uint)userID);
                
                if (input == null)
                {
                    Log.LogMessageSameLine("No data receied number of times: " + ++dataNotReceived);
                    Thread.Sleep(100);
                    continue;
                }
                
                double max = input[EdkDll.EE_DataChannel_t.TIMESTAMP].Max();
                for (int j = (int)EdkDll.EE_DataChannel_t.AF3; j <= (int)EdkDll.EE_DataChannel_t.AF4; j++)
                {
                    for (int i = 0; i < input[(EdkDll.EE_DataChannel_t)j].Length; i++)
                    {
                        //TODO: Find out which way the data needs to be added (might be the otherway around)
                        EEGDataReading dataReading = new EEGDataReading();
                        double offset = max - input[EdkDll.EE_DataChannel_t.TIMESTAMP][i];
                        dataReading.data.Add(((EdkDll.EE_DataChannel_t)j).ToString(), input[(EdkDll.EE_DataChannel_t)j][i]);
                        dataReading.timestamp -= (long)offset;
                        fd.AddEEGData(dataReading);

                    }
                }

                Log.LogMessage(input[0].Length);
                input = null;
                
                Thread.Sleep(100);
            }


        }

        public void StartCollect()
        {
            dataCollectThread = null;
            dataCollectThread = new Thread(Collect);
            dataCollectThread.Start();
        }

        public void StopCollect()
        {
            GSRDataReading.StaticEndWrite("EEG");
            Log.LogMessage("EEG data Collection stopped");
            shouldCollectData = false;
        }

        #region [Events]
        private void EegEngine_UserAdded(object sender, EmoEngineEventArgs e)
        {
            // record the user 
            userID = (int)e.userId;

            // enable data aquisition for this user.
            eegEngine.DataAcquisitionEnable((uint)userID, true);

            // ask for up to 1 second of buffered data
            eegEngine.EE_DataSetBufferSizeInSec(1);

            //Fire event that the eeg is ready
            if (DeviceReady == null)
                DeviceReady();
        }
        #endregion

        

    }
}
