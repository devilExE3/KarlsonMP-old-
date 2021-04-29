using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace KarlsonMP
{
    class Client
    {
        public static Client instance = null;
        const int dataBufferSize = 4096;

        public string ip = "127.0.0.1";
        public int port = 11337;
        public int myId = 0;
        public string username = "";
        public TCP tcp;

        public bool isConnected = false;
        public bool isConnecting = false;

        public int ping = -1;

        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        public Dictionary<int, OnlinePlayer> players = new Dictionary<int, OnlinePlayer>();

        public void Start()
        {
            tcp = new TCP();
        }

        public void ConnectToServer()
        {
            InitializeClientData();
            tcp.Connect();
        }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };
                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
                instance.isConnecting = true;
                // TODO: add timer to check if we connected to a server, and disconnect if not
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                socket.EndConnect(ar);
                if (!socket.Connected)
                    // TODO: reconnect to the server, increasing a counter that cancels this process when it reaches 10
                    return;
                stream = socket.GetStream();
                receivedData = new Packet();
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                if (stream == null || receiveBuffer == null || socket == null)
                {
                    Disconnect();
                    return;
                }
                try
                {
                    int _byteLength = stream.EndRead(ar);
                    if (_byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);
                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Main.AddToChat($"<color=red>Error while receiving data: {_ex}</color>".Replace("\n", "</color>\n<color=red>"));
                    Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;
                receivedData.SetBytes(_data);
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                        return true;
                }
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        Packet _packet = new Packet(_packetBytes);
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                            return true;
                    }
                }
                if (_packetLength <= 1)
                    return true;
                return false;
            }

            public void SendData(byte[] _data)
            {
                try
                {
                    if (socket == null || stream == null)
                        return;
                    stream.BeginWrite(_data, 0, _data.Length, null, null);
                }
                catch (Exception _ex)
                {
                    Main.AddToChat($"<color=red>Error while sending data {_ex}</color>".Replace("\n", "</color>\n<color=red>"));
                }
            }

            public void Disconnect()
            {
                if (socket == null)
                    return;
                instance.Disconnect();
                stream = null;
                receiveBuffer = null;
                receivedData = null;
                socket = null;
            }
        }

        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { (int)PacketID.welcome,        ClientHandle.Welcome },
                { (int)PacketID.enterScene,     ClientHandle.EnterScene },
                { (int)PacketID.leaveScene,     ClientHandle.LeaveScene },
                { (int)PacketID.clientsInScene, ClientHandle.ClientsInScene },
                { (int)PacketID.clientInfo,     ClientHandle.ClientInfo },
                { (int)PacketID.clientMove,     ClientHandle.ClientMove },
                { (int)PacketID.chat,           ClientHandle.Chat },
                { (int)PacketID.ping,           ClientHandle.Ping },
            };
        }

        public void Disconnect()
        {
            if (!isConnected)
                return;
            isConnected = false;
            isConnecting = false;
            tcp.socket.Close();
            Main.AddToChat("Disconnected from the server");
        }
    }
}
