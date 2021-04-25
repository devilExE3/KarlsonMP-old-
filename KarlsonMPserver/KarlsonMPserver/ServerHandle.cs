using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _checkId = _packet.ReadInt();
            string _username = _packet.ReadString();
            if(_checkId != _fromClient)
            {
                Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} assumed wrong ID {_checkId} (Sent from {_fromClient})");
                Server.clients[_fromClient].Disconnect();
                return;
            }
            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected with ID {_fromClient} and username {_username}");
        }
    }
}
