using CalculationEngine.HouseholdElements;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public class CalcTransformationCondition : CalcBase {
        [NotNull] private readonly CalcVariable _calcVariable;

        private readonly double _maxValue;
        private readonly double _minValue;



        public CalcTransformationCondition([NotNull] string pName,
             [NotNull] CalcVariable calcVariable, double minValue,
            double maxValue, [NotNull] string guid) : base(pName, guid) {
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