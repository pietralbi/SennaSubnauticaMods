using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace SlotExtender.Configuration
{
    [Menu("Slot Extender", SaveOn = MenuAttribute.SaveEvents.ChangeValue | MenuAttribute.SaveEvents.SaveGame)]
    public class SEOptions : ConfigFile
    {
        public static SEOptions Instance { get; } = OptionsPanelHandler.RegisterModOptions<SEOptions>();

        [Slider(Label = "Extra player chip slots", Min = 0f, Max = 2f, DefaultValue = 2f, Format = "{0:F0}", Step = 1, Tooltip = "Requires a restart")]
        public int PlayerExtraChipSlots = 2;

        [Slider(Label = "Extra Seamoth module slots", Min = 0f, Max = 8f, DefaultValue = 8f, Format = "{0:F0}", Step = 1, Tooltip = "Requires a restart")]
        public int SeamothExtraModuleSlots = 8;

        [Slider(Label = "Extra Prawn module slots", Min = 0f, Max = 8f, DefaultValue = 8f, Format = "{0:F0}", Step = 1, Tooltip = "Requires a restart")]
        public int PrawnExtraModuleSlots = 8;

        [Choice(Label = "Text color", Options = new string[] { "Red", "Green", "Blue", "Yellow", "White", "Magenta", "Cyan", "Orange", "Lime", "Amethyst", "LightBlue" })]
        public string TextColor = "White";

        [Slider(Label = "Seamoth storage slots offset", Min = 0f, Max = 8f, DefaultValue = 4f, Format = "{0:F0}", Step = 1, Tooltip = "Requires a restart")]
        public int SeamothStorageSlotsOffset = 4;

        [Choice(Label = "Slot layout", Options = new string[] { "Grid", "Circle" }, Tooltip = "Requires a restart")]
        public string SlotLayout = "Circle";
    }
}
