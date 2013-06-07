using System;

namespace GameHelper.Multiplayer.Packets
{
    [Serializable]
    public class ClientInfoResponsePacket : Packet
    {
        public string Alias;
        public ClientInfoResponsePacket(string alias) 
            :base(Types.csClientInfoResponse)
        {
            Alias = alias;
        }
    }
}
