using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.CalcDto {
    public class DeviceEnergyProfileTupleDto
    {
        /// <summary>
        /// for json
        /// </summary>
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public DeviceEnergyProfileTupleDto() { }

        public DeviceEnergyProfileTupleDto([NotNull]string calcDeviceName, [NotNull]string calcDeviceGuid, [NotNull]  CalcProfileDto ep, [NotNull] string calcLoadTypeName,
                                           [NotNull] string calcLoadTypeGuid, decimal timeOffset,
                                           TimeSpan stepsize, double multiplier, double probability)
        {
            CalcDeviceName = calcDeviceName;
            CalcDeviceGuid = calcDeviceGuid;
            TimeProfile = ep;
            CalcLoadTypeName = calcLoadTypeName;
            CalcLoadTypeGuid = calcLoadTypeGuid;
            TimeOffset = timeOffset;
            Multiplier = multiplier;
            var minutesperstep = (decimal)stepsize.TotalMinutes;
            TimeOffsetInSteps = (int)(timeOffset / minutesperstep);
            Probability = probability;
        }
        [JsonProperty]
        public double Multiplier { get; private set; }
        [JsonProperty]
        public double Probability { get; private set; }
        [JsonProperty]
        public decimal TimeOffset { get; private set; }
        [JsonProperty]
        public int TimeOffsetInSteps { get; private set; }
        [NotNull]
        [JsonProperty]
        public string CalcDeviceName { get; private set; }
        [NotNull]
        [JsonProperty]
        public string CalcDeviceGuid { get; private set; }
        [NotNull]
        [JsonProperty]
        public CalcProfileDto TimeProfile { get; private set; }
        [NotNull]
        [JsonProperty]
        public string CalcLoadTypeName { get; private set; }
        [NotNull]
        [JsonProperty]
        public string CalcLoadTypeGuid { get; private set; }

        public override string ToString() => "Device:" + CalcDeviceName + ", Profile " + TimeProfile.Name +
                                             ", Offset " + TimeOffset;
    }
}