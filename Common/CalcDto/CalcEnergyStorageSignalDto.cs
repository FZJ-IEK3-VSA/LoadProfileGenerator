
using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcEnergyStorageSignalDto {
        public double TriggerOffPercent { get; }
        public double TriggerOnPercent { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        public double Value { get; }
        [NotNull]
        public CalcVariableDto CalcVariableDto { get; }
        public StrGuid Guid { get; }

        public CalcEnergyStorageSignalDto([NotNull]string name, int id, double triggerOff, double triggerOn, double value,
                                          [NotNull] CalcVariableDto calcVariableDto, StrGuid guid)
        {
            TriggerOffPercent = triggerOff / 100;
            TriggerOnPercent = triggerOn / 100;
            Name = name;
            ID = id;
            Value = value;
            CalcVariableDto = calcVariableDto;
            Guid = guid;
        }
    }
}