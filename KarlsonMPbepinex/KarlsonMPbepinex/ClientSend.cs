using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                _packet.Write(Main.instance.Info.Metadata.Version.ToString());
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

        public static void ClientMove(Vector3 pos, Vector3 _rot)
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

        public static void ChangeGun(bool empty = false)
        {
            if (empty)
            {
                using (Packet _packet = new Packet((int)PacketID.changeGun))
                {
                    _packet.Write(0);
                    SendTCPData(_packet);
                }
                return;
            }
            DetectWeapons dw = (DetectWeapons)PlayerMovement.Instance.GetType().GetField("detectWeapons",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance).GetValue(PlayerMovement.Instance);
            GameObject gun = (GameObject)dw.GetType().GetField("gun",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance).GetValue(dw);
            int _gunid = 0;
            if (gun.name.Contains("Pistol"))
                _gunid = 1;
            if (gun.name.Contains("Ak47") || gun.name.Contains("Uzi"))
                _gunid = 2;
            if (gun.name.Contains("Boomer"))
                _gunid = 3;
            if (gun.name.Contains("Shotgun"))
                _gunid = 4;
            if (gun.name.Contains("Grapple"))
                _gunid = 5;
            using(Packet _packet = new Packet((int)PacketID.changeGun))
            {
                _packet.Write(_gunid);
                SendTCPData(_packet);
            }
        }

        public static void Grapple(bool use, Vector3? pos = null)
        {
            if (pos == null || !use)
            {
                using (Packet _packet = new Packet((int)PacketID.changeGrapple))
                {
                    _packet.Write(false);
                    SendTCPData(_packet);
                }
                return;
            }
            using (Packet _packet = new Packet((int)PacketID.changeGrapple))
            {
                _packet.Write(true);
                _packet.Write((Vector3)pos);
                SendTCPData(_packet);
            }
        }
    }
}
