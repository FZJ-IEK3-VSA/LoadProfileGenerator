using Automation.ResultFiles;

namespace CalculationEngine.CitySimulation
{
    /// <summary>
    /// Unique identifier for a CalcPerson in the city simulation
    /// </summary>
    /// <param name="personName">the name of the person</param>
    /// <param name="householdKey">the household key</param>
    /// <param name="targetId">the simulation target, i.e. the building</param>
    /// <param name="workerId">the rank of the worke that simulates the target</param>
    public class PersonIdentifier(string personName, HouseholdKey householdKey, string targetId, int workerId)
    {
        public readonly string PersonName = personName;
        public readonly HouseholdKey HouseholdKey = householdKey;
        public readonly string TargetId = targetId;
        public readonly int WorkerId = workerId;
    }

    /// <summary>
    /// Unique identifier for a point of interest and its simulator.
    /// </summary>
    /// <param name="id">ID of the point of interest</param>
    /// <param name="workerId">rank of the worker that simulates the POI</param>
    public class PointOfInterestId(string id, int workerId)
    {
        public readonly string Id = id;
        public readonly int WorkerId = workerId;
    }
}
