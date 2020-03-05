
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class OutputLoadTypeDto
    {
        public OutputLoadTypeDto([NotNull] CalcLoadTypeDto loadType, double valueScalingFactor, TransformationOutputFactorType factorType)
        {
            LoadType = loadType;
            ValueScalingFactor = valueScalingFactor;
            FactorType = factorType;
        }

        public double ValueScalingFactor { get; }

        public TransformationOutputFactorType FactorType { get; }

        //        public string FactorTypeStr => FactorType.ToString();
        [NotNull]
        public CalcLoadTypeDto LoadType { get; }
    }
}