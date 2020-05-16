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

using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    using CalcDto;
    using JetBrains.Annotations;

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
                                     [NotNull] string affordanceName,
                                     [NotNull] CalcLoadTypeDto loadType,
                                     double value,
                                     [NotNull] string activatorName,
                                     int durationInSteps,
                                     TimeStep timestep, [NotNull] CalcDeviceDto calcDeviceDto)
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
            CalcDeviceDto = calcDeviceDto;
            DurationInSteps = durationInSteps;
        }

        [UsedImplicitly]
        [CanBeNull]
        public StrGuid DeviceGuid {
            get {
                if(CalcDeviceDto!= null) {
                    return CalcDeviceDto?.Guid;
                }
                return null;
            }
        }

        [UsedImplicitly]
        [JsonProperty]
        public TimeStep Timestep { get;  set; }

        public CalcDeviceDto CalcDeviceDto { get;  set; }

        [UsedImplicitly]
        public HouseholdKey HouseholdKey => CalcDeviceDto.HouseholdKey;

        [NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public string AffordanceName { get;  set; }

        [NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public StrGuid LoadTypeGuid { get;  set; }

        [NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public string LoadTypeName { get;  set; }

        [UsedImplicitly]
        [JsonProperty]
        public double TotalEnergySum { get;  set; }
        /*
        [NotNull]
        [UsedImplicitly]
        public double[] AllValues { get; set; }
        */

        [NotNull]
        [UsedImplicitly]
        [JsonProperty]
        public string ActivatorName { get;  set; }

        [NotNull]
        [UsedImplicitly]
        public string DeviceName => CalcDeviceDto.Name;

        [UsedImplicitly]
        [JsonProperty]
        public int DurationInSteps { get;  set; }
    }
}