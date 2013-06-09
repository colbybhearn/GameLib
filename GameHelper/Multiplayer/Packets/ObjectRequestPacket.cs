using System;

namespace GameHelper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectRequestPacket : Packet
    {
        public string AssetName;

        public ObjectRequestPacket(string asset) 
            : base(Types.csObjectRequest)
        {
            AssetName = asset;
        }
    }
}
