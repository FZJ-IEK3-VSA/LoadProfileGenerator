using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using MassSimulation.Scenarios;
using MassSimulation.SimulationTargets;
using Newtonsoft.Json;
using PowerArgs;
using SimulationEngineLib.HouseJobProcessor;

namespace MassSimulation.CityGeneration
{
    /// <summary>
    /// Class for loading a city scenario from a directory. The directory must contain
    /// a Calcspec.json file with the calculation configuration, and files specifying all
    /// residential and non-residential buildings in the target area.
    /// </summary>
    internal class CityScenarioImport
    {
        public static Scenario ReadScenarioFromConfigDirectory(string inputDirectoryPath)
        {
            var inputDirectory = new DirectoryInfo(inputDirectoryPath);
            // read file Calcspec.json; it is a HouseCreationAndCalculationJob object, but only calcspec
            // and database path are required
            var hcj = ParseJsonFile<HouseCreationAndCalculationJob>(inputDirectory.CombineName("Calcspec.json"));
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

            // save settings to the database copy in the result directory
            JsonCalculator.SaveSettingsToDatabase(sim, calcSpec);

            // initialize an RNG to generate an individual seed for each simulation target
            int seed = CalcParameters.GetActualRandomSeed(calcSpec.RandomSeed);
            var random = new Random(seed);

            // create house configs and POI configs from the files in the input directory
            var houseConfigs = CollectHouseConfigs(inputDirectory.CombineName("houses"), random);
            var cityData = ParseJsonFile<CityData>(inputDirectory.CombineName("city.json"));
            var poiConfigs = cityData.PointsOfInterest.Select(entry => new PointOfInterestConfig(new(entry.Key)));

            // log the seed used for each target to make simulation reproducible
            CreateTargetSeedFile(resultDir, houseConfigs);

            // check if routes are defined in a separate file
            string routesFile = inputDirectory.CombineName("routes.json");
            if (File.Exists(routesFile))
            {
                if (!cityData.Routes.IsNullOrEmpty())
                    throw new LPGException("Routes are defined in both city.json and routes.json. Only one of them is allowed at a time.");

                var routes = ParseJsonFile<Dictionary<string, RouteData>>(routesFile);
                cityData.Routes = [.. routes.Values];
            }

            // create a new scenario object containing all house and POI configs
            return new Scenario(newDbPath, calcSpec, houseConfigs, poiConfigs, cityData);
        }

        /// <summary>
        /// Collects all house config files in the input directory and creates a simulation target object for
        /// each of them.
        /// </summary>
        /// <param name="directory">the subdirectory in the input directory containing the house configs</param>
        /// <returns>all house configs from the directory</returns>
        private static ICollection<MassSimTargetReference> CollectHouseConfigs(string directory, Random random)
        {
            var files = Directory.GetFiles(directory);
            // sort filenames to ensure that they are always in the same order
            Array.Sort(files);
            // return the result as a collection instead of an enumerable to avoid assigning different random values on each access
            return [.. files.Select(f => new MassSimTargetReference(Path.GetFileNameWithoutExtension(f), f, random.Next()))];
        }

        /// <summary>
        /// Create a JSON file containing the seed used for each simulation target. With this seed, the simulation
        /// of individual targets can be reproduced.
        /// </summary>
        /// <param name="resultDir">the output directory where the file will be created</param>
        /// <param name="targets">the target references with their seeds</param>
        private static void CreateTargetSeedFile(string resultDir, IEnumerable<MassSimTargetReference> targets)
        {
            var seedDict = targets.ToDictionary(t => t.Id, t => t.Seed);
            var jsonString = JsonConvert.SerializeObject(seedDict, Formatting.Indented);
            File.WriteAllText(Path.Combine(resultDir, Constants.HouseSeedMappingFile), jsonString);
        }

        /// <summary>
        /// Reads a JSON file and tries to parse the specified object from it.
        /// </summary>
        /// <typeparam name="T">the type of the object to parse</typeparam>
        /// <param name="filename">the name of the file containing the JSON</param>
        /// <returns>the parsed object</returns>
        /// <exception cref="LPGException">if the parsed object is null</exception>
        private static T ParseJsonFile<T>(string filename)
        {
            // use a StreamReader to avoid loading large files as a single string
            using var filereader = new StreamReader(filename);
            using var jsonreader = new JsonTextReader(filereader);
            var serializer = new JsonSerializer();
            var parsedObject = serializer.Deserialize<T>(jsonreader);
            return parsedObject ?? throw new LPGException($"Input file {filename} did not contain valid data.");
        }
    }
}
