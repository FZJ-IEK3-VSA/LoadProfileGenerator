
namespace Common.CalcDto {
    public class OutputLoadTypeDto
    {
        public OutputLoadTypeDto([JetBrains.Annotations.NotNull] CalcLoadTypeDto loadType, double valueScalingFactor, TransformationOutputFactorType factorType)
        {
            LoadType = loadType;
            ValueScalingFactor = valueScalingFactor;
            FactorType = factorType;
        }

        public double ValueScalingFactor { get; }

        public TransformationOutputFactorType FactorType { get; }

        //        public string FactorTypeStr => FactorType.ToString();
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto LoadType { get; }
    }
}