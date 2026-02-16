using Nautilus.Options.Attributes;
using UnityEngine;

namespace SlotExtender.Configuration
{
    [Menu("Slot Extender")]
    public class SEOptions : Nautilus.Json.ConfigFile
    {
        [Keybind(Label = "Upgrade", Tooltip = "Access to upgrades from inside")]
        public KeyCode Upgrade;

        [Keybind(Label = "Storage", Tooltip = "Access to storage from inside")]
        public KeyCode Storage;

        [Keybind(Label = "Slot 6")]
        public KeyCode Slot_6;

        [Keybind(Label = "Slot 7")]
        public KeyCode Slot_7;

        [Keybind(Label = "Slot 8")]
        public KeyCode Slot_8;

        [Keybind(Label = "Slot 9")]
        public KeyCode Slot_9;

        [Keybind(Label = "Slot 10")]
        public KeyCode Slot_10;

        [Keybind(Label = "Slot 11")]
        public KeyCode Slot_11;

        [Keybind(Label = "Slot 12")]
        public KeyCode Slot_12;

        [Keybind(Label = "Left Seamoth arm")]
        public KeyCode SeamothArmLeft;

        [Keybind(Label = "Right Seamoth arm")]
        public KeyCode SeamothArmRight;

        [Slider(Label = "Maximum number of slots", Min = 5f, Max = 12f, Format = "{0:F0}", Step = 1)]
        public float MaxSlots;

        [Choice(Label = "Text color", Options = new string[] { "White", "Red", "Green", "Blue", "Yellow", "Cyan", "Magenta" })]
        public int TextColor;

        [Slider(Label = "Seamoth storage slots offset", Min = 3f, Max = 8f, Format = "{0:F0}", Step = 1)]
        public float SeamothStorageSlotsOffset;

        [Choice(Label = "Slot layout", Options = new string[] { "Grid", "Circle" })]
        public int SlotLayout;

        // public SEOptions() : base()
        // {
        //     KeybindChanged += SEOptions_KeybindChanged;
        //     ChoiceChanged += SEOptions_ChoiceChanged;
        //     SliderChanged += SEOptions_SliderChanged;
        // }

        // public override void BuildModOptions()
        // {
        //     AddKeybindOption("Upgrade", "Access to upgrades from inside", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Upgrade"]));
        //     AddKeybindOption("Storage", "Access to storage from inside", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Storage"]));
        //     AddKeybindOption("Slot_6", "Slot 6", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_6"]));
        //     AddKeybindOption("Slot_7", "Slot 7", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_7"]));
        //     AddKeybindOption("Slot_8", "Slot 8", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_8"]));
        //     AddKeybindOption("Slot_9", "Slot 9", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_9"]));
        //     AddKeybindOption("Slot_10", "Slot 10", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_10"]));
        //     AddKeybindOption("Slot_11", "Slot 11", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_11"]));
        //     AddKeybindOption("Slot_12", "Slot 12", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["Slot_12"]));
        //     AddKeybindOption("SeamothArmLeft", "Left Seamoth arm", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["SeamothArmLeft"]));
        //     AddKeybindOption("SeamothArmRight", "Right Seamoth arm", GameInput.Device.Keyboard, InputHelper.GetInputNameAsKeyCode(SEConfig.Section_Hotkeys["SeamothArmRight"]));

        //     AddSliderOption("MaxSlots", "Maximum number of slots", 5f, 12f, float.Parse(SEConfig.Section_Settings["MaxSlots"]), 12f, "{0:F0}", 1f);
        //     AddChoiceOption("TextColor", "Textcolor", ColorHelper.ColorNames, ColorHelper.GetColorInt(SEConfig.Section_Settings["TextColor"]));
        //     AddSliderOption("SeamothStorageSlotsOffset", "Seamoth storage slots offset", 3f, 8f, float.Parse(SEConfig.Section_Settings["SeamothStorageSlotsOffset"]), 4f, "{0:F0}", 1f);
        //     AddChoiceOption("SlotLayout", "Slot layout", new string[] { "Grid", "Circle" }, (int)SEConfig.SLOT_LAYOUT);
        // }

        // private void SEOptions_KeybindChanged(object sender, KeybindChangedEventArgs args)
        // {
        //     switch (args.Id)
        //     {
        //         case "Upgrade":
        //             SEConfig.Section_Hotkeys["Upgrade"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Storage":
        //             SEConfig.Section_Hotkeys["Storage"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_6":
        //             SEConfig.Section_Hotkeys["Slot_6"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_7":
        //             SEConfig.Section_Hotkeys["Slot_7"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_8":
        //             SEConfig.Section_Hotkeys["Slot_8"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_9":
        //             SEConfig.Section_Hotkeys["Slot_9"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_10":
        //             SEConfig.Section_Hotkeys["Slot_10"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_11":
        //             SEConfig.Section_Hotkeys["Slot_11"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "Slot_12":
        //             SEConfig.Section_Hotkeys["Slot_12"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "SeamothArmLeft":
        //             SEConfig.Section_Hotkeys["SeamothArmLeft"] = args.KeyName;
        //             SyncConfig();
        //             break;

        //         case "SeamothArmRight":
        //             SEConfig.Section_Hotkeys["SeamothArmRight"] = args.KeyName;
        //             SyncConfig();
        //             break;
        //     }
        // }

        // private void SEOptions_ChoiceChanged(object sender, ChoiceChangedEventArgs args)
        // {
        //     switch (args.Id)
        //     {
        //         case "TextColor":
        //             SEConfig.Section_Settings["TextColor"] = args.Value;
        //             SyncConfig();
        //             break;

        //         case "SlotLayout":
        //             SEConfig.Section_Settings["SlotLayout"] = args.Value;
        //             SyncConfig();
        //             break;
        //     }
        // }

        // private void SEOptions_SliderChanged(object sender, SliderChangedEventArgs args)
        // {
        //     switch (args.Id)
        //     {
        //         case "MaxSlots":
        //             SEConfig.Section_Settings["MaxSlots"] = args.Value.ToString();
        //             SyncConfig();
        //             break;

        //         case "SeamothStorageSlotsOffset":
        //             SEConfig.Section_Settings["SeamothStorageSlotsOffset"] = args.Value.ToString();
        //             SyncConfig();
        //             break;
        //     }
        // }

        // private void SyncConfig()
        // {
        //     SEConfig.Init();
        // }
    }
}
