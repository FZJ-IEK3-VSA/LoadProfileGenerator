using System.IO;
using System.Threading;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using NUnit.Framework;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ChartGeneratorTests : UnitTestBaseClass
    {
        [Test]
        [Category("LongTest3")]
        public void RunChartGeneratorTests() {
            Config.ReallyMakeAllFilesIncludingBothSums = true;
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x => {
                    x.ApplyOptionDefault(OutputFileDefault.All);
                    x.Disable(CalcOption.MakeGraphics);
                    x.Disable(CalcOption.MakePDF);
                });
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccp = new ChartCreationParameters(144,2000,1000,false,GlobalConsts.CSVCharacter,
                new DirectoryInfo(cs.DstDir));
            ChartGeneratorManager cgm = new ChartGeneratorManager(cp,cs.GetFileTracker(),ccp);
            Logger.Info("Making picture");
            cgm.Run(cs.DstDir);
            Logger.Info("finished picture");
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
    }
}