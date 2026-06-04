using System;
using HarmonyLib;
using Nautilus.Json;
using Common;

namespace SlotExtender.Configuration
{
    [HarmonyPatch(typeof(ConfigFile), nameof(ConfigFile.Save))]
    internal static class ConfigSavePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ConfigFile __instance)
        {
            if (!(__instance is SEOptions cfg))
            {
                return;
            }

            SEOptions options = SEOptions.Instance;

            if (!ReferenceEquals(cfg, options))
            {
                try
                {
                    options.Load();
                }
                catch (Exception ex)
                {
                    SNLogger.Error($"Failed to reload Nautilus options after save: {ex}");
                    return;
                }
            }

            SEConfig.ApplyRuntimeOptions(options);
        }
    }
}
