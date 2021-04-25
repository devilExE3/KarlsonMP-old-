using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace KarlsonMPclient
{
    class HarmonyHooks
    {
        [HarmonyPatch(typeof(Enemy), "LateUpdate")]
        public class Enemy_LateUpdate
        {
            static bool Prefix(Enemy __instance)
            {
                if (__instance.IsDead())
                    return true;
                return __instance.GetComponent<NavMeshAgent>().enabled; // LateUpdate cancels animations
            }
        }
        
    }
}
