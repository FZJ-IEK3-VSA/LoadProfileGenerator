using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTransformationDeviceDto
    {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull][ItemNotNull]
        public List<CalcTransformationConditionDto> Conditions { get; }
        [CanBeNull]
        [ItemNotNull]
        public List<DataPointDto> Datapoints { get; }
        public double MaxPower { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        public StrGuid Guid { get; }
        public double MaxValue { get; }
        public double MinPower { get; }
        public double MinValue { get; }
        [NotNull]
        [ItemNotNull]
        public List<OutputLoadTypeDto> OutputLoadTypes { get; }
        [NotNull]
        public CalcLoadTypeDto InputLoadType { get; }

        public CalcTransformationDeviceDto([NotNull]string name, int id, double minValue,
                                           double maxValue, double minimumOutputPower, double maximumInputPower, [NotNull]HouseholdKey householdKey,
                                           StrGuid guid, [NotNull][ItemNotNull] List<CalcTransformationConditionDto> conditions,
                                           [CanBeNull][ItemNotNull]List<DataPointDto> datapoints, [NotNull][ItemNotNull]List<OutputLoadTypeDto> outputLoadTypes,
                                           [NotNull] CalcLoadTypeDto inputLoadType)
        {
            Name = name;
            ID = id;
            MinValue = minValue;
            MaxValue = maxValue;
            MinPower = minimumOutputPower;
            MaxPower = maximumInputPower;
            HouseholdKey = householdKey;
            Guid = guid;
            Conditions = conditions;
            Datapoints = datapoints;
            OutputLoadTypes = outputLoadTypes;
            InputLoadType = inputLoadType;
        }
    }
}