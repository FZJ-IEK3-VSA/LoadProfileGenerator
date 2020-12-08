
using Automation;

namespace Common.CalcDto {
    public class CalcEnergyStorageSignalDto {
        public double TriggerOffPercent { get; }
        public double TriggerOnPercent { get; }
        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public int ID { get; }
        public double Value { get; }
        [JetBrains.Annotations.NotNull]
        public CalcVariableDto CalcVariableDto { get; }
        public StrGuid Guid { get; }

        public CalcEnergyStorageSignalDto([JetBrains.Annotations.NotNull]string name, int id, double triggerOff, double triggerOn, double value,
                                          [JetBrains.Annotations.NotNull] CalcVariableDto calcVariableDto, StrGuid guid)
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