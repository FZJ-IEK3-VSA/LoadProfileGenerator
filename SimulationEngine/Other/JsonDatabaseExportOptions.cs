using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngine.Other {
    public enum TypesToProcess {
        None,
        HouseholdTemplates,
        ModularHouseholds,
        HouseholdTraits,
        HouseholdTraitsWithDeviceCategories
    }
    public class JsonDatabaseExportOptions
    {
        [UsedImplicitly]
        [ArgDescription("Sets the filename of the json file to use. Required.")]
        [ArgShortcut("o")]
        [CanBeNull]
        [ArgRequired]
        public string Output { get; set; }

        [UsedImplicitly]
        [ArgDescription("Determines what to export. Required.")]
        [ArgShortcut("t")]
        [ArgRequired]
        public TypesToProcess ProcessingType { get; set; }
    }
}