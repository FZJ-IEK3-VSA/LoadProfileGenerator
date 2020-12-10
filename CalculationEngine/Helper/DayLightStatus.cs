using JetBrains.Annotations;
using System.Collections;

namespace CalculationEngine.Helper
{
    public class DayLightStatus
    {
        public DayLightStatus([NotNull][ItemNotNull] BitArray status )
        {
            Status = status;
        }

        [NotNull]
        [ItemNotNull]
        public BitArray Status { get; }
    }
}
