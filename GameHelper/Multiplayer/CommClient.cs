using GameHelper.Collections;
using GameHelper.Communication;
using GameHelper.Multiplayer.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace GameHelper.Multiplayer
{
    public class CommClient
    {
        /* Notes
         * This is added after-the-fact 6-14-2013 JAP
         * The CommClient should receive information about objects from the server.
         * The CommClient should communicate player actions to the server.
         */

        #region Properties
        public int iPort;
        public string sAlias;
        IPAddress a;

        TcpEventClient client;
        ServerInfo Server;

        ThreadQueue<Packet> InputQueue = new ThreadQueue<Packet>();
        Thread inputThread;

        bool ShouldBeRunning = false;

        public event Handlers.voidEH Disconnected;
        public event Handlers.IntEH PlayerDisconnected;
        public event Handlers.IntEH ObjectDeleteReceived;
        public event Handlers.ObjectAttributeEH ObjectAttributeReceived;
        public event Handlers.ClientConnectedEH OtherClientConnectedToServer;
        public event Handlers.ObjectActionEH ObjectActionReceived;
        public event Handlers.ObjectUpdateEH ObjectUpdateReceived;
        public event Handlers.ObjectAddedResponseEH ObjectAddedReceived;
        public event Handlers.ChatMessageEH ChatMessageReceived;
        public event Handlers.IntEH ClientInfoRequestReceived;
        #endregion

        #region Initilization
        public CommClient(string ip, int port, string alias)
        {
            if (IPAddress.TryParse(ip, out a) == false)
                throw new ArgumentException("Unparsable IP");

            iPort = port;
            sAlias = alias;
            Server = new ServerInfo(new IPEndPoint(a, iPort));
        }

        public bool Connect()
        {
            bool connected = false;
            Debug.WriteLine("Client: Connection " + Server.endPoint.Address.ToString() + " " + iPort);

            try
            {
                ShouldBeRunning = true;
                inputThread = new Thread(new ThreadStart(inputWorker));
                inputThread.Start();
                client = new TcpEventClient();
                client.PacketReceived += new Handlers.PacketReceivedEH(client_PacketReceived);
                client.Disconnected += new Handlers.voidEH(client_Disconnected);
                connected = client.Connect(Server.endPoint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Comm Client Error..... " + ex.Message);
                connected = false;
            }

            return connected;
        }

        public void Stop()
        {
            ShouldBeRunning = false;
            client.Stop();
        } 
        #endregion

        #region TcpEventClient Callbacks
        void client_Disconnected()
        {
            if (Disconnected == null)
                return;
            Disconnected();
        }

        void client_PacketReceived(Packet packet)
        {
            InputQueue.Enqueue(packet);
        } 
        #endregion

        #region Packet Input
        private void inputWorker()
        {
            while (ShouldBeRunning)
            {
                while (InputQueue.Count > 0)
                    ProcessInputPacket(InputQueue.Dequeue());

                Thread.Sleep(1);
            }
        }

        private void ProcessInputPacket(Packet packet)
        {
            if (packet is ClientInfoRequestPacket)
            {
                Trace.WriteLine("Received ClientInfoRequest");
                ClientInfoRequestPacket cir = packet as ClientInfoRequestPacket;
                ClientInfoResponsePacket clientInfoResponse = new ClientInfoResponsePacket(sAlias);
                client.Send(clientInfoResponse);
                CallClientInfoRequestReceived(cir.ID);
            }
            else if (packet is ChatPacket)
            {
                ChatPacket cp = packet as ChatPacket;
                CallChatMessageReceived(new ChatMessage(cp.message, cp.player));
            }
            else if (packet is ObjectAddedPacket)
            {
                Trace.WriteLine("Received ObjectAdded");
                ObjectAddedPacket corp = packet as ObjectAddedPacket;
                CallObjectRequestResponseReceived(corp.Owner, corp.ID, corp.AssetName);
            }
            else if (packet is ObjectUpdatePacket)
            {
                ObjectUpdatePacket oup = packet as ObjectUpdatePacket;
                CallObjectUpdateReceived(oup.objectId, oup.assetName, oup.position, oup.orientation, oup.velocity);
            }
            else if (packet is ObjectActionPacket)
            {
                ObjectActionPacket oap = packet as ObjectActionPacket;
                CallObjectActionReceived(oap.objectId, oap.actionParameters);
            }
            else if (packet is ClientDisconnectPacket)
            {
                ClientDisconnectPacket cdp = packet as ClientDisconnectPacket;
                CallPlayerDisconnected(cdp.id);
            }
            else if (packet is ClientConnectedPacket)
            {
                ClientConnectedPacket ccp = packet as ClientConnectedPacket;
                CallOtherClientConnectedToServer(ccp.ID, ccp.Alias);
            }
            else if (packet is ObjectAttributePacket)
            {
                ObjectAttributePacket oap = packet as ObjectAttributePacket;
                CallObjectAttributeReceived(oap);
            }
            else if (packet is ObjectDeletedPacket)
            {
                Trace.WriteLine("Received ObjectDelete");
                ObjectDeletedPacket odp = packet as ObjectDeletedPacket;
                CallObjectDeleteReceived(odp);
            }
        }

        #region Process Packet Callbacks
        private void CallPlayerDisconnected(int id)
        {
            if (PlayerDisconnected == null)
                return;
            PlayerDisconnected(id);
        }

        private void CallObjectDeleteReceived(ObjectDeletedPacket odp)
        {
            if (ObjectDeleteReceived == null)
                return;
            ObjectDeleteReceived(odp.objectId);
        }

        private void CallObjectAttributeReceived(ObjectAttributePacket oap)
        {
            if (ObjectAttributeReceived == null)
                return;
            ObjectAttributeReceived(oap);
        }

        private void CallOtherClientConnectedToServer(int id, string alias)
        {
            if (OtherClientConnectedToServer == null)
                return;
            OtherClientConnectedToServer(id, alias);
        }

        private void CallObjectActionReceived(int id, object[] parameters)
        {
            if (ObjectActionReceived == null)
                return;
            ObjectActionReceived(id, parameters);
        }

        private void CallObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            if (ObjectUpdateReceived == null)
                return;
            ObjectUpdateReceived(id, asset, pos, orient, vel);
        }

        private void CallObjectRequestResponseReceived(int owner, int id, string asset)
        {
            if (ObjectAddedReceived == null)
                return;
            ObjectAddedReceived(owner, id, asset);
        }

        private void CallChatMessageReceived(ChatMessage cm)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(cm);
        }

        private void CallClientInfoRequestReceived(int id)
        {
            if (ClientInfoRequestReceived == null)
                return;
            ClientInfoRequestReceived(id);
        }
        #endregion 
        #endregion

        #region Packet Sending
        public void SendChatPacket(string msg, int player)
        {
            // TODO, fix
            // JAP 6-14-2013 - What needs to be fixed here? The TODO is very old.
            client.Send(new ChatPacket(msg, player));
        }

        public void SendObjectRequest(string assetname)
        {
            client.Send(new ObjectRequestPacket(assetname));
        }

        /* TODO
         * Remove?
         * This is not used, nor should it, based on what a CommClient is.
         */
        [Obsolete]
        public void SendObjectUpdate(int id, Vector3 pos, Matrix orient, Vector3 vel)
        {
            // NEVER USED?
            // the 0 here is WRONG if this IS ever Used
            client.Send(new ObjectUpdatePacket(id, string.Empty, pos, orient, vel));
        }

        public void SendObjectAction(int id, object[] actionvals)
        {
            client.Send(new ObjectActionPacket(id, actionvals));
        }

        public void Send(Packet clientReadyPacket)
        {
            client.Send(clientReadyPacket);
        } 
        #endregion
    }
}
