using CalculationEngine;

namespace CitySimulation.SimulationTargets
{
    internal class CitySimulationHouse
    {
        public readonly string Id;
        public readonly CalcManager CalcManager;
        public readonly string ResultDirectory;

        public CitySimulationHouse(string id, CalcManager calcManager, string resultDirectory)
        {
            Id = id;
            CalcManager = calcManager;
            ResultDirectory = resultDirectory;
        }
    }
}
