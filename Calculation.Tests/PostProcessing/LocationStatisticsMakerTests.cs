using System;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using CalculationEngine;
using Common;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.PostProcessing
{
    public class LocationStatisticsMakerTests : UnitTestBaseClass
    {
        public LocationStatisticsMakerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunLocationStatisticsMakerTest()
        {
            using WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            using DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {StartDateUIString = "15.1.2015", EndDateUIString = "18.1.2015", InternalTimeResolution = "00:01:00"}};
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
            sim.MyGeneralConfig.Enable(CalcOption.LocationsFile);
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
            sim.Should().NotBeNull();
            sim.MyGeneralConfig.RandomSeed = 10;
            CalcManagerFactory cmf = new CalcManagerFactory();
                CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0],sim.ModularHouseholds[0],
                EnergyIntensityType.Random,false,null,
                LoadTypePriority.All,null,null,null,sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015,1,15), new DateTime(2015,1,18), new TimeSpan(0,1,0),";",10,new TimeSpan(0,1,0),false,false,false,3 ,3,
                calculationProfiler, wd.WorkingDirectory,false,false, ".");
            CalcManager cm = cmf.GetCalcManager(sim, csps, false);
                //wd.WorkingDirectory, sim.ModularHouseholds[0],  false,
                //sim.TemperatureProfiles[0], sim.GeographicLocations[0], EnergyIntensityType.Random, version,
                //LoadTypePriority.All, null,null,null);

                static bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            ResultFileEntryLogger rfel = new ResultFileEntryLogger(wd.SqlResultLoggingService);
            var resultfileEntries = rfel.Load();
            foreach (var rfe in resultfileEntries) {
                Logger.Info("Result file: " + rfe.FullFileName);
            }
            string dstfilename = Path.Combine(wd.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports), "LocationStatistics.HH1.csv");
            if (!File.Exists(dstfilename)) {
                string filenames = "";
                string dstDir = Path.Combine(wd.WorkingDirectory, DirectoryNames.CalculateTargetdirectory(TargetDirectory.Reports));
                filenames += "Directory to search is:" +dstDir + Environment.NewLine;
                foreach (var fil in new DirectoryInfo(dstDir).GetFiles()) {
                    filenames += fil.FullName + Environment.NewLine;
                }
                throw new LPGException("The file LocationStatistics.HH1.csv was missing. Files in the directory: " + filenames);
            }

            db.Cleanup();
            wd.CleanUp();
        }
    }
}