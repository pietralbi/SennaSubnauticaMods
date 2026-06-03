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

        public static Dictionary<string, string> Section_Hotkeys;
        public static Dictionary<string, string> Section_Settings;
        public static Dictionary<SlotConfigID, string> SLOTKEYBINDINGS = new Dictionary<SlotConfigID, string>();
        public static Dictionary<string, KeyCode> KEYBINDINGS;

        public static List<string> SLOTKEYSLIST = new List<string>();

        public static int MAXSLOTS;
        public static int EXTRASLOTS;
        public static Color TEXTCOLOR;
        public static int STORAGE_SLOTS_OFFSET = 4;
        public static SlotLayout SLOT_LAYOUT = SlotLayout.Grid;
        public static bool isSeamothArmsExists = false;

        internal static void SLOTKEYBINDINGS_Update()
        {
            EnsureRuntimeConfig();

            SLOTKEYBINDINGS.Clear();
            SLOTKEYSLIST.Clear();

            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_1, GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.Slot1, GameInput.BindingSet.Primary));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_2, GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.Slot2, GameInput.BindingSet.Primary));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_3, GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.Slot3, GameInput.BindingSet.Primary));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_4, GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.Slot4, GameInput.BindingSet.Primary));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_5, GameInput.GetBinding(GameInput.Device.Keyboard, GameInput.Button.Slot5, GameInput.BindingSet.Primary));
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_6, Section_Hotkeys[SlotConfigID.Slot_6.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_7, Section_Hotkeys[SlotConfigID.Slot_7.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_8, Section_Hotkeys[SlotConfigID.Slot_8.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_9, Section_Hotkeys[SlotConfigID.Slot_9.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_10, Section_Hotkeys[SlotConfigID.Slot_10.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_11, Section_Hotkeys[SlotConfigID.Slot_11.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.Slot_12, Section_Hotkeys[SlotConfigID.Slot_12.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.SeamothArmLeft, Section_Hotkeys[SlotConfigID.SeamothArmLeft.ToString()]);
            SLOTKEYBINDINGS.Add(SlotConfigID.SeamothArmRight, Section_Hotkeys[SlotConfigID.SeamothArmRight.ToString()]);

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

            SLOTKEYBINDINGS_SyncToAll();

            SNLogger.Log("Configuration initialized.");
        }

        internal static void Save()
        {
            SEOptions.Instance.Save();
        }

        internal static void SLOTKEYBINDINGS_SyncToAll()
        {
            EnsureRuntimeConfig();

            foreach (KeyValuePair<SlotConfigID, string> kvp in SLOTKEYBINDINGS)
            {
                SNLogger.Debug($"key: {kvp.Key.ToString()}, Value: {kvp.Value}");

                string key = kvp.Key.ToString();

                if (Section_Hotkeys.ContainsKey(key))
                {
                    Section_Hotkeys[key] = kvp.Value;
                }

                KEYBINDINGS[key] = InputHelper.GetInputNameAsKeyCode(kvp.Value);
            }
        }

        internal static void KEYBINDINGS_Set()
        {
            EnsureRuntimeConfig();
        }

        private static void EnsureRuntimeConfig()
        {
            if (Section_Settings == null || Section_Hotkeys == null || KEYBINDINGS == null)
            {
                ApplyOptions(SEOptions.Instance);
            }
        }

        private static void ApplyOptions(SEOptions options)
        {
            Section_Settings = new Dictionary<string, string>
            {
                { "MaxSlots", options.MaxSlots.ToString() },
                { "TextColor", options.TextColor },
                { "SeamothStorageSlotsOffset", options.SeamothStorageSlotsOffset.ToString() },
                { "SlotLayout", options.SlotLayout }
            };

            Section_Hotkeys = new Dictionary<string, string>
            {
                { "Upgrade", InputHelper.GetKeyCodeAsInputName(options.Upgrade) },
                { "Storage", InputHelper.GetKeyCodeAsInputName(options.Storage) },
                { SlotConfigID.Slot_6.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_6) },
                { SlotConfigID.Slot_7.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_7) },
                { SlotConfigID.Slot_8.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_8) },
                { SlotConfigID.Slot_9.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_9) },
                { SlotConfigID.Slot_10.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_10) },
                { SlotConfigID.Slot_11.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_11) },
                { SlotConfigID.Slot_12.ToString(), InputHelper.GetKeyCodeAsInputName(options.Slot_12) },
                { SlotConfigID.SeamothArmLeft.ToString(), InputHelper.GetKeyCodeAsInputName(options.SeamothArmLeft) },
                { SlotConfigID.SeamothArmRight.ToString(), InputHelper.GetKeyCodeAsInputName(options.SeamothArmRight) }
            };

            MAXSLOTS = options.MaxSlots < 5 || options.MaxSlots > 12 ? 12 : options.MaxSlots;
            EXTRASLOTS = MAXSLOTS - 4;

            TEXTCOLOR = ColorHelper.GetColor(options.TextColor);

            int slotOffset = options.SeamothStorageSlotsOffset;
            STORAGE_SLOTS_OFFSET = slotOffset < 3 ? 0 : slotOffset > 8 ? 8 : slotOffset;

            SLOT_LAYOUT = string.Equals(options.SlotLayout, SlotLayout.Circle.ToString(), StringComparison.OrdinalIgnoreCase)
                ? SlotLayout.Circle
                : SlotLayout.Grid;

            isSeamothArmsExists = true;

            KEYBINDINGS = new Dictionary<string, KeyCode>();

            foreach (KeyValuePair<string, string> kvp in Section_Hotkeys)
            {
                KEYBINDINGS[kvp.Key] = InputHelper.GetInputNameAsKeyCode(kvp.Value);
            }
        }
    }
}
