using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SimulationEngine")]
[assembly: InternalsVisibleTo("ChartCreator2.Tests")]
[assembly: InternalsVisibleTo("FullCalculation.Tests")]
[assembly: InternalsVisibleTo("LoadProfileGenerator")]
[assembly: InternalsVisibleTo("Calculation")]

namespace ChartCreator2
{
    public static class GlobalConsts
    {
        public const string CSVCharacter = ";";
    }
}
