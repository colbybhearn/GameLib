﻿using GameHelper.Collections;
using GameHelper.Communication;
using GameHelper.Multiplayer.Packets;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace GameHelper.Multiplayer
{
    /*Vision
     * this will have a game
     * accept clients into the game.
     * send updates about a client to all other clients
     * accept client messages
     * process client messages
     * 
     * 
     * Server game is Reality
     * Client games are nearby realities
     * Clients update the server about their controlled objects only
     * Server updates client controlled objects
     * Server updates all clients about all objects
     * 
     * interpolation between realities may be needed to smooth out jitter
     * UDP packets may be needed to facilitate high update rates
     */

    public class CommServer
    {

        #region Properties
        List<int> Clients = new List<int>();
        TcpEventServer tcpServer;
        ThreadQueue<ClientPacketInfo> InputQueue = new ThreadQueue<ClientPacketInfo>();
        Thread inputThread;
        bool ShouldBeRunning = false; 
        #endregion

        #region Initilization
        public CommServer(int lobbyport)
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress[] ips = ipEntry.AddressList;
            string ip = ips[0].ToString();

            tcpServer = new TcpEventServer(ip, lobbyport);
            tcpServer.ClientAccepted += new Handlers.IntEH(ClientAccepted);
            tcpServer.PacketReceived += new Handlers.IntPacketEH(PacketReceived);
            tcpServer.ClientDisconnected += new Handlers.IntEH(ClientDisconnected);
        }

        public void Start()
        {
            ShouldBeRunning = true;
            tcpServer.Start();
            inputThread = new Thread(new ThreadStart(inputWorker));
            inputThread.Start();
        }

        public void Stop()
        {
            ShouldBeRunning = false;
            tcpServer.Stop();
            Clients.Clear();
        } 
        #endregion

        #region TCP Server Callbacks
        void ClientAccepted(int id)
        {
            ClientInfoRequestPacket cirp = new ClientInfoRequestPacket(id);
            tcpServer.Send(cirp, id);
            Clients.Add(id);
        }

        void ClientDisconnected(int id)
        {
            BroadcastPacket(new ClientDisconnectPacket(id));
            Clients.Remove(id);
        } 
        #endregion

        #region Packet Receiving

        private void inputWorker()
        {
            while (ShouldBeRunning)
            {
                while (InputQueue.Count > 0)
                {
                    ProcessInputPacket(InputQueue.Dequeue());
                }
                Thread.Sleep(1);
            }
        }

        void PacketReceived(int id, Packet p)
        {
            if (Clients.Contains(id) == false)
                return;
            ClientPacketInfo cpi = new ClientPacketInfo(id, p);
            InputQueue.Enqueue(cpi);
        }

        private void ProcessInputPacket(ClientPacketInfo cpi)
        {
            if (cpi == null)
                return;

            Packet packet = cpi.packet;

            if (packet is ClientInfoResponsePacket)
            {
                Debug.WriteLine("Received Client info Response");
                //client connects. Server sends infoRequest, Client sends infoResponse.
                ClientInfoResponsePacket cirp = packet as ClientInfoResponsePacket;
                CallClientConnected(cpi.id, cirp.Alias);

                // Let everyone know they joined
                ClientConnectedPacket ccp = new ClientConnectedPacket(cpi.id, cirp.Alias);
                BroadcastPacket(ccp);
            }
            else if(packet is ClientReadyPacket)
            {
                ClientReadyPacket crp = packet as ClientReadyPacket;
                CallClientReadyReceived(crp.Id, crp.Alias);
            }
            else if (packet is ChatPacket)
            {
                ChatPacket cp = packet as ChatPacket;
                BroadcastChatMessage(cp.message, cp.player);
                CallChatMessageReceived(new ChatMessage(cp.message, cp.player));
                
            }
            else if (packet is ObjectRequestPacket)
            {
                Debug.WriteLine("Received ObjectRequestPacket");
                ObjectRequestPacket corp = packet as ObjectRequestPacket;
                CallObjectRequestReceived(cpi.id,corp.AssetName);
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
            else if (packet is ObjectAttributePacket)
            {
                ObjectAttributePacket oap = packet as ObjectAttributePacket;
                CallObjectAttributeReceived(oap);
            }
        }

        public event Handlers.IntStringEH ClientReadyReceived;
        private void CallClientReadyReceived(int id, string alias)
        {
            if (ClientReadyReceived == null)
                return;
            ClientReadyReceived(id, alias);
        }

        public event Handlers.ObjectAttributeEH ObjectAttributeReceived;
        private void CallObjectAttributeReceived(ObjectAttributePacket oap)
        {
            if (ObjectAttributeReceived == null)
                return;
            ObjectAttributeReceived(oap);
        }


        public event Handlers.ObjectActionEH ObjectActionReceived;
        private void CallObjectActionReceived(int id, object[] parameters)
        {
            if (ObjectActionReceived == null)
                return;
            ObjectActionReceived(id, parameters);
        }

        public event Handlers.ObjectRequestEH ObjectRequestReceived;
        private void CallObjectRequestReceived(int clientId, string asset)
        {
            if (ObjectRequestReceived == null)
                return;
            ObjectRequestReceived(clientId, asset);
        }

        public event Handlers.ChatMessageEH ChatMessageReceived;
        private void CallChatMessageReceived(ChatMessage cm)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(cm);
        }

        public event Handlers.IntStringEH ClientConnected;
        private void CallClientConnected(int id, string alias)
        {
            if (ClientConnected == null)
                return;
            ClientConnected(id, alias);
        }

        public event Handlers.ObjectUpdateEH ObjectUpdateReceived;
        private void CallObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            if (ObjectUpdateReceived == null)
                return;
            ObjectUpdateReceived(id, asset, pos, orient, vel);
        } 
        #endregion

        #region Packet Sending

        public void BroadcastPacketToAllButOne(Packet p, int clientid)
        {
            tcpServer.SendToAllButOne(p, clientid);
        }

        public void BroadcastPacket(Packet p)
        {
            tcpServer.Send(p);
        }

        public void SendPacket(Packet p, int clientID)
        {
            tcpServer.Send(p, clientID);
        }

        /* TODO
         * Should this be in the general CommServer?
         */
        public void BroadcastChatMessage(string msg, int player)
        {
            BroadcastPacket(new ChatPacket(msg, player));
        }

        public void SendPlayerInformation(int receivingClient, int id, string alias)
        {
            SendPacket(new ClientConnectedPacket(id, alias), receivingClient);
        }

        public void BroadcastObjectAddedPacket(int clientid, int objectId, string asset)
        {
            if (Clients.Contains(clientid) == false)
                return;
            tcpServer.Send(new ObjectAddedPacket(clientid, objectId, asset));
        }

        public void SendObjectAddedPacket(int receivingClient, int owner, int objectId, string asset)
        {
            if (Clients.Contains(owner) == false)
                return;

            if (Clients.Contains(receivingClient) == false)
                return;

            tcpServer.Send(new ObjectAddedPacket(owner, objectId, asset), receivingClient);
        }

        public void BroadcastObjectUpdate(Packet p)
        {
            BroadcastPacket(p);
        } 
        #endregion
    }
}