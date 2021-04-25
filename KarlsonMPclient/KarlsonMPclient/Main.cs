using modloader;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Net;

namespace KarlsonMPclient
{
    [ModInfo("KarlsonMP", "devilExE, Xiloe", "a1.0")]
    public class Main : IMod
    {
        public void Start()
        {
			SceneManager.sceneLoaded += OnSceneLoaded;
			if(Client.instance == null)
            {
				Client.instance = new Client();
				Client.instance.Start();
            }
        }

		void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			
		}

		public void OnGUI()
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
				if (GUI.Button(new Rect(Screen.width / 2 + 70f, Screen.height - 40f, 80f, 40f), "Connect"))
				{
					// parse ip
					if (ipField.Split(':').Length != 2)
						return;
					int port;
					if (!int.TryParse(ipField.Split(':')[1], out port))
						return;
					Client.instance.ip = Dns.GetHostAddresses(ipField.Split(':')[0])[0].MapToIPv4().ToString();
					Client.instance.port = port;
					Client.instance.username = usernameField;
					Client.instance.ConnectToServer();
				}
			}
		}

		private string usernameField = "";
		private string ipField = "";
		private bool historyShown = false;

		public void Update(float deltaTime) {
			ThreadManager.UpdateMain();
		}
        public void FixedUpdate(float fixedDeltaTime) { }
        public void OnApplicationQuit()
		{
			Client.instance.Disconnect();
		}
    }
}
