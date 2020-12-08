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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using CalculationEngine.Transportation;
using Common;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public abstract class CalcAffordanceBase : CalcBase, ICalcAffordanceBase {
        private static int _calcAffordanceBaseSerialTracker;

        public BodilyActivityLevel BodilyActivityLevel { get; }
        public CalcRepo CalcRepo { get; }

        private readonly ActionAfterInterruption _actionAfterInterruption;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly BitArray _isBusyArray;

        protected CalcAffordanceBase([JetBrains.Annotations.NotNull] string pName,  [JetBrains.Annotations.NotNull] CalcLocation loc,
                                     [JetBrains.Annotations.NotNull] [ItemNotNull] List<CalcDesire> satisfactionvalues,
                                     int miniumAge, int maximumAge, PermittedGender permittedGender, bool needsLight,
                                     bool randomEffect,
                                     [JetBrains.Annotations.NotNull] string pAffCategory, bool isInterruptable, bool isInterrupting,
                                     ActionAfterInterruption actionAfterInterruption, int weight,
                                     bool requireAllAffordances,
                                     CalcAffordanceType calcAffordanceType,
                                     StrGuid guid,
                                     [ItemNotNull] [JetBrains.Annotations.NotNull] BitArray isBusyArray,
                                     BodilyActivityLevel bodilyActivityLevel,[JetBrains.Annotations.NotNull] CalcRepo calcRepo,
                                     CalcSite? site = null) : base(pName, guid)
        {
            CalcAffordanceType = calcAffordanceType;
            BodilyActivityLevel = bodilyActivityLevel;
            CalcRepo = calcRepo;
            Site = site;
            ParentLocation = loc;
            Satisfactionvalues = satisfactionvalues;
            _isBusyArray = new BitArray(calcRepo.CalcParameters.InternalTimesteps);
            //copy to make sure that it is a separate instance
            for (var i = 0; i < isBusyArray.Length; i++)
            {
                _isBusyArray[i] = isBusyArray[i];
            }
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
            CalcAffordanceSerial = _calcAffordanceBaseSerialTracker;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            _calcAffordanceBaseSerialTracker++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
        }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public BitArray IsBusyArray => _isBusyArray;

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public abstract void Activate(TimeStep startTime, string activatorName,
                                      CalcLocation personSourceLocation,
                                      out ICalcProfile personTimeProfile);

        public string AffCategory { get; }

        public abstract ColorRGB AffordanceColor { get; }

        public ActionAfterInterruption AfterInterruption => _actionAfterInterruption;

        public abstract string? AreDeviceProfilesEmpty();

        public abstract bool AreThereDuplicateEnergyProfiles();

        public int CalcAffordanceSerial { get; }

        public CalcAffordanceType CalcAffordanceType { get; }

        public abstract List<CalcSubAffordance> CollectSubAffordances(TimeStep time,
                                                                      bool onlyInterrupting,
                                                                      CalcLocation srcLocation);

        public abstract int DefaultPersonProfileLength { get; }

        public abstract List<CalcAffordance.DeviceEnergyProfileTuple> Energyprofiles { get; }

        //public abstract ICalcProfile CollectPersonProfile();

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public abstract BusynessType IsBusy(TimeStep time,
                                    CalcLocation srcLocation, string calcPersonName,
                                    bool clearDictionaries = true);

        public bool IsInterruptable { get; }

        public bool IsInterrupting { get; }

        public int MaximumAge { get; }

        public int MiniumAge { get; }

        public bool NeedsLight { get; }

        public CalcLocation ParentLocation { get; }

        public PermittedGender PermittedGender { get; }

        public string PrettyNameForDumping => Name;

        //public abstract int PersonProfileDuration { get; }

        public bool RandomEffect { get; }

        public bool RequireAllAffordances { get; }

        public List<CalcDesire> Satisfactionvalues { get; }

        public CalcSite? Site { get; }

        public abstract string SourceTrait { get; }

        public abstract List<CalcSubAffordance> SubAffordances { get; }

        public abstract string? TimeLimitName { get; }

        public int Weight { get; }
    }
}