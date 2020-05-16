using System.IO;
using System.Threading;
using Automation;
using ChartCreator2.OxyCharts;
using ChartCreator2.PDF;
using ChartCreator2.Tests.Oxyplot;
using Common;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.PDF {

    public class MigraPDFCreatorTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest4)]
        public void MakeDocumentTest()
        {
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            var name = cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory, null, x =>
                {
                    x.Enable(CalcOption.TotalsPerLoadtype);
                    x.Enable(CalcOption.HouseholdContents);
                });
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccp = new ChartCreationParameters(144, 1600, 1000,
                false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
            using (var fft = cs.GetFileTracker())
            {
                var cg = new ChartGeneratorManager(cp, fft, ccp);
                Logger.Info("Making picture");
                cg.Run(cs.DstDir);
                Logger.Info("finished picture");
                MigraPDFCreator mpc = new MigraPDFCreator(cp);
                mpc.MakeDocument(cs.DstDir, name + ".test", false, false, GlobalConsts.CSVCharacter, fft);
            }
            cs.CleanUp(1);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MakeDocumentTestFull()
        {
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            var name = cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.All, null, x => x.ApplyOptionDefault(OutputFileDefault.All));
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccp = new ChartCreationParameters(144, 1600, 1000, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
            using (FileFactoryAndTracker fft = cs.GetFileTracker())
            {
                var cg = new ChartGeneratorManager(cp, fft, ccp);
                Logger.Info("Making picture");
                cg.Run(cs.DstDir);
                Logger.Info("finished picture");
                MigraPDFCreator mpc = new MigraPDFCreator(cp);
                mpc.MakeDocument(cs.DstDir, name + ".test", false, false, GlobalConsts.CSVCharacter, fft);
            }
            //cs.CleanUp(1);
        }
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MakeDocumentTestFullPDF() {
            var path = @"E:\unittest\MigraPDFCreatorTests.MakeDocumentTestFull";
            var di = new DirectoryInfo(path);
            var fis = di.GetFiles("*.pdf");
            foreach (var fi in fis) {
                fi.Delete();
            }
            FileFactoryAndTracker fft = new FileFactoryAndTracker(path, "hh1", );
            CalculationProfiler cp = new CalculationProfiler();
            MigraPDFCreator mpc  =new MigraPDFCreator(cp);
            mpc.MakeDocument(path, "FullTest.test", false, false, GlobalConsts.CSVCharacter,fft);
            //cs.CleanUp(1);
        }*/
#pragma warning disable VSD0020 // The condition is a constant and thus unnecessary.
#pragma warning disable RECS0110 // Condition is always 'true' or always 'false'
#pragma warning disable S2583 // Conditions should not unconditionally evaluate to "true" or to "false"

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BrokenTest)]
        public void MakeFullPDFDocumentTest()
        {
            //const bool blub = true;
            //// doesn't work on the testing server somehow
            //if (blub)
            //    return;

            Config.ReallyMakeAllFilesIncludingBothSums = true;
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            var name = cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory, null, x =>
                {
                    x.ApplyOptionDefault(OutputFileDefault.All);
                    x.CarpetPlotWidth = 7;
                    x.Disable(CalcOption.MakeGraphics);
                    x.Disable(CalcOption.MakePDF);
                });
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccp = new ChartCreationParameters(144, 1600, 1000, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
            using (var fft = cs.GetFileTracker())
            {
                var cg = new ChartGeneratorManager(cp, fft, ccp);
                Logger.Info("Making picture");
                cg.Run(cs.DstDir);
                Logger.Info("finished picture");
                MigraPDFCreator mpc = new MigraPDFCreator(cp);
                mpc.MakeDocument(cs.DstDir, name + ".test", false, true, GlobalConsts.CSVCharacter, fft);
            }

            Thread.Sleep(5000);
            cs.CleanUp();
#pragma warning restore CS0162 // Unreachable code detected
#pragma warning restore VSD0020 // The condition is a constant and thus unnecessary.
#pragma warning restore S2583 // Conditions should not unconditionally evaluate to "true" or to "false"
#pragma warning restore RECS0110 // Condition is always 'true' or always 'false'
        }
#pragma warning disable CS0162 // Unreachable code detected
        public MigraPDFCreatorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}