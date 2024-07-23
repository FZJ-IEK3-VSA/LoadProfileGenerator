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
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// Implementation of a Subaffordance, an affordance that can be offered to other household members as part of a normal affordance.
    /// </summary>
    public class CalcSubAffordance : CalcKnownDurationAffordance
    {
        private int personProfileDuration;

        public CalcSubAffordance([NotNull] string pName, [NotNull] CalcLocation loc, [NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues,
            int miniumAge, int maximumAge, int delaytimesteps, PermittedGender permittedGender, [NotNull] string pAffCategory,
            bool isInterruptable, bool isInterrupting, [NotNull] CalcAffordance parentAffordance,
            [NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps, int weight,
                                 [NotNull] string sourceTrait,
                                 StrGuid guid, [ItemNotNull][NotNull] BitArray isBusy,
            [NotNull] CalcVariableRepository variableRepository, BodilyActivityLevel bodilyActivityLevel, [NotNull] CalcRepo calcRepo, HouseholdKey hhkey)
            : base(
                pName, loc, satisfactionvalues, miniumAge, maximumAge, permittedGender, false, false, pAffCategory,
                isInterruptable, isInterrupting, ActionAfterInterruption.GoBackToOld, weight, false,
                CalcAffordanceType.Subaffordance, guid, isBusy, bodilyActivityLevel, calcRepo, hhkey, new List<DeviceEnergyProfileTuple>(), LPGColors.Black, sourceTrait, null, variableRepository, variableOps, new())
        {
            Delaytimesteps = delaytimesteps;
            ParentAffordance = parentAffordance;
        }

        public int Delaytimesteps { get; }

        [NotNull] public readonly CalcAffordance ParentAffordance;

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation, out ICalcProfile personTimeProfile)
        {
            MarkAffordanceAsBusy(startTime, personProfileDuration);

            var endTime = startTime.AddSteps(personProfileDuration);
            ExecuteVariableOperations(startTime, endTime, endTime);

            personTimeProfile = new CalcSubAffTimeProfile(personProfileDuration, personProfileDuration + " timesteps Person profile");
        }

        public override BusynessType IsBusy(TimeStep time, CalcLocation srcLocation, CalcPersonDto calcPerson, bool clearDictionaries = true)
        {
            return base.IsBusy(time, srcLocation, calcPerson, clearDictionaries);
        }

        public override List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting, CalcLocation srcLocation) => throw new NotImplementedException();

        public override bool AreThereDuplicateEnergyProfiles() => false;

        [NotNull] public override string AreDeviceProfilesEmpty() => throw new NotImplementedException();

        public override string ToString() => "Sub-Affordance:" + Name;

        public void SetDurations(int duration)
        {
            personProfileDuration = duration;
        }
    }
}