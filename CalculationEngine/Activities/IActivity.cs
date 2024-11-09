using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common;

namespace CalculationEngine.Activities
{
    /// <summary>
    /// Stores basic information about a single affordance activation. This can be an affordance with
    /// known duration or a remote affordance of unknown duration, both with or without traveling.
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Specifies whether the duration of the affordance activation is already
        /// known in advance. If not, the affordance is a remote affordance or a
        /// dynamic travel, whose duration is determined externally.
        /// </summary>
        bool IsDetermined { get; }

        /// <summary>
        /// Specifies whether this activation is a travel activity or an actual
        /// affordance.
        /// </summary>
        bool IsTravel { get; }

        /// <summary>
        /// Name of the affordance activation
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The source of this activation.
        /// </summary>
        string DataSource { get; }

        /// <summary>
        /// Name of the person activating the affordance
        /// </summary>
        string PersonName { get; }

        ICalcAffordanceBase Affordance { get; }

        /// <summary>
        /// ID of the point of interest where the affordance will be carried out, or where
        /// the destination of the travel is. null means the destination is at home.
        /// </summary>
        PointOfInterestId? Destination { get; }

        /// <summary>
        /// The timestep in which the activity started
        /// </summary>
        TimeStep? StartTime { get; }

        bool LightingSwitchedOn { get; }

        bool WasInterrupted { get; set; }

        /// <summary>
        /// Creates a message about starting the activity. Can be logged as a thought of
        /// the activating person.
        /// </summary>
        /// <returns>thought message about starting the activity</returns>
        string GetStartThought();

        void Start(TimeStep timestep, DayLightStatus dayLightStatus, ICalcSite? currentSite);

        bool IsFinished(TimeStep timestep, RemoteActivityFinished? remoteActivityResult);

        int Finish(TimeStep timestep, RemoteActivityFinished? remoteActivityResult);
    }
}
