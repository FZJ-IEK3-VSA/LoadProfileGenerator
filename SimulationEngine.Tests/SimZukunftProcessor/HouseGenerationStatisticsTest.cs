using Automation;
using NUnit.Framework;
using SimulationEngine.SimZukunftProcessor;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    public class HouseGenerationStatisticsTest
    {
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunStatistics()
        {
            const string path = @"C:\work\GeneratedHouses";
            const string districtDataJsonPath = @"V:\BurgdorfStatistics\Present\08-ValidationExporting # 005-LPGExporter";
            HouseGenerationStatistics hgs = new HouseGenerationStatistics();
            hgs.Run(path, districtDataJsonPath);
        }
    }
}
