using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace KarlsonMP
{
    [HarmonyPatch(typeof(Debug), "Update")]
    public static class Debug_Update
    {
        public static bool Prefix(Debug __instance)
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                if (__instance.console.isActiveAndEnabled)
                    __instance.GetType().GetMethod("CloseConsole", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, Array.Empty<object>());
                else
                    __instance.GetType().GetMethod("OpenConsole", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, Array.Empty<object>());
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Enemy), "LateUpdate")]
    public static class Enemy_LateUpdate
    {
        public static bool Prefix(Enemy __instance)
        {
            if (__instance.IsDead())
                return true;
            return __instance.GetComponent<NavMeshAgent>().enabled;
        }
    }
}
