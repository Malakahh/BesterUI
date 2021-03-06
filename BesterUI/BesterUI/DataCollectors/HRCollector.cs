﻿using System;
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
                Log.LogMessage("Trying to bind HR to port: " + item);
                if (COMHandler.IsArduino(item))
                {
                    arduino = COMHandler.PortNamed(item, 115200, Parity.None, StopBits.One, 8);
                    connected = true;
                    Log.LogMessageSameLine("Trying to bind HR to port: " + item + " - SUCCES");
                    break;
                }
                else
                {
                    Log.LogMessageSameLine("Trying to bind HR to port: " + item + " - FAILED");
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
            Log.LogMessage("Stopped HR");
        }

        HRDataReading ReadData()
        {
            try
            {
                string msg = arduino.ReadLine();
                char id1 = msg[0];
                char id2 = msg[1];

                if (id1 == 'B' && id2 == ':') //valid reading
                {
                    var data = msg.Split('|');
                    bool disBeat = data[0].Split(':')[1] == "Y";
                    int dBPM = int.Parse(data[1].Split(':')[1]);
                    int dIBI = int.Parse(data[2].Split(':')[1]);
                    int dSignal = int.Parse(data[3].Split(':')[1]);

                    return new HRDataReading(true) { isBeat = disBeat, signal = dSignal, IBI = dIBI, BPM = dBPM };
                }

                Log.LogMessage("ERROR: Faulty HeartRate reading! ");
                return new HRDataReading(true) { signal = int.MaxValue, BPM = int.MaxValue, IBI = int.MaxValue };
            }
            catch (Exception e)
            {
                Log.LogMessage("ERROR: Faulty HeartRate reading! ");
                Log.LogMessage("Error Code: " + e.Message);
                return new HRDataReading(true) { signal = int.MaxValue, BPM = int.MaxValue, IBI = int.MaxValue };
            }

        }
    }
}
