using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPclient
{
    class ClientHandle
    {
        public static void Welcome(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _msg = _packet.ReadString();
            Client.instance.myId = _id;
            UnityEngine.Debug.Log($"Received welcome message: {_msg}");
            ClientSend.WelcomeReceived();
            Client.instance.isConnected = true;
        }
    }
}
