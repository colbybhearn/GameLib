using GameHelper.Multiplayer.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameHelper.Communication
{
    public class TcpEventServer
    {
        /* Vision
         * One instance of the TcpEventServer handles the lobby, and thus the ClientConnected will hand off to a function to connect them to the correct "server"
         * A second instance of the TcpEventServer will also accept connections, but this time it will handle them appropriately via SocketComm and all its goodness
         * 
         * TcpEventServer will keep a list of "connected clients" (was ClientInfo.cs)
         */

        #region Properties
        bool ShouldBeRunning = false;
        IPAddress listenIpAddress;
        int listenPort;

        int nextClientId = 0;

        TcpListener Listener;
        Thread ListenerThread;
        SortedList<int, ClientInfoSocket> Clients = new SortedList<int, ClientInfoSocket>();

        public event Handlers.IntPacketEH PacketReceived;
        public event Handlers.IntEH ClientAccepted;
        public event Handlers.IntEH ClientDisconnected;

        #endregion

        #region Initialization

        public TcpEventServer(string ip, int port)
        {
            listenIpAddress = IPAddress.Parse(ip);
            listenPort = port;
            ListenerThread = new Thread(new ThreadStart(ServerWorker));
        }

        public void Start()
        {
            ShouldBeRunning = true;
            ListenerThread.Start();
            Debug.WriteLine("Lobby Listener: ServerListener.Start() finished: " + listenPort.ToString());
        }

        public void Stop()
        {
            this.ShouldBeRunning = false;
            foreach (ClientInfoSocket s in Clients.Values)
                s.Disconnect();
        }

        #endregion

        #region Socket Listeners
        private void ServerWorker()
        {
            try
            {
                // use local m/c IP address, and 
                Debug.WriteLine("Lobby Listener: creating Listener");

                // Initializes the Listener 
                Listener = new TcpListener(IPAddress.Any, listenPort);
                Debug.WriteLine("Lobby Listener: starting Listener");
                // Start Listeneting at the specified port         
                Listener.Start();
                Debug.WriteLine("Lobby Listener: Waiting to Accept on: " + listenPort.ToString());

                // poll for pending connections
                while (ShouldBeRunning)
                {
                    if (Listener.Pending())
                    {
                        AcceptNewClient();
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                string s = "LobbyListener Error: " + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(s);
            }
            Listener.Stop();
        }

        public void AcceptNewClient()
        {
            Socket socket = Listener.AcceptSocket();

            nextClientId++;

            ClientInfoSocket ciSocket = new ClientInfoSocket(socket, nextClientId);
            ciSocket.ClientDisconnected += new Handlers.IntEH(socket_ClientDisconnected);
            ciSocket.PacketReceived += new ClientInfoSocket.PacketReceivedEventHandler(socket_PacketReceived);
            Clients.Add(nextClientId, ciSocket);

            if (ClientAccepted != null)
                ClientAccepted(nextClientId);
        }
        #endregion

        #region Socket Callbacks
        public void socket_PacketReceived(int id, byte[] data)
        {
            if (PacketReceived == null)
                return;

            Packet p = Packet.Read(data);
            if (p == null)
                return;

            PacketReceived(id, p);
        }

        public void socket_ClientDisconnected(int id)
        {
            if (ClientDisconnected == null)
                return;

            ClientDisconnected(id);
        } 
        #endregion

        #region Send Packets
        public void Send(Packet p)
        {
            // To everyone
            byte[] data = p.Serialize();
            foreach (SocketComm s in Clients.Values)
            {
                if (s == null)
                    return;

                s.Send(data);
            }
        }

        public void Send(Packet p, int id)
        {
            // To Specific id
            ClientInfoSocket s;
            if (Clients.TryGetValue(id, out s))
            {
                if (s == null)
                    return;

                s.Send(p.Serialize());
            }
        }

        public void SendToAllButOne(Packet p, int id)
        {
            // Avoid the Specified id
            foreach (ClientInfoSocket s in Clients.Values)
            {
                if (s == null)
                    continue;
                if (s.ClientID == id)
                    continue;
                s.Send(p.Serialize());
            }
        } 
        #endregion
    }
}
