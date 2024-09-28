using Automation;

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// Stores basic information about a single affordance activation. This can be an affordance with
    /// known duration or a remote affordance of unknown duration, both with or without traveling.
    /// </summary>
    public interface IAffordanceActivation
    {
        /// <summary>
        /// Specifies whether the duration of the affordance activation is already
        /// known in advance. If not, the affordance is a remote affordance or a
        /// dynamic travel, whose duration is determined externally.
        /// </summary>
        bool IsDetermined { get; }

        /// <summary>
        /// Name of the affordance activation
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The source of this activation.
        /// </summary>
        string DataSource { get; }
    }
}
