using System.Collections;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    /// <summary> TODO: not valid anymore
    /// Represents an affordance whose duration is known. Its duration can be sampled from mean and standard deviation,
    /// but it does not depend on external influences, and therefore the execution time can be determined in advance.
    /// </summary>
    public abstract class CalcKnownDurationAffordance : CalcAffordanceBase
    {
        /// <summary>
        /// Specifies when this affordance is unavailable, based on its timelimit.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        protected readonly BitArray IsBusyArray;

        protected CalcKnownDurationAffordance([NotNull] string pName, [NotNull] CalcLocation loc, [NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, int miniumAge, int maximumAge,
            PermittedGender permittedGender, bool needsLight, bool randomEffect, [NotNull] string pAffCategory, bool isInterruptable, bool isInterrupting, ActionAfterInterruption actionAfterInterruption, int weight,
            bool requireAllAffordances, CalcAffordanceType calcAffordanceType, StrGuid guid, [ItemNotNull][NotNull] BitArray isBusyArray, BodilyActivityLevel bodilyActivityLevel,
            [NotNull] CalcRepo calcRepo, HouseholdKey householdKey, List<DeviceEnergyProfileTuple> energyProfiles, ColorRGB affordanceColor, string sourceTrait, string? timeLimitName,
            [NotNull] CalcVariableRepository variableRepository, [NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps,
            [NotNull][ItemNotNull] List<VariableRequirement> variableRequirements, CalcSite? site = null)
            : base(pName, loc, satisfactionvalues,miniumAge, maximumAge, permittedGender, needsLight, randomEffect, pAffCategory, isInterruptable, isInterrupting, actionAfterInterruption, weight, requireAllAffordances,
                  calcAffordanceType, guid, isBusyArray, bodilyActivityLevel, calcRepo, householdKey, energyProfiles, affordanceColor, sourceTrait, timeLimitName, variableRepository, variableOps, variableRequirements, site)
        {
            IsBusyArray = new BitArray(calcRepo.CalcParameters.InternalTimesteps);
            //copy to make sure that it is a separate instance
            // TODO: copying not necessary anymore if IsBusyArray is never changed
            for (var i = 0; i < isBusyArray.Length; i++)
            {
                IsBusyArray[i] = isBusyArray[i];
            }
        }

        public override BusynessType IsBusy(TimeStep time, CalcLocation srcLocation, CalcPersonDto calcPerson, bool clearDictionaries = true)
        {
            if (time.InternalStep >= IsBusyArray.Length)
            {
                return BusynessType.BeyondTimeLimit;
            }

            // for the timelimit, only the starting timestep is relevant
            if (IsBusyArray[time.InternalStep])
            {
                return BusynessType.Occupied;
            }

            return base.IsBusy(time, srcLocation, calcPerson, clearDictionaries);
        }
    }
}
