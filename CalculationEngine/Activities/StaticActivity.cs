using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using System.Collections.Generic;

namespace CalculationEngine.Activities
{
    public abstract class StaticActivity : Activity
    {
        protected StaticActivity(string name, string dataSource, string personName, ICalcProfile profile) : base(name, dataSource, personName, null)
        {
            Profile = profile;
        }

        protected StaticActivity(string personName, CalcProfile calcProfile)
            : this(calcProfile.Name, calcProfile.DataSource, personName, calcProfile) { }

        protected StaticActivity(string personName, CalcSubAffTimeProfile calcProfile)
            : this(calcProfile.Name, calcProfile.DataSource, personName, calcProfile) { }

        public override bool IsDetermined => true;

        /// <summary>
        /// The profile specifies how long the person is busy with
        /// this activity.
        /// </summary>
        public ICalcProfile Profile { get; }


        public override string GetStartThought()
        {
            return $"Starting to execute local affordance {Name}, basis duration {Profile.StepValues.Count} time " +
                $"factor {Profile.TimeFactor}, total duration {Profile.StepValues.Count}";
        }

        public override bool IsFinished(TimeStep time, RemoteActivityFinished? remoteActivityResult)
        {
            if (StartTime is null)
                throw new LPGException("Activity has not been activated yet.");
            if (remoteActivityResult is not null)
                throw new LPGException("Received an unexpected remote activity update during a local activity.");

            return StartTime.InternalStep + Profile.StepValues.Count >= time.InternalStep;
        }
    }
}
