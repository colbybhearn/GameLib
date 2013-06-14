using System.Net.Sockets;

namespace GameHelper.Communication
{
    public class ClientInfoSocket : SocketComm
    {
        // Using new keywords suppresses warnings about "hiding" the SocketComm's versions
        new public delegate void PacketReceivedEventHandler(int id, byte[] data);
        new public event PacketReceivedEventHandler PacketReceived;
        new public event Handlers.IntEH ClientDisconnected;
        public int ClientID;

        public ClientInfoSocket(Socket s, int id) 
            : base(s, false, id.ToString()) // false because the server uses one ClientInfoSocket per client
        {
            ClientID = id;
        }

        public override void CallPacketReceived(byte[] data)
        {
            if (PacketReceived == null)
                return;
            PacketReceived(ClientID, data);
        }

        protected override void CallClientDisconnected()
        {
            if (ClientDisconnected == null)
                return;
            ClientDisconnected(ClientID);
        }
    }
}
