using System;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Newtonsoft.Json;

namespace Common.CalcDto {
    public class DeviceEnergyProfileTupleDto
    {
        /// <summary>
        /// for json
        /// </summary>
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public DeviceEnergyProfileTupleDto() { }

        public DeviceEnergyProfileTupleDto([JetBrains.Annotations.NotNull]string calcDeviceName, StrGuid calcDeviceGuid, [JetBrains.Annotations.NotNull]  CalcProfileDto ep, [JetBrains.Annotations.NotNull] string calcLoadTypeName,
                                           StrGuid calcLoadTypeGuid, decimal timeOffset,
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
        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public string CalcDeviceName { get; private set; }
        [JsonProperty]
        public StrGuid CalcDeviceGuid { get; private set; }
        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public CalcProfileDto TimeProfile { get; private set; }
        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public string CalcLoadTypeName { get; private set; }
        [JsonProperty]
        public StrGuid CalcLoadTypeGuid { get; private set; }

        [JetBrains.Annotations.NotNull]
        public override string ToString() => "Device:" + CalcDeviceName + ", Profile " + TimeProfile.Name +
                                             ", Offset " + TimeOffset;
    }
}