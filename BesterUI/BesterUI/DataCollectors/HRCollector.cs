using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BesterUI.Data;
using BesterUI.Helpers;
using System.IO.Ports;

namespace BesterUI.DataCollectors
{
    class HRCollector
    {
        SerialPort arduino;
        volatile bool stopCollecting = false;
        Thread collectionThread;
        FusionData fd;

        public HRCollector(FusionData fd)
        {
            this.fd = fd;

            foreach (var item in COMHandler.Ports())
            {
                if (COMHandler.IsArduino(item))
                {
                    arduino = COMHandler.PortNamed(item, 115200, Parity.None, StopBits.One, 8);
                    break;
                }
            }
        }

        public void StartCollecting()
        {
            COMHandler.OpenPort(arduino);
            collectionThread = new Thread(new ThreadStart(CollectorTask));
            collectionThread.Start();
        }

        public void StopCollecting()
        {
            stopCollecting = true;
            collectionThread.Join(5000);
            COMHandler.ClosePort(arduino);
        }

        void CollectorTask()
        {
            while (!stopCollecting)
            {
                if (arduino.BytesToRead > 0)
                {
                    fd.bandData.Add(ReadData());
                }
            }
        }

        HRDataReading ReadData()
        {
            string msg = arduino.ReadLine();
            char id = msg[0];

            if (id == 'Y') //beat found!
            {
                return new HRDataReading() { isBeat = true, signal = int.Parse(msg.Substring(1)) };
            }
            else if (id == 'N') //no beat this time. :(
            {
                return new HRDataReading() { isBeat = false, signal = int.Parse(msg.Substring(1)) };
            }

            Log.LogMessage("ERROR: Faulty HeartRate reading!!");
            return new HRDataReading() { signal = int.MaxValue };
        }
    }
}
