using Automation;
using Automation.ResultFiles;
using CitySimulation.SimulationTargets;
using Common;
using Common.JSON;
using PowerArgs;
using SimulationEngineLib.HouseJobProcessor;
using System.Text.RegularExpressions;

namespace CitySimulation.Scenarios
{
    /// <summary>
    /// Class for loading a city scenario from a directory. The directory must contain
    /// a calcspec.json file with the calculation configuration, and files specifying all
    /// residential and non-residential buildings in the target area.
    /// </summary>
    internal class CityScenarioImport
    {
        /// <summary>
        /// Parses a city scenario from a directory. Loads general info from a calcspec.json file, residential buildings
        /// from the houses subdirectory, city data from city.json, and optionally route data from the routes subdirectory.
        /// </summary>
        /// <param name="inputDirectoryPath">path to the scenario input directory</param>
        /// <param name="numWorkers">the number of workers to use</param>
        /// <param name="logger">the logger object to use for messages</param>
        /// <returns>the generated scenario parsed from the directory</returns>
        /// <exception cref="LPGException">if the scenario was invalid or could not be parsed</exception>
        public static Scenario ReadScenarioFromConfigDirectory(string inputDirectoryPath, int numWorkers, MPILogger logger)
        {
            logger.Info($"Loading city scenario from path {inputDirectoryPath}");
            var inputDirectory = new DirectoryInfo(inputDirectoryPath);
            // read file calcspec.json; it is a HouseCreationAndCalculationJob object, but only calcspec
            // and database path are required
            var hcj = AutomationUtili.ParseJsonFile<HouseCreationAndCalculationJob>(inputDirectory.CombineName("calcspec.json"));
            var calcSpec = hcj.CalcSpec ?? throw new LPGException("No CalcSpec was given in the input file");
            if (!calcSpec.EnableTransportation)
                throw new LPGException("Transport must be enabled for the city simulation.");

            // create result directory
            var resultDir = calcSpec.OutputDirectory ??= HouseGenerator.DefaultResultDirectory;
            if (!Directory.Exists(resultDir))
            {
                Directory.CreateDirectory(resultDir);
                Thread.Sleep(100);
            }

            if (string.IsNullOrEmpty(hcj.PathToDatabase))
                throw new LPGException("No database source path given");
            if (!File.Exists(hcj.PathToDatabase))
                throw new LPGException("Could not find source database file: " + hcj.PathToDatabase);
            bool reuseDBs = CanUseExistingDatabases(resultDir, numWorkers, hcj.PathToDatabase);

            // check if a file with RNG seeds for each house already exists
            var seedFile = Path.Combine(resultDir, Constants.HouseSeedMappingFile);
            bool reuseSeedFile = File.Exists(seedFile);

            // check for existing files in the result directory
            HouseGenerator.CleanResultDirectoryBeforeSimulation(resultDir, false, reuseDBs, reuseSeedFile);

            try
            {
                // try to create a link to the scenario directory, as a reference
                Directory.CreateSymbolicLink(Path.Combine(resultDir, "scenario"), inputDirectoryPath);
            }
            catch (IOException)
            {
                logger.Warning("Could not create a symbolic link to the scenario directory.");
            }

            // copy DB file to result directory and open a connection to it
            var sim = HouseGenerator.CopyAndOpenDatabase(hcj.PathToDatabase, resultDir, out string newDbPath);
            string fullDbPath = Path.GetFullPath(hcj.PathToDatabase!);
            logger.Info("Using database file: " + fullDbPath);

            // disable cleanup checks, as they can severly degrade performance in large scenarios, e.g., with many travel routes
            sim.MyGeneralConfig.PerformCleanUpChecksBool = false;

            // save settings to the database copy in the result directory
            JsonCalculator.SaveSettingsToDatabase(sim, calcSpec);

            // create common CalcParameters object from the CalcSpecification and the general config
            var calcParameters = JsonCalculator.CreateCalcParameters(sim.MyGeneralConfig, calcSpec, true);

            Func<string, int> seedProvider;
            if (reuseSeedFile)
            {
                // load the seed file and use the already defined seed for each house
                var seedsPerHouse = AutomationUtili.ParseJsonFile<Dictionary<string, int>>(seedFile);
                seedProvider = id => seedsPerHouse[id];
            }
            else
            {
                // initialize an RNG to generate an individual seed for each simulation target
                int seed = CalcParameters.GetActualRandomSeed(calcSpec.RandomSeed);
                var random = new Random(seed);
                seedProvider = _ => random.Next();
            }

            // create house configs and POI configs from the files in the input directory
            var houseConfigs = CollectHouseConfigs(inputDirectory.CombineName("houses"), seedProvider);
            var cityData = AutomationUtili.ParseJsonFile<CityData>(inputDirectory.CombineName("city.json"));
            var poiConfigs = cityData.PointsOfInterest.Select(entry => new PointOfInterestConfig(new(entry.Key),
                entry.Value.LocationType, entry.Value.QueueCapacity));

            // log the seed used for each target to make simulation reproducible
            CreateTargetSeedFile(resultDir, houseConfigs);

            // check if there is travel data in separate files and assign it to the CityData object
            ParseTravelData(inputDirectory, cityData, logger);

            // create a new scenario object containing all house and POI configs
            return new Scenario(newDbPath, calcSpec, calcParameters, houseConfigs, poiConfigs, cityData, inputDirectoryPath);
        }

        /// <summary>
        /// Parses route data from a separate "routes" subdirectory, if it exists.
        /// Each contained json file specifies the available routes for a different time slot.
        /// Additionally, the optional fiel "cluster_info.json" can provide a mapping of building IDs to
        /// clusters, if buildings were clustered for the route definitions.
        /// </summary>
        /// <param name="inputDirectory">the scenario input directory</param>
        /// <param name="city">the parsed city definition</param>
        /// <param name="logger">logger object to use</param>
        /// <exception cref="LPGException">if two conflicting travel definitions are found</exception>
        private static void ParseTravelData(DirectoryInfo inputDirectory, CityData city, MPILogger logger)
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
            logger.Info($"Found {routeFiles.Length} files in the routes subdirectory.");
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
            var dayTypes = $"\\d+|{KEY_ALL}";
            Match match = Regex.Match(text, @"_(" + dayTypes + @")_(\d+)to(\d+)");
            if (!match.Success)
                throw new LPGPBadParameterException($"Could not parse time slot: {text}");

            // parse the weekday this time slot applies to
            string dayTypeText = match.Groups[1].Value;
            HashSet<DayOfWeek> daysOfWeek;
            if (int.TryParse(dayTypeText, out int dayTypeIndex))
            {
                // time slot only applies to this specific day of week
                // 0 means Monday in the input, unlike with DayOfWeek --> convert
                var dayOfWeek = (DayOfWeek)((dayTypeIndex + 1) % 7);
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
        /// <param name="seedProvider">a function to get the RNG seed for each house</param>
        /// <returns>all house configs from the directory</returns>
        private static ICollection<ResidentialBuildingConfig> CollectHouseConfigs(string directory, Func<string, int> seedProvider)
        {
            var files = Directory.GetFiles(directory);
            // sort filenames to ensure that they are always in the same order
            Array.Sort(files);
            // return the result as a collection instead of an enumerable to avoid assigning different random values on each access
            return [.. files.Select(f => CreateHouseConfig(f, seedProvider))];
        }

        /// <summary>
        /// Create a single house config from a filepath, using a function to assign the RNG seed for this hosue.
        /// </summary>
        /// <param name="filepath">house config filepath</param>
        /// <param name="seedProvider">function to get the RNG seed for the new house</param>
        /// <returns>a new house config</returns>
        private static ResidentialBuildingConfig CreateHouseConfig(string filepath, Func<string, int> seedProvider)
        {
            string id = Path.GetFileNameWithoutExtension(filepath);
            return new ResidentialBuildingConfig(id, filepath, seedProvider(id));
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
            string path = Path.Combine(resultDir, Constants.HouseSeedMappingFile);
            AutomationUtili.WriteToJsonFile(seedDict, path);
        }

        /// <summary>
        /// Check whether there are existing database files from a previous city simulation that can be reused.
        /// Does not check the file contents, only if a suitable number of files exists.
        /// </summary>
        /// <param name="resultDir">result directory of the simulation</param>
        /// <param name="numWorkers">number of workers for the simulation</param>
        /// <param name="pathToDb">path to the source database to use</param>
        /// <returns>true if database files exist and can be used, otherwhise false</returns>
        /// <exception cref="LPGPBadParameterException">if files exist, but the number of workers does not match</exception>
        private static bool CanUseExistingDatabases(string resultDir, int numWorkers, string pathToDb)
        {
            var databaseDir = Path.Combine(resultDir, Constants.DataBaseDirectory);
            var seedFile = Path.Combine(resultDir, Constants.HouseSeedMappingFile);
            if (!Directory.Exists(databaseDir) || !File.Exists(seedFile))
                return false; // no usable cached data exists, proceed as usual

            // database directory and seed file exist already
            var files = new DirectoryInfo(databaseDir).GetFiles("*.db3");
            if (files.Length == 0)
                return false; // database directory is empty, proceed without using existing files

            if (files.Length != numWorkers)
                throw new LPGPBadParameterException($"Starting a city simulation with {numWorkers} workers, but found {files.Length} existing database files. "
                    + "The city simulation can only reuse existing databases when using the same number of workers. Please either start the simulation again with "
                     + $"{numWorkers} workers or delete the existing database directory: {databaseDir}");

            // check if the source database has been modified after creation of the worker databases
            var oldestWorkerDbDate = files.Min(f => f.LastWriteTime);
            if (oldestWorkerDbDate < File.GetLastWriteTime(pathToDb))
                throw new LPGPBadParameterException($"Existing databases are older than the source database to use: {pathToDb}");

            // reuse the existing databases to skip house generation from templates
            return true;
        }
    }
}
