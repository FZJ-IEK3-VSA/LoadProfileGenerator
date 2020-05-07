using System;
using System.IO;
using System.Reflection;
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
using NUnit.Framework;

namespace Calculation.Tests.PostProcessing
{
    [TestFixture]
    public class LocationStatisticsMakerTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            using WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            using DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {StartDateUIString = "15.1.2015", EndDateUIString = "18.1.2015", InternalTimeResolution = "00:01:00"}};
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
            sim.MyGeneralConfig.Enable(CalcOption.LocationsFile);
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
            Assert.AreNotEqual(null, sim);
            sim.MyGeneralConfig.RandomSeed = 10;
            CalcManagerFactory cmf = new CalcManagerFactory();
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0],sim.ModularHouseholds[0],
                EnergyIntensityType.Random,false,version,null,
                LoadTypePriority.All,null,null,null,sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015,1,15), new DateTime(2015,1,18), new TimeSpan(0,1,0),";",10,new TimeSpan(0,1,0),false,false,false,3 ,3,
                calculationProfiler);
            CalcManager cm = cmf.GetCalcManager(sim, wd.WorkingDirectory, csps, false);
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