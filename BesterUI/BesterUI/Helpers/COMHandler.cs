using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Management;

namespace BesterUI.Helpers
{
    static class COMHandler
    {
        static public List<String> Ports()
        {
            return new List<String>(SerialPort.GetPortNames());
        }

        static public void OpenPort(SerialPort port)
        {
            if (port.IsOpen == false)
            {
                port.Open();
            }
        }

        static public void ClosePort(SerialPort port)
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }

        static public SerialPort PortNamed(
            string portName,
            int baudrate,
            Parity parity,
            StopBits stopbits,
            int bytesize)
        {
            var port = new SerialPort(portName);
            port.BaudRate = baudrate;
            port.Parity = parity;
            port.StopBits = stopbits;
            port.DataBits = bytesize;
            return port;
        }

        static public byte[] ReadFromPort(SerialPort port, int size, int timeOut)
        {
            byte[] buf = new byte[size];
            port.ReadTimeout = timeOut > 0 ? timeOut : SerialPort.InfiniteTimeout;
            try
            {
                port.Read(buf, 0, size);
            }
            catch (TimeoutException e)
            {
                Array.Clear(buf, 0, size);
            }
            port.DiscardOutBuffer();
            return buf;
        }

        static public List<ManagementBaseObject> SerialPorts()
        {
            var os = new ManagementObjectSearcher("select * from Win32_SerialPort");
            List<ManagementBaseObject> objects = new List<ManagementBaseObject>();
            foreach (var o in os.Get())
            {
                objects.Add(o);
            }
            return objects;
        }

        static public bool IsArduino(string portname)
        {
            SerialPort arduino = PortNamed(portname, 115200, Parity.None, StopBits.One, 8);
            bool retVal = true;

            try
            {
                arduino.ReadTimeout = 500;
                OpenPort(arduino);

                if (arduino.BytesToRead <= 0)
                {
                    Thread.Sleep(500);
                }

                string msg = arduino.ReadLine();

                if (msg.Length < 2)
                {
                    retVal = false;
                }


                char id = msg[0];

                if (id == 'Y' || id == 'N')
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }

                arduino.Close();
                
            }
            catch (Exception e)
            {
                retVal = false;
            }
            finally
            {
                ClosePort(arduino);
            }

            return retVal;
        }
    }
}
