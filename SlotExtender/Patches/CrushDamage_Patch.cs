using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UWE;

namespace SlotExtender.Patches
{
    [HarmonyPatch(typeof(CrushDamage), "SetExtraCrushDepth")]
    internal static class CrushDamage_SetExtraCrushDepth_Patch
    {
        private static readonly FieldInfo ExtraCrushDepthField = AccessTools.Field(typeof(CrushDamage), "<extraCrushDepth>k__BackingField");
        private static readonly FieldInfo CrushDepthField = AccessTools.Field(typeof(CrushDamage), "<crushDepth>k__BackingField");
        private static readonly Dictionary<CrushDamage, PendingDepthMessage> PendingMessages = new Dictionary<CrushDamage, PendingDepthMessage>();
        private static int moduleSlotReplayDepth;

        internal static bool Prefix(CrushDamage __instance, float depth)
        {
            if (__instance == null || ExtraCrushDepthField == null || CrushDepthField == null || !IsSlotExtenderVehicle(__instance))
                return true;

            ExtraCrushDepthField.SetValue(__instance, depth);

            if (!__instance.gameObject.activeInHierarchy)
                return false;

            float previousCrushDepth = __instance.crushDepth;
            float currentCrushDepth = __instance.kBaseCrushDepth + depth;
            CrushDepthField.SetValue(__instance, currentCrushDepth);

            if (Mathf.Approximately(previousCrushDepth, currentCrushDepth))
                return false;

            if (WaitScreen.IsWaiting || IsReplayingModuleSlots())
            {
                PendingMessages.Remove(__instance);
                return false;
            }

            QueueMessage(__instance, previousCrushDepth);
            return false;
        }

        private static bool IsSlotExtenderVehicle(CrushDamage crushDamage)
        {
            Vehicle vehicle = crushDamage.vehicle ?? crushDamage.GetComponent<Vehicle>();
            return vehicle is SeaMoth || vehicle is Exosuit;
        }

        private static void QueueMessage(CrushDamage crushDamage, float previousCrushDepth)
        {
            if (!PendingMessages.ContainsKey(crushDamage))
            {
                PendingMessages.Add(crushDamage, new PendingDepthMessage(previousCrushDepth));
                CoroutineHost.StartCoroutine(FlushMessage(crushDamage));
            }
        }

        private static bool IsReplayingModuleSlots()
        {
            return moduleSlotReplayDepth > 0;
        }

        private static IEnumerator FlushMessage(CrushDamage crushDamage)
        {
            yield return null;

            if (!PendingMessages.TryGetValue(crushDamage, out PendingDepthMessage pendingMessage))
                yield break;

            PendingMessages.Remove(crushDamage);

            if (crushDamage == null || !crushDamage.gameObject.activeInHierarchy || WaitScreen.IsWaiting)
                yield break;

            if (!Mathf.Approximately(pendingMessage.InitialCrushDepth, crushDamage.crushDepth))
                ErrorMessage.AddMessage(Language.main.GetFormat("CrushDepthNow", crushDamage.crushDepth));
        }

        private sealed class PendingDepthMessage
        {
            internal PendingDepthMessage(float initialCrushDepth)
            {
                InitialCrushDepth = initialCrushDepth;
            }

            internal float InitialCrushDepth { get; }
        }

        [HarmonyPatch(typeof(Vehicle), "UpdateModuleSlots")]
        private static class Vehicle_UpdateModuleSlots_Patch
        {
            internal static void Prefix(Vehicle __instance)
            {
                if (__instance is SeaMoth || __instance is Exosuit)
                    moduleSlotReplayDepth++;
            }

            internal static void Postfix(Vehicle __instance)
            {
                if ((__instance is SeaMoth || __instance is Exosuit) && moduleSlotReplayDepth > 0)
                    moduleSlotReplayDepth--;
            }
        }
    }
}
