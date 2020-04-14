using JetBrains.Annotations;

namespace SimulationEngineLib
{
    public static class SimulationEngineConfig
    {
        private static bool _isUnitTest;

        [UsedImplicitly]
        public static bool CatchErrors { get; set; } = true;

        public static bool IsUnitTest
        {
            get => _isUnitTest;
            set => _isUnitTest = value;
        }
    }
}
