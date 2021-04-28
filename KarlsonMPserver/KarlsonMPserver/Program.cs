// define logs (they can be removed)
#define LOG_SCENE_ENTRY

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace KarlsonMPserver
{
    class Program
    {
        private static bool isRunning = false;

        static void Main()
        {
            Console.Title = "KarlsonMP Server";
            isRunning = true;
            Thread mainThread = new(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start(11337, 10);
            Thread pingThread = new(new ThreadStart(PingThread));
            pingThread.Start();
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TPS}tps");
            DateTime _nextLoop = DateTime.Now;
            while(isRunning)
            {
                while(_nextLoop < DateTime.Now)
                {
                    ThreadManager.UpdateMain();
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                    if (_nextLoop > DateTime.Now)
                        Thread.Sleep(_nextLoop - DateTime.Now);
                }
            }
        }

        private static void PingServer()
        {
            using (TcpClient client = new("api.xiloe.fr", 80))
            using (StreamWriter writer = new(client.GetStream()))
            {
                writer.AutoFlush = true;
                writer.WriteLine("GET /karlson/status/index.php?server=1 HTTP/1.1");
                writer.WriteLine("Host: api.xiloe.fr:80");
                writer.WriteLine("Connection: close");
                writer.WriteLine("");
                writer.WriteLine("");
            }
        }

        private static void PingThread()
        {
            while(isRunning)
            {
                ServerSend.PingAll();
                PingServer();
                Thread.Sleep(1000);
            }
        }
    }
}
