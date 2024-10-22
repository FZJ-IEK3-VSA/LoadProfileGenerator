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
    /// <summary>
    /// Represents an affordance that has a timelimit which specifies when the affordance
    /// is available. This limit is implemented using a BitArray.
    /// </summary>
    public abstract class CalcAffordanceWithTimeLimit : CalcAffordanceBase
    {
        /// <summary>
        /// Specifies when this affordance is unavailable, based on its timelimit.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        private readonly BitArray IsBusyArray;

        protected CalcAffordanceWithTimeLimit([NotNull] string pName, [NotNull] CalcLocation loc, [NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, int miniumAge, int maximumAge,
            PermittedGender permittedGender, bool needsLight, bool randomEffect, [NotNull] string pAffCategory, bool isInterruptable, bool isInterrupting, ActionAfterInterruption actionAfterInterruption, int weight,
            bool requireAllAffordances, CalcAffordanceType calcAffordanceType, StrGuid guid, [ItemNotNull][NotNull] BitArray isBusyArray, BodilyActivityLevel bodilyActivityLevel,
            [NotNull] CalcRepo calcRepo, HouseholdKey householdKey, List<DeviceEnergyProfileTuple> energyProfiles, ColorRGB affordanceColor, string sourceTrait, string? timeLimitName,
            [NotNull] CalcVariableRepository variableRepository, [NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps,
            [NotNull][ItemNotNull] List<VariableRequirement> variableRequirements)
            : base(pName, loc, satisfactionvalues, miniumAge, maximumAge, permittedGender, needsLight, randomEffect, pAffCategory, isInterruptable, isInterrupting, actionAfterInterruption, weight, requireAllAffordances,
                  calcAffordanceType, guid, bodilyActivityLevel, calcRepo, householdKey, energyProfiles, affordanceColor, sourceTrait, timeLimitName, variableRepository, variableOps, variableRequirements)
        {
            IsBusyArray = new BitArray(calcRepo.CalcParameters.InternalTimesteps);
            //copy to make sure that it is a separate instance
            // TODO: copying not necessary anymore if IsBusyArray is never changed
            for (var i = 0; i < isBusyArray.Length; i++)
            {
                IsBusyArray[i] = isBusyArray[i];
            }
        }

        /// <summary>
        /// Creates a shallow copy of the passed affordance object.
        /// </summary>
        /// <param name="original">The affordance to copy</param>
        protected CalcAffordanceWithTimeLimit(CalcAffordanceWithTimeLimit original) : base(original)
        {
            IsBusyArray = original.IsBusyArray;
        }

        public override BusynessType IsBusy(TimeStep time, ICalcSite? srcSite, CalcPersonDto calcPerson, bool clearDictionaries = true)
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

            return base.IsBusy(time, srcSite, calcPerson, clearDictionaries);
        }
    }
}
