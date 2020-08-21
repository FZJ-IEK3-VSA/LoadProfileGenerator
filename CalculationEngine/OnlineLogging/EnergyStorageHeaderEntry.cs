using JetBrains.Annotations;

namespace CalculationEngine.OnlineLogging {
    public class EnergyStorageHeaderEntry {
        [NotNull]
        private readonly string _capacity;
        [NotNull]
        private readonly string _loadType;
        [NotNull]
        private readonly string _name;
        private string? _signals;

        public EnergyStorageHeaderEntry([NotNull] string name, [NotNull] string capacity, [NotNull] string loadType) {
            _name = name;
            _capacity = capacity;
            _loadType = loadType;
        }

        [NotNull]
        public string TotalHeader => _name + " (Capacity " + _capacity + ") (Loadtype " + _loadType + ") (" + _signals +
                                     ")";

        public void AddSignal([NotNull] string signal) {
            _signals += signal;
        }
    }
}