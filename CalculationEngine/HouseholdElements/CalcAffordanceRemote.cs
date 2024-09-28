#region

using System.Collections;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements
{
    public class CalcAffordanceRemote : CalcAffordanceWithTimeLimit
    {
        /// <summary>
        /// Stores all current activations of this affordance. Maps name of the activating person
        /// to the start time of the activation.
        /// </summary>
        private Dictionary<string, TimeStep> _currentActivations = [];

        public CalcAffordanceRemote([NotNull] string pName, [NotNull] CalcLocation loc, bool randomEffect,
            [NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, int miniumAge, int maximumAge, PermittedGender permittedGender, bool needsLight,
            ColorRGB affordanceColor, [NotNull] string pAffCategory, bool isInterruptable, bool isInterrupting, [NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps,
            [NotNull][ItemNotNull] List<VariableRequirement> variableRequirements, ActionAfterInterruption actionAfterInterruption, [NotNull] string timeLimitName, int weight,
            bool requireAllDesires, [NotNull] string srcTrait, StrGuid guid, [NotNull] CalcVariableRepository variableRepository,
            [ItemNotNull][NotNull] BitArray isBusy, BodilyActivityLevel bodilyActivityLevel,
            [NotNull] CalcRepo calcRepo, HouseholdKey householdKey)
            : base(pName, loc, satisfactionvalues, miniumAge, maximumAge, permittedGender, needsLight, randomEffect, pAffCategory, isInterruptable, isInterrupting, actionAfterInterruption, weight, requireAllDesires,
                  CalcAffordanceType.Affordance, guid, isBusy, bodilyActivityLevel, calcRepo, householdKey, [], affordanceColor, srcTrait, timeLimitName, variableRepository, variableOps, variableRequirements)
        {
        }

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation, out IAffordanceActivation personTimeProfile)
        {
            // execute only variable operations that occur in the beginning
            ExecuteVariableOperations(startTime, startTime, startTime, [VariableExecutionTime.Beginning]);

            personTimeProfile = new RemoteAffordanceActivation(Name, Name, startTime, null, personSourceLocation.CalcSite);
        }

        /// <summary>
        /// Finishes an activation of this affordance. Must be called once for each activation.
        /// </summary>
        /// <param name="endTime">the timestep in which the activity ended</param>
        /// <param name="activatorName">the person activating the affordance</param>
        public void Finish(TimeStep endTime, string activatorName)
        {
            // execute only variable operations that occur at the end of the affordance, which is
            // also always the end of the person time
            ExecuteVariableOperations(endTime, endTime, endTime, [VariableExecutionTime.EndofDevices, VariableExecutionTime.EndOfPerson]);
            // TODO: manually check if the variable operations are actually executed in this case
        }

        /// <summary>
        /// Collect all subaffordances of this affordance that are currently available. This depends
        /// on whether this affordance is currently active, whether the activation is far enough behind 
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <param name="onlyInterrupting">whether only interrupting subaffordances should be collected</param>
        /// <param name="srcLocation">the current location of the person</param>
        /// <returns>a list of available subaffordances</returns>
        public override List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting, CalcLocation srcLocation)
        {
            if (SubAffordances.Count == 0)
            {
                return [];
            }

            if (_currentActivations.Count == 0)
            {
                // the affordance is currently not active
                return [];
            }

            // collect all available subaffordances
            var availableSubAffs = new List<CalcSubAffordance>();
            foreach (var subAffordance in SubAffordances)
            {
                if (!onlyInterrupting || subAffordance.IsInterrupting)
                {
                    if (IsSubaffordanceAvailable(time, srcLocation, subAffordance))
                    {
                        availableSubAffs.Add(subAffordance);
                    }
                }
            }
            return availableSubAffs;
        }

        /// <summary>
        /// Checks if there is a current activation of the affordance that offers the specified subaffordance.
        /// </summary>
        /// <param name="time">the timestep for which to check if the subaffordance is available</param>
        /// <param name="srcLocation">the location of the affordance</param>
        /// <param name="subAffordance">the affordance to check</param>
        /// <returns>whether the subaffordance is currently available</returns>
        private bool IsSubaffordanceAvailable(TimeStep time, CalcLocation srcLocation, CalcSubAffordance subAffordance)
        {
            // check all current activations if one of them offers the subaffordance now
            foreach (var kvPair in _currentActivations)
            {
                // start and end time for this activation
                int personStartTime = kvPair.Value.InternalStep;

                // the subaffordance can only be activated after the delay time, but before the buffer time is over
                var isDelayTimePassed = personStartTime + subAffordance.Delaytimesteps < time.InternalStep;
                var isBufferTimePassed = personStartTime + subAffordance.Delaytimesteps + SubAffordanceStartFrame <= time.InternalStep;
                // check if the subaffordance could be activated right now
                var person = new CalcPersonDto("name", null, -1, PermittedGender.All, null, null, null, -1, null, null);
                var isSubAffordanceBusy = subAffordance.IsBusy(time, srcLocation, person);
                if (isDelayTimePassed && !isBufferTimePassed && isSubAffordanceBusy == BusynessType.NotBusy)
                {
                    // TODO: Subaffordance needs to be a RemoteAffordance with variable duration too
                    return true;
                }
            }
            return false;
        }

        public override string ToString() => "RemoteAffordance:" + Name;
    }
}
