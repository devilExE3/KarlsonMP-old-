using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class Player
    {
        public Player(int _id, string _username)
        {
            id = _id;
            username = _username;
        }
        public readonly int id;
        public readonly string username;
        public string color = "white";
        public string scene;
        public DateTime lastPing = DateTime.MinValue;
        public int ping = -1;
        public int activeGun = 0;
    }
}
