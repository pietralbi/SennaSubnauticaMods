using System;
using System.Reflection;
using Common;
using Nautilus.Handlers;
using UnityEngine.InputSystem;

namespace SlotExtender.Configuration
{
    internal static class SEInput
    {
        private const string Category = "Slot Extender";
        private const string Language = "English";

        private static bool isRegistered;

        internal static GameInput.Button Upgrade { get; private set; }
        internal static GameInput.Button Storage { get; private set; }
        internal static GameInput.Button Slot6 { get; private set; }
        internal static GameInput.Button Slot7 { get; private set; }
        internal static GameInput.Button Slot8 { get; private set; }
        internal static GameInput.Button Slot9 { get; private set; }
        internal static GameInput.Button Slot10 { get; private set; }
        internal static GameInput.Button Slot11 { get; private set; }
        internal static GameInput.Button Slot12 { get; private set; }

        internal static void Register()
        {
            if (isRegistered)
            {
                return;
            }

            Upgrade = RegisterButton("SlotExtenderUpgrade", "Upgrade", "Access upgrades from inside a vehicle.", "<Keyboard>/t");
            Storage = RegisterButton("SlotExtenderStorage", "Storage", "Access exosuit storage from inside.", "<Keyboard>/r");

            Slot6 = RegisterButton("SlotExtenderSlot6", "Slot 6", "Use slot 6.", "<Keyboard>/6");
            Slot7 = RegisterButton("SlotExtenderSlot7", "Slot 7", "Use slot 7.", "<Keyboard>/7");
            Slot8 = RegisterButton("SlotExtenderSlot8", "Slot 8", "Use slot 8.", "<Keyboard>/8");
            Slot9 = RegisterButton("SlotExtenderSlot9", "Slot 9", "Use slot 9.", "<Keyboard>/9");
            Slot10 = RegisterButton("SlotExtenderSlot10", "Slot 10", "Use slot 10.", "<Keyboard>/0");
            Slot11 = RegisterButton("SlotExtenderSlot11", "Slot 11", "Use slot 11.", "<Keyboard>/slash");
            Slot12 = RegisterButton("SlotExtenderSlot12", "Slot 12", "Use slot 12.", "<Keyboard>/equals");

            isRegistered = true;
        }

        private static GameInput.Button RegisterButton(string name, string displayName, string tooltip, string primaryBinding)
        {
            EnumBuilder<GameInput.Button> builder = EnumHandler.AddEntry<GameInput.Button>(name, Assembly.GetExecutingAssembly());

            if (builder == null)
            {
                try
                {
                    return (GameInput.Button)Enum.Parse(typeof(GameInput.Button), name);
                }
                catch (Exception ex)
                {
                    SNLogger.Error($"Failed to reuse existing GameInput button '{name}': {ex}");
                    throw;
                }
            }

            builder
                .CreateInput(displayName, tooltip, Language, InputActionType.Button)
                .WithKeyboardBinding(primaryBinding, string.Empty)
                .WithCategory(Category)
                .SetBindable(GameInput.Device.Keyboard)
                .AvoidConflicts(GameInput.Device.Keyboard);

            return builder.Value;
        }
    }
}
