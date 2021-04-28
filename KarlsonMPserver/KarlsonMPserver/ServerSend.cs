using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null)
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }
        private static void SendTCPData(Packet _packet, int[] _exceptClients)
        {
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && !_exceptClients.Contains(i))
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using Packet _packet = new((int)PacketID.welcome);
            _packet.Write(_toClient);
            _packet.Write(_msg);
            SendTCPData(_toClient, _packet);
        }

        public static void EnterScene(int _toClient, int _fromClient)
        {
            using Packet _packet = new((int)PacketID.enterScene);
            _packet.Write(_fromClient);
            SendTCPData(_toClient, _packet);
        }
        public static void LeaveScene(int _toClient, int _fromClient)
        {
            using Packet _packet = new((int)PacketID.leaveScene);
            _packet.Write(_fromClient);
            SendTCPData(_toClient, _packet);
        }

        public static void ClientsInScene(int _toClient, string _scene)
        {
            List<int> _clients = new();
            foreach (Client client in from x in Server.clients
                                      where x.Value.tcp.socket != null && x.Value.player != null
                                         && x.Value.player.scene == _scene
                                      select x.Value)
                _clients.Add(client.id);
            using Packet _packet = new((int)PacketID.clientsInScene);
            _packet.Write(_clients.Count);
            foreach (int i in _clients)
                _packet.Write(i);
            SendTCPData(_toClient, _packet);
        }

        public static void ClientInfo(int _toClient, int _id)
        {
            using Packet _packet = new((int)PacketID.clientInfo);
            _packet.Write(_id);
            _packet.Write(Server.clients[_id].player.username);
            SendTCPData(_toClient, _packet);
        }

        public static void ClientMove(int _toClient, int _fromClient, Vector3 _pos, float _rot)
        {
            using Packet _packet = new((int)PacketID.clientMove);
            _packet.Write(_fromClient);
            _packet.Write(_pos);
            _packet.Write(_rot);
            SendTCPData(_toClient, _packet);
        }

        public static void Chat(int _toClient, string _message)
        {
            using Packet _packet = new((int)PacketID.chat);
            _packet.Write(_message);
            SendTCPData(_toClient, _packet);
        }
        public static void Chat(string _message)
        {
            using Packet _packet = new((int)PacketID.chat);
            _packet.Write(_message);
            SendTCPData(_packet);
        }

        public static void PingAll()
        {
            using Packet _packet = new((int)PacketID.ping);
            List<int> _clients = new();
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && Server.clients[i].player.lastPing != DateTime.MinValue)
                    _clients.Add(i);
            SendTCPData(_clients.ToArray(), _packet);
        }
    }
}
