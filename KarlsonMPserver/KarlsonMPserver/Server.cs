using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KarlsonMPserver
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new();
        private static TcpListener listener;
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        public static void Start(int _port, int _maxPlayers)
        {
            Port = _port;
            MaxPlayers = _maxPlayers;
            InitializeServerData();
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            listener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Server listening for connections on *:{Port}");
        }
        private static void TCPConnectCallback(IAsyncResult ar)
        {
            TcpClient _client = listener.EndAcceptTcpClient(ar);
            listener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}");
            for(int i = 1; i <= MaxPlayers; i++)
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            _client.Close();
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
                clients.Add(i, new Client(i));
            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { (int)PacketID.welcome, ServerHandle.WelcomeReceived },
            };
        }
    }
}
