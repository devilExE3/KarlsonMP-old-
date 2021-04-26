using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMP
{
    class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.tcp.SendData(_packet.ToArray());
        }

        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)PacketID.welcome))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(Client.instance.username);
                SendTCPData(_packet);
            }

        }

        public static void EnterScene(string _sceneName)
        {
            using (Packet _packet = new Packet((int)PacketID.enterScene))
            {
                _packet.Write(_sceneName);
                SendTCPData(_packet);
            }
        }
        public static void LeaveScene(string _sceneName)
        {
            using (Packet _packet = new Packet((int)PacketID.leaveScene))
            {
                _packet.Write(_sceneName);
                SendTCPData(_packet);
            }
        }

        public static void GetPlayerInfo(int _id)
        {
            using (Packet _packet = new Packet((int)PacketID.clientInfo))
            {
                _packet.Write(_id);
                SendTCPData(_packet);
            }
        }

        public static void ClientMove(UnityEngine.Vector3 pos, float _rot)
        {
            using (Packet _packet = new Packet((int)PacketID.clientMove))
            {
                _packet.Write(pos);
                _packet.Write(_rot);
                SendTCPData(_packet);
            }
        }
    }
}
