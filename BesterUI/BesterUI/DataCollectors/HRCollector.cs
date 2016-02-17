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

        public string MyPort()
        {
            return (arduino == null) ? "" : arduino.PortName;
        }

        public HRCollector(FusionData fd)
        {
            this.fd = fd;
        }

        public bool Connect()
        {
            bool connected = false;
            foreach (var item in COMHandler.Ports())
            {
                if (COMHandler.IsArduino(item))
                {
                    arduino = COMHandler.PortNamed(item, 115200, Parity.None, StopBits.One, 8);
                    connected = true;
                    break;
                }
            }

            return connected;
        }

        public void StartCollecting()
        {
            Log.LogMessage("Starting HR");
            stopCollecting = false;
            collectionThread = new Thread(new ThreadStart(CollectorTask));
            collectionThread.Start();
        }

        public void StopCollecting()
        {
            Log.LogMessage("Stopping HR...");
            stopCollecting = true;
        }

        void CollectorTask()
        {
            COMHandler.OpenPort(arduino);
            while (!stopCollecting)
            {
                if (arduino.BytesToRead > 0)
                {
                    fd.AddHRData(ReadData());
                }
            }
            COMHandler.ClosePort(arduino);
            DataReading.StaticEndWrite("HR");
            Log.LogMessage("Stopped HR");
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
