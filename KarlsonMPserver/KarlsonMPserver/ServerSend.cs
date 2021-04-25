using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet.ToArray());
        }

        private static void SendTCPData(int[] _toClients, Packet _packet)
        {
            _packet.WriteLength();
            foreach (int _client in _toClients)
                Server.clients[_client].tcp.SendData(_packet.ToArray());
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using Packet _packet = new((int)PacketID.welcome);
            _packet.Write(_toClient);
            _packet.Write(_msg);
            SendTCPData(_toClient, _packet);
        }
    }
}
