using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;

namespace KarlsonMP
{
    [HarmonyPatch(typeof(Enemy), "LateUpdate")]
    public class Enemy_LateUpdate
    {
        public static bool Prefix(Enemy __instance)
        {
            /*if (__instance.IsDead())
                return true;
            return __instance.GetComponent<NavMeshAgent>().enabled;*/
            return true;
        }
    }
}
