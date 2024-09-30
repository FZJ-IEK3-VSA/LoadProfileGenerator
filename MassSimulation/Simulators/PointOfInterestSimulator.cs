using CalculationEngine.CitySimulation;
using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation.Simulators
{
    /// <summary>
    /// Simulates agent stays in any point of interest, for example a small enterprise.
    /// </summary>
    internal class PointOfInterestSimulator : ISimulator
    {
        private Collection<AgentStayState> presentAgents = [];

        public PointOfInterestId PoiId => throw new NotImplementedException();

        public void FinishSimulation()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RemoteActivityInfo> SimulateOneStep(TimeStep timeStep, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            throw new NotImplementedException();
        }
    }
}
