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
        //Thread inputProcThread;

        public delegate void PacketReceivedEventHandler(byte[] data);
        public event PacketReceivedEventHandler PacketReceived;
        public event GameHelper.Handlers.voidEH ClientDisconnected;

        bool ShouldBeRunning = false;
        //ThreadQueue<byte[]> DataReceived = new ThreadQueue<byte[]>();
        ThreadQueue<byte[]> DataToSendQueue;
        #endregion

        public SocketComm(Socket s, bool clientSide, string socketAlias)
        {
            ShouldBeRunning = true;
            DataToSendQueue = new ThreadQueue<byte[]>();
            socket = s;
            inputThread = new Thread(new ThreadStart(inputWorker));
            outputThread = new Thread(new ThreadStart(outputWorker));
            //inputProcThread = new Thread(new ThreadStart(inputProcWorker));
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
            //inputProcThread.Start();
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
            //int count = 0;
            socket.ReceiveBufferSize = 1000000;
            socket.Blocking = true; // JAP 6-14-2013 Part of new code
            while (ShouldBeRunning && socket.Connected)
            {
                socket.Receive(lenBytes);
                dataLength = BitConverter.ToInt32(lenBytes, 0);
                if (dataLength > 0)
                {
                    byte[] data = new byte[dataLength];
                    socket.Receive(data);

                    CallPacketReceived(data);
                }

                /* JAP 6-14-2013
                 * Commented this code until above new code can be tested.
                if (length == -1 && socket.Available >= 4)
                {
                    //count = socket.Receive(lenBytes);
                    socket.Receive(lenBytes);
                    length = BitConverter.ToInt32(lenBytes, 0);

                    if (length > 5000)
                        throw new FormatException("packet length " + length + " is unreasonably long.");
                }

                if (length > 0 && socket.Available >= length)
                {
                    byte[] data = new byte[length];
                    socket.Receive(data);

                    //int datacount = socket.Receive(data);

                    length = -1;

                    //if (data != null)
                    CallPacketReceived(data);
                }
                else
                    Thread.Sleep(1);
                */
            }

            CallClientDisconnected();
            socket.Disconnect(false);
        }
        

        /* TODO
         * JAP 6-14-2013 - Check with Colby and delete code
        
        private void fasterTcpInputWorker()
        {
            int length = 1024;
            int count = 0;
            socket.ReceiveBufferSize = 1000000;
            byte[] data = new byte[length];
            socket.Blocking = true;
            while (ShouldBeRunning)
            {
                try
                {
                    if (!socket.Connected)
                        break;
                    count = socket.Receive(data);
                    if (data != null)
                    {
                        byte[] dr = new byte[count];
                        Array.Copy(data, 0, dr, 0, count);
                        CallDataReceived(dr);
                    }
                }
                catch (Exception E)
                {
                    Trace.WriteLine(E.Message);
                }
            }

            CallClientDisconnected();
            socket.Disconnect(false);
        }


        private void CallDataReceived(byte[] b)
        {
            DataReceived.Enqueue(b);
        }
        
        private void inputProcWorker()
        {
            byte[] aPacket;
            byte[] lenBytes = new byte[4];
            byte[] data = new byte[0];
            int packetLength = -1;
            byte[] oldData = new byte[0];
            while (ShouldBeRunning)
            {
                while (DataReceived.myCount > 0)
                {
                    try
                    {
                        byte[] inData = DataReceived.Dequeue() as byte[];
                        int requiredLength = oldData.Length + inData.Length;
                        //if (data.Length < requiredLength)
                            data = new byte[requiredLength]; // make data large enough to contain old and new
                        Array.Copy(oldData, 0, data, 0, oldData.Length); // copy the existing data into the resized array, at the front
                        Array.Copy(inData, 0, data, oldData.Length, inData.Length); // copy the newly dequeued data into the resized array, at the end.

                        if (packetLength == -1 && // if the length of the next packet is unknown, and
                            data.Length >= 4) // we have that much data on hand,
                        {
                            Array.Copy(data, lenBytes, 4); // copy
                            packetLength = BitConverter.ToInt32(lenBytes, 0);
                            if (packetLength <= 0 || packetLength > 5000)
                                throw new FormatException("fast packet length " + packetLength + " is unreasonably long.");
                        }

                        if (packetLength != -1 &&  // we know how much data goes in the next packet, and
                            data.Length >= packetLength + 4) // we have that much data on hand,
                        {
                            //Trace.WriteLine("fast Processing " + packetLength + " / " + data.Length);
                            aPacket = new byte[packetLength]; // make a packet buffer
                            Array.Copy(data, 4, aPacket, 0, packetLength); // copy the data for this packet into the packet buffer
                            CallPacketReceived(aPacket); // send this packet off for processing
                            int unCopiedBytes = data.Length - (packetLength + 4); // calculate bytes that were not in that packet (length of packet + 4 byte length header)
                            oldData = new byte[unCopiedBytes];
                            Array.Copy(data, packetLength + 4, oldData, 0, unCopiedBytes); // shift the uncopied bytes up to the front (for queue-like behavior)
                            packetLength = -1; // signal the unknown length of the next packet
                        }
                    }
                    catch(Exception E)
                    {
                        Trace.WriteLine(E.Message);
                    }
                }
                //Trace.WriteLine("sleeping");
                Thread.Sleep(10);
            }
        }*/

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

        protected virtual void CallClientDisconnected()
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected();
        }

        public virtual void CallPacketReceived(byte[] data)
        {
            //BytesToString(data);
            if (PacketReceived == null)
                return;
            PacketReceived(data);
        }
    }
}
