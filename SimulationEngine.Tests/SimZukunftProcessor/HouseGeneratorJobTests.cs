using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tests;
using Newtonsoft.Json;
using SimulationEngineLib.HouseJobProcessor;
using Xunit;
using Xunit.Abstractions;
using Formatting = Newtonsoft.Json.Formatting;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class HouseGeneratorJobTests :UnitTestBaseClass
    {

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void HouseGeneratorTestForPrecreated()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {
                    const string fn = @"C:\work\LPGUnitTest\HouseJob.Felseggstrasse 29.json";
                    string txt = File.ReadAllText(fn);
                    HouseCreationAndCalculationJob houseJob = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(txt);
                    MakeAndCalculateHouseJob(houseJob, sim, wd, db);
                }
            }
        }
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithPersonSpecAndTransport()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir workingDir = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {
                    WorkingDir wd = workingDir;

                    //housedata
                    HouseData houseData = new HouseData(Guid.NewGuid().ToStrGuid(),
                        "HT01", 10000, 1000, "HouseGeneratorJobHouse");
                    var chargingStationSet = sim.ChargingStationSets.SafeFindByName("Charging At Home with 03.7 kW, output results to Car Electricity").GetJsonReference();
                    Logger.Info("Using charging station " + chargingStationSet);
                    var transportationDeviceSet = sim.TransportationDeviceSets[0].GetJsonReference();
                    var travelRouteSet = sim.TravelRouteSets[0].GetJsonReference();
                    var work = new TransportationDistanceModifier("Work", "Car", 0);
                    var entertainment = new TransportationDistanceModifier("Entertainment", "Car", 12000);
                    List<TransportationDistanceModifier> tdm = new List<TransportationDistanceModifier>() { work, entertainment };
                    var householdData = new HouseholdData(Guid.NewGuid().ToString(),
                        "blub", chargingStationSet, transportationDeviceSet,
                        travelRouteSet, tdm, HouseholdDataSpecificationType.ByPersons);
                    houseData.Households.Add(householdData);
                    var persons = new List<PersonData>() {
                new PersonData(30, Gender.Male)
            };
                    householdData.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(persons);
                    HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis", HouseDefinitionType.HouseData);
                    houseJob.House = houseData;

                    MakeAndCalculateHouseJob(houseJob, sim, wd, db);
                }
            }
        }
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithPersonSpec()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {

                    //housedata
                    HouseData houseData = new HouseData(Guid.NewGuid().ToStrGuid(),
                        "HT01", 10000, 1000, "HouseGeneratorJobHouse");
                    var householdData = new HouseholdData(Guid.NewGuid().ToString(),
                        "blub", null, null, null,
                        null, HouseholdDataSpecificationType.ByPersons);
                    houseData.Households.Add(householdData);
                    var persons = new List<PersonData>() {
                new PersonData(30, Gender.Male)
            };
                    householdData.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(persons);
                    HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis", HouseDefinitionType.HouseData);
                    houseJob.House = houseData;

                    MakeAndCalculateHouseJob(houseJob, sim, wd, db);
                }
            }
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithTemplateSpec()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {

                    //housedata
                    HouseData houseData = new HouseData(Guid.NewGuid().ToStrGuid(),
                        "HT01", 10000, 1000, "HouseGeneratorJobHouse");
                    var householdData = new HouseholdData(Guid.NewGuid().ToString(),
                        "blub", null, null, null, null,
                        HouseholdDataSpecificationType.ByTemplateName);
                    houseData.Households.Add(householdData);
                    householdData.HouseholdTemplateSpecification = new HouseholdTemplateSpecification("CHR01");
                    HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis", HouseDefinitionType.HouseData);
                    houseJob.House = houseData;

                    MakeAndCalculateHouseJob(houseJob, sim, wd, db);
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseJobForHeatpump()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {
                    File.Copy(db.FileName, wd.Combine("profilegenerator.db3"));
                    Directory.SetCurrentDirectory(wd.WorkingDirectory);
                    //housedata
                    HouseData houseData = new HouseData(Guid.NewGuid().ToStrGuid(),
                        "HT01", 10000, 1000, "HouseGeneratorJobHouse");
                    HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob(
                        "present", "2019", "trafokreis", HouseDefinitionType.HouseData);
                    houseJob.House = houseData;
                    var householdData = new HouseholdData(Guid.NewGuid().ToString(),
                        "blub", null, null, null, null, HouseholdDataSpecificationType.ByPersons);
                    houseData.Households.Add(householdData);
                    var persons = new List<PersonData>() {
                new PersonData(30, Gender.Male)
            };
                    householdData.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(persons);
                    houseJob.CalcSpec = new JsonCalcSpecification
                    {
                        DefaultForOutputFiles = OutputFileDefault.NoFiles,
                        StartDate = new DateTime(2017, 1, 1),
                        EndDate = new DateTime(2017, 1, 3),
                        GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ??
                                             throw new LPGException("No Berlin in the DB"),
                        TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference(),
                        OutputDirectory = wd.Combine("Results"),
                        CalcOptions = new List<CalcOption>() {
                    CalcOption.IndividualSumProfiles,CalcOption.DeviceProfiles,
                    CalcOption.EnergyStorageFile, CalcOption.EnergyCarpetPlot,
                    CalcOption.HouseholdContents
                }
                    };
                    StartHouseJob(houseJob, wd, "xxx");
                }
            }
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseJobForHouseTypes()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {
                    int count = 0;
                    foreach (var houseType in sim.HouseTypes.It)
                    {
                        count++;
                        if (count < 22)
                        {
                            continue;
                        }
                        Logger.Info("================================================");
                        Logger.Info("================================================");
                        Logger.Info("================================================");
                        Logger.Info("Starting " + houseType.Name);
                        Logger.Info("================================================");
                        Logger.Info("================================================");
                        Logger.Info("================================================");
                        Logger.Get().StartCollectingAllMessages();
                        string htcode = houseType.Name.Substring(0, 4);
                        //housedata
                        const int targetheatdemand = 10000;
                        HouseData houseData = new HouseData(Guid.NewGuid().ToStrGuid(), htcode, targetheatdemand, 1000, "HouseGeneratorJobHouse");
                        HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis", HouseDefinitionType.HouseData);
                        houseJob.House = houseData;

                        houseJob.CalcSpec = new JsonCalcSpecification
                        {
                            DefaultForOutputFiles = OutputFileDefault.Reasonable,
                            StartDate = new DateTime(2017, 1, 1),
                            EndDate = new DateTime(2017, 12, 31),
                            GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ??
                                                 throw new LPGException("No Berlin in the DB"),
                            TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference(),
                            OutputDirectory = wd.Combine("Results." + htcode),
                            SkipExisting = false,

                            CalcOptions = new List<CalcOption>() {
                        CalcOption.IndividualSumProfiles, CalcOption.DeviceProfiles,
                        CalcOption.EnergyStorageFile, CalcOption.EnergyCarpetPlot,
                        CalcOption.HouseholdContents, CalcOption.TotalsPerLoadtype
                    },
                            DeleteDAT = false
                        };
                        StartHouseJob(houseJob, wd, htcode);
                        SqlResultLoggingService srls = new SqlResultLoggingService(houseJob.CalcSpec.OutputDirectory);
                        HouseholdKeyLogger hhkslogger = new HouseholdKeyLogger(srls);
                        var hhks = hhkslogger.Load();
                        TotalsPerLoadtypeEntryLogger tel = new TotalsPerLoadtypeEntryLogger(srls);
                        foreach (var entry in hhks)
                        {
                            if (entry.KeyType == HouseholdKeyType.General)
                            {
                                continue;
                            }
                            Logger.Info(entry.HouseholdKey.ToString());
                            var res = tel.Read(entry.HouseholdKey);
                            foreach (var totalsEntry in res)
                            {
                                Logger.Info(totalsEntry.Loadtype + ": " + totalsEntry.Value);
                                if (totalsEntry.Loadtype.Name == "Space Heating")
                                {
                                    if (Math.Abs(totalsEntry.Value - targetheatdemand) > 10)
                                    {
                                        throw new LPGException("Target heat demand didn't match for " + houseType.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithHouseholdSpec()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {

                    //housedata
                    HouseData houseData = new HouseData(Guid.NewGuid().ToStrGuid(),
                        "HT01", 10000, 1000, "HouseGeneratorJobHouse");
                    var householdData = new HouseholdData(Guid.NewGuid().ToString(),
                        "blub", null, null, null, null,
                        HouseholdDataSpecificationType.ByHouseholdName);
                    houseData.Households.Add(householdData);
                    householdData.HouseholdNameSpecification = new HouseholdNameSpecification(sim.ModularHouseholds[0].GetJsonReference());
                    HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis", HouseDefinitionType.HouseData);
                    houseJob.House = houseData;

                    MakeAndCalculateHouseJob(houseJob, sim, wd, db);
                }
            }
        }




        private static void MakeAndCalculateHouseJob([JetBrains.Annotations.NotNull] HouseCreationAndCalculationJob houseJob, [JetBrains.Annotations.NotNull] Simulator sim, [JetBrains.Annotations.NotNull] WorkingDir wd, [JetBrains.Annotations.NotNull] DatabaseSetup db)
        {
//calcSpec
            houseJob.CalcSpec = new JsonCalcSpecification {
                DefaultForOutputFiles = OutputFileDefault.Reasonable,
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2017, 1, 3),
                GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ??
                                     throw new LPGException("No Berlin in the DB"),
                TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference(),
                OutputDirectory = wd.Combine("Results")
            };
            var dstDir = wd.Combine("profilegenerator.db3");
            File.Copy(db.FileName,dstDir,true);
            houseJob.PathToDatabase = dstDir;

            StartHouseJob(houseJob,  wd, "xxx");
        }

        private static void StartHouseJob([JetBrains.Annotations.NotNull] HouseCreationAndCalculationJob houseJob, [JetBrains.Annotations.NotNull] WorkingDir wd,string fnSuffix)
        {
            string houseJobFile = wd.Combine("houseJob." + fnSuffix+".json");
            if(File.Exists(houseJobFile)) {
                File.Delete(houseJobFile);
            }

            using (StreamWriter sw = new StreamWriter(houseJobFile)) {
                sw.WriteLine(JsonConvert.SerializeObject(houseJob, Formatting.Indented));
                sw.Close();
            }

            Logger.Info("======================================================");

            Logger.Info("======================================================");
            Logger.Info("starting house generation");
            Logger.Info("======================================================");
            HouseGenerator houseGenerator = new HouseGenerator();
            houseGenerator.ProcessSingleHouseJob(houseJobFile,null);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunSinglePredefinedJson()
        {
            Logger.Get().StartCollectingAllMessages();
            Logger.Threshold = Severity.Debug;
            const string srcfile = @"V:\Dropbox\LPGReleases\releases9.4.0\ExampleHouseJob-1.json";
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    FileInfo srcfi = new FileInfo(srcfile);
                    string targetfile = wd.Combine(srcfi.Name);
                    string targetdb = wd.Combine("profilegenerator.db3");
                    File.Copy(db.FileName, targetdb, true);
                    srcfi.CopyTo(targetfile, true);
                    Directory.SetCurrentDirectory(wd.WorkingDirectory);
                    HouseGenerator houseGenerator = new HouseGenerator();
                    houseGenerator.ProcessSingleHouseJob(targetfile,null);
                }
            }
        }

        public HouseGeneratorJobTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
