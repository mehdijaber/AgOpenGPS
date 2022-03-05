
using System;
using System.IO.Ports;
using System.Linq;

namespace AgIO
{
    // Declared as a partial class to define the controller as a
    // nested class to access private members
    partial class SerialComm
    {
        private SerialPort serialPort =
            new SerialPort("", 4800, Parity.None, 8, StopBits.One);

        public string PortName {
            get { return serialPort.PortName; }
            protected set {
                if (IsOpen)
                    throw new InvalidOperationException("Cannot change port name when connected to serialPort device!");
                else
                    serialPort.PortName = value;
            }
        }

        public int BaudRate {
            get { return serialPort.BaudRate; }
            protected set {
                if (IsOpen)
                    throw new InvalidOperationException("Cannot change baud rate when connected to serialPort device!");
                else
                    serialPort.BaudRate = value;
            }
        }

        public bool IsOpen {
            get { return serialPort.IsOpen; }
        }

        public SerialComm()
        {
            serialPort.DataReceived += DataReceivedHandler;
            serialPort.WriteTimeout = 500;
        }

        public event EventHandler Connected;
        protected virtual void OnConnected()
        {
            EventHandler raiseConnected = Connected;

            if (raiseConnected != null)
            {
                raiseConnected(this, EventArgs.Empty);
            }
        }

        public event EventHandler Disconnected;
        protected virtual void OnDisconnected()
        {
            EventHandler raiseDisconnected = Disconnected;

            if (raiseDisconnected != null)
            {
                raiseDisconnected(this, EventArgs.Empty);
            }
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        protected virtual void OnDataReceived(DataReceivedEventArgs data)
        {
            EventHandler<DataReceivedEventArgs> raiseDataReceived = DataReceived;

            if (raiseDataReceived != null)
            {
                raiseDataReceived(this, data);
            }
        }

        public class DataReceivedEventArgs : EventArgs
        {
            public DataReceivedEventArgs(string data)
            {
                Data = data;
            }

            public string Data { get; private set; }
        }

        public event EventHandler<DataSentEventArgs> DataSent;
        protected virtual void OnDataSent(DataSentEventArgs data)
        {
            EventHandler<DataSentEventArgs> raiseDataSent = DataSent;

            if (raiseDataSent != null)
            {
                raiseDataSent(this, data);
            }
        }

        public class DataSentEventArgs : EventArgs
        {
            public DataSentEventArgs(string data)
            {
                Data = data;
            }

            public string Data { get; private set; }
        }

        protected void Connect()
        {
            if (!IsOpen)
            {
                serialPort.Open();

                if (serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();

                    OnConnected();
                }
            }
        }

        protected void Disconnect()
        {
            if (IsOpen)
            {
                // throws
                serialPort.Close();
                serialPort.Dispose();
                OnDisconnected();
            }
        }

        protected void SendData(string data)
        {
            DataSentEventArgs eventData;

            serialPort.Write(data);
            eventData = new DataSentEventArgs(data);
            OnDataSent(eventData);
        }

        protected virtual void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string packet;
            DataReceivedEventArgs eventData;

            try
            {
                packet = serialPort.ReadExisting();
                eventData = new DataReceivedEventArgs(packet);

                OnDataReceived(eventData);
            }
            catch (InvalidOperationException error)
            {
                // TODO: Handle this exception better by ensuring
                // disconnection is done gracefully.
                Console.WriteLine(error);
            }
        }
    }
}
