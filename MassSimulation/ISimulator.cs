using Common;

namespace MassSimulation
{
    /// <summary>
    /// Generic interface for simulators, handling simulation of houses, SMEs or similar
    /// </summary>
    internal interface ISimulator
    {
        void SimulateOneStep(TimeStep timeStep, DateTime dateTime);

        void FinishSimulation();
    }
}