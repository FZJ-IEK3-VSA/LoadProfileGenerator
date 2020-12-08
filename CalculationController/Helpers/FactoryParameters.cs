/*//-----------------------------------------------------------------------

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
using System.Collections.Generic;
using Calculation.Helper;
using Calculation.HouseholdElements;
using Calculation.OnlineDeviceLogging;
using Calculation.OnlineLogging;
using CommonDataWPF;
using CommonDataWPF.Enums;
using DatabaseIO.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalcController.Helpers {
    public class CalcFactoryParameters {
        private static bool _skipChecking;

        [JetBrains.Annotations.NotNull] private readonly DeviceCategoryPicker _picker;

        private EnergyIntensityType _energyIntensity;
        private bool _isinChecking;

        [CanBeNull] private LogFile _logfile;

        [CanBeNull] private Dictionary<VLoadType, CalcLoadType> _ltdict;

        [CanBeNull] private NormalRandom _normalDistributedRandom;

        [CanBeNull] private OnlineDeviceActivationProcessor _odap;

        [CanBeNull] private Random _randomGenerator;

        public CalcFactoryParameters(DeviceCategoryPicker picker) => _picker = picker;

        public EnergyIntensityType EnergyIntensity {
            get {
                CheckCompleteness();
                return _energyIntensity;
            }
            set => _energyIntensity = value;
        }

        public LogFile Logfile {
            get {
                CheckCompleteness();
                return _logfile;
            }
            set => _logfile = value;
        }

        public Dictionary<VLoadType, CalcLoadType> Ltdict {
            get => _ltdict;
            set => _ltdict = value;
        }

        public NormalRandom NormalDistributedRandom {
            get {
                CheckCompleteness();
                return _normalDistributedRandom;
            }
            set => _normalDistributedRandom = value;
        }

        public OnlineDeviceActivationProcessor Odap {
            get {
                CheckCompleteness();
                return _odap;
            }
            set => _odap = value;
        }

        [JetBrains.Annotations.NotNull]
        public DeviceCategoryPicker Picker {
            get {
                CheckCompleteness();
                return _picker;
            }
        }

        public Random RandomGenerator {
            get {
                CheckCompleteness();
                return _randomGenerator;
            }
            set => _randomGenerator = value;
        }

        private void CheckCompleteness() {
            if (_isinChecking) {
                return;
            }
            _isinChecking = true;
            if (_skipChecking) // for unit testing
            {
                return;
            }
            // this function checks for complete initialisation
            if (NormalDistributedRandom == null) {
                throw new LPGException("CalcParameters Bug!");
            }
            if (RandomGenerator == null) {
                throw new LPGException("CalcParameters Bug!");
            }
            if (Logfile == null) {
                throw new LPGException("CalcParameters Bug!");
            }
            if (Picker == null) {
                throw new LPGException("CalcParameters Bug!");
            }
            if (Odap == null) {
                throw new LPGException("CalcParameters Bug!");
            }
            if (_ltdict == null) {
                throw new LPGException("CalcParameters Bug!");
            }
            _isinChecking = false;
        }

        public static void SetSkipChecking(bool skipChecking) {
            _skipChecking = skipChecking;
        }
    }
}*/