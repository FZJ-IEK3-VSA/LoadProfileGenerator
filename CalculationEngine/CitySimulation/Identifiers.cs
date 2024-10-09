using Automation.ResultFiles;

namespace CalculationEngine.CitySimulation
{
    /// <summary>
    /// Unique identifier for a CalcPerson in the city simulation. Can be initialized as an incomplete
    /// identifier without target ID and worker ID, making it only unique within the simulation target.
    /// </summary>
    /// <param name="personName">the name of the person</param>
    /// <param name="householdKey">the key of the person's household</param>
    /// <param name="targetId">the simulation target, i.e. the building that contains the household</param>
    /// <param name="workerId">the rank of the worker that simulates the target</param>
    public record PersonIdentifier(string personName, HouseholdKey householdKey, string targetId = "", int workerId = -1)
    {
        public string PersonName { get; } = personName;
        public HouseholdKey HouseholdKey { get; } = householdKey;
        public string TargetId { get; private set; } = targetId;
        public int WorkerId { get; private set; } = workerId;

        /// <summary>
        /// If the identifier is incomplete, i.e. it is missing target ID and worker ID, this method
        /// adds the missing information.
        /// </summary>
        /// <param name="targetId">the missing target ID</param>
        /// <param name="workerId">the missing rank of the worker</param>
        /// <exception cref="LPGException">if the identifier was already complete before</exception>
        public void AddMissingInfo(string targetId, int workerId)
        {
            if (IsComplete())
                throw new LPGException("Tried to change an already complete PersonIdentifier");
            TargetId = targetId;
            WorkerId = workerId;
            if (!IsComplete())
                throw new LPGException($"The added target ID '{targetId}' or worker ID '{workerId}' are invalid.");
        }

        /// <summary>
        /// Checks if this identifier is complete with all four fields, or if any of them has a
        /// default value.
        /// </summary>
        /// <returns>whether this identifier is complete</returns>
        public bool IsComplete()
        {
            return !string.IsNullOrEmpty(PersonName) && !string.IsNullOrEmpty(HouseholdKey.Key)
                && !string.IsNullOrEmpty(TargetId) && WorkerId >= 0;
        }
    }

    /// <summary>
    /// Unique identifier for a point of interest and its simulator.
    /// </summary>
    /// <param name="id">ID of the point of interest</param>
    /// <param name="workerId">rank of the worker that simulates the POI</param>
    public record PointOfInterestId(int id, int workerId)
    {
        public readonly int Id = id;
        public readonly int WorkerId = workerId;
    }
}
