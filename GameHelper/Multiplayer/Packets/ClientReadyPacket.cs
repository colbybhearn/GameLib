using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameHelper.Multiplayer.Packets
{
    [Serializable]
    public class ClientReadyPacket : Packet
    {
        public int Id;
        public string Alias;
        public ClientReadyPacket(int id, string alias)
            : base(Types.csClientReady)
        {
            Id = id;
            Alias = alias;
        }
    }
}
