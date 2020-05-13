using Automation;
using CalculationEngine.HouseholdElements;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation
{
    public class CalcTransportationDeviceCategory : CalcBase
    {
        public CalcTransportationDeviceCategory([NotNull] string pName, bool isLimitedToSingleLocation,
                                                [NotNull] StrGuid guid) : base(pName, guid) => IsLimitedToSingleLocation = isLimitedToSingleLocation;
        public bool IsLimitedToSingleLocation { get; }
    }
}
