using JetBrains.Annotations;
using PowerArgs;

namespace SimEngine2.Other {
    public class CsvImportOptions
    {
        [UsedImplicitly]
        [ArgDescription("Sets the filename of the csv file to import. Required.")]
        [ArgShortcut("i")]
        [CanBeNull]
        [ArgRequired]
        public string Input { get; set; }

        [UsedImplicitly]
        [ArgDescription("Sets the delimiter. Required.")]
        [ArgShortcut("d")]
        [ArgRequired]
        [CanBeNull]
        public string Delimiter { get; set; }

        [UsedImplicitly]
        [ArgDescription("Name to use in the database. Required.")]
        [ArgShortcut("n")]
        [ArgRequired]
        [CanBeNull]
        public string Name { get; set; }
    }
}