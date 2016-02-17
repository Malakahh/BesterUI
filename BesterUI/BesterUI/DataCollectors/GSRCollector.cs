using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using BesterUI.Helpers;
using BesterUI.Data;

namespace BesterUI.DataCollectors
{
    class GSRCollector
    {
        const int TIMEOUT_MS = 100;
        const float MAGIC_NUMBER = 7700010000;
        const int SHUNT_RESISTANCE = 470000;
        SerialPort comPort;
        volatile bool stopCollecting = false;
        Thread collectionThread;
        FusionData fd;

        public GSRCollector(string PortName, FusionData fd)
        {
            comPort = COMHandler.PortNamed(PortName, 19200, Parity.None, StopBits.One, 8);
            this.fd = fd;
        }

        public void StartCollecting()
        {
            Log.LogMessage("Starting GSR");
            stopCollecting = false;
            collectionThread = new Thread(new ThreadStart(CollectorTask));
            collectionThread.Start();
        }

        public void StopCollecting()
        {
            Log.LogMessage("Stopping GSR...");
            stopCollecting = true;
        }

        void CollectorTask()
        {
            COMHandler.OpenPort(comPort);
            while (!stopCollecting)
            {
                if (comPort.BytesToRead > 0)
                {
                    fd.AddGSRData(ReadData());
                }
            }
            COMHandler.ClosePort(comPort);
            DataReading.StaticEndWrite("GSR");
            Log.LogMessage("Stopped GSR");
        }

        public GSRDataReading ReadData()
        {
            byte[] input = new byte[8];
            input[0] = COMHandler.ReadFromPort(comPort, 1, TIMEOUT_MS)[0];
            COMHandler.ReadFromPort(comPort, 7, TIMEOUT_MS).CopyTo(input, 1);

            bool headerError = input[0] != 0xA3 || input[1] != 0x5b || input[2] != 8;
            int adc = (input[3] << 8) + input[4];
            int resist = (int)(MAGIC_NUMBER / adc) - SHUNT_RESISTANCE;
            byte status = input[5];
            bool probe_error = (status & 1) == 1;
            bool battery = (status & 2) == 1;
            bool new_data = (status & 4) == 1;
            bool recalc = (status & 8) == 1;
            int chksum = (input[6] << 8) + input[7];
            bool checksumError = input[0] + input[1] + input[2] + input[3] + input[4] + input[5] != chksum;

            return new GSRDataReading() { resistance = resist };
        }
    }
}
