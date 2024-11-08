using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using System.Collections.Generic;

namespace CalculationEngine.Activities
{
    public class LocalActivity : StaticActivity
    {
        public LocalActivity(string personName, CalcProfile calcProfile, CalcAffordanceWithTimeLimit affordance)
            : base(personName, calcProfile)
        {
            Affordance = affordance;
        }

        public LocalActivity(string personName, CalcSubAffTimeProfile calcProfile, CalcAffordanceWithTimeLimit affordance)
            : base(personName, calcProfile)
        {
            Affordance = affordance;
        }

        public override bool IsTravel => false;

        public override CalcAffordanceWithTimeLimit Affordance { get; }

        public override void Start(TimeStep timestep, DayLightStatus dayLightStatus, ICalcSite? currentSite)
        {
            base.Start(timestep, dayLightStatus, currentSite);
            // activate lighting devices; remark: no lighting simulation for remote activities
            LightingSwitchedOn = ActivateLighting(timestep, Profile, Affordance.ParentLocation, dayLightStatus, Affordance.NeedsLight);
        }

        /// <summary>
        /// Activates ligthing devices for an affordance activation, if necessary.
        /// </summary>
        /// <param name="timestep">current timestep</param>
        /// <param name="personCalcProfile">the profile specifying when the person should be marked as busy</param>
        /// <param name="loc">location of the activity</param>
        /// <param name="isDaylight">daylight info object</param>
        /// <param name="needsLight">whether the activity requires light</param>
        /// <returns>whether any lighting device had to be switched on</returns>
        /// <exception cref="LPGException"></exception>
        private bool ActivateLighting(TimeStep timestep, ICalcProfile personCalcProfile, CalcLocation loc, DayLightStatus isDaylight, bool needsLight)
        {
            // initialize light profile
            var isLightActivationneeded = false;
            var lightprofile = new List<double>(personCalcProfile.StepValues.Count);
            for (var i = 0; i < personCalcProfile.StepValues.Count; i++)
            {
                lightprofile.Add(0);
            }
            // determine the lighting profile for the activity
            for (var idx = 0; idx < personCalcProfile.StepValues.Count && idx + timestep.InternalStep < isDaylight.Status.Count; idx++)
            {
                if (personCalcProfile.StepValues[idx] > 0)
                {
                    if (!isDaylight.Status[timestep.InternalStep + idx] && needsLight)
                    {
                        lightprofile[idx] = 1;
                        isLightActivationneeded = true;
                    }
                }
            }

            // activate all light devices at the location
            if (isLightActivationneeded)
            {
                var cp = new CalcProfile(loc.Name + " - light", System.Guid.NewGuid().ToStrGuid(), lightprofile, ProfileType.Relative,
                    "Synthetic for Light Device");

                // this function is for a light device so that the light is turned on, even if someone else was already in the room
                if (loc.LightDevices.Count > 0 && loc.LightDevices[0].LoadCount > 0 &&
                    !loc.LightDevices[0].IsBusyDuringTimespan(timestep, 1, 1, loc.LightDevices[0].Loads[0].LoadType))
                {
                    for (var i = 0; i < loc.LightDevices.Count; i++)
                    {
                        loc.LightDevices[i].SetAllLoadTypesToTimeprofile(cp, timestep, "Light", Name, 1);
                    }
                }
            }
            return isLightActivationneeded;
        }
    }
}
