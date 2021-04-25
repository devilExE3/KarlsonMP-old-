using System;
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
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start(11337, 10);
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
    }
}
