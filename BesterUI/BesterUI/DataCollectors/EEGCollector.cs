using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emotiv;
using System.Threading.Tasks;

namespace BesterUI.DataCollectors
{
    class EEGCollector
    {

        private EmoEngine eegEngine;
        int userID = -1;

        public EEGCollector()
        {
            eegEngine = EmoEngine.Instance;
        }

        public void Connect()
        {
            eegEngine.UserAdded += EegEngine_UserAdded;
            eegEngine.Connect();
        }


        private void Collector()
        {
            // Handle any waiting events
            eegEngine.ProcessEvents();
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
