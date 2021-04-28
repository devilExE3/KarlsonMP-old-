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
            _username = Utils.RemoveRichText(_username);
            string _version = "0.0.0";
            try
            {
                _version = _packet.ReadString();
            }
            catch
            {
                Program.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} outdated version {_version}");
                ServerSend.Chat(_fromClient, "<color=red>You are running an older version of karlson.</color>");
                Server.clients[_fromClient].Disconnect();
                return;
            }
            if (_checkId != _fromClient)
            {
                Program.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} assumed wrong ID {_checkId} (Sent from {_fromClient})");
                Server.clients[_fromClient].Disconnect();
                return;
            }
            if(_version != Constants.version)
            {
                Program.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} outdated version {_version}");
                ServerSend.Chat(_fromClient, "<color=red>You are running different version of KarlsonMP than the server.</color>\nPlease refer to api.xiloe.fr/karlson/status for more information");
                Server.clients[_fromClient].Disconnect();
                return;
            }
            Program.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected with ID {_fromClient} and username {_username}");
            Server.clients[_fromClient].player = new Player(_fromClient, _username);
            ServerSend.Chat("<color=green>[+] <b>" + _username + "</b></color>");
        }

        public static void EnterScene(int _fromClient, Packet _packet)
        {
            string _sceneName = _packet.ReadString();
            if (!Constants.allowedSceneNames.Contains(_sceneName))
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
            Server.clients[_fromClient].player.scene = _sceneName;
        }

        public static void LeaveScene(int _fromClient, Packet _packet)
        {
            string _sceneName = _packet.ReadString();
            if (!Constants.allowedSceneNames.Contains(_sceneName))
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
                                      select x.Value)
            {
                if (client.player == null)
                    continue;
                if (Server.clients[_fromClient].player == null)
                    continue;
                if(client.player.scene == Server.clients[_fromClient].player.scene && client.id != _fromClient)
                    ServerSend.ClientMove(client.id, _fromClient, pos, _rot);
            }
        }

        public static void Chat(int _fromClient, Packet _packet)
        {
            string _msg = _packet.ReadString();
            // handle commands ?
            ServerSend.Chat(Utils.RemoveRichText(Server.clients[_fromClient].player.username + ": " + _msg));
        }

        public static void FinishLevel(int _fromClient, Packet _packet)
        {
            if (Server.clients[_fromClient].player.scene == "")
                return; // client isn't in any scene that we know of
            int miliseconds = _packet.ReadInt();
            ServerSend.Chat("<color=yellow>* <b>" + Server.clients[_fromClient].player.username + "</b> finished <b>" + Constants.sceneNames[Constants.allowedSceneNames.ToList().IndexOf(Server.clients[_fromClient].player.scene)] + "</b> in " + Constants.FormatMiliseconds(miliseconds) + "</color>");
        }

        public static void Ping(int _fromClient, Packet _)
        {
            Server.clients[_fromClient].player.ping = (int)(DateTime.Now - Server.clients[_fromClient].player.lastPing).TotalMilliseconds;
            Server.clients[_fromClient].player.lastPing = DateTime.MinValue;
        }
    }
}
