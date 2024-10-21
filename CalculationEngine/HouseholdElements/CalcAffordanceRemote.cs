#region

using System;
using System.Collections;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.Transportation;
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

        /// <summary>
        /// The specific site where the affordance takes place, including the ID of the
        /// selcted point of interest.
        /// </summary>
        public override CitySite Site { get; }

        /// <summary>
        /// Creates a remote affordance from another affordance that uses a time limit. Creates a shallow copy
        /// of the original affordance.
        /// </summary>
        /// <param name="affordance">the original affordance</param>
        /// <param name="poinOfInterest">the point of interest for the new remote affordance</param>
        public CalcAffordanceRemote(CalcAffordanceWithTimeLimit affordance, CitySite poinOfInterest) : base(affordance)
        {
            Site = poinOfInterest;
        }

        /// <summary>
        /// Creates a new remote from a non-remote affordance. The new remote affordance is based on a shallow copy of the
        /// original affordance.
        /// </summary>
        /// <param name="affordance">the original affordance</param>
        /// <param name="pointOfInterest">the point of interest for the new remote affordance</param>
        /// <returns>the new remote affordance</returns>
        /// <exception cref="LPGException">if the original affordance cannot be turned into a remote affordance</exception>
        public static CalcAffordanceRemote CreateFromNormalAffordance(ICalcAffordanceBase affordance, CitySite pointOfInterest)
        {
            // check if the original affordance can be turned into a remote affordance
            if (affordance is CalcAffordanceRemote)
            {
                throw new LPGException("The affordance to convert is already a remote affordance.");
            }
            if (affordance is AffordanceBaseTransportDecorator)
            {
                throw new LPGException("Cannot convert a transport decorator - pass the source affordance instead");
            }
            if (affordance is not CalcAffordanceWithTimeLimit timelimitAff)
            {
                throw new LPGException("Trying to create a remote affordance from unknown affordance type.");
            }

            return new CalcAffordanceRemote(timelimitAff, pointOfInterest);
        }

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation, out IAffordanceActivation personTimeProfile)
        {
            // execute only variable operations that occur in the beginning
            ExecuteVariableOperations(startTime, startTime, startTime, [VariableExecutionTime.Beginning]);

            // TODO alternative approach: choose an affordance duration just like a normal affordance, and include it in RemoteAffordanceActivation as
            // 'requested stay duration', which the POI can use for stay simulation
            personTimeProfile = new RemoteAffordanceActivation(Name, Name, startTime, Site.PointOfInterest, null, personSourceLocation.CalcSite, this);
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
        /// on whether this affordance is currently active and whether the activation was long enough ago, but
        /// not longer than the permitted buffer time frame.
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <param name="onlyInterrupting">whether only interrupting subaffordances should be collected</param>
        /// <param name="srcLocation">the current location of the person</param>
        /// <returns>a list of available subaffordances</returns>
        public override IEnumerable<ICalcAffordanceBase> CollectSubAffordances(TimeStep time, bool onlyInterrupting, CalcLocation srcLocation)
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
            var availableSubAffs = new List<ICalcAffordanceBase>();
            foreach (var subAffordance in SubAffordances)
            {
                if (!onlyInterrupting || subAffordance.IsInterrupting)
                {
                    if (IsSubaffordanceAvailable(time, srcLocation, subAffordance.GetAsSubAffordance()))
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
        /// <param name="subAffordance">the subaffordance to check</param>
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
