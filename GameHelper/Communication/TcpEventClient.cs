using GameHelper.Collections;
using GameHelper.Multiplayer.Packets;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace GameHelper.Communication
{
    class TcpEventClient
    {
        #region Properties
        SocketComm socket;
        public event Handlers.PacketReceivedEH PacketReceived;
        public event Handlers.voidEH Disconnected;
        #endregion

        #region Connection Methods
        public bool Connect(IPEndPoint remoteEndPoint)
        {
            TcpClient client = new TcpClient();
            bool connected = false;
            try
            {
                client.Connect(remoteEndPoint);
                socket = new SocketComm(client.Client, true, "Only Client"); // true because this is the client side
                socket.PacketReceived += new SocketComm.PacketReceivedEventHandler(socket_PacketReceived);
                socket.ClientDisconnected += new Handlers.voidEH(socket_ClientDisconnected);
                connected = true;
            }
            catch (Exception E)
            {
                connected = false;
                Debug.WriteLine(E.StackTrace);
            }
            return connected;
        }

        public void Stop()
        {
            if (socket == null)
                return;

            socket.Disconnect();
        } 
        #endregion

        #region Socket Callbacks
        void socket_PacketReceived(byte[] data)
        {
            if (PacketReceived == null)
                return;

            Packet p = Packet.Read(data);
            if (p == null)
                return;

            PacketReceived(p);
        }

        void socket_ClientDisconnected()
        {
            if (Disconnected == null)
                return;
            Disconnected();
        } 
        #endregion

        public void Send(Packet packet)
        {
            Counter.AddTick("pps_out");

            if (socket == null)
                return;

            socket.Send(packet.Serialize());
        }
    }
}
