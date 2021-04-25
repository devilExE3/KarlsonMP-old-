using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            if (_checkId != _fromClient)
            {
                Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} assumed wrong ID {_checkId} (Sent from {_fromClient})");
                Server.clients[_fromClient].Disconnect();
                return;
            }
            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected with ID {_fromClient} and username {_username}");
            Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

        private static readonly string[] allowedSceneNames = new string[]
        {
            "0Tutorial",
            "1Sandbox0", "2Sandbox1", "3Sandbox2",
            "4Escape0", "5Escape1", "6Escape2", "7Escape3",
            "8Sky0", "9Sky1", "10Sky2"
        };

        public static void EnterScene(int _fromClient, Packet _packet)
        {
            string _sceneName = _packet.ReadString();
            if (!allowedSceneNames.Contains(_sceneName))
                // TODO: maybe drop client, because he entered a scene that doesn't exist in the normal
                // version (or a scene we don't care about entering, such as Initialization and MainMenu)
                return;
            if(Server.clients[_fromClient].player == null)
                // TODO: drop client, because he entered a scene before "connecting" (sending back the WelcomeReceived message)
                return;
            if (Server.clients[_fromClient].player.scene == _sceneName)
                // TODO: maybe drop client, because he entered the same scene, which might result in
                // the client crashing (idk didn't test)
                return;
            foreach(Client client in from x in Server.clients
                                     where x.Value.tcp.socket != null && x.Value.player != null
                                        && x.Value.player.scene == _sceneName
                                     select x.Value)
                ServerSend.EnterScene(client.id, _fromClient);
            ServerSend.ClientsInScene(_fromClient, _sceneName);
            Console.WriteLine($"ID {_fromClient} entered scene {_sceneName}");
            Server.clients[_fromClient].player.scene = _sceneName;
        }

        public static void LeaveScene(int _fromClient, Packet _packet)
        {
            string _sceneName = _packet.ReadString();
            if (!allowedSceneNames.Contains(_sceneName))
                // TODO: maybe drop client, because he entered a scene that doesn't exist in the normal
                // version (or a scene we don't care about entering, such as Initialization and MainMenu)
                return;
            if (Server.clients[_fromClient].player == null)
                // TODO: drop client, because he entered a scene before "connecting" (sending back the WelcomeReceived message)
                return;
            if (Server.clients[_fromClient].player.scene != _sceneName)
                // TODO: maybe drop client, because he left a scene which he wasn't in which will cause
                // the client to crash (idk, didn't test)
                return;
            Server.clients[_fromClient].player.scene = ""; // big time mistake xD
            foreach (Client client in from x in Server.clients
                                      where x.Value.tcp.socket != null && x.Value.player != null
                                         && x.Value.player.scene == _sceneName
                                      select x.Value)
                ServerSend.LeaveScene(client.id, _fromClient);
            Console.WriteLine($"ID {_fromClient} left scene {_sceneName}");
        }

        public static void GetClientInfo(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            ServerSend.ClientInfo(_fromClient, _id);
        }

        public static void ClientMove(int _fromClient, Packet _packet)
        {
            Vector3 pos = _packet.ReadVector3();
            float _rot = _packet.ReadFloat();
            foreach (Client client in from x in Server.clients
                                      where x.Value.tcp.socket != null && x.Value.player != null
                                         && x.Value.player.scene == Server.clients[_fromClient].player.scene
                                         && x.Value.id != _fromClient
                                      select x.Value)
                ServerSend.ClientMove(client.id, _fromClient, pos, _rot);
        }
    }
}
