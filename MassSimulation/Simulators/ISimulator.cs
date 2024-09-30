using CalculationEngine.CitySimulation;
using Common;

namespace MassSimulation.Simulators
{
    /// <summary>
    /// Generic interface for simulators, handling simulation of houses, SMEs or similar
    /// </summary>
    internal interface ISimulator
    {
        IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities);

        void FinishSimulation();
    }
}
