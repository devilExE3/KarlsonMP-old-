﻿using System;
using UnityEngine;
using UnityEngine.AI;
using HarmonyLib;

namespace KarlsonMP
{
    [HarmonyPatch(typeof(Enemy), "LateUpdate")]
    public static class Enemy_LateUpdate
    {
        public static bool Prefix(Enemy __instance)
        {
            if (__instance.IsDead())
                return false;
            return __instance.GetComponent<NavMeshAgent>().enabled; // LateUpdate cancels animations
        }
    }

    [HarmonyPatch(typeof(Debug), "Update")]
    public static class Debug_Update
    {
        public static bool Prefix(Debug __instance)
        {
            __instance.GetType().GetMethod("Fps").Invoke(__instance, Array.Empty<object>());
            if (Input.GetKeyDown(KeyCode.Tilde))
            {
                if (__instance.console.isActiveAndEnabled)
                    __instance.GetType().GetMethod("CloseConsole").Invoke(__instance, Array.Empty<object>());
                else
                    __instance.GetType().GetMethod("OpenConsole").Invoke(__instance, Array.Empty<object>());
            }
            return false;
        }
    }

    /*[HarmonyPatch]
    class Enemy_LateUpdate
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(Enemy).GetMethod("LateUpdate");
        }
        static bool Prefix(Enemy __instance)
        {
            if (__instance.IsDead())
                return false;
            return __instance.GetComponent<NavMeshAgent>().enabled; // LateUpdate cancels animations
        }
    }

    [HarmonyPatch]
    class Debug_Update
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(Debug).GetMethod("Update");
        }
        static bool Prefix(Debug __instance)
        {
            __instance.GetType().GetMethod("Fps").Invoke(__instance, Array.Empty<object>());
            if (Input.GetKeyDown(KeyCode.Tilde))
            {
                if (__instance.console.isActiveAndEnabled)
                    __instance.GetType().GetMethod("CloseConsole").Invoke(__instance, Array.Empty<object>());
                else
                    __instance.GetType().GetMethod("OpenConsole").Invoke(__instance, Array.Empty<object>());
            }
            return false;
        }
    }*/
}
