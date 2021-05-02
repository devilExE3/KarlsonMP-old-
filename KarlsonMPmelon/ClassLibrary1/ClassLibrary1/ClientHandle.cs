using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            scoreboard.motd = _msg;
            if(SceneManager.GetActiveScene().name != "MainMenu" && SceneManager.GetActiveScene().name != "Initialize")
                ClientSend.EnterScene(SceneManager.GetActiveScene().name);
            if(Client.instance.connectionRetryToken != null)
            {
                MelonLoader.MelonCoroutines.Stop(Client.instance.connectionRetryToken);
                Client.instance.connectionRetryToken = null;
            }
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
            ClientChangeGun(_id, _packet.ReadInt());
        }

        public static void ClientMove(Packet _packet)
        {
            int _id = _packet.ReadInt();
            if (!Client.instance.players.ContainsKey(_id))
            {
                Client.instance.players.Add(_id, new OnlinePlayer(_id));
                return;
            }
            Vector3 pos = _packet.ReadVector3();
            Vector3 _rot = _packet.ReadVector3();
            if(!(Vector3.Distance(Client.instance.players[_id].pos, pos) > 0.07f) && Client.instance.players[_id].activeGun != null)
                Client.instance.players[_id].enemy.GetComponentInChildren<Animator>().SetBool("Aiming", true);
            else
                Client.instance.players[_id].enemy.GetComponentInChildren<Animator>().SetBool("Aiming", false);
            Client.instance.players[_id].enemy.GetComponentInChildren<Animator>().SetBool("Running", Vector3.Distance(Client.instance.players[_id].pos, pos) > 0.07f);
            ClientUpdateGrapplePoint(_id);
            Client.instance.players[_id].pos = pos;
            Client.instance.players[_id].rot = _rot.y;
            Client.instance.players[_id].enemy.transform.position = pos + new Vector3(0f, 1.4f, 0f); // pos correction
            Client.instance.players[_id].enemy.transform.rotation = Quaternion.Euler(0f, _rot.y, 0f);
            /*if(Client.instance.players[_id].enemy.GetComponentInChildren<Animator>().GetBool("Aiming"))
            {
                GameObject targetFinder = new GameObject("TargetFinder"); // tried to make player hands at player pitch
                targetFinder.transform.position = pos + new Vector3(0f, 1.8f, 0f);
                targetFinder.transform.rotation = Quaternion.Euler(_rot);
                Enemy _enemy = Client.instance.players[_id].enemy.GetComponent<Enemy>();
                GameObject _go = Client.instance.players[_id].enemy;
                Vector3 vector = PlayerMovement.Instance.transform.position - _go.transform.position;
                if (Vector3.Angle(_go.transform.forward, vector) > 70f)
                {
                    _go.transform.rotation = Quaternion.Slerp(_go.transform.rotation, Quaternion.LookRotation(vector), Time.deltaTime * (float)_enemy.GetType().GetField("hipSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_enemy));
                }
                Client.instance.players[_id].enemy.GetComponent<Enemy>().head.transform.rotation = Quaternion.Slerp(_enemy.head.transform.rotation, Quaternion.LookRotation(vector), Time.deltaTime * (float)_enemy.GetType().GetField("headAndHandSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_enemy));
                Client.instance.players[_id].enemy.GetComponent<Enemy>().rightArm.transform.rotation = Quaternion.Slerp(_enemy.head.transform.rotation, Quaternion.LookRotation(vector), Time.deltaTime * (float)_enemy.GetType().GetField("headAndHandSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_enemy));
                Client.instance.players[_id].enemy.GetComponent<Enemy>().leftArm.transform.rotation = Quaternion.Slerp(_enemy.head.transform.rotation, Quaternion.LookRotation(vector), Time.deltaTime * (float)_enemy.GetType().GetField("headAndHandSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_enemy));
                UnityEngine.Object.Destroy(targetFinder);
            }*/
        }

        private static void ClientUpdateGrapplePoint(int _id)
        {
            if (Client.instance.players[_id].grappleLine != null)
            {
                UnityEngine.Object.Destroy(Client.instance.players[_id].grappleLine);
                Client.instance.players[_id].grappleLine = null;
            }
            if (Client.instance.players[_id].grapplePos != Vector3.zero)
            {
                Client.instance.players[_id].grappleLine = new GameObject("grapple renderer");
                LineRenderer lr = Client.instance.players[_id].grappleLine.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.widthMultiplier = 0.2f;
                lr.positionCount = 2;
                lr.startColor = Color.black;
                lr.endColor = Color.black;
                lr.SetPositions(new Vector3[]
                {
                    Client.instance.players[_id].enemy.GetComponent<Enemy>().gunPosition.position,
                    Client.instance.players[_id].grapplePos
                });
            }
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

        public static void ScoreboardHandle(Packet _packet)
        {
            scoreboard.onlinePlayers = _packet.ReadInt();
            scoreboard.maxPlayers = _packet.ReadInt();
            for (int i = 0; i < 11; i++)
                scoreboard.perLevel[i] = _packet.ReadInt();
            scoreboard.players.Clear();
            for (int i = 0; i < scoreboard.onlinePlayers; i++)
                scoreboard.players.Add(new Scoreboard.ScoreboardPlayer(_packet.ReadInt(), _packet.ReadString(), _packet.ReadString(), _packet.ReadInt()));
        }

        public static Scoreboard scoreboard;

        public class Scoreboard
        {
            public string motd = "not connected";

            public int onlinePlayers = 0;
            public int maxPlayers = 0;
            public int[] perLevel = new int[11];
            public List<ScoreboardPlayer> players = new List<ScoreboardPlayer>();

            public class ScoreboardPlayer
            {
                public ScoreboardPlayer(int _id, string _username, string _scene, int _ping)
                {
                    id = _id;
                    username = _username;
                    scene = _scene;
                    ping = _ping;
                }

                public readonly string username;
                public readonly string scene;
                public readonly int ping;
                public readonly int id;
            }
        }

        public static void ClientChangeGun(Packet _packet)
        {
            int _id = _packet.ReadInt();
            int _gunType = _packet.ReadInt();
            ClientChangeGun(_id, _gunType);
        }
        private static void ClientChangeGun(int _id, int _gunType)
        {
            if (Client.instance.players[_id].activeGun)
            {
                UnityEngine.Object.Destroy(Client.instance.players[_id].activeGun);
                Client.instance.players[_id].activeGun = null;
            }
            if (_gunType == 1)
                EnemyPickupGun(Client.instance.players[_id], PrefabInstancer.CreatePistol());
            if (_gunType == 2)
                EnemyPickupGun(Client.instance.players[_id], PrefabInstancer.CreateAk47());
            if (_gunType == 3)
                EnemyPickupGun(Client.instance.players[_id], PrefabInstancer.CreateBoomer());
            if (_gunType == 4)
                EnemyPickupGun(Client.instance.players[_id], PrefabInstancer.CreateShotgun());
            if (_gunType == 5)
                EnemyPickupGun(Client.instance.players[_id], PrefabInstancer.CreateGrappler());
        }

        private static void EnemyPickupGun(OnlinePlayer plr, GameObject _gun)
        {
            plr.activeGun = _gun;
            Enemy _enemy = plr.enemy.GetComponent<Enemy>();
            UnityEngine.Object.Destroy(_gun.GetComponent<Rigidbody>());
            _gun.GetComponent<Pickup>().PickupWeapon(false);
            _gun.transform.parent = _enemy.gunPosition;
            _gun.transform.localPosition = Vector3.zero;
            _gun.transform.localRotation = Quaternion.identity;
        }

        public static void PlayerGrapple(Packet _packet)
        {
            int _id = _packet.ReadInt();
            bool _use = _packet.ReadBool();
            if (_use)
                Client.instance.players[_id].grapplePos = _packet.ReadVector3();
            else
                Client.instance.players[_id].grapplePos = Vector3.zero;
        }
    }
}
