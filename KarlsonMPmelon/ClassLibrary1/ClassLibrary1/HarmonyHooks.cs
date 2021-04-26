using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;

namespace KarlsonMP
{
    class HarmonyHooks
    {
        public static bool Enemy_LateUpdate(Enemy __instance)
        {
            if (__instance.IsDead())
                return true;
            return __instance.GetComponent<NavMeshAgent>().enabled; // LateUpdate cancels animations
        }
    }
}
