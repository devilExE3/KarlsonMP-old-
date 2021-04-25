using UnityEngine;
using System;

namespace KarlsonMPclient
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
    }
}
