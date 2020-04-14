using System;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Integrity;
using CalculationController.Queue;
using Common;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tests;
using NUnit.Framework;

namespace IntegrationTests.FullCalc
{
    public static class FullCalculationStarter
    {
        public static void StartHouse(int house)
        {
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = false;
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;
            sim.MyGeneralConfig.StartDateUIString = "1.1.2015";
            sim.MyGeneralConfig.EndDateUIString = "31.1.2015";
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.RandomSeed = 5;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.IndividualSumProfiles);
            sim.MyGeneralConfig.CSVCharacter = ";";
            sim.MyGeneralConfig.SelectedLoadTypePriority = LoadTypePriority.All;
            SimIntegrityChecker.Run(sim);
            //CalcParameters
            //CalcParametersFactory.SetGlobalTimeParameters(sim.MyGeneralConfig);
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //CalcDevice.UseRanges = true;
            var geoloc = sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial);
            if (geoloc == null)
            {
                throw new LPGException("Geoloc was null");
            }
            var house1 =
                sim.Houses.It[house];
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(geoloc,
                sim.TemperatureProfiles[0],house1,EnergyIntensityType.Random,false,
                version,null,LoadTypePriority.All,null,null,null, sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015,1,1),new DateTime(2015,1,31),  new TimeSpan(0,1,0),";",5, new TimeSpan(0,1,0),false,false,false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, path, csps, house1, false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            Logger.ImportantInfo("loading Duration:" + (calcstart - start).TotalSeconds + " seconds");
            wd1.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
        /*
        private static bool OpenTabFunc([CanBeNull] object o) => true;

        private static bool ReportCancelFunc() => true;

        private static bool ReportFinishFuncForHouseAndSettlement([CanBeNull][ItemCanBeNull] List<CalculationResult> a1, bool a2, [CanBeNull] string a3,
            [CanBeNull][ItemCanBeNull] ObservableCollection<ResultFileEntry> a4) => true;

        private static bool ReportFinishFuncForHousehold([CanBeNull] CalculationResult a1, bool a2, [CanBeNull] string a3) => true;

        private static bool SetCalculationEntries([NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries) => true;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static void StartHousehold(int householdNumber, [CanBeNull] string externalTime = null, [CanBeNull] string enddate = null,
            [CanBeNull] string internalTimeResolution = null, [CanBeNull] string showhiddenTime = null,
            CalcObjectType calcObjectType = CalcObjectType.ModularHousehold, bool cleanAfterwards = true,
            bool useRamDrive = true, EnergyIntensityType intensity = EnergyIntensityType.EnergySaving,
            bool skipResults = false, [NotNull] string additionalName = "", [CanBeNull] DatabaseSetup dbSetup = null,
            [CanBeNull] string calcobjectName = null, [CanBeNull] Severity? loggingSeverity = null, [CanBeNull] string startDate = null,
            bool makePDF = false, [CanBeNull] TransportationDeviceSet transportationDeviceSet = null, [CanBeNull] TravelRouteSet travelRouteSet = null)
        {
            var db = dbSetup;
            if (Directory.Exists(@"e:\unittest"))
            {
                try
                {
                    Directory.Delete(@"e:\unittest", true);
                }
                catch (Exception e)
                {
                    Logger.Exception(e);
                }
            }
            var start = DateTime.Now;
            Config.IsInUnitTesting = true;
            var wd = new WorkingDir("FullCalc_" + calcObjectType + "_" + householdNumber + additionalName,
                useRamDrive);
            if (db == null)
            {
                db = new DatabaseSetup("FullCalc_" + calcObjectType + "_" + householdNumber + additionalName, DatabaseSetup.TestPackage.FullCalculation);
            }
            if (loggingSeverity == null)
            {
                Logger.Threshold = Severity.Error;
            }
            else
            {
                Logger.Threshold = (Severity)loggingSeverity;
            }
            var sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.All);
            sim.MyGeneralConfig.StartDateUIString = "01.01.2013";
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";

            if (startDate != null)
            {
                sim.MyGeneralConfig.StartDateUIString = startDate;
            }
            sim.MyGeneralConfig.EndDateUIString = "05.01.2013";
            sim.MyGeneralConfig.Disable(CalcOption.SMAImportFiles);
            if (makePDF)
            {
                sim.MyGeneralConfig.Enable(CalcOption.MakePDF);
                sim.MyGeneralConfig.Enable(CalcOption.MakeGraphics);
                if (!skipResults)
                {
                    throw new LPGException("Creating two sets of PDF doesn't make sense.");
                }
            }
            else
            {
                sim.MyGeneralConfig.Disable(CalcOption.MakePDF);
                sim.MyGeneralConfig.Disable(CalcOption.MakeGraphics);
            }
            sim.MyGeneralConfig.ExternalTimeResolution = sim.MyGeneralConfig.InternalTimeResolution;
            if (enddate != null)
            {
                sim.MyGeneralConfig.EndDateUIString = enddate;
            }
            else
            {
                ConfigSetter.SetSettlingPeriod(-1);
            }
            sim.MyGeneralConfig.RandomSeed = -1; // always randomseed

            if (externalTime != null)
            {
                sim.MyGeneralConfig.ExternalTimeResolution = externalTime;
            }
            if (internalTimeResolution != null)
            {
                sim.MyGeneralConfig.InternalTimeResolution = internalTimeResolution;
            }
            if (showhiddenTime != null)
            {
                sim.MyGeneralConfig.ShowSettlingPeriod = showhiddenTime;
            }
            Assert.AreNotEqual(null, sim);

            var cs = new CalcStarter(sim);

            ICalcObject calcObject;
            switch (calcObjectType)
            {
                case CalcObjectType.ModularHousehold:
                    if (calcobjectName == null)
                    {
                        if (sim.ModularHouseholds.MyItems.Count <= householdNumber)
                        {
                            return;
                        }
                        calcObject = sim.ModularHouseholds[householdNumber];
                    }
                    else
                    {
                        calcObject = sim.ModularHouseholds.FindByName(calcobjectName, FindMode.Partial);
                    }
                    Logger.Error("Number of modular households:" + sim.ModularHouseholds.MyItems.Count);
                    break;
                case CalcObjectType.House:
                    if (sim.Houses.MyItems.Count <= householdNumber)
                    {
                        return;
                    }
                    if (calcobjectName == null)
                    {
                        calcObject = sim.Houses[householdNumber];
                    }
                    else
                    {
                        calcObject = sim.ModularHouseholds.FindByName(calcobjectName, FindMode.Partial);
                    }
                    Logger.Error("Number of houses:" + sim.Houses.MyItems.Count);
                    break;
                case CalcObjectType.Settlement:
                    if (sim.Settlements.MyItems.Count <= householdNumber)
                    {
                        return;
                    }
                    if (calcobjectName == null)
                    {
                        calcObject = sim.Settlements[householdNumber];
                    }
                    else
                    {
                        calcObject = sim.ModularHouseholds.FindByName(calcobjectName, FindMode.Partial);
                    }
                    Logger.Error("Number of settlements:" + sim.Settlements.MyItems.Count);
                    break;
                default: throw new LPGException("Unknown calcObjectType");
            }
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var csps = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                wd.WorkingDirectory, ReportFinishFuncForHousehold, OpenTabFunc, null, sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], calcObject, SetCalculationEntries, intensity, ReportCancelFunc, false,
                version, null, LoadTypePriority.All, transportationDeviceSet, travelRouteSet);
            cs.Start(csps);
            var duration = DateTime.Now - start;
            Logger.Warning("Calculation duration:" + duration);
            if (!skipResults)
            {
                start = DateTime.Now;
                var cg = new ChartGenerator(1600, 1000, 144, false);
                Logger.Error("Making picture");
                var fft = new FileFactoryAndTracker(wd.WorkingDirectory, Utili.GetCurrentMethodAndClass());
                cg.RunAll(wd.WorkingDirectory, GlobalConsts.CSVCharacter, fft);
                duration = DateTime.Now - start;
                Logger.Error("Image creation duration:" + duration);
                start = DateTime.Now;
                //var simulationTimespan = (int) (_calcParameters.OfficialEndTime -_calcParameters.OfficialStartTime).TotalDays;
                //var now = DateTime.Now;
                //var pdfname = Config.CleanFileName(calcObject.Name + "." + simulationTimespan + "days" + "." +now.Year + "." + now.Month + "." + now.Day + "." + now.Hour);

                MigraPDFCreator.MakeDocument(wd.WorkingDirectory, calcObject.Name, false, false,
                    GlobalConsts.CSVCharacter, fft);
                //var rfe = fft.ResultFileList._ResultFiles.First(x => x.Value.ResultFileID == ResultFileID.PDF).Value;
                //File.Copy(rfe.FullFileName, @"c:\work\pdfs\" + pdfname);
                Logger.Info("finished picture");
                GC.Collect();
            }
            if (cleanAfterwards)
            {
                db.Cleanup();
                wd.CleanUp(50);
            }
            duration = DateTime.Now - start;
            Logger.Info("PDF creation duration:" + duration);
        }*/
    }
}