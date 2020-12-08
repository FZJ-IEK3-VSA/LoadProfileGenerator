using JetBrains.Annotations;
using System.Collections;

namespace CalculationEngine.Helper
{
    public class DayLightStatus
    {
        public DayLightStatus([JetBrains.Annotations.NotNull][ItemNotNull] BitArray status )
        {
            Status = status;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public BitArray Status { get; }
    }
}
