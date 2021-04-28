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
            if(nVer[0] <= oVer[0] && nVer[1] <= oVer[1] && nVer[2] <= oVer[2])
                return false; // returns false also if the 'newVer' < 'oldVer'
            return true;
        }

        private static bool needToUpdate = false;
        private static string updateLink = string.Empty;
        private static string oldVer = string.Empty;
        private static string newVer = string.Empty;
        private static string changelog = string.Empty;
        public static HarmonyLib.Harmony Harmony { get; private set; }

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
            Harmony = new HarmonyLib.Harmony("me.devilexe.karlsonmp");
            //Harmony.Patch(typeof(Enemy).GetMethod("LateUpdate"), prefix: new HarmonyLib.HarmonyMethod(typeof(HarmonyHooks).GetMethod("Enemy_LateUpdate")));
            Harmony.PatchAll();
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
        private bool isChatEnabled = true;
        private string chatField = "";
        private static string chat = "";
        private bool showPing = true;
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
						// TODO: cancel the timeout timer
						Client.instance.Disconnect();
						return;
                    }
					// parse ip
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
                        bool succes = false; // we don't want to block every text with a `/`, we might add commands on the server as well
                        if (message.ToLower() == "/c")
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
                            succes = true;
                        }
                        if(message.ToLower() == "/cc")
                        {
                            chat = "";
                            succes = true;
                        }
                        if(message.ToLower() == "/tog")
                        {
                            isChatEnabled = !isChatEnabled;
                            succes = true;
                        }
                        if(message.ToLower() == "/ping")
                        {
                            showPing = !showPing;
                            succes = true;
                        }
                        if(succes)
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
        }

        public static void AddToChat(string str)
        {
            while (chat.Split('\n').Length > 30)
                chat = chat.Substring(chat.IndexOf('\n') + 1); // limit to 30 lines
            chat += str + "\n";
        }

        private static bool firstWinFrame = false;

        public override void OnUpdate()
        {
            ThreadManager.UpdateMain();
            PosSender.Update();
            if (!isChatOpened && (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Return)))
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
        }
        public override void OnApplicationQuit()
        {
            Client.instance.Disconnect();
        }
    }
}
