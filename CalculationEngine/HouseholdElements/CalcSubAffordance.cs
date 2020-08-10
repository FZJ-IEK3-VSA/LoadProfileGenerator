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
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class CalcSubAffordance : CalcAffordanceBase {
        [ItemNotNull]
        [NotNull]
        private readonly List<CalcAffordanceVariableOp> _variableOps;

        [NotNull] private readonly CalcVariableRepository _repository;
        private int _durations;

        public CalcSubAffordance([NotNull] string pName,[NotNull] CalcLocation loc,[NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues,
            int miniumAge, int maximumAge, int delaytimesteps, PermittedGender permittedGender,[NotNull] string pAffCategory,
            bool isInterruptable, bool isInterrupting,[NotNull] CalcAffordance parentAffordance,
            [NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps, int weight,
                                 [NotNull] string sourceTrait,
                                 StrGuid guid, [ItemNotNull] [NotNull] BitArray isBusy,
            [NotNull] CalcVariableRepository repository, BodilyActivityLevel bodilyActivityLevel, [NotNull] CalcRepo calcRepo)
            : base(
                pName, loc, satisfactionvalues, miniumAge, maximumAge, permittedGender, false, false, pAffCategory,
                isInterruptable, isInterrupting, ActionAfterInterruption.GoBackToOld, weight, false,
                CalcAffordanceType.Subaffordance, guid,isBusy, bodilyActivityLevel,calcRepo )
        {
            Delaytimesteps = delaytimesteps;
            _variableOps = variableOps;
            _repository = repository;
            SubAffordances = new List<CalcSubAffordance>();
            Energyprofiles = new List<CalcAffordance.DeviceEnergyProfileTuple>();
            AffordanceColor = LPGColors.Black;
            SourceTrait = sourceTrait;
            TimeLimitName = null;
            ParentAffordance = parentAffordance;
        }

        public int Delaytimesteps { get; }

        [NotNull]
        public CalcAffordance ParentAffordance { get; }

        private int PersonProfileDuration => _durations;

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation,
             out ICalcProfile personTimeProfile)
        {
            for (var i = 0; i < PersonProfileDuration && i + startTime.InternalStep < IsBusyArray.Length; i++) {
                IsBusyArray[i + startTime.InternalStep] = true;
            }
            foreach (var op in _variableOps) {
                // figure out end time
                TimeStep time;
                switch (op.ExecutionTime) {
                    case VariableExecutionTime.Beginning:
                        time = startTime;
                        break;
                    case VariableExecutionTime.EndOfPerson:
                    case VariableExecutionTime.EndofDevices:
                        time =  startTime.AddSteps( PersonProfileDuration);
                        break;
                    default:
                        throw new LPGException("Forgotten Variable Execution Time");
                }
                _repository.AddExecutionEntry(op.Name, op.Value, op.CalcLocation, op.VariableAction, time,op.VariableGuid);
            }
            personTimeProfile  = new CalcSubAffTimeProfile(_durations,
                _durations + " timesteps Person profile");
        }

        public override int DefaultPersonProfileLength => PersonProfileDuration;

        public override BusynessType IsBusy(TimeStep time, CalcLocation srcLocation, string calcPersonName,
            bool clearDictionaries = true)
        {
            if (IsBusyArray[time.InternalStep]) {
                return BusynessType.Occupied;
            }
            return BusynessType.NotBusy;
        }

        public override List<CalcSubAffordance> CollectSubAffordances(TimeStep time,
                                                                      bool onlyInterrupting,
                                                                      CalcLocation srcLocation) => throw new NotImplementedException();

        public override List<CalcSubAffordance> SubAffordances { get; }
        public override List<CalcAffordance.DeviceEnergyProfileTuple> Energyprofiles { get; }
        public override ColorRGB AffordanceColor { get; }
        public override string SourceTrait { get; }
        public override string TimeLimitName { get; }
        public override bool AreThereDuplicateEnergyProfiles() => false;

        [NotNull]
        public override string AreDeviceProfilesEmpty()
        {
            throw new NotImplementedException();
        }

        public void SetDurations(int duration)
        {
            _durations = duration;
        }

        public override string ToString() => "Sub-Affordance:" + Name;
    }
}