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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Newtonsoft.Json;
using Common.CalcDto;
using JetBrains.Annotations;

namespace Common.SQLResultLogging.Loggers {

    public record TimeShiftableDeviceActivation : IHouseholdKey
    {
        public TimeShiftableDeviceActivation(CalcDeviceDto device, TimeStep earliestStart, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            Device = device;
            EarliestStart = earliestStart;
            HouseholdKey = householdKey;
        }

        public CalcDeviceDto Device { get; }
        public TimeStep EarliestStart { get; set; }
        public TimeStep LatestStart { get; set; }
        public int TotalDuration { get; set; }
        public List<TimeShiftableDeviceProfile> Profiles { get; set; } = new List<TimeShiftableDeviceProfile>();
        public HouseholdKey HouseholdKey { get; }
    }
    public record TimeShiftableDeviceProfile
    {
        public TimeShiftableDeviceProfile(CalcLoadTypeDto loadType, int timeOffsetInSteps, List<double> values)
        {
            LoadType = loadType;
            TimeOffsetInSteps = timeOffsetInSteps;
            Values = values;
        }

        public CalcLoadTypeDto LoadType { get; set; }
        public int TimeOffsetInSteps { get; set; }
        public List<double> Values { get; set; }
    }

    public class  DeviceActivationEntry : IHouseholdKey {
        /// <summary>
        /// for json
        /// </summary>
        [UsedImplicitly]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public DeviceActivationEntry()
        {
        }
        public DeviceActivationEntry(
                                     [JetBrains.Annotations.NotNull] string affordanceName,
                                     [JetBrains.Annotations.NotNull] CalcLoadTypeDto loadType,
                                     double value,
                                     [JetBrains.Annotations.NotNull] string activatorName,
                                     int durationInSteps,
                                     TimeStep timestep, [JetBrains.Annotations.NotNull] CalcDeviceDto calcDeviceDto)
        {
            if (calcDeviceDto == null) {
                throw new LPGException("Calcdevicedto was null");
            }
            AffordanceName = affordanceName;
            LoadTypeGuid = loadType.Guid;
            LoadTypeName = loadType.Name;
            TotalEnergySum = value;
            ActivatorName = activatorName;
            Timestep = timestep;
            DeviceInstanceGuid = calcDeviceDto.DeviceInstanceGuid;
            DurationInSteps = durationInSteps;
            HouseholdKey = calcDeviceDto.HouseholdKey;
        }

        [UsedImplicitly]
        [CanBeNull]
        public StrGuid? DeviceInstanceGuid {
            get;
            set;
        }


        [UsedImplicitly]
        [JsonProperty]
        public TimeStep Timestep { get;  set; }

        //public CalcDeviceDto CalcDeviceDto { get;  set; }
//[UsedImplicitly]
        //public HouseholdKey HouseholdKey => CalcDeviceDto.HouseholdKey;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public string AffordanceName { get;  set; }

        [UsedImplicitly]
        [JsonProperty]
        public StrGuid LoadTypeGuid { get;  set; }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public string LoadTypeName { get;  set; }

        [UsedImplicitly]
        [JsonProperty]
        public double TotalEnergySum { get;  set; }
        /*
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public double[] AllValues { get; set; }
        */

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public string ActivatorName { get;  set; }


        [UsedImplicitly]
        [JsonProperty]
        public int DurationInSteps { get;  set; }

        public HouseholdKey HouseholdKey { get; set; }
    }
}