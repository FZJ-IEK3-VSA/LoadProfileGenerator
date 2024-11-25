using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;
using Common;

namespace CalculationEngine.Activities
{
    public abstract class StaticActivity : Activity
    {
        protected StaticActivity(string dataSource, string personName, ICalcProfile profile) : base(dataSource, personName, null)
        {
            Profile = profile;
        }

        protected StaticActivity(string personName, CalcProfile calcProfile)
            : this(calcProfile.DataSource, personName, calcProfile) { }

        protected StaticActivity(string personName, CalcSubAffTimeProfile calcProfile)
            : this(calcProfile.DataSource, personName, calcProfile) { }

        public override bool IsDetermined => true;

        public override int? ExpectedDuration => Profile.StepValues.Count;

        /// <summary>
        /// The profile specifies how long the person is busy with
        /// this activity.
        /// </summary>
        public ICalcProfile Profile { get; }


        public override string GetStartThought()
        {
            return $"Starting to execute static activity {Name}, time factor {Profile.TimeFactor}, total duration {ExpectedDuration}";
        }

        public override bool IsFinished(TimeStep time, RemoteActivityFinished? remoteActivityResult)
        {
            if (StartTime is null)
                throw new LPGException("Activity has not been activated yet.");
            if (remoteActivityResult is not null)
                throw new LPGException("Received an unexpected remote activity update during a local activity.");

            return time.InternalStep >= StartTime.InternalStep + Profile.StepValues.Count;
        }
    }
}
