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
        public bool UseEmotivTimestamp = true;

        public EEGCollector(FusionData fusionData)
        {
            eegEngine = EmoEngine.Instance;
            fd = fusionData;
        }

        public void FindUsers()
        {

            Thread checker = new Thread(() =>
            {
                while (userID == -1)
                {
                    eegEngine.ProcessEvents();
                    Thread.Sleep(100);
                }
            }
            );
            checker.Start();
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

            int numReadings = 0;
            //Clearing of buffer
            Dictionary<EdkDll.EE_DataChannel_t, double[]> input = eegEngine.GetData((uint)userID);

            double startTime = input[EdkDll.EE_DataChannel_t.TIMESTAMP].Max();

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
                int len = input[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
                for (int i = 0; i < len; i++)
                {
                    EEGDataReading dataReading = new EEGDataReading(true);

                    if (UseEmotivTimestamp)
                    {
                        dataReading.timestamp = (long)((input[EdkDll.EE_DataChannel_t.TIMESTAMP][i] - startTime) * 1000);
                    }
                    else
                    {
                        dataReading.timestamp = (long)(1000f / 128 * numReadings);
                        numReadings++;
                    }


                    for (int j = (int)EdkDll.EE_DataChannel_t.AF3; j <= (int)EdkDll.EE_DataChannel_t.GYROY; j++)
                    {
                        dataReading.data.Add(((EdkDll.EE_DataChannel_t)j).ToString(), input[(EdkDll.EE_DataChannel_t)j][i]);
                    }

                    fd.AddEEGData(dataReading);
                }

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
            if (DeviceReady != null)
                DeviceReady();
        }
        #endregion



    }
}
