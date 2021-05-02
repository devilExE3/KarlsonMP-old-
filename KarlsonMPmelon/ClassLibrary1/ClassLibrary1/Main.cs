using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMP
{
    public class Main : MelonMod
    {
        public class KarlsonMPversion
        {
            public KarlsonMPversion(string _ver, string _link, string _chanelog)
            {
                version = _ver;
                link = _link;
                changelog = _chanelog;
            }
            public string version;
            public string link;
            public string changelog;
        }

        private bool OutOfDate(string newVer, string oldVer)
        {
            int[] nVer = Array.ConvertAll(newVer.Split('.'), s => int.Parse(s));
            int[] oVer = Array.ConvertAll(oldVer.Split('.'), s => int.Parse(s));
            if (nVer[0] <= oVer[0])
                return false;
            if (nVer[1] <= oVer[1])
                return false;
            if (nVer[2] <= oVer[2])
                return false; // returns false also if the 'newVer' < 'oldVer'
            return true;
        }

        private static bool needToUpdate = false;
        private static string updateLink = string.Empty;
        private static string oldVer = string.Empty;
        private static string newVer = string.Empty;
        private static string changelog = string.Empty;

        private static string GetUpdateInfo()
        {
            using(TcpClient client = new TcpClient("api.xiloe.fr", 80))
            using (StreamWriter writer = new StreamWriter(client.GetStream()))
            using (StreamReader reader = new StreamReader(client.GetStream()))
            {
                writer.AutoFlush = true;
                writer.WriteLine("GET /karlson/version.json HTTP/1.1");
                writer.WriteLine("Host: api.xiloe.fr:80");
                writer.WriteLine("Connection: close");
                writer.WriteLine("Content-Type: application/json");
                writer.WriteLine("");

                string str = reader.ReadToEnd();
                return str.Substring(str.IndexOf('{'));
            }
        }

        private void CheckForUpdates()
        {
            string json = GetUpdateInfo();
            KarlsonMPversion ver = JsonUtility.FromJson<KarlsonMPversion>(json);
            // get current version from assembly info, so we only need to change it in one place :D
            object[] customAttributes = GetType().Assembly.GetCustomAttributes(false);
            string oldVer = "0.0.0"; // should always return true in OutOfDate if foreach failes
            foreach (var ca in customAttributes)
            {
                if (ca.GetType() != typeof(MelonInfoAttribute))
                    continue;
                oldVer = ((MelonInfoAttribute)ca).Version;
            }
            if (OutOfDate(ver.version, oldVer))
            {
                needToUpdate = true;
                updateLink = ver.link;
                Main.oldVer = oldVer;
                newVer = ver.version;
                changelog = ver.changelog;
            }
        }

        public override void OnApplicationLateStart()
        {
            if (!Environment.GetCommandLineArgs().Contains("-disable-updater"))
                CheckForUpdates();
            if (Client.instance == null)
            {
                Client.instance = new Client();
                Client.instance.Start();
            }
            usernameField = DataSave.Load(); // returns username, and loads ip history
            if (DataSave.IpHistory.Count > 0)
                ipField = DataSave.IpHistory.Last(); // load last connected to ip
            SceneManager.sceneLoaded += OnSceneLoaded;
            ClientHandle.scoreboard = new ClientHandle.Scoreboard();

            // load scoreboard tables
            TperLevel = TableGui.TableView(new Rect(20f, 50f, 250f, 475f), "Players Per Level",
                TableGui.TableStyle.window |
                TableGui.TableStyle.dragWindow);
            TperLevel.SetHeader("Level Name\tCount", new float[] { 210f, 40f });
            TperLevel.items.Add("Tutorial\t0");
            TperLevel.items.Add("Sandbox 0\t0");
            TperLevel.items.Add("Sandbox 1\t0");
            TperLevel.items.Add("Sandbox 2\t0");
            TperLevel.items.Add("Escape 0\t0");
            TperLevel.items.Add("Escape 1\t0");
            TperLevel.items.Add("Escape 2\t0");
            TperLevel.items.Add("Escape 3\t0");
            TperLevel.items.Add("Sky 0\t0");
            TperLevel.items.Add("Sky 1\t0");
            TperLevel.items.Add("Sky 2\t0");
            TplayerList = TableGui.TableView(new Rect(350f, 50f, 800f, 400f), "Online Player List",
                TableGui.TableStyle.window |
                TableGui.TableStyle.dragWindow |
                TableGui.TableStyle.alterBg |
                TableGui.TableStyle.lineColumn);
            TplayerList.SetHeader("ID\tName\tScene\tPing", new float[]{
                800f * 5/100,
                800f * 50/100,
                800f * 30/100,
                800f * 15/100,
            });
        }

        private void DownloadNewFile()
        {
            File.Delete(Path.Combine(Application.dataPath, "..", "Mods", "KarlsonMP.dll"));
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(new Uri(updateLink), Path.Combine(Application.dataPath, "..", "Mods", "KarlsonMP.dll"));
                Process.Start(Path.Combine(Application.dataPath, "..", "Karlson.exe"));
                Application.Quit(0);
            }
        }

        private static string oldScene = "";
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name == "MainMenu" && !PrefabInstancer.Initialized)
            {
                SceneManager.LoadScene("0Tutorial");
                return;
            }
            if (scene.name == "0Tutorial" && !PrefabInstancer.Initialized)
            {
                PrefabInstancer.LoadPrefabs();
                return;
            }
            if (Client.instance.isConnected)
                Client.instance.players.Clear();
            // we only need to send to the server entering/leaving levels
            if (oldScene != "" && oldScene != "Initialize" && oldScene != "MainMenu" && Client.instance.isConnected)
                ClientSend.LeaveScene(oldScene);
            if (scene.name != "Initialize" && scene.name != "MainMenu" && Client.instance.isConnected)
                ClientSend.EnterScene(scene.name);
            oldScene = scene.name;
        }

        private static string usernameField = "";
        private static string ipField = "";
        private static bool historyShown = false;
        private static Vector2 ipHistoryScroll = Vector2.zero;
        private static Rect popupWindow = new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 100f, 300f, 200f);
        private static int progress = 0;
        private bool isChatOpened = false;
        public static bool isChatEnabled { get; private set; } = true;
        private string chatField = "";
        private static string chat = "";
        private bool showPing = true;
        private bool isScoreboardOpened = false;

        private TableView TperLevel, TplayerList, TsessionPbs, Tgamerules;
        public override void OnGUI()
        {
            if(needToUpdate)
            {
                GUI.Box(popupWindow, "KarlsonMP Update");
                GUIStyle middle = new GUIStyle();
                middle.normal.textColor = Color.white;
                middle.alignment = TextAnchor.UpperCenter;
                GUI.BeginGroup(popupWindow);
                if(progress == 0)
                    GUI.Label(new Rect(3f, 20f, 300f, 380f), "KarlsonMP just got updated! YAY\nCurrent version: " + oldVer + " -> New version: " + newVer + "\n\nChangelog:\n" + changelog);
                else
                    GUI.Label(new Rect(0f, 20f, 300f, 380f), "\n\nUpdating, please do not exit the game..\n(your install will be corrupted otherwise)", middle);
                //GUI.Box(new Rect(0f, 360f, 300f * progress, 10f), "", whiteBox); // progress bar to show how fast 23kb of data downloaded lmao
                if (GUI.Button(new Rect(150f, 170f, 150f, 30f), "Cancel - You won't be\nable play multiplayer"))
                    needToUpdate = false;
                if (GUI.Button(new Rect(0f, 170f, 150f, 30f), "Update"))
                {
                    progress = 1;
                    DownloadNewFile();
                }
                GUI.EndGroup();
            }
            if (SceneManager.GetActiveScene().name == "MainMenu" || UIManger.Instance.deadUI.activeSelf || UIManger.Instance.winUI.activeSelf)
			{
				GUI.Box(new Rect(Screen.width / 2 - 150f, Screen.height - 40f, 300f, 40f), "");
				GUI.Label(new Rect(Screen.width / 2 - 150f, Screen.height - 40f, 300f, 40f), "Username");
				usernameField = GUI.TextField(new Rect(Screen.width / 2 - 85f, Screen.height - 40f, 155f, 20f), usernameField);
                usernameField = usernameField.Substring(0, Math.Min(32, usernameField.Length));
				GUI.Label(new Rect(Screen.width / 2 - 150f, Screen.height - 20f, 300f, 40f), "IP");
				ipField = GUI.TextField(new Rect(Screen.width / 2 - 130f, Screen.height - 20f, 180f, 20f), ipField);
				if (GUI.Button(new Rect(Screen.width / 2 + 50f, Screen.height - 20f, 20f, 20f), "^"))
				{
					historyShown = !historyShown;
				}
				string connButtonStr = "Connect"; // button doesn't work for some reason 
				if (Client.instance.isConnected)
					connButtonStr = "Leave";
				if (Client.instance.isConnecting)
					connButtonStr = "Cancel";
				if (GUI.Button(new Rect(Screen.width / 2 + 70f, Screen.height - 40f, 80f, 40f), connButtonStr))
				{
					if(Client.instance.isConnected)
                    {
						Client.instance.tcp.Disconnect();
						return;
                    }
					if(Client.instance.isConnecting)
                    {
						Client.instance.Disconnect();
                        Client.instance.isConnected = false;
                        Client.instance.isConnecting = false;
                        if(Client.instance.connectionRetryToken != null)
                        {
                            MelonCoroutines.Stop(Client.instance.connectionRetryToken);
                            Client.instance.connectionRetryToken = null;
                        }
                        return;
                    }
                    // parse ip
                    if (!ipField.Contains(':'))
                        ipField += ":11337"; // default port
					if (ipField.Split(':').Length != 2)
						return;
                    if (!int.TryParse(ipField.Split(':')[1], out int port))
                        return;
                    Client.instance.ip = Dns.GetHostAddresses(ipField.Split(':')[0])[0].MapToIPv4().ToString();
					Client.instance.port = port;
					Client.instance.username = usernameField;
					Client.instance.ConnectToServer();
                    DataSave.AddToList(ipField);
                    DataSave.Save(usernameField);
				}
				if(historyShown)
                {
                    GUI.Box(new Rect(Screen.width / 2 - 130f, Screen.height - 190f, 216f, 150f), "");
                    ipHistoryScroll = GUI.BeginScrollView(new Rect(Screen.width / 2 - 130f, Screen.height - 190f, 216f, 150f), ipHistoryScroll, new Rect(0f, 0f, 200f, DataSave.IpHistory.Count * 50f), false, true);
                    int i = 0;
                    GUIStyle ipButton = new GUIStyle();
                    ipButton.normal.background = Texture2D.blackTexture;
                    ipButton.alignment = TextAnchor.UpperLeft;
                    ipButton.fontSize = 20;
                    foreach(string _ip in DataSave.IpHistory.ToArray().Reverse())
                    {
                        string color = "<color=white>";
                        if (ipField == _ip)
                            color = "<color=green>";
                        if(GUI.Button(new Rect(0f, i * 50f, 140f, 40f), color + _ip.Split(':')[0] + "\n:" + _ip.Split(':')[1] + "</color>", ipButton))
                            ipField = _ip;
                        if (GUI.Button(new Rect(140f, i * 50f, 60f, 20f), "<color=red>Remove</color>"))
                            DataSave.Remove(DataSave.IpHistory.Count - 1 - i); // we fliped the list when showing it, we need to shout count - 1 - i | count - 1 => last index, - i => flips it, last being first, first being last
                        if (GUI.Button(new Rect(140f, i * 50f + 20f, 30f, 20f), "▲"))
                            DataSave.MoveUp(DataSave.IpHistory.Count - 1 - i); // see comment above
                        if (GUI.Button(new Rect(170f, i * 50f + 20f, 30f, 20f), "▼"))
                            DataSave.MoveDown(DataSave.IpHistory.Count - 1 - i); // see comment above
                        i++;
                    }
                    GUI.EndScrollView(true);
                }
			}
			if(Client.instance.isConnected && SceneManager.GetActiveScene().buildIndex >= 2)
            {
                foreach(OnlinePlayer player in Client.instance.players.Values)
                {
                    string text = "(" + player.id + ") " + player.username;
                    
                    Vector3 pos = Camera.main.WorldToScreenPoint(player.pos + new Vector3(0f, 2.0f, 0f));
                    if (Vector3.Distance(player.pos, PlayerMovement.Instance.transform.position) >= 150f)
                        continue; // player is too far
                    if (pos.z < 0)
                        continue; // point is behind our camera
                    Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
                    textSize.x += 10f;
                    GUI.Box(new Rect(pos.x - textSize.x / 2, (Screen.height - pos.y) - textSize.y / 2, textSize.x, textSize.y), text);
                }
            }
            if (isChatEnabled)
                GUI.Label(new Rect(2f, 20f, Screen.width, Screen.height), chat);
            if (isChatOpened)
            {
                GUI.SetNextControlName("chatfield");
                chatField = GUI.TextArea(new Rect(0f, 0f, 400f, 20f), chatField);
                chatField = chatField.Substring(0, Math.Min(128, chatField.Length)); // woopsies
                GUI.FocusControl("chatfield");
                if (chatField.Contains("\n")) // pressed return
                {
                    string message = chatField.Replace("\n", "").Trim();
                    if (message.StartsWith("/"))
                    {
                        bool success = false; // we don't want to block every text with a `/`, we might add commands on the server as well
                        if (message.ToLower() == "/c" || message.ToLower() == "/cursor")
                        {
                            if (Cursor.visible)
                            {
                                Cursor.visible = false;
                                Cursor.lockState = CursorLockMode.Locked;
                            }
                            else
                            {
                                Cursor.visible = true;
                                Cursor.lockState = CursorLockMode.None;
                            }
                            success = true;
                        }
                        if (message.ToLower() == "/cc" || message.ToLower() == "/clearchat")
                        {
                            chat = "";
                            success = true;
                        }
                        if (message.ToLower() == "/chat")
                        {
                            isChatEnabled = !isChatEnabled;
                            success = true;
                        }
                        if (message.ToLower() == "/ping")
                        {
                            showPing = !showPing;
                            success = true;
                        }
                        if(message.ToLower() == "/grap")
                        {
                            GameObject go = PrefabInstancer.CreateGrappler();
                            go.transform.position = PlayerMovement.Instance.transform.position;
                            success = true;
                        }
                        if (success)
                        {
                            isChatOpened = false;
                            return;
                        }
                    }
                    if (message.Length > 0)
                        ClientSend.ChatMsg(message);
                    isChatOpened = false;
                }
                if (chatField.Contains("`"))
                    isChatOpened = false;
            }
            if (Client.instance.isConnected && showPing)
            {
                string pingStr = "Ping: " + Client.instance.ping + "ms";
                Vector2 pingSize = GUI.skin.label.CalcSize(new GUIContent(pingStr));
                GUI.Box(new Rect(0f, Screen.height - pingSize.y, pingSize.x + 8f, pingSize.y), "");
                GUI.Label(new Rect(4f, Screen.height - pingSize.y, pingSize.x, pingSize.y), pingStr); // wierd, i know
            }

            if(Client.instance.isConnected && isScoreboardOpened)
            {
                GUI.Box(new Rect(-5f, -5f, Screen.width + 10f, Screen.height + 10f), $"\n<size=30>[{ClientHandle.scoreboard.onlinePlayers}/{ClientHandle.scoreboard.maxPlayers}] - {ClientHandle.scoreboard.motd}</size>");
                for(int i = 0; i < 11; i++)
                    TperLevel.items[i] = sceneNames[i] + '\t' + ClientHandle.scoreboard.perLevel[i];
                TperLevel.Draw();

                TplayerList.items.Clear();
                foreach(var player in ClientHandle.scoreboard.players)
                    TplayerList.items.Add(player.id + "\t" + player.username + "\t" + sceneNames[allowedSceneNames.ToList().IndexOf(player.scene)] + "\t" + player.ping + "ms");
                TplayerList.Draw();

                /*Tgamerules.Draw();
                TsessionPbs.Draw();*/
            }
        }

        public static void AddToChat(string str)
        {
            while (chat.Split('\n').Length > 30)
                chat = chat.Substring(chat.IndexOf('\n') + 1); // limit to 30 lines
            chat += str + "\n";
        }

        private static bool firstWinFrame = false;

        private bool noclip = false;

        public override void OnUpdate()
        {
            ThreadManager.UpdateMain();
            PosSender.Update();
            if (!isChatOpened && Input.GetKeyDown(KeyCode.T))
            {
                isChatOpened = true;
                chatField = "";
            }
            if (UIManger.Instance)
            {
                if (UIManger.Instance.winUI.activeSelf && !firstWinFrame)
                {
                    firstWinFrame = true;
                    ClientSend.FinishLevel(Mathf.FloorToInt(Timer.Instance.GetTimer() * 1000f));
                }
                if (!UIManger.Instance.winUI.activeSelf && firstWinFrame)
                    firstWinFrame = false;
            }
            if (Client.instance.isConnected && Input.GetKeyDown(KeyCode.Tab))
                isScoreboardOpened = !isScoreboardOpened;
            if (Input.GetKeyDown(KeyCode.V))
            {
                noclip = !noclip;
                if (noclip)
                {
                    PlayerMovement.Instance.GetComponent<Collider>().enabled = false;
                    PlayerMovement.Instance.GetComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    PlayerMovement.Instance.GetComponent<Collider>().enabled = true;
                    PlayerMovement.Instance.GetComponent<Rigidbody>().isKinematic = false;
                }
            }
            if (noclip)
            {
                float speed = 0.7f;
                if (Input.GetKey(KeyCode.LeftShift))
                    speed = 1.2f;
                if (Input.GetKey(KeyCode.W))
                    PlayerMovement.Instance.transform.position += Camera.main.transform.forward * speed;
                if (Input.GetKey(KeyCode.S))
                    PlayerMovement.Instance.transform.position -= Camera.main.transform.forward * speed;
                if (Input.GetKey(KeyCode.A))
                    PlayerMovement.Instance.transform.position -= Camera.main.transform.right * speed;
                if (Input.GetKey(KeyCode.D))
                    PlayerMovement.Instance.transform.position += Camera.main.transform.right * speed;
            }
        }
        public override void OnApplicationQuit()
        {
            Client.instance.Disconnect();
        }

        public static readonly string[] allowedSceneNames = new string[]
        {
            "0Tutorial",
            "1Sandbox0", "2Sandbox1", "3Sandbox2",
            "4Escape0", "5Escape1", "6Escape2", "7Escape3",
            "8Sky0", "9Sky1", "10Sky2"
        };
        public static readonly string[] sceneNames = new string[]
        {
            "Tutorial",
            "Sandbox 0", "Sandbox 1", "Sandbox 2",
            "Escape 0", "Escape 1", "Escape 2", "Escape 3",
            "Sky 0", "Sky 1", "Sky 2"
        };
    }
}
