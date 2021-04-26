using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMP
{
    public class Main : MelonMod
    {
        public override void OnApplicationLateStart()
        {
            if (Client.instance == null)
            {
                Client.instance = new Client();
                Client.instance.Start();
            }
            usernameField = DataSave.Load(); // returns username, and loads ip history
            if (DataSave.IpHistory.Count > 0)
                ipField = DataSave.IpHistory.Last(); // load last connected to ip
            SceneManager.sceneLoaded += OnSceneLoaded;
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
        public override void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu" || UIManger.Instance.deadUI.activeSelf || UIManger.Instance.winUI.activeSelf)
			{
				GUI.Box(new Rect(Screen.width / 2 - 150f, Screen.height - 40f, 300f, 40f), "");
				GUI.Label(new Rect(Screen.width / 2 - 150f, Screen.height - 40f, 300f, 40f), "Username");
				usernameField = GUI.TextField(new Rect(Screen.width / 2 - 85f, Screen.height - 40f, 155f, 20f), usernameField);
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
				}
				if(historyShown)
                {
					// TODO: add history list (I already have something like this, just need to port it)
                }
			}
			if(Client.instance.isConnected && SceneManager.GetActiveScene().buildIndex >= 2)
            {
                foreach(OnlinePlayer player in Client.instance.players.Values)
                {
                    GUI.Box(new Rect(Camera.main.WorldToScreenPoint(player.pos).x, Camera.main.WorldToScreenPoint(player.pos).y, 100f, 50f), "(" + player.id + ") " + player.username);
                }
            }
        }

        public override void OnUpdate()
        {
            ThreadManager.UpdateMain();
            PosSender.Update();
        }
        public override void OnApplicationQuit()
        {
            Client.instance.Disconnect();
        }
    }
}
