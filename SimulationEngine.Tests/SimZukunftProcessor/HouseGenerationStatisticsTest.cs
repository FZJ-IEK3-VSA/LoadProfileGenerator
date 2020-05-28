using Automation;
using Automation.ResultFiles;
using SimulationEngineLib.HouseJobProcessor;
using Xunit;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    public class HouseGenerationStatisticsTest
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunStatistics()
        {
            const string path = @"C:\work\GeneratedHouses";
            const string districtDataJsonPath = @"V:\BurgdorfStatistics\Present\08-ValidationExporting # 005-LPGExporter";
            HouseGenerationStatistics hgs = new HouseGenerationStatistics();
            hgs.Run(path, districtDataJsonPath);
            throw new LPGException("Fix this");
        }
    }
}
