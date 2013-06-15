using GameHelper.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameHelper.Communication
{
    public class SocketComm
    {
        #region Properties
        Socket socket;
        Thread inputThread;
        Thread outputThread;

        public delegate void PacketReceivedEventHandler(byte[] data);
        public event PacketReceivedEventHandler PacketReceived;
        public event GameHelper.Handlers.voidEH ClientDisconnected;

        bool ShouldBeRunning = false;
        ThreadQueue<byte[]> DataToSendQueue;
        #endregion

        public SocketComm(Socket s, bool clientSide, string socketAlias)
        {
            ShouldBeRunning = true;
            DataToSendQueue = new ThreadQueue<byte[]>();
            socket = s;
            inputThread = new Thread(new ThreadStart(inputWorker));
            outputThread = new Thread(new ThreadStart(outputWorker));
            if (clientSide)
            {
                inputThread.Name = "Client Socket Input Worker (" + socketAlias + ")";
                outputThread.Name = "Client Socket Input Worker (" + socketAlias + ")";
            }
            else
            {
                inputThread.Name = "Server Socket Input Worker (" + socketAlias + ")";
                outputThread.Name = "Server Socket Input Worker (" + socketAlias + ")";
            }
            
            inputThread.Start();
            outputThread.Start();
        }

        public void Disconnect()
        {
            ShouldBeRunning = false;
        }

        public void Send(byte[] data)
        {
            DataToSendQueue.Enqueue(data);
        }

        /* TODO
         * Test new code
         */
        private void inputWorker()
        {
            byte[] lenBytes = new byte[4];
            int dataLength = -1;
            socket.ReceiveBufferSize = 1000000;
            socket.Blocking = true;
            while (ShouldBeRunning && socket.Connected)
            {
                socket.Receive(lenBytes);
                dataLength = BitConverter.ToInt32(lenBytes, 0);

                if (dataLength > 5000)
                    throw new FormatException("Packet Length " + dataLength + " is unreasonably long.");

                if (dataLength > 0)
                {
                    byte[] data = new byte[dataLength];
                    socket.Receive(data);

                    CallPacketReceived(data);
                }
            }

            CallClientDisconnected();
            socket.Disconnect(false);
        }

        /* TODO
         * Unused code
         * Move to a library?
         */
        private void BytesToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2")); // Hex
        }

        private void outputWorker()
        {
            List<byte> dataToSend = new List<byte>();

            while (ShouldBeRunning && socket.Connected)
            {
                Thread.Sleep(1);

                dataToSend.Clear();

                while (DataToSendQueue.Count > 0)
                {
                    dataToSend.AddRange(DataToSendQueue.Dequeue());
                }                

                if (dataToSend.Count == 0)
                    continue;
                
                try
                {
                    //Send ALL bytes at once instead of "per packet", should be better
                    int r = socket.Send(dataToSend.ToArray());
                }
                catch (Exception E)
                {
                    Debug.WriteLine(E.StackTrace);
                }
            }
        }

        public virtual void CallPacketReceived(byte[] data)
        {
            if (PacketReceived == null)
                return;
            PacketReceived(data);
        }

        protected virtual void CallClientDisconnected()
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected();
        }
    }
}
