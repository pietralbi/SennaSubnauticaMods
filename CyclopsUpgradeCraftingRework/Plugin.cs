using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using UnityEngine;

namespace CyclopsUpgradeCraftingRework
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Subnautica.exe")]
    [BepInDependency("com.mrpurple6411.MoreCyclopsUpgrades", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.mrpurple6411.CyclopsEngineUpgrades", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.mrpurple6411.CyclopsNuclearUpgrades", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.mrpurple6411.CyclopsSpeedUpgrades", BepInDependency.DependencyFlags.SoftDependency)]
    internal sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.senna.cyclopsupgradecraftingrework";
        private const string PluginName = "Cyclops Upgrade Crafting Rework";
        private const string PluginVersion = "1.2.5";

        private const string CyclopsMenuNode = "CyclopsMenu";
        private const string EngineMk2ClassId = "PowerUpgradeModuleMk2";
        private const string EngineMk3ClassId = "PowerUpgradeModuleMk3";
        private const string NuclearModuleClassId = "CyclopsNuclearModule";
        private const string SpeedModuleClassId = "CyclopsSpeedModule";

        private readonly Dictionary<string, TechType> resolvedTechTypes = new Dictionary<string, TechType>(StringComparer.Ordinal);
        private ManualLogSource log;
        private Coroutine reworkCoroutine;
        private Harmony harmony;

        private static float statusMessagesAllowedUntil = -1f;
        private static int forcedEquipmentMutationDepth;

        private void Awake()
        {
            log = Logger;
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            PatchSlotChangeDetection();
            PatchErrorMessageQueue();
        }

        private void Start()
        {
            reworkCoroutine = StartCoroutine(ApplyWhenReady());
        }

        private void OnDestroy()
        {
            if (reworkCoroutine != null)
            {
                StopCoroutine(reworkCoroutine);
                reworkCoroutine = null;
            }
        }

        private void OnDisable()
        {
            harmony?.UnpatchSelf();
            harmony = null;
            statusMessagesAllowedUntil = -1f;
        }

        private IEnumerator ApplyWhenReady()
        {
            const int maxAttempts = 40;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (TryResolveRequiredTechTypes())
                {
                    ApplyCraftTreeRework();
                    reworkCoroutine = null;
                    yield break;
                }

                yield return new WaitForSecondsRealtime(0.25f);
            }

            log.LogWarning(
                "Unable to apply Cyclops upgrade crafting rework; missing TechTypes: " +
                string.Join(", ", GetMissingTechTypes().ToArray()));
            reworkCoroutine = null;
        }

        private bool TryResolveRequiredTechTypes()
        {
            ResolveTechType(EngineMk2ClassId);
            ResolveTechType(EngineMk3ClassId);
            ResolveTechType(NuclearModuleClassId);
            ResolveTechType(SpeedModuleClassId);

            return resolvedTechTypes.ContainsKey(EngineMk2ClassId) &&
                   resolvedTechTypes.ContainsKey(EngineMk3ClassId) &&
                   resolvedTechTypes.ContainsKey(NuclearModuleClassId);
        }

        private void ResolveTechType(string classId)
        {
            if (resolvedTechTypes.ContainsKey(classId))
            {
                return;
            }

            TechType techType;
            if (EnumHandler.TryGetValue(classId, out techType) && techType != TechType.None)
            {
                resolvedTechTypes[classId] = techType;
            }
        }

        private List<string> GetMissingTechTypes()
        {
            List<string> missing = new List<string>();

            AddMissing(EngineMk2ClassId, missing);
            AddMissing(EngineMk3ClassId, missing);
            AddMissing(NuclearModuleClassId, missing);

            return missing;
        }

        private void AddMissing(string classId, List<string> missing)
        {
            if (!resolvedTechTypes.ContainsKey(classId))
            {
                missing.Add(classId);
            }
        }

        private void ApplyCraftTreeRework()
        {
            RemoveOriginalWorkbenchNode(EngineMk2ClassId);
            RemoveOriginalWorkbenchNode(EngineMk3ClassId);
            RemoveOriginalWorkbenchNode(NuclearModuleClassId);
            CraftTreeHandler.RemoveNode(CraftTree.Type.Workbench, CyclopsMenuNode);

            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Workbench, resolvedTechTypes[EngineMk2ClassId]);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Workbench, resolvedTechTypes[EngineMk3ClassId]);

            CraftTreeHandler.AddCraftingNode(CraftTree.Type.CyclopsFabricator, resolvedTechTypes[NuclearModuleClassId]);

            if (resolvedTechTypes.ContainsKey(SpeedModuleClassId))
            {
                RemoveOriginalCyclopsFabricatorNode(SpeedModuleClassId);
                CraftTreeHandler.RemoveNode(CraftTree.Type.CyclopsFabricator, CyclopsMenuNode);
                CraftTreeHandler.AddCraftingNode(CraftTree.Type.CyclopsFabricator, resolvedTechTypes[SpeedModuleClassId]);
            }

            log.LogInfo("Moved Cyclops engine upgrades to the Modification Station root and Cyclops-only modules to the Cyclops fabricator root.");
        }

        private static void RemoveOriginalWorkbenchNode(string classId)
        {
            CraftTreeHandler.RemoveNode(CraftTree.Type.Workbench, CyclopsMenuNode, classId);
        }

        private static void RemoveOriginalCyclopsFabricatorNode(string classId)
        {
            CraftTreeHandler.RemoveNode(CraftTree.Type.CyclopsFabricator, CyclopsMenuNode, classId);
        }

        private void PatchSlotChangeDetection()
        {
            Type upgradeHandlerType = AccessTools.TypeByName("MoreCyclopsUpgrades.API.Upgrades.UpgradeHandler");
            MethodInfo onEquip = AccessTools.Method(upgradeHandlerType, "OnEquip");
            MethodInfo onUnequip = AccessTools.Method(upgradeHandlerType, "OnUnequip");

            if (onEquip == null || onUnequip == null)
            {
                log.LogWarning("Could not find MoreCyclopsUpgrades UpgradeHandler slot hooks; Cyclops status messages will remain suppressed.");
                return;
            }

            HarmonyMethod slotChanged = new HarmonyMethod(typeof(Plugin), nameof(SlotChangedPrefix));
            harmony.Patch(onEquip, prefix: slotChanged);
            harmony.Patch(onUnequip, prefix: slotChanged);
        }

        private void PatchErrorMessageQueue()
        {
            MethodInfo addMessage = AccessTools.Method(typeof(ErrorMessage), "_AddMessage");

            if (addMessage == null)
            {
                log.LogWarning("Could not find ErrorMessage._AddMessage; final status-message suppression hook is disabled.");
                return;
            }

            harmony.Patch(addMessage, prefix: new HarmonyMethod(typeof(Plugin), nameof(ErrorMessageQueuePrefix)));
        }

        private static void SlotChangedPrefix()
        {
            if (forcedEquipmentMutationDepth == 0)
            {
                statusMessagesAllowedUntil = Time.realtimeSinceStartup + 3f;
            }
        }

        private static bool ShouldAllowMessage(string message)
        {
            if (!IsCyclopsStatusMessage(message))
            {
                return true;
            }

            return IsSlotChangeMessageWindowActive();
        }

        private static bool IsCyclopsStatusMessage(string message)
        {
            return IsPowerRatingMessage(message) || IsSpeedUpgradeMessage(message);
        }

        private static bool IsSlotChangeMessageWindowActive()
        {
            return statusMessagesAllowedUntil > 0f && Time.realtimeSinceStartup <= statusMessagesAllowedUntil;
        }

        private static bool IsSpeedUpgradeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            string maxRatingMessage = Language.main?.Get("CySpeedMaxed");

            return string.Equals(message, maxRatingMessage, StringComparison.Ordinal) ||
                   string.Equals(message, "Maximum speed rating reached", StringComparison.Ordinal) ||
                   message.StartsWith("Speed rating is now at ", StringComparison.Ordinal);
        }

        private static bool IsPowerRatingMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            return message.IndexOf("Engine efficiency", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   message.IndexOf("Power rating", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ErrorMessageQueuePrefix(string messageText)
        {
            return ShouldAllowMessage(messageText);
        }

        [HarmonyPatch(typeof(Equipment), nameof(Equipment.AddItem), typeof(string), typeof(InventoryItem), typeof(bool))]
        private static class EquipmentAddItemPatch
        {
            private static void Prefix(bool forced)
            {
                if (forced)
                {
                    forcedEquipmentMutationDepth++;
                }
            }

            private static void Finalizer(bool forced)
            {
                if (forced)
                {
                    forcedEquipmentMutationDepth = Math.Max(0, forcedEquipmentMutationDepth - 1);
                }
            }
        }

        [HarmonyPatch(typeof(Equipment), nameof(Equipment.RemoveItem), typeof(string), typeof(bool), typeof(bool))]
        private static class EquipmentRemoveItemPatch
        {
            private static void Prefix(bool forced)
            {
                if (forced)
                {
                    forcedEquipmentMutationDepth++;
                }
            }

            private static void Finalizer(bool forced)
            {
                if (forced)
                {
                    forcedEquipmentMutationDepth = Math.Max(0, forcedEquipmentMutationDepth - 1);
                }
            }
        }
    }
}
