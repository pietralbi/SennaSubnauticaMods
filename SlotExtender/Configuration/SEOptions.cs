using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace SlotExtender.Configuration
{
    [Menu("Slot Extender", SaveOn = MenuAttribute.SaveEvents.ChangeValue | MenuAttribute.SaveEvents.SaveGame)]
    public class SEOptions : ConfigFile
    {
        public static SEOptions Instance { get; } = OptionsPanelHandler.RegisterModOptions<SEOptions>();

        [Slider(Label = "Maximum number of slots", Min = 5f, Max = 12f, DefaultValue = 12f, Format = "{0:F0}", Step = 1)]
        public int MaxSlots = 12;

        [Choice(Label = "Text color", Options = new string[] { "Red", "Green", "Blue", "Yellow", "White", "Magenta", "Cyan", "Orange", "Lime", "Amethyst", "LightBlue" })]
        public string TextColor = "White";

        [Slider(Label = "Seamoth storage slots offset", Min = 0f, Max = 8f, DefaultValue = 4f, Format = "{0:F0}", Step = 1)]
        public int SeamothStorageSlotsOffset = 4;

        [Choice(Label = "Slot layout", Options = new string[] { "Grid", "Circle" })]
        public string SlotLayout = "Circle";
    }
}
