using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace KarlsonMPserver
{
    class Program
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(logFile, message + '\n');
        }

        public class ServerConfig
        {
            public ServerConfig(string configFile)
            {
                int line = 1;
                foreach(var set in File.ReadAllText(configFile).Split('\n'))
                {
                    string[] options = set.Split('=');
                    if(options.Length != 2)
                    {
                        Log("[CONFIG] Failed to load option " + set + " at line " + line);
                        continue;
                    }
                    if (!validOptions.Contains(options[0]))
                    {
                        Log("[CONFIG] Undentified key " + options[0] + " at line " + line);
                        continue;
                    }
                    switch (options[0])
                    {
                        case "port":
                            {
                                if (!int.TryParse(options[1], out int num))
                                {
                                    Log("[CONFIG] Failed to parse int " + options[1] + " at line " + line);
                                    break;
                                }
                                port = num;
                                break;
                            }
                        case "maxplayers":
                            {
                                if (!int.TryParse(options[1], out int num))
                                {
                                    Log("[CONFIG] Failed to parse int " + options[1] + " at line " + line);
                                    break;
                                }
                                maxplayers = num;
                                break;
                            }
                        case "motd":
                            motd = options[1];
                            break;
                        case "tps":
                            {
                                if (!int.TryParse(options[1], out int num))
                                {
                                    Log("[CONFIG] Failed to parse int " + options[1] + " at line " + line);
                                    break;
                                }
                                tps = num;
                                break;
                            }
                        case "rcon-password":
                            rconpassword = options[1];
                            break;
                        case "iplimit":
                            {
                                if (!int.TryParse(options[1], out int num))
                                {
                                    Log("[CONFIG] Failed to parse int " + options[1] + " at line " + line);
                                    break;
                                }
                                iplimit = num;
                                break;
                            }
                        default:
                            Log("[CONFIG] Failed to load option " + set + " at line " + line);
                            break;
                    }
                    line++;
                }
            }

            private readonly static string[] validOptions = new string[]
            {
                "port", "maxplayers", "motd", "tps", "rcon-password", "iplimit"
            };

            public readonly int port            = 11337;
            public readonly int maxplayers      = 15;
            public readonly string motd         = "changeme";
            public readonly int tps             = 30;
            public readonly string rconpassword = "changeme";
            public readonly int iplimit         = 2;
        }

        private static bool isRunning = false;

        public static ServerConfig config;

        public readonly static string logFile = Path.Combine(AppContext.BaseDirectory, "log");

        static void Main()
        {
            if (File.Exists(logFile))
                File.Delete(logFile);
            Console.Title = "KarlsonMP Server";
            // load settings
            string configFile = Path.Combine(AppContext.BaseDirectory, "config");
            if (!File.Exists(configFile))
            {
                File.AppendAllText(configFile, "port=11337\n");
                File.AppendAllText(configFile, "maxplayers=50\n");
                File.AppendAllText(configFile, "motd=Official main server\n");
                File.AppendAllText(configFile, "tps=30\n");
                File.AppendAllText(configFile, "rcon-password=changeme\n");
                File.AppendAllText(configFile, "iplimit=2\n");
            }
            config = new(configFile);
            if(config.rconpassword == "changeme")
            {
                Log("Please change the RCON password");
                return;
            }
            isRunning = true;
            Thread mainThread = new(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start(config.port, config.maxplayers);
            Thread pingThread = new(new ThreadStart(PingThread));
            pingThread.Start();
        }

        private static void MainThread()
        {
            Log($"Main thread started. Running at {config.tps}tps");
            DateTime _nextLoop = DateTime.Now;
            while(isRunning)
            {
                while(_nextLoop < DateTime.Now)
                {
                    ThreadManager.UpdateMain();
                    _nextLoop = _nextLoop.AddMilliseconds(1000 / config.tps);
                    if (_nextLoop > DateTime.Now)
                        Thread.Sleep(_nextLoop - DateTime.Now);
                }
            }
        }

        private static void PingServer()
        {
            using TcpClient client = new("api.xiloe.fr", 80);
            using StreamWriter writer = new(client.GetStream());
            writer.AutoFlush = true;
            writer.WriteLine($"GET /karlson/status/index.php?server={Server.Branch}&port={Server.Port}&version={Constants.version}&players={Server.OnlinePlayers()}&maxplayers={Server.MaxPlayers} HTTP/1.1");
            writer.WriteLine("Host: api.xiloe.fr:80");
            writer.WriteLine("Connection: close");
            writer.WriteLine("");
            writer.WriteLine("");
        }

        private static void PingThread()
        {
            while(isRunning)
            {
                ServerSend.PingAll();
                ServerSend.ScoreboardAll();
                PingServer();

                // this kinda runs every second, which is useful ngl

                for(int i = 1; i <= Server.MaxPlayers; i++)
                {
                    if(Server.clients[i].tcp.socket != null && Server.clients[i].player == null && (DateTime.Now - Server.clients[i].tcp.connectedTimeStamp).TotalMilliseconds >= 5000)
                    {
                        ServerSend.Chat(i, "<color=red>[SERVER] Didn't respond to welcome packet in time, disconnected.</color>");
                        Log($"ID {i} timed out.");
                        Server.clients[i].tcp.Disconnect();
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
