using Automation;
using CalculationEngine.HouseholdElements;

namespace CalculationEngine.HouseElements {
    public class CalcTransformationCondition : CalcBase {
        [JetBrains.Annotations.NotNull] private readonly CalcVariable _calcVariable;

        private readonly double _maxValue;
        private readonly double _minValue;



        public CalcTransformationCondition([JetBrains.Annotations.NotNull] string pName,
             [JetBrains.Annotations.NotNull] CalcVariable calcVariable, double minValue,
            double maxValue, StrGuid guid) : base(pName, guid) {
            _minValue = minValue;
            _maxValue = maxValue;
            _calcVariable = calcVariable;
        }

        public bool GetResult()
        {
            if (_calcVariable.Value >= _minValue && _calcVariable.Value <= _maxValue) {
                return true;
            }

            return false;
        }
    }
}