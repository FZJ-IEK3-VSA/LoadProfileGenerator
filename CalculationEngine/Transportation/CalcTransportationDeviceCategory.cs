using Automation;
using CalculationEngine.HouseholdElements;

namespace CalculationEngine.Transportation
{
    public class CalcTransportationDeviceCategory : CalcBase
    {
        public CalcTransportationDeviceCategory([JetBrains.Annotations.NotNull] string pName, bool isLimitedToSingleLocation,
                                                StrGuid guid) : base(pName, guid) => IsLimitedToSingleLocation = isLimitedToSingleLocation;
        public bool IsLimitedToSingleLocation { get; }
    }
}
