using MelonLoader;
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
                object[] customAttributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(false);
                string oldVer = "0.0.0"; // should always return true in OutOfDate if foreach failes
                foreach (var ca in customAttributes)
                {
                    if (ca.GetType() != typeof(MelonInfoAttribute))
                        continue;
                    oldVer = ((MelonInfoAttribute)ca).Version;
                }
                _packet.Write(oldVer);
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

        public static void ChatMsg(string _msg)
        {
            using (Packet _packet = new Packet((int)PacketID.chat))
            {
                _packet.Write(_msg);
                SendTCPData(_packet);
            }
        }

        public static void FinishLevel(int miliseconds)
        {
            using (Packet _packet = new Packet((int)PacketID.finishLevel))
            {
                _packet.Write(miliseconds);
                SendTCPData(_packet);
            }
        }

        public static void Ping()
        {
            using (Packet _packet = new Packet((int)PacketID.ping))
                SendTCPData(_packet);
        }
    }
}
