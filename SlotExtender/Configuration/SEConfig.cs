using System;
using System.Collections.Generic;
using UnityEngine;
using Common;
using Common.Helpers;

namespace SlotExtender.Configuration
{
    public static class SEConfig
    {
        public static string PROGRAM_VERSION = "1.0.0";
        public static string CONFIG_VERSION = PROGRAM_VERSION;

        public static Dictionary<string, string> Section_Settings;
        public static Dictionary<SlotConfigID, string> SLOTKEYBINDINGS = new Dictionary<SlotConfigID, string>();

        public static List<string> SLOTKEYSLIST = new List<string>();

        public static int MAXSLOTS;
        public static int EXTRASLOTS;
        public static int PLAYER_EXTRA_CHIP_SLOTS;
        public static int SEAMOTH_EXTRA_SLOTS;
        public static int EXOSUIT_EXTRA_SLOTS;
        public static Color TEXTCOLOR;
        public static int STORAGE_SLOTS_OFFSET = 4;
        public static SlotLayout SLOT_LAYOUT = SlotLayout.Grid;

        internal static void SLOTKEYBINDINGS_Update()
        {
            SEInput.Register();
            EnsureRuntimeConfig();

            SLOTKEYBINDINGS.Clear();
            SLOTKEYSLIST.Clear();

            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_1, GetGameInputBindingDisplay(GameInput.Button.Slot1));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_2, GetGameInputBindingDisplay(GameInput.Button.Slot2));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_3, GetGameInputBindingDisplay(GameInput.Button.Slot3));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_4, GetGameInputBindingDisplay(GameInput.Button.Slot4));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_5, GetGameInputBindingDisplay(GameInput.Button.Slot5));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_6, GetGameInputBindingDisplay(SEInput.Slot6));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_7, GetGameInputBindingDisplay(SEInput.Slot7));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_8, GetGameInputBindingDisplay(SEInput.Slot8));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_9, GetGameInputBindingDisplay(SEInput.Slot9));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_10, GetGameInputBindingDisplay(SEInput.Slot10));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_11, GetGameInputBindingDisplay(SEInput.Slot11));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_12, GetGameInputBindingDisplay(SEInput.Slot12));

            foreach (KeyValuePair<SlotConfigID, string> kvp in SLOTKEYBINDINGS)
            {
                SLOTKEYSLIST.Add(kvp.Value);
            }
        }

        internal static void Load()
        {
            SEOptions.Instance.Load();

            ApplyOptions(SEOptions.Instance);

            SNLogger.Log("Nautilus configuration loaded.");
        }

        internal static void Init()
        {
            ApplyOptions(SEOptions.Instance);

            SLOTKEYBINDINGS_Update();

            SNLogger.Log("Configuration initialized.");
        }

        internal static void Save()
        {
            SEOptions.Instance.Save();
        }

        internal static void ApplyRuntimeOptions(SEOptions options)
        {
            if (options == null)
            {
                return;
            }

            if (Section_Settings == null)
            {
                Section_Settings = new Dictionary<string, string>();
            }

            Section_Settings["TextColor"] = options.TextColor;
            TEXTCOLOR = ColorHelper.GetColor(options.TextColor);

            SLOTKEYBINDINGS_Update();
            SlotHelper.ALLSLOTS_Update();

            if (uGUI_SlotTextHandler.Instance != null)
            {
                uGUI_SlotTextHandler.Instance.UpdateSlotText();
            }

            SNLogger.Log("Runtime configuration updated from Nautilus options.");
        }

        private static void EnsureRuntimeConfig()
        {
            if (Section_Settings == null)
            {
                ApplyOptions(SEOptions.Instance);
            }
        }

        private static void ApplyOptions(SEOptions options)
        {
            Section_Settings = CreateSettingsSection(options);

            PLAYER_EXTRA_CHIP_SLOTS = Clamp(options.PlayerExtraChipSlots, 0, 2);
            SEAMOTH_EXTRA_SLOTS = Clamp(options.SeamothExtraModuleSlots, 0, 8);
            EXOSUIT_EXTRA_SLOTS = Clamp(options.PrawnExtraModuleSlots, 0, 8);

            MAXSLOTS = 4 + Math.Max(SEAMOTH_EXTRA_SLOTS, EXOSUIT_EXTRA_SLOTS);
            EXTRASLOTS = MAXSLOTS - 4;

            TEXTCOLOR = ColorHelper.GetColor(options.TextColor);

            int slotOffset = options.SeamothStorageSlotsOffset;
            STORAGE_SLOTS_OFFSET = slotOffset < 3 ? 0 : slotOffset > 8 ? 8 : slotOffset;

            SLOT_LAYOUT = string.Equals(options.SlotLayout, SlotLayout.Circle.ToString(), StringComparison.OrdinalIgnoreCase)
                ? SlotLayout.Circle
                : SlotLayout.Grid;
        }

        private static Dictionary<string, string> CreateSettingsSection(SEOptions options)
        {
            return new Dictionary<string, string>
            {
                { "PlayerExtraChipSlots", options.PlayerExtraChipSlots.ToString() },
                { "SeamothExtraModuleSlots", options.SeamothExtraModuleSlots.ToString() },
                { "PrawnExtraModuleSlots", options.PrawnExtraModuleSlots.ToString() },
                { "TextColor", options.TextColor },
                { "SeamothStorageSlotsOffset", options.SeamothStorageSlotsOffset.ToString() },
                { "SlotLayout", options.SlotLayout }
            };
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static string GetGameInputBindingDisplay(GameInput.Button button)
        {
            string binding = GameInput.GetBinding(GameInput.Device.Keyboard, button, GameInput.BindingSet.Primary);

            if (string.IsNullOrEmpty(binding))
            {
                return string.Empty;
            }

            int pathSeparatorIndex = binding.LastIndexOf('/');

            if (pathSeparatorIndex >= 0 && pathSeparatorIndex + 1 < binding.Length)
            {
                binding = binding.Substring(pathSeparatorIndex + 1);
            }

            if (binding.StartsWith("digit", StringComparison.OrdinalIgnoreCase))
            {
                binding = binding.Substring("digit".Length);
            }
            else if (binding.StartsWith("numpad", StringComparison.OrdinalIgnoreCase))
            {
                binding = "Num" + binding.Substring("numpad".Length);
            }

            return binding.Length == 1 ? binding.ToUpperInvariant() : binding;
        }
    }
}
