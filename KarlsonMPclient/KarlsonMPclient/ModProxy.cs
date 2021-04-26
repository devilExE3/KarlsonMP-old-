using modloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KarlsonMPclient
{
    [ModInfo("me.devilexe.karlsonmp", "KarlsonMP", "devilExE", "alpha-1.1")]
    class ModProxy : IMod
    {
        public void FixedUpdate(float fixedDeltaTime)
        {
            //Main.FixedUpdate(fixedDeltaTime);
        }

        public void OnApplicationQuit()
        {
            //Main.OnApplicationQuit();
        }

        public void OnGUI()
        {
            //Main.OnGUI();
            GUI.Label(new Rect(0f, 0f, 100f, 100f), test);
            GUI.Label(new Rect(0f, 20f, 100f, 100f), Main.test);
        }

        public void Start()
        {
            //Main.instance = new Main();
            //Main.Start();
        }

        public void Update(float deltaTime)
        {
            //Main.Update(deltaTime);
        }
    }
}
