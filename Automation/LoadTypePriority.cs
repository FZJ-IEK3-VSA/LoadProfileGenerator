namespace Automation {
    /// <summary>
    /// Determines the set of load types that are considered in the simulation. Priorities are ordered
    /// from smallest to largest set, so a larger enum int value means more load types are included.
    /// </summary>
    public enum LoadTypePriority
    {
        Undefined,
        Mandatory,
        RecommendedForHouseholds,
        RecommendedForHouses,
        OptionalLoadtypes,
        All
    }
}