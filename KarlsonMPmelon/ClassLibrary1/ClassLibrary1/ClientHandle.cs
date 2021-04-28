using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMP
{
    class ClientHandle
    {
        public static void Welcome(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _msg = _packet.ReadString();
            Client.instance.myId = _id;
            ClientSend.WelcomeReceived();
            Client.instance.isConnecting = false;
            Client.instance.isConnected = true;
            Main.AddToChat($"Connected with ID {_id} | {_msg}");
        }

        public static void EnterScene(Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(Client.instance.players.ContainsKey(_id))
            {
                if (Client.instance.players[_id].enemy)
                    UnityEngine.Object.Destroy(Client.instance.players[_id].enemy);
                Client.instance.players.Remove(_id);
            }
            Client.instance.players.Add(_id, new OnlinePlayer(_id));
        }
        public static void LeaveScene(Packet _packet)
        {
            int _id = _packet.ReadInt();
            UnityEngine.Object.Destroy(Client.instance.players[_id].enemy);
            Client.instance.players.Remove(_id);
        }
        public static void ClientsInScene(Packet _packet)
        {
            int _len = _packet.ReadInt();
            string clients = "";
            for (int i = 0; i < _len; i++)
            {
                int _id = _packet.ReadInt();
                clients += _id + ", ";
                if(Client.instance.players.ContainsKey(_id))
                {
                    if (Client.instance.players[_id].enemy)
                        UnityEngine.Object.Destroy(Client.instance.players[_id].enemy);
                    Client.instance.players.Remove(_id);
                }
                    
                Client.instance.players.Add(_id, new OnlinePlayer(_id));
            }
        }

        public static void ClientInfo(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            Client.instance.players[_id].username = _username;
        }

        public static void ClientMove(Packet _packet)
        {
            int _id = _packet.ReadInt();
            if (!Client.instance.players.ContainsKey(_id))
            {
                Client.instance.players.Add(_id, new OnlinePlayer(_id));
                return;
            }
            UnityEngine.Vector3 pos = _packet.ReadVector3();
            float _rot = _packet.ReadFloat();
            Client.instance.players[_id].pos = pos;
            Client.instance.players[_id].rot = _rot;
            Client.instance.players[_id].enemy.transform.position = pos + new UnityEngine.Vector3(0f, 1.4f, 0f); // pos correction
            Client.instance.players[_id].enemy.transform.rotation = UnityEngine.Quaternion.Euler(0f, _rot, 0f);
        }

        public static void Chat(Packet _packet)
        {
            Main.AddToChat(_packet.ReadString());
        }

        public static void Ping(Packet _packet)
        {
            Client.instance.ping = _packet.ReadInt();
            ClientSend.Ping();
        }
    }
}
