using System;
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
using Common.Extensions;
using Common.JSON;
using Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PowerArgs;

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

        public const string DefaultResultDirectory = "Results";

        public static readonly char[] charsToTrim = { '\n', ' ' };

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

        /// <summary>
        /// Checks if the finished flag file in the result directory already exists, and if it 
        /// is for the same simulation job.
        /// </summary>
        /// <param name="houseJobStr">the simulation job string of this simulation</param>
        /// <param name="finishedFlagFile">the path of the finished flag file</param>
        /// <returns>true if a matching flag file exists, else false</returns>
        public bool CheckFinishedFlagFile(string houseJobStr, string finishedFlagFile)
        {
            // check if the result directory already contains data, and if so if it came from the same simulation configuration
            if (File.Exists(finishedFlagFile))
            {
                Logger.Info("File already exists: " + finishedFlagFile);
                string filecontent = File.ReadAllText(finishedFlagFile).Trim(charsToTrim);
                if (filecontent == houseJobStr)
                {
                    // the existing finished flag file is from the same configuration
                    return true;
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
            return false;
        }

        /// <summary>
        /// Deletes existing files in the result directory before the calculation.
        /// </summary>
        /// <param name="resultDirectory">the result directory to clean up</param>
        /// <param name="keepLogsAndDBFiles">if true, log files and .db3 files are not deleted</param>
        public void CleanResultDirectoryBeforeSimulation(string resultDirectory, bool keepLogsAndDBFiles = true)
        {
            var resultDir = new DirectoryInfo(resultDirectory);
            if (Directory.Exists(resultDir.FullName))
            {
                Logger.Warning("Result directory already exists, but calculation is not finished or skip existing is not specified. Deleting folder.");
                var files = resultDir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (keepLogsAndDBFiles && file.Name.StartsWith("Log.", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (keepLogsAndDBFiles && file.Name.EndsWith(".db3", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    file.Delete();
                }

                var directories = resultDir.GetDirectories();
                foreach (DirectoryInfo info in directories)
                {
                    info.Delete(true);
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Copies the database to the result directory and opens it
        /// </summary>
        /// <param name="databasePath">path to the source database file</param>
        /// <param name="resultDirectory">path to the result directory</param>
        /// <param name="resultDatabasePath">file path of the opened database copy</param>
        /// <param name="newfileName">name of the new database file</param>
        /// <returns>database access object</returns>
        /// <exception cref="LPGException">if the source database path was invalid</exception>
        public Simulator CopyAndOpenDatabase(string databasePath, string resultDirectory, out string resultDatabasePath, string newfileName = "profilegenerator.copy.db3")
        {
            if (databasePath.IsNullOrEmpty())
                throw new LPGException("No db source path");
            if (!File.Exists(databasePath))
                throw new LPGException("Could not find source database file: " + databasePath);
            
            // create the target directory if it does not exist yet
            var targetDirectory = Directory.CreateDirectory(resultDirectory);
            resultDatabasePath = targetDirectory.CombineName(newfileName);

            File.Copy(databasePath, resultDatabasePath, true);
            
            string dstConnectionString = "Data Source=" + resultDatabasePath;
            Simulator sim = new Simulator(dstConnectionString);
            return sim;
        }

        public void ProcessSingleHouseJob([NotNull] string houseJobFile)
        {
            string resultDir = DefaultResultDirectory;
            try
            {
                // read house job file
                string houseJobStr = File.ReadAllText(houseJobFile).Trim(charsToTrim);
                HouseCreationAndCalculationJob hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr);
                if (hcj == null)
                    throw new LPGException("housejob was null");
                if (hcj.CalcSpec == null)
                    throw new LPGException("No CalcSpec was specified");

                // create result directory
                resultDir = hcj.CalcSpec.OutputDirectory ??= DefaultResultDirectory;
                if (!Directory.Exists(resultDir))
                {
                    Directory.CreateDirectory(resultDir);
                    Thread.Sleep(100);
                }

                // check for the finished flag file and other existing files in the result directory
                string finishedFlagFile = Path.Combine(resultDir, Constants.FinishedFileFlag);
                bool existingResultsFound = CheckFinishedFlagFile(houseJobStr, finishedFlagFile);
                if (existingResultsFound && hcj.CalcSpec.SkipExisting)
                {
                    // the same simulation has already been completed before
                    Logger.Info("This calculation seems to be finished. Quitting.");
                    return;
                }
                CleanResultDirectoryBeforeSimulation(resultDir);

                // copy DB file to result directory and open a connection to it
                var sim = CopyAndOpenDatabase(hcj.PathToDatabase, resultDir, out _);

                ProcessSingleHouseJob(hcj, sim);

                // create the finished flag file if the simulation succeeded
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
                }

                throw;
            }
        }

        /// <summary>
        /// Gets the JsonReference of the house to calculate from a HouseCreationAndCalculationjob.
        /// Depending on the configuration, the reference of an existing house is returned or a new
        /// house is created.
        /// </summary>
        /// <param name="hcj">the simulation job configuration containing the house definition</param>
        /// <param name="sim">database access object</param>
        /// <param name="random">an optional random object to use for creation of a house or household, if necessary</param>
        /// <returns>reference of the house to simulate</returns>
        /// <exception cref="LPGException">if the configuration does not contain a valid house definition</exception>
        public JsonReference GetHouseReference([NotNull] HouseCreationAndCalculationJob hcj, [NotNull] Simulator sim, Random? random = null)
        {
            JsonReference calcObjectReference;
            if (hcj.HouseDefinitionType == HouseDefinitionType.HouseData)
            {
                calcObjectReference = CreateSingleHouse(hcj, sim, random);
            }
            else
            {
                calcObjectReference = hcj.HouseRef?.House;
                if (calcObjectReference == null)
                {
                    throw new LPGException("no house reference was set");
                }
            }
            return calcObjectReference;
        }

        /// <summary>
        /// Executes a simulation with the JsonCalculator.
        /// </summary>
        /// <param name="hcj">the simulation configuration</param>
        /// <param name="sim">database access object</param>
        /// <exception cref="LPGException">if the configuration was invalid or the simulation failed</exception>
        public void ProcessSingleHouseJob([NotNull] HouseCreationAndCalculationJob hcj, [NotNull] Simulator sim)
        {
            if (hcj.CalcSpec == null)
            {
                throw new LPGException("No calc spec was defined");
            }
            var calcObjectReference = GetHouseReference(hcj, sim);
            JsonCalculator jc = new JsonCalculator();
            jc.StartHousehold(sim, hcj.CalcSpec, calcObjectReference);
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

        /// <summary>
        /// Creates a new house with all its households from the configuration specified in the
        /// <see cref="HouseCreationAndCalculationJob"/>.
        /// </summary>
        /// <param name="hj">the house configuration</param>
        /// <param name="sim">database access object</param>
        /// <param name="random">an optional random object to use for house generation</param>
        /// <returns>JsonReference of the newly created house</returns>
        /// <exception cref="LPGPBadParameterException">if the provided house configuration was insufficient or invalid</exception>
        [NotNull]
        private static JsonReference CreateSingleHouse([NotNull] HouseCreationAndCalculationJob hj, [NotNull] Simulator sim, Random? random = null)
        {
            if (hj.House == null)
            {
                throw new LPGPBadParameterException("No house data was set in the file");
            }
            if (hj.CalcSpec == null)
            {
                throw new LPGPBadParameterException("No calcspecification was set.");
            }

            // make the house
            Logger.Info("Creating new house with " + hj.House?.Households.Count + " households...");
            var house = MakeHouse(sim, hj.House);

            house.GeographicLocation = sim.GeographicLocations.FindOrDefault(hj.CalcSpec.GeographicLocation);
            house.TemperatureProfile = sim.TemperatureProfiles.FindOrDefault(hj.CalcSpec.TemperatureProfile);

            // check if locations should be replaced with new POI locations for the city simulation
            PointOfInterestTraitReplacer poiTraitReplacer = null;
            if (hj.City is not null)
            {
                poiTraitReplacer = new PointOfInterestTraitReplacer(sim, hj.City);
            }

            // use the specified random object or create a new one
            if (random is null)
            {
                var seed = CalcParameters.GetActualRandomSeed(hj.CalcSpec?.RandomSeed);
                random = new(seed);
            }

            // create and add the Households
            int householdidx = 1;
            foreach (var householdData in hj.House.Households)
            {
                var hhs = MakeHousehold(sim, householdData, random);

                if (poiTraitReplacer is not null)
                {
                    // replace locations in all traits with new POI locations
                    if (householdData.PointOfInterestPreferences is null)
                        throw new LPGException($"No person travel preferences specified for household #{householdidx}");
                    poiTraitReplacer.ReplaceTraitsInHousehold(hhs, householdData.PointOfInterestPreferences);
                }

                // get or create all transportation objects, if required
                bool transportEnabled = hj.CalcSpec.EnableTransportation;
                var chargingStationSet = sim.ChargingStationSets.FindWithException(householdData.ChargingStationSet, !transportEnabled);
                var transportationDeviceSet = sim.TransportationDeviceSets.FindWithException(householdData.TransportationDeviceSet, !transportEnabled);
                var travelRouteSet = transportEnabled ? DetermineTravelRouteSet(sim, householdData, hhs, hj, poiTraitReplacer?.LocationReplacements, transportationDeviceSet) : null;

                // check if the distances in the travel route set should be modified
                if (!householdData.TransportationDistanceModifiers.IsNullOrEmpty() && travelRouteSet is not null)
                {
                    Logger.Info($"Setting new travel distances for {hhs.Name} ");
                    travelRouteSet = AdjustTravelDistancesBasedOnModifiers(travelRouteSet, sim, house, householdData, householdidx);
                    Logger.Info("Name of the new travel route set to be used is " + travelRouteSet.Name);
                }

                // add the new household to the house
                house.AddHousehold(hhs, chargingStationSet, travelRouteSet, transportationDeviceSet);
                householdidx++;
            }

            house.SaveToDB();
            Logger.Info("Successfully created house.");
            return house.GetJsonReference();
        }


        private static TravelRouteSet DetermineTravelRouteSet(Simulator sim, HouseholdData householdData, ModularHousehold household, HouseCreationAndCalculationJob hj,
            IReadOnlyDictionary<string, PoiLocationReplacement>? locationReplacements = null, TransportationDeviceSet transportationDeviceSet = null)
        {
            // there are multiple ways how traveling behavior can be specified in the calcspe; check if only exactly one is used
            bool travelRouteSetGiven = householdData.TravelRouteSet is not null;
            bool travelPreferencesGiven = TravelRouteSetBuilderFromPersonData.IsRequiredDataAvailable(householdData);
            bool poiPreferencesGiven = TravelRouteSetBuilderCity.IsRequiredDataAvailable(householdData);
            
            bool[] areParametersGiven = [travelRouteSetGiven, travelPreferencesGiven, poiPreferencesGiven];
            int numberOfParametersGiven = areParametersGiven.Count(x => x);
            if (numberOfParametersGiven == 0)
                throw new LPGPBadParameterException($"Transportation is enabled, but no information about travel behavior of household {householdData.Name} was given.");
            if (numberOfParametersGiven > 1)
                throw new LPGPBadParameterException($"Two or more properties specifying travel behavior were set, but only one is allowed at a time.");

            // determine the travel route set depending on the given travel parameter
            TravelRouteSet travelrouteset;
            if (travelRouteSetGiven)
            {
                // try to load the specified TravelRouteSet
                travelrouteset = sim.TravelRouteSets.FindWithException(householdData.TravelRouteSet);
            } else if (poiPreferencesGiven)
            {
                var travelRouteSetBuilder = new TravelRouteSetBuilderCity(sim, locationReplacements);
                travelrouteset = travelRouteSetBuilder.CreateTravelRouteSetFromPoiPreferences(householdData, household, hj, transportationDeviceSet);
            }
            else if (travelPreferencesGiven)
            {
                // use transportation preferences of each person from the person data
                var travelRouteSetBuilder = new TravelRouteSetBuilderFromPersonData(sim);
                travelrouteset = travelRouteSetBuilder.CreateTravelRouteSetFromPersonPreferences(householdData);
            } else
            {
                throw new LPGException("None of the travel parameters was set, this should have been caught by the checks above.");
            }
            return travelrouteset;
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
                newRoute.AddStep(step.Name, step.TransportationDeviceCategory, distance, step.StepNumber, step.StepKey, save: false);
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
                    return MakeHouseholdBaseOnTemplateSpec(sim, householdData, r);
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
        private static ModularHousehold MakeHouseholdBaseOnTemplateSpec([NotNull] Simulator sim, [NotNull] HouseholdData householdData, Random random)
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

            var hhs = template.GenerateHouseholds(sim, false, new List<STTraitLimit>(), forbiddenTraitTags, random);
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
