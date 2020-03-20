using JetBrains.Annotations;
using PowerArgs;

namespace SimEngine2.Other {
    public class JsonDatabaseImportOptions
    {
        [UsedImplicitly]
        [ArgDescription("Sets the filename of the json file to use. Required.")]
        [ArgShortcut("i")]
        [CanBeNull]
        [ArgRequired]
        public string Input { get; set; }

        [UsedImplicitly]
        [ArgDescription("Sets what type you want to import. Required.")]
        [ArgShortcut("t")]
        [ArgRequired]
        public TypesToProcess Type { get; set; }
    }
}