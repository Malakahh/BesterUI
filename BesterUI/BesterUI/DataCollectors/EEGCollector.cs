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

        public EEGCollector()
        {
            eegEngine = EmoEngine.Instance;
        }

        public void Connect()
        {
            eegEngine.UserAdded += EegEngine_UserAdded;
            eegEngine.Connect();
        }

        #region [Events]
        private void EegEngine_UserAdded(object sender, EmoEngineEventArgs e)
        {

        }
        #endregion

    }
}
