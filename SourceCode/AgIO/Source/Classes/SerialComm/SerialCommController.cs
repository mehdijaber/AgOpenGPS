
using AgIO.Properties;
using System;

namespace AgIO
{
    partial class SerialComm
    {
        public class Controller
        {
            private static Controller instance = new Controller();
            public static Controller Instance { get => instance; }

            public SerialComm GPS { get; private set; }

            private Controller()
            {
                // TODO: Move settings management entirely out of the controller?
                GPS = new SerialComm();
                GPS.BaudRate = Settings.Default.setPort_baudRateGPS;
                GPS.PortName = Settings.Default.setPort_portNameGPS;
            }

            public void Connect(SerialComm device)
            {
                if (device.IsOpen)
                    throw new InvalidOperationException("Serial device is already connected!");
                device.Connect(); // throws
            }

            public void SetBaudRate(SerialComm device, int baudRate)
            {
                if (device.IsOpen)
                    throw new InvalidOperationException("Cannot set baud rate on a connected device!");
                device.BaudRate = baudRate;
            }

            public void SetPortName(SerialComm device, string portName)
            {
                if (device.IsOpen)
                    throw new InvalidOperationException("Cannot set port name on a connected device!");
                device.PortName = portName;
            }

            public void CloseConnection(SerialComm device)
            {
                if (!device.IsOpen)
                    throw new InvalidOperationException("Cannot disconnect a serial device that is not connected!");
                device.Disconnect(); // throws
            }

            public void SendData(SerialComm device, string data)
            {
                if (!device.IsOpen)
                    throw new InvalidOperationException("Cannot send data to a serial device that is not connected!");
                device.SendData(data); // throws
            }
        }
    }
}
