﻿using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;


namespace Helper
{
    public static class Handlers
    {

        // more generic and reusable
        public delegate void IntEH(int i);
        public delegate void StringEH(string s);
        public delegate void IntStringEH(int i, string s);
        //public delegate void StringStringEH(string s1, string s2);

        public delegate void voidEH();

        // more specific
        public delegate void ObjectRequestEH(int clientId, int asset);
        public delegate void ObjectAddedResponseEH(int ownerId, int objectId, int asset);
        public delegate void ObjectUpdateEH(int id, int asset, Vector3 pos, Matrix orient, Vector3 vel);
        public delegate void ObjectActionEH(int id, object[] parameters);
        public delegate void ObjectAttributeEH(ObjectAttributePacket oap);
        public delegate void ClientConnectedEH(int id, string alias);
        public delegate void PacketReceivedEH(Packet p);
        public delegate void DataReceivedEH(byte[] b);
        public delegate void IntPacketEH(int i, Packet p);
        public delegate void ChatMessageEH(ChatMessage cm);

    }
}
