using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameHelper.Multiplayer;
using GameHelper.Multiplayer.Packets;
using GameHelper;
using Microsoft.Xna.Framework;
using GameHelper.Objects;
using GameHelper.Physics.PhysicsObjects;

namespace GameHelper.Base
{
    public class ClientBase : GameBase 
    {
        private bool isConnectedToServer;
        public bool IsConnectedToServer
        {
            get
            {
                return isConnectedToServer;
            }
        }

        public CommClient commClient;

        public List<int> clientControlledObjects = new List<int>(); // TODO - Client Side only, merge with ownedObjects somehow?
        public int MyClientID; // Used by the client

        public ClientBase()
            : base()
        {
        }



        public override void Initialize(System.ComponentModel.Design.ServiceContainer services, Microsoft.Xna.Framework.Graphics.GraphicsDevice gd, GameBase.myCallbackDelegate updateCamCallback)
        {
            base.Initialize(services, gd, updateCamCallback);

            if (commClient != null)
                commClient.Send(new ClientReadyPacket(MyClientID, "someone"));
        }

        public override void InitializeContent()
        {
            base.InitializeContent();
        }

        public override void InitializeMultiplayer()
        {
            base.InitializeMultiplayer();
            commClient.ClientInfoRequestReceived += new Handlers.IntEH(commClient_ClientInfoRequestReceived);
            commClient.ChatMessageReceived += new Handlers.ChatMessageEH(commClient_ChatMessageReceived);
            commClient.ObjectAddedReceived += new Handlers.ObjectAddedResponseEH(commClient_ObjectAddedReceived);
            commClient.ObjectActionReceived += new Handlers.ObjectActionEH(commClient_ObjectActionReceived);
            commClient.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commClient_ObjectUpdateReceived);
            commClient.Disconnected += new Handlers.voidEH(commClient_ThisClientDisconnectedFromServer);
            commClient.OtherClientConnectedToServer += new Handlers.ClientConnectedEH(commClient_OtherClientConnected);
            commClient.PlayerDisconnected += new Handlers.IntEH(commClient_OtherClientDisconnectedFromServer);
            commClient.ObjectAttributeReceived += new Handlers.ObjectAttributeEH(base.commClient_ObjectAttributeReceived);
            commClient.ObjectDeleteReceived += new Handlers.IntEH(commClient_ObjectDeleteReceived);
        }


        public override void Start()
        {
            base.Start();
            
        }

        // CLIENT only
        public virtual bool ConnectToServer(string ip, int port, string alias)
        {
            commClient = new CommClient(ip, port, alias);
            InitializeMultiplayer();
            return commClient.Connect();
        }
        /// <summary>
        /// CLIENT SIDE
        /// Client received an info request packet from the server, which contains our ID to use
        /// </summary>
        /// <param name="id"></param>
        void commClient_ClientInfoRequestReceived(int id)
        {
            MyClientID = id;
        }

        void commClient_ChatMessageReceived(ChatMessage cm)
        {
            String alias;
            if (players.TryGetValue(cm.Owner, out alias))
                cm.OwnerAlias = alias;

            CallChatMessageReceived(cm);
        }
        /// <summary>
        /// CLIENT SIDE
        /// The client has received an Object Action packet and it needs to be processed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        void commClient_ObjectActionReceived(int id, object[] parameters)
        {
            // TODO, fill in
        }

        /// <summary>
        /// CLIENT SIDE
        /// The client has received a response back from the server about the object the client requested
        /// This is called from the Network code, thus in the Network threads
        /// </summary>
        /// <param name="i"></param>
        /// <param name="asset"></param>
        void commClient_ObjectAddedReceived(int owner, int id, string asset)
        {
            // MINE!
            if (owner == MyClientID)
                clientControlledObjects.Add(id);
            objectsOwned.Add(id, owner);
            ProcessObjectAdded(owner, id, asset);
        }


        public void commClient_ObjectDeleteReceived(int id)
        {
            DeleteObject(id);
        }
        
        public void commClient_ThisClientDisconnectedFromServer()
        {
            isConnectedToServer = false;
            CallThisClientDisconnectedFromServer();            
        }

        public void commClient_OtherClientConnected(int id, string alias)
        {
            if (players.ContainsKey(id) == false)
                players.Add(id, alias);

            isConnectedToServer = true;
            CallOtherClientConnectedToServer(id, alias);
            
        }

        // Colby fixed the name of this handler
        public void commClient_OtherClientDisconnectedFromServer(int id)
        {
            isConnectedToServer = false; // this is wrong 2012.10.09
            CallOtherClientDisconnectedFromServer(id);
        }

        public void commClient_ObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            lock (MultiplayerUpdateQueue)
            {
                MultiplayerUpdateQueue.Add(new GameHelper.Multiplayer.Packets.ObjectUpdatePacket(id, asset, pos, orient, vel));
            }
        }


        /// <summary>
        /// CLIENT SIDE 
        /// This should be handled in the specific game, to do something game-specific, like adding a specific model by asset name.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="asset"></param>
        public virtual void ProcessObjectAdded(int owner, int id, string asset)
        {

        }

        /// <summary>
        /// CLIENT SIDE
        /// calls this to disconnect from the server
        /// </summary>
        public void DisconnectFromServer()
        {
            commClient.Stop();
            isConnectedToServer = false;
        }

        public override void SendChatPacket(ChatMessage msg)
        {
            if (commClient != null)
                commClient.SendChatPacket(msg.Message, MyClientID);
        }

        public override void SetNominalInputState()
        {
            lock (gameObjects)
            {
                foreach (int i in assetManager.GetObjectsOwnedByClient(MyClientID))
                {
                    if (!gameObjects.ContainsKey(i))
                        return;
                    gameObjects[i].SetNominalInput();
                }
            }
        }
        public override void physicsManager_PreIntegrate()
        {
            lock (gameObjects)
            {
                #region Send Action Updates to the server
                foreach (int i in clientControlledObjects)
                {
                    if (!gameObjects.ContainsKey(i))
                        continue;
                    Entity go = gameObjects[i];
                    if (!go.actionManager.actionApplied)
                        continue;

                    if (go is RoverObject)
                    {
                    }
                    object[] vals = go.actionManager.GetActionValues();
                    go.actionManager.ValueSwap();
                    commClient.SendObjectAction(go.ID, vals);
                }
                #endregion

                #region Process packets from the server
                List<object> ShouldRetry = new List<object>();
                lock (MultiplayerUpdateQueue)
                {
                    while (MultiplayerUpdateQueue.Count > 0)
                    {
                        Packet p = MultiplayerUpdateQueue[0] as Packet;
                        MultiplayerUpdateQueue.RemoveAt(0);

                        if (p is ObjectUpdatePacket)
                        {
                            #region Process Update Packets from the server
                            ObjectUpdatePacket oup = p as ObjectUpdatePacket;

                            if (!gameObjects.ContainsKey(oup.objectId))
                            {
                                AddNewObject(oup.objectId, oup.assetName);
                                ShouldRetry.Add(oup);
                                continue;
                                // TODO -  Should we continue instead of not updating this frame?
                            }
                            // (can't yet due to AddNewObject waiting until the next integrate to actually add it)
                            Entity go = gameObjects[oup.objectId];
                            if (go.hasNotDoneFirstInterpoladation)
                                go.Interpoladate(oup.position, oup.orientation, oup.velocity, 1.0f); // Server knows best!
                            else
                                go.Interpoladate(oup.position, oup.orientation, oup.velocity); // split realities 50/50
                            #endregion
                        }
                        else if (p is ObjectAttributePacket)
                        {
                            #region Process Attribute Packets from the server
                            ObjectAttributePacket oap = p as ObjectAttributePacket;
                            if (gameObjects.ContainsKey(oap.objectId))
                            {
                                Entity go = gameObjects[oap.objectId];
                                bool locallyOwnedAndOperated = false;

                                if (go.OwningClientId == MyClientID)
                                    locallyOwnedAndOperated = true;
                                go.SetObjectAttributes(oap.booleans, oap.ints, oap.floats, locallyOwnedAndOperated);
                            }
                            #endregion
                        }
                    }
                    while (ShouldRetry.Count > 0)
                    {
                        MultiplayerUpdateQueue.Add(ShouldRetry[0]);
                        ShouldRetry.RemoveAt(0);
                    }
                }

                #endregion
            }
            
        }

        public override void Stop()
        {
            base.Stop();
            if (isConnectedToServer) // Client Side
                DisconnectFromServer();
        }

    }
}
