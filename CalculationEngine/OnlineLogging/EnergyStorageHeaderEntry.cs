namespace CalculationEngine.OnlineLogging {
    public class EnergyStorageHeaderEntry {
        [JetBrains.Annotations.NotNull]
        private readonly string _capacity;
        [JetBrains.Annotations.NotNull]
        private readonly string _loadType;
        [JetBrains.Annotations.NotNull]
        private readonly string _name;
        private string? _signals;

        public EnergyStorageHeaderEntry([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string capacity, [JetBrains.Annotations.NotNull] string loadType) {
            _name = name;
            _capacity = capacity;
            _loadType = loadType;
        }

        [JetBrains.Annotations.NotNull]
        public string TotalHeader => _name + " (Capacity " + _capacity + ") (Loadtype " + _loadType + ") (" + _signals +
                                     ")";

        public void AddSignal([JetBrains.Annotations.NotNull] string signal) {
            _signals += signal;
        }
    }
}