using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;
using UnityEngine;

namespace SlotExtender.Configuration
{
    [Menu("Slot Extender", SaveOn = MenuAttribute.SaveEvents.ChangeValue | MenuAttribute.SaveEvents.SaveGame)]
    public class SEOptions : ConfigFile
    {
        public static SEOptions Instance { get; } = OptionsPanelHandler.RegisterModOptions<SEOptions>();

        [Keybind(Label = "Upgrade", Tooltip = "Access to upgrades from inside")]
        public KeyCode Upgrade = KeyCode.T;

        [Keybind(Label = "Storage", Tooltip = "Access to storage from inside")]
        public KeyCode Storage = KeyCode.R;

        [Keybind(Label = "Slot 6")]
        public KeyCode Slot_6 = KeyCode.Alpha6;

        [Keybind(Label = "Slot 7")]
        public KeyCode Slot_7 = KeyCode.Alpha7;

        [Keybind(Label = "Slot 8")]
        public KeyCode Slot_8 = KeyCode.Alpha8;

        [Keybind(Label = "Slot 9")]
        public KeyCode Slot_9 = KeyCode.Alpha9;

        [Keybind(Label = "Slot 10")]
        public KeyCode Slot_10 = KeyCode.Alpha0;

        [Keybind(Label = "Slot 11")]
        public KeyCode Slot_11 = KeyCode.Slash;

        [Keybind(Label = "Slot 12")]
        public KeyCode Slot_12 = KeyCode.Equals;

        [Keybind(Label = "Left Seamoth arm")]
        public KeyCode SeamothArmLeft = KeyCode.O;

        [Keybind(Label = "Right Seamoth arm")]
        public KeyCode SeamothArmRight = KeyCode.P;

        [Slider(Label = "Maximum number of slots", Min = 5f, Max = 12f, DefaultValue = 12f, Format = "{0:F0}", Step = 1)]
        public int MaxSlots = 12;

        [Choice(Label = "Text color", Options = new string[] { "Red", "Green", "Blue", "Yellow", "White", "Magenta", "Cyan", "Orange", "Lime", "Amethyst", "LightBlue" })]
        public string TextColor = "Green";

        [Slider(Label = "Seamoth storage slots offset", Min = 0f, Max = 8f, DefaultValue = 4f, Format = "{0:F0}", Step = 1)]
        public int SeamothStorageSlotsOffset = 4;

        [Choice(Label = "Slot layout", Options = new string[] { "Grid", "Circle" })]
        public string SlotLayout = "Circle";
    }
}
