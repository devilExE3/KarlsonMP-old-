using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPclient
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
    }
}
