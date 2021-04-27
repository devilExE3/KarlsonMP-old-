﻿using System;
using UnityEngine;
using UnityEngine.AI;
using HarmonyLib;

namespace KarlsonMP
{
    class HarmonyHooks
    {
        //[HarmonyPatch(typeof(Enemy), "LateUpdate")]
        public static bool Enemy_LateUpdate(Enemy __instance)
        {
            if (__instance.IsDead())
                return true;
            return __instance.GetComponent<NavMeshAgent>().enabled; // LateUpdate cancels animations
        }

        //[HarmonyPatch(typeof(Debug), "Update")]
        public class Debug_Update
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
    }
}