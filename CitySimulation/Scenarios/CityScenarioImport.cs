using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using CitySimulation.Scenarios;
using CitySimulation.SimulationTargets;
using Newtonsoft.Json;
using PowerArgs;
using SimulationEngineLib.HouseJobProcessor;
using CalculationEngine.OnlineLogging;
using System.Text.RegularExpressions;

namespace CitySimulation.CityGeneration
{
    /// <summary>
    /// Class for loading a city scenario from a directory. The directory must contain
    /// a calcspec.json file with the calculation configuration, and files specifying all
    /// residential and non-residential buildings in the target area.
    /// </summary>
    internal class CityScenarioImport
    {
        /// <summary>
        /// Maps the day of week key in route data filenames to the corresponding enum value
        /// </summary>
        private static readonly Dictionary<string, DayOfWeek> DayTypeMapping = new() {
            { "Mon", DayOfWeek.Monday },
            { "Tue", DayOfWeek.Tuesday },
            { "Wed", DayOfWeek.Wednesday },
            { "Thu", DayOfWeek.Thursday },
            { "Fri", DayOfWeek.Friday },
            { "Sat", DayOfWeek.Saturday },
            { "Sun", DayOfWeek.Sunday },
        };

        public static Scenario ReadScenarioFromConfigDirectory(string inputDirectoryPath)
        {
            var inputDirectory = new DirectoryInfo(inputDirectoryPath);
            // read file calcspec.json; it is a HouseCreationAndCalculationJob object, but only calcspec
            // and database path are required
            var hcj = AutomationUtili.ParseJsonFile<HouseCreationAndCalculationJob>(inputDirectory.CombineName("calcspec.json"));
            var calcSpec = hcj.CalcSpec ?? throw new LPGException("No CalcSpec was given in the input file");
            // TODO: calcspec should be complete and single-source-of-parameters
            // --> check and fill all missing values in the calcspec first, then move on
            if (!calcSpec.EnableTransportation)
                throw new LPGException("Transport must be enabled for the city simulation.");

            // create result directory
            var resultDir = calcSpec.OutputDirectory ??= HouseGenerator.DefaultResultDirectory;
            if (!Directory.Exists(resultDir))
            {
                Directory.CreateDirectory(resultDir);
                Thread.Sleep(100);
            }

            HouseGenerator houseGenerator = new();

            // check for existing files in the result directory
            houseGenerator.CleanResultDirectoryBeforeSimulation(resultDir, false);

            // copy DB file to result directory and open a connection to it
            var sim = houseGenerator.CopyAndOpenDatabase(hcj.PathToDatabase, resultDir, out string newDbPath);
            string fullDbPath = Path.GetFullPath(hcj.PathToDatabase!);
            Logger.Info("Using database file: " + fullDbPath);

            // disable cleanup checks, as they can severly degrade performance in large scenarios, e.g., with many travel routes
            sim.MyGeneralConfig.PerformCleanUpChecksBool = false;

            // save settings to the database copy in the result directory
            JsonCalculator.SaveSettingsToDatabase(sim, calcSpec);

            // initialize an RNG to generate an individual seed for each simulation target
            int seed = CalcParameters.GetActualRandomSeed(calcSpec.RandomSeed);
            var random = new Random(seed);

            // create house configs and POI configs from the files in the input directory
            var houseConfigs = CollectHouseConfigs(inputDirectory.CombineName("houses"), random);
            var cityData = AutomationUtili.ParseJsonFile<CityData>(inputDirectory.CombineName("city.json"));
            var poiConfigs = cityData.PointsOfInterest.Select(entry => new PointOfInterestConfig(new(entry.Key), entry.Value.LocationType));

            // log the seed used for each target to make simulation reproducible
            CreateTargetSeedFile(resultDir, houseConfigs);

            // check if there is travel data in separate files and assign it to the CityData object
            ParseTravelData(inputDirectory, cityData);

            // create a new scenario object containing all house and POI configs
            return new Scenario(newDbPath, calcSpec, houseConfigs, poiConfigs, cityData);
        }

        /// <summary>
        /// Parses route data from a separate "routes" subdirectory, if it exists.
        /// Each contained json file specifies the available routes for a different time slot.
        /// Additionally, the optional fiel "cluster_info.json" can provide a mapping of building IDs to
        /// clusters, if buildings were clustered for the route definitions.
        /// </summary>
        /// <param name="inputDirectory">the scenario input directory</param>
        /// <param name="city">the parsed city definition</param>
        /// <exception cref="LPGException">if two conflicting travel definitions are found</exception>
        private static void ParseTravelData(DirectoryInfo inputDirectory, CityData city)
        {
            // check if routes are defined in a separate file
            var routesDir = new DirectoryInfo(inputDirectory.CombineName("routes"));
            if (!routesDir.Exists)
            {
                // no separate travel data files
                return;
            }
            if (city.TravelDefinition.TimeSlotRouteLists.IsNullOrEmpty() is false)
                throw new LPGException("Routes are defined in both city.json and the routes subdirectory. Only one of them is allowed at a time.");

            // check if POIs are clustered for the route data, and if so, load the corresponding mapping
            string clusterFile = routesDir.CombineName("cluster_info.json");
            if (File.Exists(clusterFile))
            {
                city.TravelDefinition.PoiClusterMapping = AutomationUtili.ParseJsonFile<Dictionary<string, string>>(clusterFile);
            }

            var routeFiles = routesDir.GetFiles();
            Logger.Info($"Found {routeFiles.Length} files in the routes subdirectory.");
            foreach (var routeFile in routeFiles)
            {
                if (routeFile.FullName == clusterFile)
                    continue;
                var routes = AutomationUtili.ParseJsonFile<Dictionary<string, RouteData>>(routeFile.FullName);
                var timeSlot = ParseTimeSlot(routeFile.Name);
                city.TravelDefinition.TimeSlotRouteLists.Add(new(timeSlot, [.. routes.Values]));
            }
        }

        /// <summary>
        /// Parses the time slot for route data from the route data filename.
        /// </summary>
        /// <param name="text">the text to parse the time slot from</param>
        /// <returns>the parsed time slot that applies for the corresponding route data</returns>
        /// <exception cref="LPGPBadParameterException">if the time slot could not be parsed</exception>
        private static TimeSlot ParseTimeSlot(string text)
        {
            const string KEY_ALL = "All";
            var dayTypes = String.Join("|", DayTypeMapping.Keys) + $"|{KEY_ALL}";
            Match match = Regex.Match(text, @"_(" + dayTypes + @")_(\d+)to(\d+)");
            if (!match.Success)
                throw new LPGPBadParameterException($"Could not parse time slot: {text}");

            // parse the weekday this time slot applies to
            string dayTypeText = match.Groups[1].Value;
            HashSet<DayOfWeek> daysOfWeek;
            if (DayTypeMapping.TryGetValue(dayTypeText, out var dayOfWeek))
            {
                // time slot only applies to this specific day of week
                daysOfWeek = [dayOfWeek];
            }
            else if (dayTypeText == KEY_ALL)
            {
                // time slot applies on all days
                daysOfWeek = [.. Enum.GetValues<DayOfWeek>()];
            }
            else
            {
                throw new LPGPBadParameterException($"Unkown day type in time slot: {dayTypeText}");
            }

            int start = int.Parse(match.Groups[2].Value);
            int end = int.Parse(match.Groups[3].Value);
            return new TimeSlot(start, end, daysOfWeek);
        }

        /// <summary>
        /// Collects all house config files in the input directory and creates a simulation target object for
        /// each of them.
        /// </summary>
        /// <param name="directory">the subdirectory in the input directory containing the house configs</param>
        /// <returns>all house configs from the directory</returns>
        private static ICollection<ResidentialBuildingConfig> CollectHouseConfigs(string directory, Random random)
        {
            var files = Directory.GetFiles(directory);
            // sort filenames to ensure that they are always in the same order
            Array.Sort(files);
            // return the result as a collection instead of an enumerable to avoid assigning different random values on each access
            return [.. files.Select(f => new ResidentialBuildingConfig(Path.GetFileNameWithoutExtension(f), f, random.Next()))];
        }

        /// <summary>
        /// Create a JSON file containing the seed used for each simulation target. With this seed, the simulation
        /// of individual targets can be reproduced.
        /// </summary>
        /// <param name="resultDir">the output directory where the file will be created</param>
        /// <param name="targets">the target references with their seeds</param>
        private static void CreateTargetSeedFile(string resultDir, IEnumerable<ResidentialBuildingConfig> targets)
        {
            var seedDict = targets.ToDictionary(t => t.Id, t => t.Seed);
            var jsonString = JsonConvert.SerializeObject(seedDict, Formatting.Indented);
            File.WriteAllText(Path.Combine(resultDir, Constants.HouseSeedMappingFile), jsonString);
        }
    }
}
