using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KarlsonMP
{
    class OnlinePlayer
    {
        public OnlinePlayer(int _id)
        {
            enemy = PrefabInstancer.CreateEnemy();
            ClientSend.GetPlayerInfo(_id);
        }

        public GameObject enemy;
        public string username;
        public Vector3 pos;
        public float rot;
    }
}
