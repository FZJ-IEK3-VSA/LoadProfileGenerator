using CalculationEngine.CitySimulation;
using Common;

namespace CitySimulation.Simulators
{
    /// <summary>
    /// Generic interface for simulators, handling simulation of POIs, traveling, or similar
    /// </summary>
    internal interface ISimulator
    {
        /// <summary>
        /// Simulates a single time step.
        /// </summary>
        /// <param name="timeStep">the time step to simulate</param>
        /// <param name="dateTime">datetime of the time step</param>
        /// <param name="newActivities">list of new activities for this simulator</param>
        /// <returns>list of activities that were finished in the time step</returns>
        IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities);

        /// <summary>
        /// Finishes everything up after the last timestep.
        /// </summary>
        void FinishSimulation();
    }
}
