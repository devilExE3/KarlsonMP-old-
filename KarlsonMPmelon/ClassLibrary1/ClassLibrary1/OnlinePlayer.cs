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
            id = _id;
            enemy = PrefabInstancer.CreateEnemy();
            ClientSend.GetPlayerInfo(id);
        }

        public GameObject enemy;
        public string username;
        public Vector3 pos;
        public float rot;
        public int id;
        public GameObject activeGun = null;
        public GameObject grappleLine = null;
        public Vector3 grapplePos = Vector3.zero;
    }
}
