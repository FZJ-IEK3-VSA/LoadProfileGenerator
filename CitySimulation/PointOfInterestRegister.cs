using Automation.ResultFiles;
using CalculationEngine.CitySimulation;

namespace CitySimulation
{
    /// <summary>
    /// Register that stores all POIs and the worker responsible for simulating them.
    /// </summary>
    public class PointOfInterestRegister(Dictionary<string, int> poiToWorkerMapping)
    {
        /// <summary>
        /// Register for all point of interests
        /// </summary>
        public Dictionary<string, int> PoiToWorkerMapping { get; } = poiToWorkerMapping;

        /// <summary>
        /// Returns the ID of the worker responsible for simulating the location. The location is either
        /// the ID of a point of interest, or null, which means the person is at home.
        /// person);
        /// </summary>
        /// <param name="pointOfInterestId">the point of interest to look up</param>
        /// <returns>the worker responsible for this POI</returns>
        /// <exception cref="LPGException">if the worker could not be determined</exception>
        public int GetWorkerForLocation(PointOfInterestId? pointOfInterestId, PersonIdentifier personId)
        {
            if (pointOfInterestId is null)
            {
                return personId.WorkerId;
            }
            return GetWorkerForPOI(pointOfInterestId);
        }

        /// <summary>
        /// Returns the ID of the worker responsible for simulating the specified point of interest.
        /// </summary>
        /// <param name="pointOfInterestId">the point of interest to look up</param>
        /// <returns>the worker responsible for this POI</returns>
        /// <exception cref="LPGException">if the worker could not be determined</exception>
        public int GetWorkerForPOI(PointOfInterestId pointOfInterestId)
        {
            if (!PoiToWorkerMapping.TryGetValue(pointOfInterestId.Id, out int workerId))
                throw new LPGException($"Unregistered point of interest: {pointOfInterestId}; could not determine responsible worker.");
            return workerId;
        }
    }
}
