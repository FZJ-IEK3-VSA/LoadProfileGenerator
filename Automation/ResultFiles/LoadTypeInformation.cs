using JetBrains.Annotations;

namespace Automation.ResultFiles {
    public class LoadTypeInformation {
        // needed for xml deserialize
        [UsedImplicitly]
        // ReSharper disable once NotNullMemberIsNotInitialized
        public LoadTypeInformation() {
        }

        public LoadTypeInformation([NotNull] string name, [NotNull] string unitOfSum, [NotNull] string unitOfPower, double conversionFaktor,
            bool showInCharts, [NotNull] string fileName, [NotNull] string guid) {
            Name = name;
            UnitOfSum = unitOfSum;
            UnitOfPower = unitOfPower;
            ConversionFaktor = conversionFaktor;
            ShowInCharts = showInCharts;
            FileName = fileName;
            Guid = guid;
        }

        // needed for xml deserialize
        [UsedImplicitly]
        public double ConversionFaktor { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string FileName { get; set; }

        [NotNull]
        public string Guid { get; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string Name { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        public bool ShowInCharts { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string UnitOfPower { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string UnitOfSum { get; set; }
    }
}