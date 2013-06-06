using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using Helper.Multiplayer.Packets;
using Helper.Objects;
using Microsoft.Xna.Framework;

namespace GameHelper.Base
{
    public class ServerBase : GameBase
    {

        public ServerBase()
            : base()
        {
        }

        public override void InitializeMultiplayer()
        {
            base.InitializeMultiplayer();
            // TODO: Should client connected and ChatMessage Received be handled elsewhere (not in BaseGame) for the server?
            commServer.ClientConnected += new Handlers.IntStringEH(commServer_ClientConnected);
            commServer.ChatMessageReceived += new Handlers.ChatMessageEH(commServer_ChatMessageReceived);
            commServer.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commServer_ObjectUpdateReceived);
            commServer.ObjectActionReceived += new Handlers.ObjectActionEH(commServer_ObjectActionReceived);
            commServer.ObjectRequestReceived += new Handlers.ObjectRequestEH(commServer_ObjectRequestReceived);
            commServer.ObjectAttributeReceived += new Handlers.ObjectAttributeEH(commServer_ObjectAttributeReceived);
            commServer.ClientReadyReceived += new Handlers.IntStringEH(commServer_ClientReadyReceived);
        }

        /// <summary>
        /// SERVER SIDE
        /// Server has received an Object Action packet and it should be processed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        public void commServer_ObjectActionReceived(int id, object[] parameters)
        {
            lock (MultiplayerUpdateQueue)
            {
                MultiplayerUpdateQueue.Add(new ObjectActionPacket(id, parameters));
            }
        }

        /// <summary>
        /// SERVER SIDE
        /// Server has received a request for a new object from a client.
        /// This is how a client requests an object it can "own"
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asset"></param>
        /// <param name="pos"></param>
        /// <param name="orient"></param>
        /// <param name="vel"></param>
        public void commServer_ObjectUpdateReceived(int id, int asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            physicsUpdateList.Add(new ObjectUpdatePacket(id, asset, pos, orient, vel));
        }
        public void commServer_ChatMessageReceived(ChatMessage cm)
        {
            String alias;
            if (players.TryGetValue(cm.Owner, out alias))
                cm.OwnerAlias = alias;

            ProcessChatMessage(cm);
        }
        public void commServer_ClientConnected(int id, string s)
        {
            ProcessClientConnected(id, s);
        }

        public virtual void ProcessClientConnected(int id, string alias)
        {

        }

        public override void DeleteObject(int objectid)
        {
            base.DeleteObject(objectid);

            // we'll handle the game-related concerns
            commServer.BroadcastPacket(new ObjectDeletedPacket(objectid));
        }

        public override void physicsManager_PreIntegrate()
        {
            base.physicsManager_PreIntegrate();
            lock (gameObjects)
            {
                #region Process Action Updates from the client
                lock (MultiplayerUpdateQueue)
                {
                    while (MultiplayerUpdateQueue.Count > 0)
                    {
                        ObjectActionPacket oap = MultiplayerUpdateQueue[0] as ObjectActionPacket;
                        if (!gameObjects.ContainsKey(oap.objectId))
                            continue; // TODO - infinite loop if this is hit
                        Gobject go = gameObjects[oap.objectId];
                        go.actionManager.ProcessActionValues(oap.actionParameters);
                        MultiplayerUpdateQueue.RemoveAt(0);
                    }
                }
                #endregion
            }
        }

        public override void physicsManager_PostIntegrate()
        {
            if (commServer != null)
            {
                lock (gameObjects)
                {
                    foreach (Gobject go in gameObjects.Values)
                    {
                        #region Send Attribute Updates to the client
                        if (go.hasAttributeChanged)
                        {
                            bool[] bools = null;
                            int[] ints = null;
                            float[] floats = null;
                            go.GetObjectAttributes(out bools, out ints, out floats);
                            ObjectAttributePacket oap = new ObjectAttributePacket(go.ID, bools, ints, floats);
                            commServer.BroadcastPacket(oap);
                        }
                        #endregion

                        #region Send Object Updates to the client

                        if (go.isMoveable && go.IsActive)
                        {
                            go.UpdateCountdown--;
                            if (go.UpdateCountdown == 0 || assetManager.isObjectOwnedByAnyClient(go.ID))
                            {
                                ObjectUpdatePacket oup = new ObjectUpdatePacket(go.ID, go.type, go.BodyPosition(), go.BodyOrientation(), go.BodyVelocity());
                                commServer.BroadcastObjectUpdate(oup);
                                go.UpdateCountdown = 10;
                            }



                        }
                        #endregion
                    }
                }
            }
            base.physicsManager_PostIntegrate();
        }

        public override void SendChatPacket(ChatMessage msg)
        {
            base.SendChatPacket(msg);
            if (commServer != null)
                commServer.BroadcastChatMessage(msg.Message, msg.Owner);
        }

        public override void Stop()
        {
            base.Stop();
            if (commServer != null) // Server Side
                commServer.Stop();
        }
        
    }
}
