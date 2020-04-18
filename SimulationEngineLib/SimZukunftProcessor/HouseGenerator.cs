using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.JSON;
using Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicElements;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using Newtonsoft.Json; //using System.Threading.Tasks;

namespace SimulationEngineLib.SimZukunftProcessor {
    public enum AgeRange {
        Child,
        Student,
        Adult,
        Retiree
    }

    public class PersonCategory {
        public PersonCategory(int age, PermittedGender gender)
        {
            switch (age) {
                case int myage when myage <= 18:
                    AgeRange = AgeRange.Child;
                    break;
                case int myage when myage > 18 && myage <= 25:
                    AgeRange = AgeRange.Student;
                    break;
                case int myage when myage > 25 && myage < 65:
                    AgeRange = AgeRange.Adult;
                    break;
                case int myage when myage >= 65:
                    AgeRange = AgeRange.Retiree;
                    break;
                default: throw new LPGException("Nothing found: Age: " + age + " Gender: " + gender);
            }

            Gender = gender;
        }

        public AgeRange AgeRange { get; set; }
        public PermittedGender Gender { get; set; }

        public bool IsMatch([NotNull] PersonCategory pc)
        {
            if (pc.Gender == Gender && pc.AgeRange == AgeRange) {
                return true;
            }

            return false;
        }

        [NotNull]
        public override string ToString() => "Age: " + AgeRange + " Gender:" + Gender;
    }

    public class HouseGenerator {
        public const string DescriptionText = "HouseGenerator Guid ";
        //[NotNull] private static readonly object _errorLogLock = new object();
        private static int _householdErrorCount;

        public static bool AreOfferedCategoriesEnough([NotNull] [ItemNotNull] List<PersonCategory> offered, [NotNull] [ItemNotNull] List<PersonCategory> demanded)
        {
            List<PersonCategory> demandedCopy = demanded.ToList();
            foreach (PersonCategory category in offered) {
                var c = demandedCopy.FirstOrDefault(x => x.IsMatch(category));
                if (c == null) {
                    return false;
                }

                demandedCopy.Remove(c);
            }

            var offeredCopy = offered.ToList();
            foreach (var personCategory in demanded) {
                var c = offeredCopy.FirstOrDefault(x => x.IsMatch(personCategory));
                if (c == null) {
                    return false;
                }

                offeredCopy.Remove(c);
            }

            return true;
        }

        public static void CreateExampleHouseJob([NotNull] string connectionString)
        {
            const string relativePath = "Example\\DistrictDefinition";
            DirectoryInfo diDistrict = new DirectoryInfo(relativePath);
            if (!diDistrict.Exists) {
                diDistrict.Create();
            }

            DirectoryInfo di = diDistrict.Parent ?? throw new LPGException("Parent was null");
            Simulator sim = new Simulator(connectionString);
            HouseData houseData1 = new HouseData(Guid.NewGuid().ToString(), "HT01", 20000, 10000, "MyFirstHouse");

            HouseholdData hhd1 = new HouseholdData(Guid.NewGuid().ToString(), false, "My First Household, template randomly chosen based on persons", null, null,
                null, new List<TransportationDistanceModifier>(), HouseholdDataSpecifictionType.ByPersons);
            HouseholdDataPersonSpecification personSpec = new HouseholdDataPersonSpecification(new List<PersonData>() {
            new PersonData(25, Gender.Male)  });
            hhd1.HouseholdDataPersonSpecification = personSpec;
            houseData1.Households.Add(hhd1);
            HouseholdData hhd2 = new HouseholdData(Guid.NewGuid().ToString(),
                false,
                "My Second Household (with transportation, template defined by name )",
                sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(),
                null, HouseholdDataSpecifictionType.ByTemplateName);
            hhd2.HouseholdTemplateSpecification = new HouseholdTemplateSpecification("CHR01");
            houseData1.Households.Add(hhd2);
            HouseData houseData2 = new HouseData(Guid.NewGuid().ToString(), "HT02", 20000, 10000, "MySecondHouse");
            HouseholdData hhd3 = new HouseholdData(Guid.NewGuid().ToString(), false, "My Third Household, using predefined household",
                null, null, null, null, HouseholdDataSpecifictionType.ByHouseholdName);
            hhd3.HouseholdNameSpecification = new HouseholdNameSpecification("CHR01");
            houseData2.Households.Add(hhd3);
            HouseholdData hhd4 = new HouseholdData(Guid.NewGuid().ToString(), false, "My Fourth Household", null, null, null, null, HouseholdDataSpecifictionType.ByPersons);
            hhd4.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
                new PersonData(75, Gender.Male),
                new PersonData(74, Gender.Female)
            });
            houseData2.Households.Add(hhd4);
            var calculationSettings = new JsonCalcSpecification {
                StartDate = new DateTime(2019, 1, 1),
                EndDate = new DateTime(2019, 1, 3),
                DeleteDAT = true,
                DefaultForOutputFiles = OutputFileDefault.OnlySums
            };
            if (calculationSettings.CalcOptions == null) {
                throw new LPGException("error");
            }

            calculationSettings.CalcOptions.Add(CalcOption.IndividualSumProfiles);
            //calculationSettings.CalcOptions.Add(CalcOption.OverallSum.ToString());
            calculationSettings.CalcOptions.Add(CalcOption.SumProfileExternalEntireHouse);
            calculationSettings.CalcOptions.Add(CalcOption.SumProfileExternalIndividualHouseholds);
            calculationSettings.LoadtypesForPostprocessing?.Add("Electricity");
            calculationSettings.CalculationName = "My Comment";
            calculationSettings.ExternalTimeResolution = "00:15:00";
            calculationSettings.InternalTimeResolution = "00:01:00";
            calculationSettings.LoadTypePriority = LoadTypePriority.RecommendedForHouses;
            calculationSettings.TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference();
            calculationSettings.GeographicLocation = sim.GeographicLocations[0].GetJsonReference();
            calculationSettings.TransportationDeviceSet = sim.TransportationDeviceSets[0].GetJsonReference();
            calculationSettings.TravelRouteSet = sim.TravelRouteSets[0].GetJsonReference();
            calculationSettings.ChargingStationSet = sim.ChargingStationSets[0].GetJsonReference();
            Logger.Info("--------");
            Logger.Info("Writing example file and additional data file that you might need.");
            HouseCreationAndCalculationJob hj = new HouseCreationAndCalculationJob("scenario","year","districtname");
            hj.House = houseData1;
            hj.CalcSpec = calculationSettings;
            HouseJobSerializer.WriteJsonToFile("ExampleHouseJob-1.json",hj);
            hj.House = houseData2;
            HouseJobSerializer.WriteJsonToFile("ExampleHouseJob-2.json", hj);
            Logger.Info("Finished writing example house jobs...");
            WriteGuidList("GuidsForAllHouseholds.csv", sim.ModularHouseholds.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllHouses.csv", sim.Houses.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllChargingStationSets.csv", sim.ChargingStationSets.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllDeviceSelections.csv", sim.DeviceSelections.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllGeographicLocations.csv", sim.GeographicLocations.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllTemperatureProfiles.csv", sim.TemperatureProfiles.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllTransportationDeviceSets.csv", sim.TransportationDeviceSets.It.Select(x => (DBBase)x).ToList(), di);
            WriteGuidList("GuidsForAllTravelRouteSets.csv", sim.TravelRouteSets.It.Select(x => (DBBase)x).ToList(), di);
        }

        /*
        private class TaskExecutor {
            public TaskExecutor([NotNull] Action action) => Action = action;

            [CanBeNull]
            public Exception Ex { get; set; }
            [NotNull]
            public Action Action { get; }

            [CanBeNull]
            public Task MyTask {
                get;
                set;
            }
            public void Run()
            {
                try {
                    Action();
                }catch(Exception ex) {
                    Ex = ex;
                }
            }
        }*/

        //public void Run([NotNull] string districtDefinitionFile,
        //                [NotNull] string sourceConnectionString,
        //                [NotNull] string dstDirectory,
        //                [NotNull] string errorCsvPath,
        //                [NotNull] string calcSpecificationPath)
        //{
        //    Logger.Threshold = Severity.Debug;
        //    Logger.SetLogFilePath("HouseGeneratorLog.txt");
        //    Logger.LogToFile = true;

        //    //json data
        //    string json = File.ReadAllText(districtDefinitionFile);
        //    var districtData = JsonConvert.DeserializeObject<DistrictData>(json);
        //    if (districtData.Houses.Count == 0) {
        //        throw new LPGException("No houses found in " + districtDefinitionFile + ", please select the right file");
        //    }

        //    string calcSpecificationJson = File.ReadAllText(calcSpecificationPath);
        //    var calcSpecification = JsonConvert.DeserializeObject<JsonCalcSpecification>(calcSpecificationJson);
        //    if (calcSpecification == null) {
        //        throw new LPGCommandlineException("Could not read calculation specification file.");
        //    }

        //    if (!Directory.Exists(dstDirectory)) {
        //        Directory.CreateDirectory(dstDirectory);
        //        Thread.Sleep(500);
        //    }

        //    Random r = new Random(1);
        //    string srcDbFile = sourceConnectionString.Replace("Data Source=", "");
        //    if (!File.Exists(srcDbFile)) {
        //        throw new LPGException("Can't find file: " + srcDbFile);
        //    }

        //    if (string.IsNullOrWhiteSpace(districtData.Name)) {
        //        districtData.Name = "NoName";
        //    }

        //    string dstErrorFile = Path.Combine(dstDirectory, "ProfileGenerator." + districtData.Name + ".Errors.txt");
        //    string dstDbFile = Path.Combine(dstDirectory, "ProfileGenerator." + districtData.Name + ".db3");
        //    File.Copy(srcDbFile, dstDbFile, true);
        //    //simulator
        //    string connectionString = "Data Source=" + dstDbFile;
        //    Simulator sim = new Simulator(connectionString);
        //    var settlement = sim.Settlements.CreateNewItem(sim.ConnectionString);
        //    settlement.Name = districtData.Name;
        //    settlement.Description = Guid.NewGuid().ToString();
        //    settlement.SaveToDB();
        //    var geoloc = FindGeographicLocation(sim, calcSpecification);

        //    var temperatureProfile = FindTemperatureProfile(sim, calcSpecification);

        //    //List<string> filesToCreate = new List<string>();
        //    List<string> generatedPathsForDuplicatePathChecking = new List<string>();
        //    List<string> outputFiles = new List<string>();
        //    foreach (var hd in districtData.Houses) {
        //        var customCalcSpec = new JsonCalcSpecification(calcSpecification)
        //        {
        //            OutputDirectory = Path.Combine(dstDirectory, AutomationUtili.CleanFileName(hd.Name??"no name"))
        //        };
        //        if (generatedPathsForDuplicatePathChecking.Contains(customCalcSpec.OutputDirectory))
        //        {
        //            throw new LPGException("The directory " + customCalcSpec.OutputDirectory + " is in two houses. This is not very useful. Please fix. Aborting.");
        //        }

        //        generatedPathsForDuplicatePathChecking.Add(customCalcSpec.OutputDirectory);

        //        CreateSingleHouse(dstDirectory,  hd, districtData.SkipExistingHouses,
        //            sim,  geoloc, temperatureProfile, r,
        //            settlement, customCalcSpec,  dstDbFile, outputFiles);
        //    }

        //    try {
        //        SimIntegrityChecker.Run(sim);
        //    }
        //    catch (Exception ex) {
        //        using (StreamWriter sw = new StreamWriter(dstErrorFile)) {
        //            sw.WriteLine(ex.Message);
        //            sw.WriteLine(ex.StackTrace);
        //            sw.Close();
        //        }
        //    }

        //    /*foreach (string s in filesToCreate)
        //    {
        //        File.Copy(srcDbFile, s, true);
        //    }*/
        //}

        [CanBeNull]
        private static TemperatureProfile FindTemperatureProfile([NotNull] Simulator sim, [NotNull] JsonCalcSpecification calcSpecification)
        {
            TemperatureProfile temperatureProfile = sim.TemperatureProfiles[0];
            if (calcSpecification.TemperatureProfile != null) {
                temperatureProfile = sim.TemperatureProfiles.FindByJsonReference(calcSpecification.TemperatureProfile);
            }

            return temperatureProfile;
        }

        [CanBeNull]
        private static GeographicLocation FindGeographicLocation([NotNull] Simulator sim, [NotNull] JsonCalcSpecification calcSpecification)
        {
            GeographicLocation geoloc = sim.GeographicLocations[0];
            if (calcSpecification.GeographicLocation != null) {
                geoloc = sim.GeographicLocations.FindByJsonReference(calcSpecification.GeographicLocation);
            }

            return geoloc;
        }

        public void ProcessSingleHouseJob([NotNull] string houseJobFile)
        {
            string resultDir = "Results";
            try {
                string houseJobStr = File.ReadAllText(houseJobFile);
                HouseCreationAndCalculationJob hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr);
                resultDir = hcj.CalcSpec?.OutputDirectory ?? "Results";
                if (!Directory.Exists(resultDir)) {
                    Directory.CreateDirectory(resultDir);
                    Thread.Sleep(100);
                }
                string finishedFlagFile = Path.Combine(resultDir, Constants.FinishedFileFlag);
                if (File.Exists(finishedFlagFile)) {
                    Logger.Info("File already exists: " + finishedFlagFile);
                    Logger.Info("This calculation seems to be finished. Quitting.");
                    return;
                }
                string srcDbFile = hcj.PathToDatabase ?? throw new LPGException("No db source path");
                if (!File.Exists(srcDbFile)) {
                    throw new LPGException("Could not found source database file: " + srcDbFile);
                }
                //string houseName = AutomationUtili.CleanFileName(hcj.House?.Name ?? "House");
                string dstDbFile = Path.Combine(resultDir, "profilegenerator.copy.db3");

                File.Copy(srcDbFile,dstDbFile, true);
                //File.SetAttributes(dstDbFile, File.GetAttributes(dstDbFile) & ~FileAttributes.ReadOnly);
                string dstConnectionString = "Data Source=" + dstDbFile;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (hcj.House == null) {
                    throw new LPGException("No house was defined");
                }
                if (hcj.CalcSpec == null)
                {
                    throw new LPGException("No calc spec was defined");
                }
                Simulator sim = new Simulator(dstConnectionString);
                var geoloc = FindGeographicLocation(sim, hcj.CalcSpec);
                var temperatureProfile = FindTemperatureProfile(sim, hcj.CalcSpec);
                Random rnd = new Random();
                var newlyCreatedCalcObject =  CreateSingleHouse(hcj,
                    sim,
                    geoloc,
                    temperatureProfile,
                    rnd);
                if (newlyCreatedCalcObject == null) {
                    throw new LPGException("Failed to create a house");
                }
                JsonCalculator jc = new JsonCalculator();
                //hcj.CalcSpec.CalcObject = null;
                jc.StartHousehold(sim, hcj.CalcSpec,newlyCreatedCalcObject);
            }
            catch (Exception ex) {
                var rdi = new DirectoryInfo(resultDir);
                if (!rdi.Exists) {
                    rdi.Create();
                }
                using (StreamWriter sw = new StreamWriter( Path.Combine( resultDir, "CalculationExceptions.txt"))) {
                    sw.WriteLine(ex.Message);
                    sw.WriteLine(ex.StackTrace);
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine(GetAllFootprints(ex));
                    sw.Close();
                }
                throw;
            }
        }
        [NotNull]
        public string GetAllFootprints([NotNull] Exception x)
        {
            var st = new StackTrace(x, true);
            var frames = st.GetFrames();
            if (frames == null) {
                return "(no frames found)";
            }

            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1) {
                    continue;
                }
                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
            }

            return traceString.ToString();
        }
        [CanBeNull]
        private static JsonReference CreateSingleHouse(
                                              [NotNull] HouseCreationAndCalculationJob hj,
                                              [NotNull] Simulator sim,
                                              GeographicLocation geoloc,
                                              TemperatureProfile temperatureProfile,
                                              Random r)
        {
            if (hj.House == null) {
                throw new LPGException("No house data was set in the file");
            }
            //string name = hj.House?.Name ?? "";
            var housedata = hj.House;
            Logger.Info("Creating new house with " + hj.House?.Households.Count + " households...");

            //List<ModularHousehold> createdHouseholds = new List<ModularHousehold>();
            //make the house
            var house = MakeHouse(sim,  hj.House);
            house.GeographicLocation = geoloc;
            house.TemperatureProfile = temperatureProfile;
            //add the Households
            int householdidx = 1;
            foreach (var householdData in housedata.Households) {
                var hhs = MakeHousehold(sim, householdData, r);

                var chargingStationSet = sim.ChargingStationSets.FindByJsonReference(householdData.ChargingStationSet);
                var travelrouteset = sim.TravelRouteSets.FindByJsonReference(householdData.TravelRouteSet);
                var transportationDeviceSet = sim.TransportationDeviceSets.FindByJsonReference(householdData.TransportationDeviceSet);
                if (householdData.TransportationDistanceModifiers != null &&  travelrouteset != null && householdData.TransportationDistanceModifiers.Count > 0
                    ) {
                    Logger.Info("Settings new travel distances for " + hhs.Name + " " + "");
                    travelrouteset = AdjustTravelDistancesBasedOnModifiers(travelrouteset, sim, house,
                        householdData, householdidx++);
                    Logger.Info("Name of the new travel route set to be used is " + travelrouteset.Name);
                }

                bool enableTransportationModelling = false;
                if (chargingStationSet != null && travelrouteset != null && transportationDeviceSet != null) {
                    Logger.Info("Enabling transportation for household " + hhs.Name);
                    enableTransportationModelling = true;
                }
                house.AddHousehold(hhs, enableTransportationModelling, chargingStationSet, travelrouteset, transportationDeviceSet);

                //createdHouseholds.Add(hhs);
            }
            /*
                if (createdHouseholds.Count == 0) {
                    sim.Houses.DeleteItem(house);
                    continue;
                }*/

            house.SaveToDB();
            Logger.Info("Successfully created house.");

            //saving matching calculation file
            Logger.Info("Creating calculation file.");
            if(house == null)
            {
                throw new LPGException("House generation failed");
            }
            return house.GetJsonReference();
        }

        [NotNull]
        private static TravelRouteSet AdjustTravelDistancesBasedOnModifiers([NotNull] TravelRouteSet travelrouteset,
                                                                            [NotNull] Simulator sim,
                                                                            [NotNull] House house,
                                                                            [NotNull] HouseholdData householdData,
                                                                            int householdidx)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (householdData.TransportationDistanceModifiers == null) {
                throw new LPGException("Was null even though this was checked before the function was called.");
            }
            var newName  = travelrouteset.Name + "(" + house.Name + " - " + householdData.Name + " " + householdidx + ")";
            var adjustedTravelrouteset = new TravelRouteSet(newName, null, sim.ConnectionString, travelrouteset.Description, Guid.NewGuid().ToString());
            adjustedTravelrouteset.SaveToDB();
            sim.TravelRouteSets.It.Add(adjustedTravelrouteset);
            int adjustingDistances = 0;
            foreach (TravelRouteSetEntry oldTravelRouteSetEntry in travelrouteset.TravelRoutes) {
                bool addUnmodifiedRoute = true;
                foreach (var modifier in householdData.TransportationDistanceModifiers)
                {
                    string modRouteKey = modifier.RouteKey.ToLower();
                    if (oldTravelRouteSetEntry.TravelRoute.RouteKey?.ToLower() == modRouteKey) {
                        Logger.Info("Adjusting distances for key " + modifier.RouteKey + "-" + modifier.StepKey + ", total routes in the db: " + sim.TravelRoutes.It.Count);
                        var modStepKey = modifier.StepKey.ToLower();
                        var oldRouteSteps = oldTravelRouteSetEntry.TravelRoute.Steps.Where(x => x.StepKey?.ToLower() == modStepKey).ToList();
                        if (oldRouteSteps.Count > 0) {
                             MakeNewAdjustedRoute(sim, oldTravelRouteSetEntry, adjustingDistances, modRouteKey, modifier, adjustedTravelrouteset);
                             addUnmodifiedRoute = false;
                             adjustingDistances++;
                        }
                    }
                }

                if (addUnmodifiedRoute) {
                    adjustedTravelrouteset.AddRoute(oldTravelRouteSetEntry.TravelRoute);
                }
            }
            //Config.ShowDeleteMessages = true;
            travelrouteset = adjustedTravelrouteset;
            adjustedTravelrouteset.SaveToDB();
            sw.Stop();
            Logger.Info("Total distances adjusted: " + adjustingDistances + ". This took " + sw.Elapsed.TotalSeconds.ToString("F2") + " seconds.");
            return travelrouteset;
        }

        private static void MakeNewAdjustedRoute([NotNull] Simulator sim,
                                                 [NotNull] TravelRouteSetEntry oldTravelRouteSetEntry,
                                                 int adjustingDistances,
                                                 [CanBeNull] string modRouteKey,
                                                 [NotNull] TransportationDistanceModifier modifier,
                                                 [NotNull] TravelRouteSet adjustedTravelrouteset)
        {
            var oldRoute = oldTravelRouteSetEntry.TravelRoute;
            TravelRoute newRoute = new TravelRoute(null,
                sim.ConnectionString,
                oldRoute.Name + " adjustment " + adjustingDistances,
                oldRoute.Description,
                oldRoute.SiteA,
                oldRoute.SiteB,
                Guid.NewGuid().ToString(),
                oldRoute.RouteKey);
            newRoute.SaveToDB();
            sim.TravelRoutes.It.Add(newRoute);
            foreach (var step in oldRoute.Steps) {
                double distance = step.Distance;
                if (step.StepKey?.ToLower() == modRouteKey) {
                    distance = modifier.NewDistanceInMeters;
                }
                newRoute.AddStep(step.Name, step.TransportationDeviceCategory, distance, step.StepNumber, step.StepKey,false);
            }


            newRoute.SaveToDB();
            //Logger.Info("Adjusted route " + newRoute.Name);
            adjustedTravelrouteset.AddRoute(newRoute);
        }

        public static void WriteGuidList([NotNull] string filename, [NotNull] [ItemNotNull] List<DBBase> elements, [NotNull] DirectoryInfo relativePath)
        {
            string fn = relativePath.CombineName(filename);
            using (StreamWriter sw = new StreamWriter(fn)) {
                foreach (var element in elements) {
                    sw.WriteLine(element.PrettyName + ";" + element.Guid);
                }
            }

            Logger.Info("Finished writing " + fn);
            string jsonFile = relativePath.CombineName(filename.Replace(".csv", ".json"));
            using (StreamWriter sw = new StreamWriter(jsonFile)) {
                List<JsonReference> jr = new List<JsonReference>();
                foreach (var element in elements) {
                    jr.Add(element.GetJsonReference());
                }

                sw.WriteLine(JsonConvert.SerializeObject(jr, Formatting.Indented));
            }

            Logger.Info("Finished writing " + jsonFile);
        }

        [NotNull]
        private static House MakeHouse([NotNull] Simulator sim, [NotNull] HouseData hd)
        {
            //house creation
            House house = sim.Houses.CreateNewItem(sim.ConnectionString);
            house.Name = hd.Name ?? hd.HouseGuid;
            house.Description = DescriptionText + hd.HouseGuid;
            house.SaveToDB();
            var housetypecode = hd.HouseTypeCode;
            if (housetypecode == null) {
                throw new LPGException("No house type was set");
            }
            if (housetypecode == null)
            {
                throw new LPGException("Could not find house type " + hd.HouseTypeCode);
            }
            //house type adjustment
            var potentialHts = sim.HouseTypes.It.Where(x => x.Name.StartsWith(housetypecode)).ToList();
            if (potentialHts.Count == 0) {
                throw new LPGException("No house type found for " + housetypecode);
            }

            if (potentialHts.Count > 1) {
                throw new LPGException("Too many house types found for " + housetypecode + ". Try adding a couple of more letters to make it unique. It searches by start of the name.");
            }
            HouseType newHouseType = (HouseType)HouseType.ImportFromItem(potentialHts[0], sim);
            newHouseType.Name = newHouseType.Name + "(" + hd.Name + ")";
            if (hd.TargetHeatDemand != null) {
                newHouseType.HeatingYearlyTotal = hd.TargetHeatDemand.Value;
            }
            if (hd.TargetCoolingDemand != null) {
                newHouseType.CoolingYearlyTotal = hd.TargetCoolingDemand.Value;
            }

            newHouseType.AdjustYearlyEnergy = false;
            newHouseType.AdjustYearlyCooling = false;
            newHouseType.SaveToDB();
            house.HouseType = newHouseType;
            return house;
        }

        [NotNull]
        private static ModularHousehold MakeHousehold([NotNull] Simulator sim, [NotNull] HouseholdData householdData,
                                                      [NotNull] Random r)
        {
            if (sim == null) {
                throw new ArgumentNullException(nameof(sim));
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (householdData.HouseholdDataSpecifictionType) {
                case HouseholdDataSpecifictionType.ByPersons when householdData.HouseholdDataPersonSpecification == null:
                    throw new LPGException("No person specification was set for the household " + householdData.Name);
                case HouseholdDataSpecifictionType.ByPersons when householdData.HouseholdDataPersonSpecification.Persons.Count == 0:
                    throw new LPGException("No persons were defined for the household " + householdData.Name);
                case HouseholdDataSpecifictionType.ByTemplateName when householdData.HouseholdTemplateSpecification == null :
                    throw new LPGException("No household template specification was set for the household " + householdData.Name);
                case HouseholdDataSpecifictionType.ByTemplateName when string.IsNullOrWhiteSpace(householdData.HouseholdTemplateSpecification.HouseholdTemplateName):
                    throw new LPGException("No household template name was set for the household " + householdData.Name);
                case HouseholdDataSpecifictionType.ByHouseholdName when householdData.HouseholdNameSpecification == null :
                    throw new LPGException("No household specification was set for the household " + householdData.Name);
                case HouseholdDataSpecifictionType.ByHouseholdName when string.IsNullOrWhiteSpace(householdData.HouseholdNameSpecification.HouseholdName):
                    throw new LPGException("No household name was set for the household " + householdData.Name);
            }

            switch (householdData.HouseholdDataSpecifictionType) {
                case HouseholdDataSpecifictionType.ByPersons:
                    return MakeHouseholdBaseOnPersonSpec(sim, householdData, r);
                case HouseholdDataSpecifictionType.ByTemplateName:
                    return MakeHouseholdBaseOnTemplateSpec(sim, householdData);
                case HouseholdDataSpecifictionType.ByHouseholdName:
                    return MakeHouseholdBaseOnHouseholdName(sim, householdData);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        private static ModularHousehold MakeHouseholdBaseOnHouseholdName([NotNull] Simulator sim, [NotNull] HouseholdData householdData)
        {
            HouseholdNameSpecification nameSpec = householdData.HouseholdNameSpecification;
            if (nameSpec == null)
            {
                throw new LPGCommandlineException("Household name specification was null");

            }

            var household = sim.ModularHouseholds.FindFirstByName(nameSpec.HouseholdName, FindMode.StartsWith);
            if (household == null)
            {
                throw new LPGException("No household found for the household name " + nameSpec.HouseholdName);
            }

            var jsonHH = household.GetJson();
            household.Description = DescriptionText + householdData.UniqueHouseholdId;
            var newHH = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
            newHH.ImportFromJsonTemplate(jsonHH, sim);
            household.SaveToDB();
            Logger.Info("Finished household with template " + household.Name);
            return newHH;
        }
        [NotNull]
        private static ModularHousehold MakeHouseholdBaseOnTemplateSpec([NotNull] Simulator sim, [NotNull] HouseholdData householdData)
        {
            HouseholdTemplateSpecification templateSpec = householdData.HouseholdTemplateSpecification;
            if (templateSpec == null)
            {
                throw new LPGCommandlineException("Household template specification was null");

            }

            var template = sim.HouseholdTemplates.FindFirstByName(templateSpec.HouseholdTemplateName, FindMode.StartsWith);
            if (template == null) {
                throw new LPGException("No household template found for the household template name " + templateSpec.HouseholdTemplateName);
            }
            template.Count = 1;
            Logger.Info("Generating household with template " + template.Name);
            var hhs = template.GenerateHouseholds(sim, false, new List<STTraitLimit>());
            if (hhs.Count != 1)
            {
                throw new Exception("Could not generate this house");
            }

            hhs[0].Description = DescriptionText + householdData.UniqueHouseholdId;
            hhs[0].SaveToDB();
            Logger.Info("Finished generating household with template " + template.Name);
            return hhs[0];
        }
        [NotNull]
        private static ModularHousehold MakeHouseholdBaseOnPersonSpec([NotNull] Simulator sim, [NotNull] HouseholdData householdData, [NotNull] Random r)
        {
            HouseholdDataPersonSpecification personSpec = householdData.HouseholdDataPersonSpecification;
            if (personSpec == null) {
                throw new LPGCommandlineException("Person specification was null");

            }
            List<HouseholdTemplate> templatesWithCorrectPersonCounts =
                sim.HouseholdTemplates.It.Where(x => x.Persons.Count == personSpec.Persons.Count).ToList();

            //make demanded person profile
            List<PersonCategory> demandedPersonCategories = new List<PersonCategory>();
            foreach (PersonData data in personSpec.Persons) {
                demandedPersonCategories.Add(new PersonCategory(data.Age, (PermittedGender)data.Gender));
            }

            List<HouseholdTemplate> selectedHouseholdTemplates = new List<HouseholdTemplate>();
            foreach (var householdTemplate in templatesWithCorrectPersonCounts) {
                List<PersonCategory> thisOfferedCategories = new List<PersonCategory>();
                foreach (var th in householdTemplate.Persons) {
                    thisOfferedCategories.Add(new PersonCategory(th.Person.Age, th.Person.Gender));
                }

                if (AreOfferedCategoriesEnough(thisOfferedCategories, demandedPersonCategories)) {
                    selectedHouseholdTemplates.Add(householdTemplate);
                }
            }

            if (selectedHouseholdTemplates.Count == 0) {
                _householdErrorCount++;
                string s = "Error " + _householdErrorCount + Environment.NewLine + "Not a single household template was found for the household " +
                           householdData.Name + Environment.NewLine;
                s += "Criteria for finding the household were: Persons: " + personSpec.Persons.Count + Environment.NewLine;
                s += "Household templates found with this criteria: " + templatesWithCorrectPersonCounts.Count + Environment.NewLine;
                s += "Requirements for the persons were:" + Environment.NewLine;
                foreach (var cat in demandedPersonCategories) {
                    s += cat + Environment.NewLine;
                }

                Logger.Error(s);
                //throw new LPGException(s);
                if (templatesWithCorrectPersonCounts.Count > 0) {
                    Logger.Error("Using a random template with the same number of people.");
                    selectedHouseholdTemplates = templatesWithCorrectPersonCounts;
                }
                else {
                    Logger.Error("No household found with " + personSpec.Persons.Count + ", using a random template.");
                    selectedHouseholdTemplates = sim.HouseholdTemplates.It.ToList();
                }
            }

            //try to find the right one based on energy use
            var pickedHht = selectedHouseholdTemplates[r.Next(selectedHouseholdTemplates.Count)];
            pickedHht.Count = 1;
            Logger.Info("Generating household with template " + pickedHht.Name);
            var hhs = pickedHht.GenerateHouseholds(sim, false, new List<STTraitLimit>());
            if (hhs.Count != 1) {
                throw new Exception("Could not generate this house");
            }

            hhs[0].Description = DescriptionText + householdData.UniqueHouseholdId;
            hhs[0].SaveToDB();
            Logger.Info("Finished generating household with template " + pickedHht.Name);
            return hhs[0];
        }

        public static void ProcessHouseJob([NotNull] HouseJobProcessingOptions args)
        {
            HouseGenerator hg = new HouseGenerator();
            if (args.JsonPath == null) {
                throw new LPGCommandlineException("Path to the house job file was not set. This won't work.");
            }
            hg.ProcessSingleHouseJob(args.JsonPath);
        }
    }
}