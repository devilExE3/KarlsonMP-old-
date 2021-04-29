using HarmonyLib;

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
