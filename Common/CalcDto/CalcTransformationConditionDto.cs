using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTransformationConditionDto {
        [NotNull]
        public CalcVariableDto CalcVariableDto { get; }

        public double MaxValue { get; }
        public StrGuid Guid { get; }
        public double MinValue { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        //todo: split in two conditiontypes with the same interface to avoid the can be null mess
        public CalcTransformationConditionDto([NotNull]string name, int id,
                                              [NotNull] CalcVariableDto variableDto, double minValue, double maxValue, StrGuid guid
                                              )
        {
            Name = name;
            ID = id;
            CalcVariableDto = variableDto;
            MinValue = minValue;
            MaxValue = maxValue;
            Guid = guid;
        }
    }
}