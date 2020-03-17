using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Common.Enums;
using Database;
using Database.Helpers;
using Database.Tables.Transportation;
using Database.Tests;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NUnit.Framework;
using SimulationEngine.SimZukunftProcessor;
using Formatting = Newtonsoft.Json.Formatting;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    [TestFixture]
    public class HouseGeneratorTests
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunMatchingTest()
        {
            PersonCategory pc10Male = new PersonCategory(10, PermittedGender.Male);
            PersonCategory pc15Male = new PersonCategory(15, PermittedGender.Male);
            PersonCategory pc20Male = new PersonCategory(20,PermittedGender.Male);
            PersonCategory pc30Male = new PersonCategory(30, PermittedGender.Male);
            PersonCategory pc31Male = new PersonCategory(31, PermittedGender.Male);
            PersonCategory pc40Male = new PersonCategory(40, PermittedGender.Male);
            PersonCategory pc41Male = new PersonCategory(41, PermittedGender.Male);
            List<PersonCategory> demand = new List<PersonCategory>
            {
                pc10Male
            };
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc15Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                Assert.IsTrue(success);
            }
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc20Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                Assert.IsFalse(success);
            }
            demand.Clear();
            demand.Add(pc31Male);
            demand.Add(pc30Male);
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc40Male,
                    pc41Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                Assert.IsTrue(success);
            }
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc40Male,
                    pc15Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                Assert.IsFalse(success);
            }
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorTest()
        {
            Logger.Get().StartCollectingAllMessages();
            HouseData hd = new HouseData(Guid.NewGuid().ToString(),
                "HT01",10000,1000, "MyFirstHouse");
            var hh = new HouseholdData(Guid.NewGuid().ToString(), 3000,  ElectricCarUse.NoElectricCar,
                "blub",null,null,null,null, HouseholdDataSpecifictionType.ByPersons);
            hd.Households.Add(hh);
            hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
                new PersonData(30, Gender.Male)
            });
            HouseGenerator hg = new HouseGenerator();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            string districtDefinitionFile = wd.Combine("DistrictData.json");
            StreamWriter sw = new StreamWriter(districtDefinitionFile);
            DistrictData tkd = new DistrictData
            {
                Name = "hi",
                Houses = new List<HouseData>()
            };
            tkd.Houses.Add(hd);
            var jcs = new JsonCalcSpecification
            {
                DefaultForOutputFiles = OutputFileDefault.Reasonable,
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2017, 1, 3),
                GeographicLocation = sim.GeographicLocations.FindByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
                TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
            };
            sw.WriteLine(JsonConvert.SerializeObject(tkd, Formatting.Indented));
            sw.Close();
            string calcspec = wd.Combine("CalculationSpecification.json");
            StreamWriter sw1 = new StreamWriter(calcspec);
            sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
            sw1.Close();
            string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
            hg.Run( districtDefinitionFile, db.FileName, wd.Combine("Results"),
                errorLog, calcspec);
            var resultsDir = wd.Combine("Results");
            DirectoryInfo di = new DirectoryInfo(resultsDir);
            var jsons = di.GetFiles("*.json");
            foreach (var info in jsons) {
                JsonCalculator jc = new JsonCalculator();
                JsonDirectoryOptions jo = new JsonDirectoryOptions
                {
                    Input = info.FullName
                };
                jc.Calculate(jo);
            }
        }
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorTransportationTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.PerformCleanUpChecks = "false";
            SimIntegrityChecker.Run(sim);
            Logger.Get().StartCollectingAllMessages();
            HouseData hd = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "MyFirstHouse");

            var chargingstationSet = sim.ChargingStationSets.FindByName("Charging At Work with 22 kW");
            if (chargingstationSet == null) {
                throw new LPGException("missing chargingstationset");
            }
            var transportationdevices = sim.TransportationDeviceSets.FindByName("Bus and two slow Cars",FindMode.Partial);
            if (transportationdevices == null) {
                throw new LPGException(("missing transportation devices"));
            }
            var travelRoutes = sim.TravelRouteSets.FindByName("Travel Route Set for 30km to Work");
            if (travelRoutes == null) {
                throw new LPGException("missing travel routes");
            }

            {
                var transportationModifiers = new List<TransportationDistanceModifier> {
                    new TransportationDistanceModifier("Work", "Car", 50000),
                    new TransportationDistanceModifier("Entertainment", "Car", 70000)
                };
                var hh = new HouseholdData(Guid.NewGuid().ToString(),
                    3000,
                    ElectricCarUse.UseElectricCar,
                    "blub",
                    chargingstationSet.GetJsonReference(),
                    transportationdevices.GetJsonReference(),
                    travelRoutes.GetJsonReference(),
                    transportationModifiers, HouseholdDataSpecifictionType.ByPersons);
                hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
                    new PersonData(30, Gender.Male)
                });
                hd.Households.Add(hh);
            }
            {
                var transportationModifiers = new List<TransportationDistanceModifier> {
                    new TransportationDistanceModifier("Work", "Car", 70000),
                    new TransportationDistanceModifier("Entertainment", "Car", 70000)
                };
                var hh = new HouseholdData(Guid.NewGuid().ToString(),
                    4000,
                    ElectricCarUse.UseElectricCar,
                    "blub",
                    chargingstationSet.GetJsonReference(),
                    transportationdevices.GetJsonReference(),
                    travelRoutes.GetJsonReference(),
                    transportationModifiers, HouseholdDataSpecifictionType.ByPersons);
                hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
                    new PersonData(31, Gender.Male)
                });
                hd.Households.Add(hh);
            }

            HouseGenerator hg = new HouseGenerator();

            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            string districtDefinitionFile = wd.Combine("DistrictData.json");
            StreamWriter sw = new StreamWriter(districtDefinitionFile);
            DistrictData tkd = new DistrictData
            {
                Name = "hi",
                Houses = new List<HouseData>()
            };
            tkd.Houses.Add(hd);
            var jcs = new JsonCalcSpecification
            {
                DefaultForOutputFiles = OutputFileDefault.Reasonable,
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2017, 1, 30),
                GeographicLocation = sim.GeographicLocations.FindByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
                TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
            };
            string jsonDistrictDefinition = JsonConvert.SerializeObject(tkd, Formatting.Indented);
            Logger.Info(jsonDistrictDefinition);
            sw.WriteLine(jsonDistrictDefinition);
            sw.Close();
            string calcspec = wd.Combine("CalculationSpecification.json");
            StreamWriter sw1 = new StreamWriter(calcspec);
            sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
            sw1.Close();
            string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
            hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
                errorLog, calcspec);
            var resultsDir = wd.Combine("Results");
            DirectoryInfo di = new DirectoryInfo(resultsDir);
            var jsons = di.GetFiles("*.json");
            foreach (var info in jsons)
            {
                JsonCalculator jc = new JsonCalculator();
                JsonDirectoryOptions jo = new JsonDirectoryOptions
                {
                    Input = info.FullName
                };
                jc.Calculate(jo);
            }
            SimIntegrityChecker.Run(sim);
        }


        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorModifiedRouteTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.PerformCleanUpChecks = "false";
            Logger.Get().StartCollectingAllMessages();
            HouseData hd = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "MyFirstHouse");

            var chargingstationSet = sim.ChargingStationSets.FindByName("Charging At Work with 22 kW");
            if (chargingstationSet == null)
            {
                throw new LPGException("missing chargingstationset");
            }
            var transportationdevices = sim.TransportationDeviceSets.FindByName("Bus and two slow Cars", FindMode.Partial);
            if (transportationdevices == null)
            {
                throw new LPGException(("missing transportation devices"));
            }
            var travelRoutes = sim.TravelRouteSets.FindByName("Travel Route Set for 30km to Work");
            if (travelRoutes == null)
            {
                throw new LPGException("missing travel routes");
            }
            Random rnd = new Random();
            for (int i = 0; i < 10; i++) {
                int distance = rnd.Next(100) * 10000;
                MakeRandomHousehold(distance, chargingstationSet, transportationdevices, travelRoutes, hd);
            }

            HouseGenerator hg = new HouseGenerator();

            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            string districtDefinitionFile = wd.Combine("DistrictData.json");
            StreamWriter sw = new StreamWriter(districtDefinitionFile);
            DistrictData tkd = new DistrictData
            {
                Name = "hi",
                Houses = new List<HouseData>()
            };
            tkd.Houses.Add(hd);
            var jcs = new JsonCalcSpecification
            {
                DefaultForOutputFiles = OutputFileDefault.Reasonable,
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2017, 1, 3),
                GeographicLocation = sim.GeographicLocations.FindByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
                TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
            };
            string jsonDistrictDefinition = JsonConvert.SerializeObject(tkd, Formatting.Indented);
            Logger.Info(jsonDistrictDefinition);
            sw.WriteLine(jsonDistrictDefinition);
            sw.Close();
            string calcspec = wd.Combine("CalculationSpecification.json");
            StreamWriter sw1 = new StreamWriter(calcspec);
            sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
            sw1.Close();
            string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
            hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
                errorLog, calcspec);
            Simulator sim2 = new Simulator(db.ConnectionString);
            SimIntegrityChecker.Run(sim2);
        }

        private static void MakeRandomHousehold(int distance, [NotNull] ChargingStationSet chargingstationSet, [NotNull] TransportationDeviceSet transportationdevices, [NotNull] TravelRouteSet travelRoutes, [NotNull] HouseData hd)
        {
            var transportationModifiers = new List<TransportationDistanceModifier> {
                new TransportationDistanceModifier("Work", "Car", distance),
                new TransportationDistanceModifier("Entertainment", "Car", distance)
            };
            var hh = new HouseholdData(Guid.NewGuid().ToString(),
                3000,
                ElectricCarUse.UseElectricCar,
                "blub",
                chargingstationSet.GetJsonReference(),
                transportationdevices.GetJsonReference(),
                travelRoutes.GetJsonReference(),
                transportationModifiers,
                HouseholdDataSpecifictionType.ByPersons);
            hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
                new PersonData(30, Gender.Male)
            });
            hd.Households.Add(hh);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorWithSkipExisitingTest()
        {
            Logger.Get().StartCollectingAllMessages();
            HouseData hd = new HouseData(Guid.NewGuid().ToString(),
                "HT01", 10000, 1000, "MyFirstHouse");
            var hh = new HouseholdData(Guid.NewGuid().ToString(), 3000,  ElectricCarUse.NoElectricCar,
                "blub", null, null, null,null,
                HouseholdDataSpecifictionType.ByPersons);
            hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
                new PersonData(30, Gender.Male)
            });
            hd.Households.Add(hh);
            HouseGenerator hg = new HouseGenerator();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            string districtDefinitionFile = wd.Combine("DistrictData.json");
            StreamWriter sw = new StreamWriter(districtDefinitionFile);
            DistrictData tkd = new DistrictData
            {
                Name = "hi",
                Houses = new List<HouseData>()
            };
            tkd.Houses.Add(hd);
            tkd.SkipExistingHouses = true;
            var jcs = new JsonCalcSpecification
            {
                DefaultForOutputFiles = OutputFileDefault.Reasonable,
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2017, 1, 3),
                GeographicLocation = sim.GeographicLocations.FindByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
                TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
            };
            sw.WriteLine(JsonConvert.SerializeObject(tkd, Formatting.Indented));
            sw.Close();
            string calcspec = wd.Combine("CalculationSpecification.json");
            StreamWriter sw1 = new StreamWriter(calcspec);
            sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
            sw1.Close();
            string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
            Logger.Info("first run");
            hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
                errorLog, calcspec);
            var resultsDir = wd.Combine("Results");
            DirectoryInfo di = new DirectoryInfo(resultsDir);
            var jsons = di.GetFiles("*.json");
            foreach (var info in jsons)
            {
                JsonCalculator jc = new JsonCalculator();
                JsonDirectoryOptions jo = new JsonDirectoryOptions
                {
                    Input = info.FullName
                };
                jc.Calculate(jo);
            }
            Logger.Info("second run");
            hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
                errorLog, calcspec);
        }

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void HouseGeneratorTestFullList()
        {
            DirectoryInfo di = new DirectoryInfo(@"V:\BurgdorfStatistics\Present\08-ValidationExporting # 005-LPGExporter");
         //   var fils = di.GetFiles();
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var ddpo = new DistrictDefinitionProcessingOptions {JsonPath = di.FullName, DstPath = wd.Combine("Results")};
            HouseGenerator.ProcessDistrictDefinitionFiles(ddpo,db.ConnectionString);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseGeneratorFullTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            HouseGenerator.CreateDistrictDefinition(db.ConnectionString);
            DistrictDefinitionProcessingOptions ddpo = new DistrictDefinitionProcessingOptions
            {
                JsonPath = wd.Combine("Example\\DistrictDefinition"),
                DstPath = wd.Combine("Results"),
                CalculationDefinition =  wd.Combine("Example\\CalculationSettings.json")

            };
            HouseGenerator.ProcessDistrictDefinitionFiles(ddpo,db.ConnectionString);
            DirectoryInfo di = new DirectoryInfo(ddpo.DstPath);
            var files = di.GetFiles("*.json");
            foreach (FileInfo fil in files)
            {
                JsonCalculator jc = new JsonCalculator();
                JsonDirectoryOptions jdo = new JsonDirectoryOptions
                {
                    Input = fil.FullName
                };
                jc.Calculate(jdo);
            }
            Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
            db.Cleanup();
        }
    }
}
