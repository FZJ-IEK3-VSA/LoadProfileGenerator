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
        /// If given, specifies the expected duration of the activity in timesteps.
        /// </summary>
        int? ExpectedDuration { get; }

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

        /// <summary>
        /// Starts execution of the activity, including activating a related affordance
        /// if necessary.
        /// </summary>
        /// <param name="timestep">start timestep for the activity</param>
        /// <param name="dayLightStatus">daylight status object for lighting simulation</param>
        void Start(TimeStep timestep, DayLightStatus dayLightStatus);

        /// <summary>
        /// Checks whether the activity is over. May only be called after the activity
        /// has been started.
        /// </summary>
        /// <param name="timestep">the current timestep</param>
        /// <param name="remoteActivityResult">an 'activity finished' message object, if
        /// one was received</param>
        /// <returns>true if the activity is over and can be finished, else false</returns>
        bool IsFinished(TimeStep timestep, RemoteActivityFinished? remoteActivityResult);

        /// <summary>
        /// Finishes the activity. This includes related data logging and freeing devices.
        /// </summary>
        /// <param name="timestep">the current timestep</param>
        /// <param name="remoteActivityResult">an 'activity finished' message object, if
        /// one was received</param>
        /// <returns>the actual duration of the activity in timesteps</returns>
        int Finish(TimeStep timestep, RemoteActivityFinished? remoteActivityResult);
    }
}
