using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KarlsonMPclient
{
    class InstanceMaker
    {
        public static void LoadInstances()
        {
            foreach(GameObject go in Object.FindObjectsOfType<GameObject>())
            {
                if(go.name == "Enemy")
                {
                    enemy = UnityEngine.Object.Instantiate(go);
                    UnityEngine.Object.DontDestroyOnLoad(enemy);
                }
            }
        }

        public static GameObject enemy;
    }
}
