using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Database;
using Database.Helpers;
using Database.Tests;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NUnit.Framework;
using SimulationEngineLib.SimZukunftProcessor;
using Formatting = Newtonsoft.Json.Formatting;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    [TestFixture]
    public class HouseGeneratorJobTests
    {

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void HouseGeneratorTestForPrecreated()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());

            const string fn = @"C:\work\LPGUnitTest\HouseJob.Felseggstrasse 29.json";
            string txt = File.ReadAllText(fn);
            HouseCreationAndCalculationJob houseJob = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(txt);
            MakeAndCalculateHouseJob(houseJob, sim, wd,db);
        }
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithPersonSpecAndTransport()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());

            //housedata
            HouseData houseData = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "HouseGeneratorJobHouse");
            var chargingStationSet = sim.ChargingStationSets.SafeFindByName("Charging At Home with 03.7 kW, output results to Car Electricity").GetJsonReference();
            Logger.Info("Using charging station " + chargingStationSet);
            var transportationDeviceSet = sim.TransportationDeviceSets[0].GetJsonReference();
            var travelRouteSet = sim.TravelRouteSets[0].GetJsonReference();
            var work = new TransportationDistanceModifier("Work", "Car",0);
            var entertainment = new TransportationDistanceModifier("Entertainment", "Car", 12000);
            List<TransportationDistanceModifier> tdm = new List<TransportationDistanceModifier>() {work, entertainment};
            var householdData = new HouseholdData(Guid.NewGuid().ToString(),true,
                "blub", chargingStationSet, transportationDeviceSet,
                travelRouteSet,tdm, HouseholdDataSpecifictionType.ByPersons);
            houseData.Households.Add(householdData);
            var persons = new List<PersonData>() {
                new PersonData(30, Gender.Male)
            };
            householdData.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(persons);
            HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis");
            houseJob.House = houseData;

            MakeAndCalculateHouseJob(houseJob, sim, wd,db);
        }
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithPersonSpec()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());

            //housedata
            HouseData houseData = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "HouseGeneratorJobHouse");
            var householdData = new HouseholdData(Guid.NewGuid().ToString(), false,
                "blub", null, null, null, null, HouseholdDataSpecifictionType.ByPersons);
            houseData.Households.Add(householdData);
            var persons = new List<PersonData>() {
                new PersonData(30, Gender.Male)
            };
            householdData.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(persons);
            HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis");
            houseJob.House = houseData;

            MakeAndCalculateHouseJob(houseJob, sim, wd,db);
        }


        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithTemplateSpec()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());

            //housedata
            HouseData houseData = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "HouseGeneratorJobHouse");
            var householdData = new HouseholdData(Guid.NewGuid().ToString(), false,
                "blub", null, null, null, null,
                HouseholdDataSpecifictionType.ByTemplateName);
            houseData.Households.Add(householdData);
            householdData.HouseholdTemplateSpecification = new HouseholdTemplateSpecification("CHR01");
            HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis");
            houseJob.House = houseData;

            MakeAndCalculateHouseJob(houseJob, sim, wd,db);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseJobForHeatpump()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            File.Copy(db.FileName,wd.Combine("profilegenerator.db3"));
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            //housedata
            HouseData houseData = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "HouseGeneratorJobHouse");
            HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob(
                "present", "2019", "trafokreis");
            houseJob.House = houseData;
            var householdData = new HouseholdData(Guid.NewGuid().ToString(), false,
                "blub", null, null, null, null, HouseholdDataSpecifictionType.ByPersons);
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
            StartHouseJob(houseJob,  wd, "xxx");
        }


        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseJobForHouseTypes()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            int count = 0;
            foreach (var houseType in sim.HouseTypes.It) {
                count++;
                if(count < 22) {
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
                HouseData houseData = new HouseData(Guid.NewGuid().ToString(),htcode, targetheatdemand, 1000, "HouseGeneratorJobHouse");
                HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis");
                houseJob.House = houseData;

                houseJob.CalcSpec = new JsonCalcSpecification {
                    DefaultForOutputFiles = OutputFileDefault.Reasonable,
                    StartDate = new DateTime(2017, 1, 1),
                    EndDate = new DateTime(2017, 12, 31),
                    GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ??
                                         throw new LPGException("No Berlin in the DB"),
                    TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference(),
                    OutputDirectory = wd.Combine("Results." + htcode),
                    SkipExisting =  false,

                    CalcOptions = new List<CalcOption>() {
                        CalcOption.IndividualSumProfiles, CalcOption.DeviceProfiles,
                        CalcOption.EnergyStorageFile, CalcOption.EnergyCarpetPlot,
                        CalcOption.HouseholdContents, CalcOption.TotalsPerLoadtype
                    },
                    DeleteDAT = false
                };
                StartHouseJob(houseJob,  wd, htcode);
                SqlResultLoggingService srls = new SqlResultLoggingService(houseJob.CalcSpec.OutputDirectory);
                HouseholdKeyLogger hhkslogger = new HouseholdKeyLogger(srls);
                var hhks = hhkslogger.Load();
                TotalsPerLoadtypeEntryLogger tel = new TotalsPerLoadtypeEntryLogger(srls);
                foreach (var entry in hhks) {
                    if (entry.KeyType == HouseholdKeyType.General) {
                        continue;
                    }
                    Logger.Info(entry.HouseholdKey.ToString());
                    var res =  tel.Read(entry.HouseholdKey);
                    foreach (var totalsEntry in res) {
                        Logger.Info(totalsEntry.Loadtype + ": " + totalsEntry.Value);
                        if (totalsEntry.Loadtype.Name == "Space Heating") {
                            if (Math.Abs(totalsEntry.Value - targetheatdemand) > 10) {
                                throw new LPGException("Target heat demand didn't match for " + houseType.Name);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorTestWithHouseholdSpec()
        {
            //setup
            Logger.Get().StartCollectingAllMessages();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());

            //housedata
            HouseData houseData = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "HouseGeneratorJobHouse");
            var householdData = new HouseholdData(Guid.NewGuid().ToString(), false,
                "blub", null, null, null, null,
                HouseholdDataSpecifictionType.ByHouseholdName);
            houseData.Households.Add(householdData);
            householdData.HouseholdNameSpecification = new HouseholdNameSpecification("CHR01");
            HouseCreationAndCalculationJob houseJob = new HouseCreationAndCalculationJob("present", "2019", "trafokreis");
            houseJob.House = houseData;

            MakeAndCalculateHouseJob(houseJob, sim, wd,db);
        }




        private static void MakeAndCalculateHouseJob([NotNull] HouseCreationAndCalculationJob houseJob, [NotNull] Simulator sim, [NotNull] WorkingDir wd, [NotNull] DatabaseSetup db)
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

        private static void StartHouseJob([NotNull] HouseCreationAndCalculationJob houseJob, [NotNull] WorkingDir wd,string fnSuffix)
        {
            string houseJobFile = wd.Combine("houseJob." + fnSuffix+".json");
            using (StreamWriter sw = new StreamWriter(houseJobFile)) {
                sw.WriteLine(JsonConvert.SerializeObject(houseJob, Formatting.Indented));
                sw.Close();
            }

            Logger.Info("======================================================");

            Logger.Info("======================================================");
            Logger.Info("starting house generation");
            Logger.Info("======================================================");
            HouseGenerator houseGenerator = new HouseGenerator();
            houseGenerator.ProcessSingleHouseJob(houseJobFile);
        }

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunSinglePredefinedJson()
        {
            Logger.Get().StartCollectingAllMessages();
            Logger.Threshold = Severity.Debug;
            const string srcfile = @"V:\Dropbox\LPGReleases\releases9.4.0\ExampleHouseJob-1.json";
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            FileInfo srcfi = new FileInfo(srcfile);
            string targetfile = wd.Combine(srcfi.Name);
            string targetdb = wd.Combine("profilegenerator.db3");
            File.Copy(db.FileName,targetdb,true);
            srcfi.CopyTo(targetfile, true);
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            HouseGenerator houseGenerator = new HouseGenerator();
            houseGenerator.ProcessSingleHouseJob(targetfile);
        }

    }
}
