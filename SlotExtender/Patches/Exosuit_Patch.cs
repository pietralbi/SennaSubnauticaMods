using HarmonyLib;
using Common;

namespace SlotExtender.Patches
{
    [HarmonyPatch(typeof(Exosuit), "slotIDs", MethodType.Getter)]    
    public class Exosuit_slotIDs_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string[] __result)
        {           
            __result = SlotHelper.SessionExosuitSlotIDs;
            return false;
        }
    }    
    
    [HarmonyPatch(typeof(Exosuit), "Awake")]    
    public class Exosuit_Awake_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Exosuit __instance)
        {            
            __instance.gameObject.EnsureComponent<SlotExtenderControl>();
            SNLogger.Debug($"Component added in Exosuit.Awake -> Postfix Patch. ID: {__instance.GetInstanceID()}");           
        }
    }

    [HarmonyPatch(typeof(Exosuit), "IsAllowedToRemove")]
    internal static class Exosuit_IsAllowedToRemove_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Exosuit __instance, Pickupable pickupable, ref bool __result)
        {
            if (pickupable == null || pickupable.GetTechType() != TechType.VehicleStorageModule || __instance.storageContainer == null)
            {
                return true;
            }

            if (__instance.storageContainer.IsEmpty())
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
