using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using Emotiv;
using System.Threading.Tasks;
using BesterUI.Data;

namespace BesterUI.DataCollectors
{
    class EEGCollector
    {
        private EmoEngine eegEngine;
        private Thread dataCollectThread;
        int userID = -1;
        private FusionData fd;

        
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
            Dictionary<EdkDll.EE_DataChannel_t, double[]> input;
            while (shouldCollectData)
            {
                // Handle any waiting events
                eegEngine.ProcessEvents();

                if (userID == -1)
                {
                    return;
                }

                input = eegEngine.GetData((uint)userID);

                if (input == null)
                {
                    return;
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

                input = null;

                Thread.Sleep(100);
            }


        }

        public void StartCollect()
        {

            dataCollectThread = new Thread(Collect);
            dataCollectThread.Start();
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
        }
        #endregion

    }
}
