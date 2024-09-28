//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// Base class for all affordance classes. Defines basic implementations of variable operations and requirement checks as
    /// well as general properties.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public abstract class CalcAffordanceBase : CalcBase, ICalcAffordanceBase, IHouseholdKey
    {
        private readonly ActionAfterInterruption _actionAfterInterruption;

        [JetBrains.Annotations.NotNull] protected readonly CalcVariableRepository _variableRepository;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        protected readonly List<CalcAffordanceVariableOp> _variableOps;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        protected readonly List<VariableRequirement> _variableRequirements;

        /// <summary>
        /// Defines the time frame in which subaffordances can be started after they become initially available.
        /// That means, more than 10 minutes after the delay time required by the subaffordance has passed, the
        /// subaffordance is not available anymore.
        /// </summary>
        private static readonly TimeSpan SubAffordanceStartFrameDuration = new(0, 10, 0);
        /// <summary>
        /// SubAffordanceStartFrameTime converted to timesteps
        /// </summary>
        protected readonly int SubAffordanceStartFrame;

        protected CalcAffordanceBase([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] CalcLocation loc, [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, int miniumAge, int maximumAge,
            PermittedGender permittedGender, bool needsLight, bool randomEffect, [JetBrains.Annotations.NotNull] string pAffCategory, bool isInterruptable, bool isInterrupting, ActionAfterInterruption actionAfterInterruption, int weight,
            bool requireAllAffordances, CalcAffordanceType calcAffordanceType, StrGuid guid, BodilyActivityLevel bodilyActivityLevel, [JetBrains.Annotations.NotNull] CalcRepo calcRepo,
            HouseholdKey householdKey, List<DeviceEnergyProfileTuple> energyProfiles, ColorRGB affordanceColor, string sourceTrait, string? timeLimitName, [JetBrains.Annotations.NotNull] CalcVariableRepository variableRepository,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps, [JetBrains.Annotations.NotNull][ItemNotNull] List<VariableRequirement> variableRequirements,
            CalcSite? site = null) : base(pName, guid)
        {
            CalcAffordanceType = calcAffordanceType;
            BodilyActivityLevel = bodilyActivityLevel;
            CalcRepo = calcRepo;
            HouseholdKey = householdKey;
            Site = site;
            ParentLocation = loc;
            Satisfactionvalues = satisfactionvalues;
            Weight = weight;
            RequireAllAffordances = requireAllAffordances;
            MiniumAge = miniumAge;
            MaximumAge = maximumAge;
            PermittedGender = permittedGender;
            NeedsLight = needsLight;
            RandomEffect = randomEffect;
            AffCategory = pAffCategory;
            IsInterruptable = isInterruptable;
            IsInterrupting = isInterrupting;
            _actionAfterInterruption = actionAfterInterruption;
            Energyprofiles = energyProfiles;
            AffordanceColor = affordanceColor;
            SourceTrait = sourceTrait;
            TimeLimitName = timeLimitName;
            _variableRepository = variableRepository;
            _variableOps = variableOps;
            _variableRequirements = variableRequirements;

            // determine the number of timesteps in the subaffordance starting frame
            SubAffordanceStartFrame = (int)Math.Ceiling(SubAffordanceStartFrameDuration / CalcRepo.CalcParameters.InternalStepsize);
        }

        public BodilyActivityLevel BodilyActivityLevel { get; }

        public CalcRepo CalcRepo { get; }

        public string AffCategory { get; }

        public ColorRGB AffordanceColor { get; }

        public ActionAfterInterruption AfterInterruption => _actionAfterInterruption;

        public CalcAffordanceType CalcAffordanceType { get; }

        public List<DeviceEnergyProfileTuple> Energyprofiles { get; }

        public bool IsInterruptable { get; }

        public bool IsInterrupting { get; }

        public int MaximumAge { get; }

        public int MiniumAge { get; }

        public bool NeedsLight { get; }

        public CalcLocation ParentLocation { get; }

        public PermittedGender PermittedGender { get; }

        public string PrettyNameForDumping => Name;

        public bool RandomEffect { get; }

        public bool RequireAllAffordances { get; }

        public List<CalcDesire> Satisfactionvalues { get; }

        public CalcSite? Site { get; }

        public string SourceTrait { get; }

        public List<CalcSubAffordance> SubAffordances { get; } = new();

        public string? TimeLimitName { get; }

        public int Weight { get; }
        public HouseholdKey HouseholdKey { get; }

        public override string ToString() => "Affordance:" + Name;

        /// <summary>
        /// Checks if any of the contained device profiles are empty or contain only one value
        /// </summary>
        /// <returns>the name of the first empty profile, or null if no profile was empty</returns>
        public virtual string? AreDeviceProfilesEmpty()
        {
            var areDeviceProfilesEmpty = Energyprofiles
                .Where(deviceEnergyProfileTuple => deviceEnergyProfileTuple.TimeProfile.TimeSpanDataPoints.Count < 2)
                .Select(deviceEnergyProfileTuple => deviceEnergyProfileTuple.TimeProfile.Name).FirstOrDefault();

            return areDeviceProfilesEmpty;
        }

        /// <summary>
        /// Checks if the affordance contains multiple energy profiles for the same device for the same load type at
        /// the same time.
        /// </summary>
        /// <returns>true if a duplicate energy profile was found, else false</returns>
        public virtual bool AreThereDuplicateEnergyProfiles()
        {
            foreach (var tuple in Energyprofiles)
            {
                foreach (var subtuple in Energyprofiles)
                {
                    if (tuple != subtuple && tuple.CalcDevice == subtuple.CalcDevice && tuple.LoadType == subtuple.LoadType &&
                        subtuple.TimeOffset == tuple.TimeOffset)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if all variable requirements for this affordance are currently met
        /// </summary>
        /// <returns>true if all variable requirements are met, else false</returns>
        protected bool AreVariableRequirementsMet()
        {

            if (_variableRequirements.Count > 0)
            {
                foreach (var requirement in _variableRequirements)
                {
                    if (!requirement.IsMet())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Schedules all variable operations caused by an activation of this affordance, and immediately triggers operations
        /// in the current time step.
        /// </summary>
        /// <param name="startTime">the starting time step of the affordance activation</param>
        /// <param name="timeLastDeviceEnds">time step in which the last device of the affordance activation ends</param>
        /// <param name="personEndTime">time step in which the person activity in the affordance activation ends</param>
        /// <param name="operationTypesToExecute">the operation execution times to schedule now; operations with other execution 
        /// times are skipped; default: execute all operations</param>
        /// <exception cref="LPGException">if the execution time was not set for a variable operation</exception>
        protected void ExecuteVariableOperations(TimeStep startTime, TimeStep timeLastDeviceEnds, TimeStep personEndTime, HashSet<VariableExecutionTime>? operationTypesToExecute = null)
        {
            foreach (var op in _variableOps)
            {
                if (operationTypesToExecute?.Contains(op.ExecutionTime) == false)
                    // the operation does not have one of the specified ExecutionTimes - skip it
                    continue;
                // figure out end time
                TimeStep time;
                switch (op.ExecutionTime)
                {
                    case VariableExecutionTime.Beginning:
                        time = startTime;
                        break;
                    case VariableExecutionTime.EndOfPerson:
                        time = personEndTime;
                        break;
                    case VariableExecutionTime.EndofDevices:
                        time = timeLastDeviceEnds;
                        break;
                    default:
                        throw new LPGException("Forgotten Variable Execution Time");
                }

                _variableRepository.AddExecutionEntry(op.Name, op.Value, op.CalcLocation, op.VariableAction, time, op.VariableGuid);
                _variableRepository.Execute(startTime);
            }
        }

        /// <summary>
        /// Activates this affordance, meaning that this affordance is carried out  according to the given parameters.
        /// </summary>
        /// <param name="startTime">the start time step the affordance is executed in</param>
        /// <param name="activatorName">the person carrying out the affordance</param>
        /// <param name="personSourceLocation">current location of the activating person</param>
        /// <param name="personTimeProfile">the resulting person profile for the activator</param>
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public abstract void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation, out IAffordanceActivation personTimeProfile);

        public abstract List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting, CalcLocation srcLocation);

        /// <summary>
        /// Determines whether the affordance can be executed with the given parameters.
        /// </summary>
        /// <param name="time">time step when the affordance shall be executed</param>
        /// <param name="srcLocation">current location of the person</param>
        /// <param name="calcPerson">the person to execute the affordance</param>
        /// <param name="clearDictionaries">whether probability and time factor dictionaries shall be cleared</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public virtual BusynessType IsBusy(TimeStep time, CalcLocation srcLocation, CalcPersonDto calcPerson, bool clearDictionaries = true)
        {
            if (!AreVariableRequirementsMet())
            {
                return BusynessType.VariableRequirementsNotMet;
            }
            return BusynessType.NotBusy;
        }
    }
}
