﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using Newtonsoft.Json;

//using System.Threading.Tasks;

namespace SimulationEngineLib.HouseJobProcessor
{
    public enum AgeRange
    {
        Child,
        Student,
        Adult,
        Retiree
    }

    public class PersonCategory
    {
        public PersonCategory(int age, PermittedGender gender)
        {
            switch (age)
            {
                case { } myage when myage <= 18:
                    AgeRange = AgeRange.Child;
                    break;
                case { } myage when myage > 18 && myage <= 25:
                    AgeRange = AgeRange.Student;
                    break;
                case { } myage when myage > 25 && myage < 65:
                    AgeRange = AgeRange.Adult;
                    break;
                case { } myage when myage >= 65:
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
            if (pc.Gender == Gender && pc.AgeRange == AgeRange)
            {
                return true;
            }

            return false;
        }

        [NotNull]
        public override string ToString() => "Age: " + AgeRange + " Gender:" + Gender;
    }

    public class HouseGenerator
    {
        public const string DescriptionText = "HouseGenerator Guid ";
        //[JetBrains.Annotations.NotNull] private static readonly object _errorLogLock = new object();
        private static int _householdErrorCount;

        public static bool AreOfferedCategoriesEnough([NotNull][ItemNotNull] List<PersonCategory> offered, [NotNull][ItemNotNull] List<PersonCategory> demanded)
        {
            List<PersonCategory> demandedCopy = demanded.ToList();
            foreach (PersonCategory category in offered)
            {
                var c = demandedCopy.FirstOrDefault(x => x.IsMatch(category));
                if (c == null)
                {
                    return false;
                }

                demandedCopy.Remove(c);
            }

            var offeredCopy = offered.ToList();
            foreach (var personCategory in demanded)
            {
                var c = offeredCopy.FirstOrDefault(x => x.IsMatch(personCategory));
                if (c == null)
                {
                    return false;
                }

                offeredCopy.Remove(c);
            }

            return true;
        }

        public static void CreateExampleHouseJob([NotNull] string connectionString)
        {
            const string relativePathHousejobs = "Example\\HouseJobs";
            DirectoryInfo diHouseJobs = new DirectoryInfo(relativePathHousejobs);
            if (!diHouseJobs.Exists)
            {
                diHouseJobs.Create();
            }
            const string relativePathGuids = "Example\\GuidLists";
            DirectoryInfo diGuids = new DirectoryInfo(relativePathGuids);
            if (!diGuids.Exists)
            {
                diGuids.Create();
            }

            Simulator sim = new Simulator(connectionString);
            HouseData houseData1 = new HouseData(Guid.NewGuid().ToStrGuid(), "HT01", 20000, 10000, "MyFirstHouse");

            HouseholdData hhd1 = new HouseholdData(Guid.NewGuid().ToString(), "My First Household, template randomly chosen based on persons", null, null,
                null, new List<TransportationDistanceModifier>(), HouseholdDataSpecificationType.ByPersons);
            HouseholdDataPersonSpecification personSpec = new HouseholdDataPersonSpecification(new List<PersonData>() {
            new PersonData(25, Gender.Male, "name")  });
            hhd1.HouseholdDataPersonSpec = personSpec;
            houseData1.Households.Add(hhd1);
            HouseholdData hhd2 = new HouseholdData(Guid.NewGuid().ToString(),
                "My Second Household (with transportation, template defined by name )",
                sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(),
                null, HouseholdDataSpecificationType.ByTemplateName);
            hhd2.HouseholdTemplateSpec = new HouseholdTemplateSpecification("CHR01");
            houseData1.Households.Add(hhd2);
            HouseData houseData2 = new HouseData(Guid.NewGuid().ToStrGuid(), "HT02", 20000, 10000, "MySecondHouse");
            HouseholdData hhd3 = new HouseholdData(Guid.NewGuid().ToString(),
                "My Third Household, using predefined household",
                null, null, null, null, HouseholdDataSpecificationType.ByHouseholdName);
            hhd3.HouseholdNameSpec = new HouseholdNameSpecification(sim.ModularHouseholds[0].GetJsonReference());
            houseData2.Households.Add(hhd3);
            HouseholdData hhd4 = new HouseholdData(Guid.NewGuid().ToString(), "My Fourth Household", null, null, null, null, HouseholdDataSpecificationType.ByPersons);
            hhd4.HouseholdDataPersonSpec = new HouseholdDataPersonSpecification(new List<PersonData>() {
                new PersonData(75, Gender.Male, "name1"),
                new PersonData(74, Gender.Female, "name2")
            });
            houseData2.Households.Add(hhd4);
            var calculationSettings = new JsonCalcSpecification
            {
                StartDate = new DateTime(2019, 1, 1),
                EndDate = new DateTime(2019, 1, 3),
                DefaultForOutputFiles = OutputFileDefault.OnlySums
            };
            if (calculationSettings.CalcOptions == null)
            {
                throw new LPGException("error");
            }

            calculationSettings.CalcOptions.Add(CalcOption.HouseSumProfilesFromDetailedDats);
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
            Logger.Info("--------");
            Logger.Info("Writing example file and additional data file that you might need.");
            HouseCreationAndCalculationJob hj = new HouseCreationAndCalculationJob("scenario", "year", "districtname", HouseDefinitionType.HouseData);
            hj.House = houseData1;
            hj.CalcSpec = calculationSettings;
            hj.CalcSpec.OutputDirectory = "Example1-Results";
            HouseJobSerializer.WriteJsonToFile(Path.Combine(diHouseJobs.FullName, "ExampleHouseJob-1.json"), hj);
            hj.House = houseData2;
            hj.CalcSpec.OutputDirectory = "Example2-Results";
            HouseJobSerializer.WriteJsonToFile(Path.Combine(diHouseJobs.FullName, "ExampleHouseJob-2.json"), hj);
            Logger.Info("Finished writing example house jobs...");
            WriteGuidList("GuidsForAllHouseholds.csv", sim.ModularHouseholds.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllHouses.csv", sim.Houses.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllChargingStationSets.csv", sim.ChargingStationSets.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllDeviceSelections.csv", sim.DeviceSelections.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllGeographicLocations.csv", sim.GeographicLocations.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllTemperatureProfiles.csv", sim.TemperatureProfiles.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllTransportationDeviceSets.csv", sim.TransportationDeviceSets.Items.Select(x => (DBBase)x).ToList(), diGuids);
            WriteGuidList("GuidsForAllTravelRouteSets.csv", sim.TravelRouteSets.Items.Select(x => (DBBase)x).ToList(), diGuids);
        }

        /*
        private class TaskExecutor {
            public TaskExecutor([JetBrains.Annotations.NotNull] Action action) => Action = action;

            [CanBeNull]
            public Exception Ex { get; set; }
            [JetBrains.Annotations.NotNull]
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

        //public void Run([JetBrains.Annotations.NotNull] string districtDefinitionFile,
        //                [JetBrains.Annotations.NotNull] string sourceConnectionString,
        //                [JetBrains.Annotations.NotNull] string dstDirectory,
        //                [JetBrains.Annotations.NotNull] string errorCsvPath,
        //                [JetBrains.Annotations.NotNull] string calcSpecificationPath)
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
        //    settlement.Description = Guid.NewGuid().ToStrGuid();
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
            if (calcSpecification.TemperatureProfile != null)
            {
                temperatureProfile = sim.TemperatureProfiles.FindByJsonReference(calcSpecification.TemperatureProfile);
            }

            return temperatureProfile;
        }

        [CanBeNull]
        private static GeographicLocation FindGeographicLocation([NotNull] Simulator sim, [NotNull] JsonCalcSpecification calcSpecification)
        {
            GeographicLocation geoloc = sim.GeographicLocations[0];
            if (calcSpecification.GeographicLocation != null)
            {
                geoloc = sim.GeographicLocations.FindByJsonReference(calcSpecification.GeographicLocation);
            }

            return geoloc;
        }

        public void ProcessSingleHouseJob([NotNull] string houseJobFile)
        {
            string resultDir = "Results";
            try
            {
                char[] charsToTrim = { '\n', ' ' };
                string houseJobStr = File.ReadAllText(houseJobFile).Trim(charsToTrim);
                HouseCreationAndCalculationJob hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr);
                if (hcj == null)
                {
                    throw new LPGException("housejob was null");
                }
                resultDir = hcj.CalcSpec?.OutputDirectory ?? "Results";
                if (!Directory.Exists(resultDir))
                {
                    Directory.CreateDirectory(resultDir);
                    Thread.Sleep(100);
                }
                string finishedFlagFile = Path.Combine(resultDir, Constants.FinishedFileFlag);
                if (File.Exists(finishedFlagFile))
                {
                    Logger.Info("File already exists: " + finishedFlagFile);
                    string filecontent = File.ReadAllText(finishedFlagFile).Trim(charsToTrim);
                    if (filecontent == houseJobStr)
                    {
                        Logger.Info("This calculation seems to be finished. Quitting.");
                        return;
                    }
                    Logger.Info("There is a previous calculation in the result directory but it used different parameters. Cleaning and recalculating.");
                    var prevarr = houseJobStr.Split('\n');
                    var newarr = filecontent.Split('\n');

                    for (int i = 0; i < newarr.Length && i < prevarr.Length; i++)
                    {
                        if (prevarr[i] != newarr[i])
                        {
                            Logger.Info("Line: " + i);
                            Logger.Info("Prev: " + prevarr[i]);
                            Logger.Info("New : " + newarr[i]);
                        }
                    }
                }
                string srcDbFile = hcj.PathToDatabase ?? throw new LPGException("No db source path");
                if (!File.Exists(srcDbFile))
                {
                    throw new LPGException("Could not found source database file: " + srcDbFile);
                }
                //string houseName = AutomationUtili.CleanFileName(hcj.House?.Name ?? "House");
                string dstDbFile = Path.Combine(resultDir, "profilegenerator.copy.db3");

                File.Copy(srcDbFile, dstDbFile, true);
                //File.SetAttributes(dstDbFile, File.GetAttributes(dstDbFile) & ~FileAttributes.ReadOnly);
                string dstConnectionString = "Data Source=" + dstDbFile;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse

                Simulator sim = new Simulator(dstConnectionString);
                ProcessSingleHouseJob(hcj, sim);
                if (Logger.Get().Errors.Count == 0)
                {
                    using (var sw = new StreamWriter(finishedFlagFile))
                    {
                        sw.Write(houseJobStr);
                    }
                }
                else
                {
                    Logger.Info("Didn't mark the calculation as finished, since there were the following errors:");
                    foreach (var logMessage in Logger.Get().Errors)
                    {
                        Logger.Info("Error: " + logMessage.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                var rdi = new DirectoryInfo(resultDir);
                if (!rdi.Exists)
                {
                    rdi.Create();
                }

                using (StreamWriter sw = new StreamWriter(Path.Combine(resultDir, "CalculationExceptions.txt")))
                {
                    sw.WriteLine(ex.Message);
                    sw.WriteLine(ex.StackTrace);
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine(GetAllFootprints(ex));
                    sw.Close();
                }

                throw;
            }
        }

        public void ProcessSingleHouseJob([NotNull] HouseCreationAndCalculationJob hcj, [NotNull] Simulator sim)
        {
            if (hcj.House == null)
            {
                throw new LPGException("No house was defined");
            }
            if (hcj.CalcSpec == null)
            {
                throw new LPGException("No calc spec was defined");
            }
            var geoloc = FindGeographicLocation(sim, hcj.CalcSpec);
            var temperatureProfile = FindTemperatureProfile(sim, hcj.CalcSpec);
            Random rnd = new Random();
            JsonReference objectToCalc;
            if (hcj.HouseDefinitionType == HouseDefinitionType.HouseData)
            {
                objectToCalc = CreateSingleHouse(hcj, sim, geoloc, temperatureProfile, rnd);
                if (objectToCalc == null)
                {
                    throw new LPGException("Failed to create a house");
                }

            }
            else
            {
                objectToCalc = hcj.HouseRef?.House;
                if (objectToCalc == null)
                {
                    throw new LPGException("no house reference was set");
                }
            }
            JsonCalculator jc = new JsonCalculator();
            //hcj.CalcSpec.CalcObject = null;
            jc.StartHousehold(sim, hcj.CalcSpec, objectToCalc);
        }
        [NotNull]
        public string GetAllFootprints([NotNull] Exception x)
        {
            var st = new StackTrace(x, true);
            var frames = st.GetFrames();
            //if (frames == null) {
            //    return "(no frames found)";
            //}

            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                {
                    continue;
                }
                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod()?.Name);
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
            if (hj.House == null)
            {
                throw new LPGException("No house data was set in the file");
            }
            //string name = hj.House?.Name ?? "";
            var housedata = hj.House;
            Logger.Info("Creating new house with " + hj.House?.Households.Count + " households...");

            //List<ModularHousehold> createdHouseholds = new List<ModularHousehold>();
            //make the house
            var house = MakeHouse(sim, hj.House);
            house.GeographicLocation = geoloc;
            house.TemperatureProfile = temperatureProfile;
            //add the Households
            if (hj.CalcSpec == null)
            {
                throw new LPGPBadParameterException("No calcspecification was set.");
            }
            int householdidx = 1;
            foreach (var householdData in housedata.Households)
            {
                var hhs = MakeHousehold(sim, householdData, r);

                var chargingStationSet = sim.ChargingStationSets.FindByJsonReference(householdData.ChargingStationSet);
                if (hj.CalcSpec.EnableTransportation && chargingStationSet == null)
                {
                    // ReSharper disable once RedundantToStringCall
                    throw new LPGPBadParameterException("Could not find charging station set: " + householdData.ChargingStationSet?.ToString());
                }
                TravelRouteSet travelrouteset;
                if (householdData.TravelRouteSet != null)
                {
                    // try to load the specified TravelRouteSet
                    travelrouteset = sim.TravelRouteSets.FindByJsonReference(householdData.TravelRouteSet);
                }
                else
                {
                    // no TravelRouteSet specified: use alternative travel specification (if available)
                    // For each person, transportation preferences can be specified. These are used to create a new TravelRouteSet.
                    travelrouteset = CreateTravelRouteSetFromPersonPreferences(householdData, sim);
                }
                if (hj.CalcSpec.EnableTransportation && travelrouteset == null)
                {
                    throw new LPGPBadParameterException("Could not find travel route set.");
                }
                var transportationDeviceSet = sim.TransportationDeviceSets.FindByJsonReference(householdData.TransportationDeviceSet);
                if (householdData.TransportationDistanceModifiers != null && householdData.TransportationDistanceModifiers.Count > 0
                && travelrouteset != null)
                {
                    Logger.Info("Settings new travel distances for " + hhs.Name + " " + "");
                    travelrouteset = AdjustTravelDistancesBasedOnModifiers(travelrouteset, sim, house,
                        householdData, householdidx++);
                    Logger.Info("Name of the new travel route set to be used is " + travelrouteset.Name);
                }
                if (hj.CalcSpec.EnableTransportation && transportationDeviceSet == null)
                {
                    throw new LPGPBadParameterException("Could not find transportation device set.");
                }
                house.AddHousehold(hhs, chargingStationSet, travelrouteset, transportationDeviceSet);

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
            if (house == null)
            {
                throw new LPGException("House generation failed");
            }
            return house.GetJsonReference();
        }

        /// <summary>
        /// Alternatively to directly specifying a TravelRouteSet the transport preferences of each person can be specified.
        /// This function collects these preferences and uses them to create a new TravelRouteSet.
        /// </summary>
        /// <param name="householdData">The HouseholdData object for which the TravelRouteSet will be created</param>
        /// <param name="sim"></param>
        /// <returns>A new TravelRouteSet that contains all required routes.</returns>
        private static TravelRouteSet CreateTravelRouteSetFromPersonPreferences(HouseholdData householdData, Simulator sim)
        {
            // find the home site of this household because it has a special role when using transportation preferences of persons
            Site home = sim.Sites.FindFirstByName("Home", FindMode.IgnoreCase);
            if (home == null || householdData.HouseholdDataPersonSpec == null)
            {
                // no "home" site or no persons available to calculate a travel route set
                return null;
            }
            // create a new empty travel route set
            var name = "Generated TravelRouteSet " + "(" + householdData.Name + ")";
            var description = "This TravelRouteSet was generated using the transportation device preferences of all persons in this household.";
            var travelRouteSet = new TravelRouteSet(name, null, sim.ConnectionString, description, Guid.NewGuid().ToStrGuid(), null);
            travelRouteSet.SaveToDB();
            foreach (PersonData person in householdData.HouseholdDataPersonSpec.Persons)
            {
                CreateTravelRoutesForPerson(person, home, travelRouteSet, sim);
            }
            return travelRouteSet;
        }

        /// <summary>
        /// Creates new TravelRoutes based on the transportation preferences of a person and adds them to the TravelRouteSet.
        /// </summary>
        /// <param name="person">The person the TravelRoutes will be created for</param>
        /// <param name="home">The site that is used as home of the person</param>
        /// <param name="travelRouteSet">The TravelRouteSet to which the new routes will be added</param>
        /// <param name="sim"></param>
        private static void CreateTravelRoutesForPerson(PersonData person, Site home, TravelRouteSet travelRouteSet, Simulator sim)
        {
            var preferences = person.TransportationPreferences;
            if (preferences == null)
            {
                return;
            }
            Dictionary<Site, TransportationPreference> sites = new Dictionary<Site, TransportationPreference>();
            // collect all sites (except from home)
            foreach (var preference in preferences)
            {
                var site = sim.Sites.FindByJsonReference(preference.DestinationSite);
                if (site == null)
                {
                    throw new LPGPBadParameterException("Could not find the site \"" + site.Name + "\".");
                }
                sites.Add(site, preference);
            }
            // create routes from each site to all others
            foreach (var destination in sites.Keys)
            {
                var preference = sites[destination];
                // add route from home
                CreateRoutesForDifferentCategories(person, home, destination, preference.DistanceFromHome, preference, travelRouteSet, sim);
                // add route from this site to home, using the same transportation preference
                CreateRoutesForDifferentCategories(person, destination, home, preference.DistanceFromHome, preference, travelRouteSet, sim);
                // add routes from all other sites
                foreach (var origin in sites.Keys.Where(site => site != destination))
                {
                    var preferenceOrigin = sites[origin];
                    double distance = CalcDistanceBetweenSites(preference.Angle, preference.DistanceFromHome, preferenceOrigin.Angle, preferenceOrigin.DistanceFromHome);
                    CreateRoutesForDifferentCategories(person, origin, destination, distance, preference, travelRouteSet, sim);
                }
            }
        }

        /// <summary>
        /// Creates all necessary TravelRoutes for a specific origin and destination, one for each transportation device category.
        /// </summary>
        /// <param name="person">The person for that the TravelRoutes are created</param>
        /// <param name="origin">The origin site for the TravelRoutes</param>
        /// <param name="destination">The destination site for the TravelRoutes</param>
        /// <param name="length">The length of the TravelRoutes</param>
        /// <param name="preference">The transportation preference of the person for the destination site</param>
        /// <param name="travelRouteSet">The TravelRouteSet to which the new TravelRoutes will be added</param>
        /// <param name="sim"></param>
        private static void CreateRoutesForDifferentCategories(PersonData person, Site origin, Site destination, double length, TransportationPreference preference, TravelRouteSet travelRouteSet, Simulator sim)
        {
            for (int i = 0; i < preference.TransportationDeviceCategories.Count; i++)
            {
                // create the route for the specified transportation device category
                var category = sim.TransportationDeviceCategories.FindByJsonReference(preference.TransportationDeviceCategories[i]);
                if (category == null)
                {
                    throw new LPGPBadParameterException("Could not find the category \"" + category.Name + "\".");
                }
                var name = "Generated (from " + origin.Name + " to " + destination.Name + " for " + person.PersonName + " using \"" + category.Name + "\")";
                var description = "This route was generated based on the transportation preferences of " + person.PersonName;
                TravelRoute route = new TravelRoute(null, sim.ConnectionString, name, description, origin, destination, StrGuid.New(), "");
                route.SaveToDB();
                // add only a single step using the specified category
                var stepName = "Generated (" + category.Name + ")";
                route.AddStep(stepName, category, length, 1, "");
                // add an entry to the travel route set with the specified weight
                travelRouteSet.AddRoute(route, weight: preference.Weights[i]);
            }
        }

        /// <summary>
        /// Calculates the distance between two sites, using their respective distances and angles to the centre (home)
        /// </summary>
        /// <param name="angle1">Angle of the first site as viewed from the centre</param>
        /// <param name="distanceFromHome1">Distance from the first site to the centre</param>
        /// <param name="angle2">Angle of the second site as viewed from the centre</param>
        /// <param name="distanceFromHome2">Distance from the second site to the centre</param>
        /// <param name="rad">True, if the angles are specified in rad, else false</param>
        /// <returns>The distance between the two sites</returns>
        private static double CalcDistanceBetweenSites(double angle1, double distanceFromHome1, double angle2, double distanceFromHome2, bool rad = false)
        {
            double difference = CalcAngleDifference(angle1, angle2, rad);
            return CalcTriangleMissingSide(distanceFromHome1, distanceFromHome2, difference, rad);
        }

        /// <summary>
        /// Calculates the minimum difference between two angles.
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <param name="rad">True, if the angles are specified in rad, else false</param>
        /// <returns>The difference angle between the two input angles</returns>
        private static double CalcAngleDifference(double angle1, double angle2, bool rad = false)
        {
            double fullCircle = rad ? 2 * Math.PI : 360;
            double difference = Math.Abs(angle1 - angle2);
            return difference > fullCircle / 2.0 ? fullCircle - difference : difference;
        }

        /// <summary>
        /// Calculates the missing side of a triangle, given two sides and the angle between them.
        /// </summary>
        /// <param name="a">The length of the first side</param>
        /// <param name="b">The length of the second side</param>
        /// <param name="angle">The angle between the two known sides</param>
        /// <param name="rad">True, if the angle is specified in rad, else false</param>
        /// <returns>The length of the third side</returns>
        private static double CalcTriangleMissingSide(double a, double b, double angle, bool rad = false)
        {
            angle = rad ? angle : Math.PI * angle / 180.0;
            return Math.Sqrt(a * a + b * b - Math.Cos(angle) * 2 * a * b);
        }

        [NotNull]
        private static TravelRouteSet AdjustTravelDistancesBasedOnModifiers([NotNull] TravelRouteSet travelrouteset,
                                                                            [NotNull] Simulator sim,
                                                                            [NotNull] House house,
                                                                            [NotNull] HouseholdData householdData,
                                                                            int householdidx)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (householdData.TransportationDistanceModifiers == null)
            {
                throw new LPGException("Was null even though this was checked before the function was called.");
            }
            var newName = travelrouteset.Name + "(" + house.Name + " - " + householdData.Name + " " + householdidx + ")";
            var adjustedTravelrouteset = new TravelRouteSet(newName, null, sim.ConnectionString, travelrouteset.Description, Guid.NewGuid().ToStrGuid(), null);
            adjustedTravelrouteset.SaveToDB();
            sim.TravelRouteSets.Items.Add(adjustedTravelrouteset);
            int adjustingDistances = 0;
            foreach (TravelRouteSetEntry oldTravelRouteSetEntry in travelrouteset.TravelRoutes)
            {
                bool addUnmodifiedRoute = true;
                foreach (var modifier in householdData.TransportationDistanceModifiers)
                {
                    string modRouteKey = modifier.RouteKey?.ToLower(CultureInfo.InvariantCulture);
                    if (oldTravelRouteSetEntry.TravelRoute.RouteKey?.ToLower(CultureInfo.InvariantCulture) == modRouteKey)
                    {
                        Logger.Info("Adjusting distances for key " + modifier.RouteKey + "-" + modifier.StepKey + ", total routes in the db: " + sim.TravelRoutes.Items.Count);
                        var modStepKey = modifier.StepKey?.ToLower(CultureInfo.InvariantCulture);
                        var oldRouteSteps = oldTravelRouteSetEntry.TravelRoute.Steps.Where(x => x.StepKey?.ToLower(CultureInfo.InvariantCulture) == modStepKey).ToList();
                        if (oldRouteSteps.Count > 0)
                        {
                            MakeNewAdjustedRoute(sim, oldTravelRouteSetEntry, adjustingDistances, modRouteKey, modifier, adjustedTravelrouteset);
                            addUnmodifiedRoute = false;
                            adjustingDistances++;
                        }
                    }
                }

                if (addUnmodifiedRoute)
                {
                    adjustedTravelrouteset.AddRoute(oldTravelRouteSetEntry.TravelRoute, oldTravelRouteSetEntry.MinimumAge, oldTravelRouteSetEntry.MaximumAge,
                        oldTravelRouteSetEntry.Gender, oldTravelRouteSetEntry.AffordanceTag, oldTravelRouteSetEntry.PersonID, oldTravelRouteSetEntry.Weight);
                }
            }
            //Config.ShowDeleteMessages = true;
            travelrouteset = adjustedTravelrouteset;
            adjustedTravelrouteset.SaveToDB();
            sw.Stop();
            Logger.Info("Total distances adjusted: " + adjustingDistances + ". This took " + sw.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture) + " seconds.");
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
                Guid.NewGuid().ToStrGuid(),
                oldRoute.RouteKey);
            newRoute.SaveToDB();
            sim.TravelRoutes.Items.Add(newRoute);
            foreach (var step in oldRoute.Steps)
            {
                double distance = step.Distance;
                if (step.StepKey?.ToLower(CultureInfo.InvariantCulture) == modRouteKey)
                {
                    distance = modifier.NewDistanceInMeters;
                }
                newRoute.AddStep(step.Name, step.TransportationDeviceCategory, distance, step.StepNumber, step.StepKey, false);
            }


            newRoute.SaveToDB();
            //Logger.Info("Adjusted route " + newRoute.Name);
            adjustedTravelrouteset.AddRoute(newRoute, oldTravelRouteSetEntry.MinimumAge, oldTravelRouteSetEntry.MaximumAge, oldTravelRouteSetEntry.Gender,
                oldTravelRouteSetEntry.AffordanceTag, oldTravelRouteSetEntry.PersonID, oldTravelRouteSetEntry.Weight);
        }

        public static void WriteGuidList([NotNull] string filename, [NotNull][ItemNotNull] List<DBBase> elements, [NotNull] DirectoryInfo relativePath)
        {
            string fn = relativePath.CombineName(filename);
            using (StreamWriter sw = new StreamWriter(fn))
            {
                foreach (var element in elements)
                {
                    sw.WriteLine(element.PrettyName + ";" + element.Guid);
                }
            }

            Logger.Info("Finished writing " + fn);
            string jsonFile = relativePath.CombineName(filename.Replace(".csv", ".json"));
            using (StreamWriter sw = new StreamWriter(jsonFile))
            {
                List<JsonReference> jr = new List<JsonReference>();
                foreach (var element in elements)
                {
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
            house.Name = hd.Name ?? hd.HouseGuid?.ToString() ?? "";
            house.Description = DescriptionText + hd.HouseGuid;
            house.SaveToDB();
            var housetypecode = hd.HouseTypeCode;
            if (housetypecode == null)
            {
                throw new LPGException("No house type was set");
            }
            if (housetypecode == null)
            {
                throw new LPGException("Could not find house type " + hd.HouseTypeCode);
            }
            //house type adjustment
            var potentialHts = sim.HouseTypes.Items.Where(x => x.Name.StartsWith(housetypecode, StringComparison.Ordinal)).ToList();
            if (potentialHts.Count == 0)
            {
                throw new LPGException("No house type found for " + housetypecode);
            }

            if (potentialHts.Count > 1)
            {
                throw new LPGException("Too many house types found for " + housetypecode + ". Try adding a couple of more letters to make it unique. It searches by start of the name.");
            }
            HouseType newHouseType = (HouseType)HouseType.ImportFromItem(potentialHts[0], sim);
            newHouseType.Name = newHouseType.Name + "(" + hd.Name + ")";
            if (hd.TargetHeatDemand != null)
            {
                newHouseType.HeatingYearlyTotal = hd.TargetHeatDemand.Value;
            }
            if (hd.TargetCoolingDemand != null)
            {
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
            if (sim == null)
            {
                throw new ArgumentNullException(nameof(sim));
            }

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (householdData.HouseholdDataSpecification)
            {
                case HouseholdDataSpecificationType.ByPersons when householdData.HouseholdDataPersonSpec == null:
                    throw new LPGException("No person specification was set for the household " + householdData.Name);
                case HouseholdDataSpecificationType.ByPersons when householdData.HouseholdDataPersonSpec.Persons.Count == 0:
                    throw new LPGException("No persons were defined for the household " + householdData.Name);
                case HouseholdDataSpecificationType.ByTemplateName when householdData.HouseholdTemplateSpec == null:
                    throw new LPGException("No household template specification was set for the household " + householdData.Name);
                case HouseholdDataSpecificationType.ByTemplateName when string.IsNullOrWhiteSpace(householdData.HouseholdTemplateSpec.HouseholdTemplateName):
                    throw new LPGException("No household template name was set for the household " + householdData.Name);
                case HouseholdDataSpecificationType.ByHouseholdName when householdData.HouseholdNameSpec == null:
                    throw new LPGException("The household is supposed to be defined with the name, but no household name specification was set for the household " + householdData.Name);
                case HouseholdDataSpecificationType.ByHouseholdName when (householdData.HouseholdNameSpec?.HouseholdReference == null):
                    throw new LPGException("No household reference was set for the household " + householdData.Name + ". So no household to calculate could be identified.");
            }

            switch (householdData.HouseholdDataSpecification)
            {
                case HouseholdDataSpecificationType.ByPersons:
                    return MakeHouseholdBaseOnPersonSpec(sim, householdData, r);
                case HouseholdDataSpecificationType.ByTemplateName:
                    return MakeHouseholdBaseOnTemplateSpec(sim, householdData);
                case HouseholdDataSpecificationType.ByHouseholdName:
                    return MakeHouseholdBaseOnHouseholdName(sim, householdData);
                default:
                    throw new LPGException(nameof(HouseholdDataSpecificationType));
            }
        }

        [NotNull]
        private static ModularHousehold MakeHouseholdBaseOnHouseholdName([NotNull] Simulator sim, [NotNull] HouseholdData householdData)
        {
            HouseholdNameSpecification nameSpec = householdData.HouseholdNameSpec;
            if (nameSpec == null)
            {
                throw new LPGCommandlineException("Household name specification was null");

            }

            var household = sim.ModularHouseholds.FindByJsonReference(nameSpec.HouseholdReference);
            if (household == null)
            {
                throw new LPGException("No household found for the household name " + nameSpec.HouseholdReference?.Name);
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
            HouseholdTemplateSpecification templateSpec = householdData.HouseholdTemplateSpec;
            if (templateSpec == null)
            {
                throw new LPGCommandlineException("Household template specification was null");

            }

            var template = sim.HouseholdTemplates.FindFirstByName(templateSpec.HouseholdTemplateName, FindMode.StartsWith);
            if (template == null)
            {
                throw new LPGException("No household template found for the household template name " + templateSpec.HouseholdTemplateName);
            }
            template.Count = 1;
            Logger.Info("Generating household with template " + template.Name);
            var forbiddenTraitTags = new List<TraitTag>();
            if (templateSpec.ForbiddenTraitTags != null)
            {
                foreach (var forbiddenTraitTag in templateSpec.ForbiddenTraitTags)
                {
                    TraitTag traitTag = sim.TraitTags.Items.FirstOrDefault(x => x.Name == forbiddenTraitTag);
                    if (traitTag == null)
                    {
                        throw new LPGPBadParameterException("Found a forbidden trait tag \"" + forbiddenTraitTag + "\", but that could not be found in the LPG. Please fix.");
                    }
                    forbiddenTraitTags.Add(traitTag);
                }
            }
            if (templateSpec.Persons != null && templateSpec.Persons.Count > 0)
            {
                foreach (var templateperson in templateSpec.Persons)
                {
                    if (templateperson.PersonName == null)
                    {
                        continue;
                    }

                    if (templateperson.LivingPatternTag == null)
                    {
                        continue;
                    }
                    var hhperson = template.Persons.FirstOrDefault(x => x.Person.Name == templateperson.PersonName);
                    if (hhperson == null)
                    {
                        string s = "Could not find a person with the name " + templateperson.PersonName + " in the household template " +
                                   template.Name + ". Please fix. The following persons are in the household template: \n";
                        foreach (var person in template.Persons)
                        {
                            s += person.Person.Name + "\n";
                        }
                        throw new LPGPBadParameterException(s);
                    }
                    var lptag = sim.LivingPatternTags.Items.FirstOrDefault(x => templateperson.LivingPatternTag == x.Name);
                    if (lptag == null)
                    {
                        throw new LPGPBadParameterException("Living pattern \"" + templateperson.LivingPatternTag + "\" from the person  " +
                                                            templateperson.PersonName +
                                                            " was not found in the living pattern list");
                    }
                    hhperson.LivingPatternTag = lptag;
                }
            }

            var hhs = template.GenerateHouseholds(sim, false, new List<STTraitLimit>(), forbiddenTraitTags);
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
            HouseholdDataPersonSpecification personSpec = householdData.HouseholdDataPersonSpec;
            if (personSpec == null)
            {
                throw new LPGCommandlineException("Person specification was null");

            }

            var templatesWithCorrectTags = sim.HouseholdTemplates.Items.ToList();
            if (personSpec.HouseholdTags != null && personSpec.HouseholdTags.Count > 0)
            {
                foreach (var tag in personSpec.HouseholdTags)
                {
                    //this does an AND filtering
                    templatesWithCorrectTags = templatesWithCorrectTags.Where(x => x.TemplateTags.Any(y => y.Tag.Classification == tag)).ToList();
                }
            }
            List<HouseholdTemplate> templatesWithCorrectPersonCounts =
                templatesWithCorrectTags.Where(x => x.Persons.Count == personSpec.Persons.Count).ToList();

            //make demanded person profile
            List<PersonCategory> demandedPersonCategories = new List<PersonCategory>();
            foreach (PersonData data in personSpec.Persons)
            {
                demandedPersonCategories.Add(new PersonCategory(data.Age, (PermittedGender)data.Gender));
            }

            List<HouseholdTemplate> selectedHouseholdTemplates = new List<HouseholdTemplate>();
            foreach (var householdTemplate in templatesWithCorrectPersonCounts)
            {
                List<PersonCategory> thisOfferedCategories = new List<PersonCategory>();
                foreach (var th in householdTemplate.Persons)
                {
                    thisOfferedCategories.Add(new PersonCategory(th.Person.Age, th.Person.Gender));
                }

                if (AreOfferedCategoriesEnough(thisOfferedCategories, demandedPersonCategories))
                {
                    selectedHouseholdTemplates.Add(householdTemplate);
                }
            }

            if (selectedHouseholdTemplates.Count == 0)
            {
                _householdErrorCount++;
                string s = "Error " + _householdErrorCount + Environment.NewLine + "Not a single household template was found for the household " +
                           householdData.Name + Environment.NewLine;
                s += "Criteria for finding the household were: Persons: " + personSpec.Persons.Count + Environment.NewLine;
                s += "Household templates found with this criteria: " + templatesWithCorrectPersonCounts.Count + Environment.NewLine;
                s += "Requirements for the persons were:" + Environment.NewLine;
                foreach (var cat in demandedPersonCategories)
                {
                    s += cat + Environment.NewLine;
                }

                Logger.Warning(s);
                //throw new LPGException(s);
                if (templatesWithCorrectPersonCounts.Count > 0)
                {
                    Logger.Warning("Using a random template with the same number of people.");
                    selectedHouseholdTemplates = templatesWithCorrectPersonCounts;
                }
                else
                {
                    Logger.Warning("No household found with " + personSpec.Persons.Count + ", using a random template.");
                    selectedHouseholdTemplates = sim.HouseholdTemplates.Items.ToList();
                }
            }

            //try to find the right one based on energy use
            var pickedHht = selectedHouseholdTemplates[r.Next(selectedHouseholdTemplates.Count)];
            pickedHht.Count = 1;
            Logger.Info("Generating household with template " + pickedHht.Name);
            var hhs = pickedHht.GenerateHouseholds(sim, false, new List<STTraitLimit>(), new List<TraitTag>());
            if (hhs.Count != 1)
            {
                throw new Exception("Could not generate this house");
            }

            hhs[0].Description = DescriptionText + householdData.UniqueHouseholdId;
            hhs[0].SaveToDB();
            Logger.Info("Finished generating household with template " + pickedHht.Name);
            return hhs[0];
        }

        public static void ProcessHouseJob([NotNull] HouseJobProcessingOptions args)
        {
            Logger.Threshold = Severity.Warning;
            HouseGenerator hg = new HouseGenerator();
            if (args.JsonPath == null)
            {
                throw new LPGCommandlineException("Path to the house job file was not set. This won't work.");
            }
            hg.ProcessSingleHouseJob(args.JsonPath);
        }
    }
}
