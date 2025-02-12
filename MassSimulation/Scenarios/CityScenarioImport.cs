using Automation;
using Automation.ResultFiles;
using Common;
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
            // read house job file; only calcspec and database path are actually needed here
            string houseJobStr = File.ReadAllText(inputDirectory.CombineName("Calcspec.json")).Trim(HouseGenerator.charsToTrim);
            HouseCreationAndCalculationJob? hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr);
            if (hcj == null)
                throw new LPGException("housejob was null");
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
            houseGenerator.CleanResultDirectoryBeforeSimulation(resultDir);

            // copy DB file to result directory and open a connection to it
            var sim = houseGenerator.CopyAndOpenDatabase(hcj.PathToDatabase, resultDir, out string newDbPath);
            string fullDbPath = Path.GetFullPath(hcj.PathToDatabase);
            Logger.Info("Using database file: " + fullDbPath);

            // save settings to the database copy in the result directory
            JsonCalculator.SaveSettingsToDatabase(sim, calcSpec);

            // create house configs and POI configs from the files in the input directory
            var houseConfigs = CollectHouseConfigs(inputDirectory.CombineName("houses"));
            var cityData = ReadCityDataFile(inputDirectory.CombineName("city.json"));
            var poiConfigs = cityData.PointsOfInterest.Select(entry => new PointOfInterestConfig(new(entry.Key)));

            // check if routes are defined in a separate file
            string routesFile = inputDirectory.CombineName("routes.json");
            if (File.Exists(routesFile))
            {
                if (!cityData.Routes.IsNullOrEmpty())
                    throw new LPGException("Routes are defined in both city.json and routes.json. Only one of them is allowed at a time.");

                var routes = ReadRoutesFile(routesFile);
                cityData.Routes = routes.ToList();
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
        private static IEnumerable<MassSimTargetReference> CollectHouseConfigs(string directory)
        {
            return Directory.GetFiles(directory).Select(f => new MassSimTargetReference(Path.GetFileNameWithoutExtension(f), f));
        }

        /// <summary>
        /// Collects all POI config files in the input directory and creates a POI config object for
        /// each of them.
        /// </summary>
        /// <param name="directory">the subdirectory in the input directory containing the POI configs</param>
        /// <returns>all POI configs from the directory</returns>
        private static IEnumerable<PointOfInterestConfig> CollectPOIConfigs(string directory)
        {
            return Directory.GetFiles(directory).Select(f => new PointOfInterestConfig(new(Path.GetFileName(f))));
        }

        /// <summary>
        /// Parse all the city data from the file city.json in the input directory.
        /// </summary>
        /// <param name="filename">path to the city.json file containing a CityData object</param>
        /// <returns>the parsed CityData object containing all relevant city information</returns>
        /// <exception cref="LPGException">if the file was invalid</exception>
        private static CityData ReadCityDataFile(string filename)
        {
            string cityDataJson = File.ReadAllText(filename).Trim(HouseGenerator.charsToTrim);
            CityData? cityData = JsonConvert.DeserializeObject<CityData>(cityDataJson);
            if (cityData is null)
                throw new LPGException($"Could not read CityData from file {filename}");
            return cityData;
        }

        /// <summary>
        /// Parse all routes used in the city from the file routes.json in the input directory.
        /// </summary>
        /// <param name="filename">path to the routes.json file containing a dictionary of routes</param>
        /// <returns>all parsed routes</returns>
        /// <exception cref="LPGException">if the file was invalid</exception>
        private static IEnumerable<RouteData> ReadRoutesFile(string filename)
        {
            string routesJson = File.ReadAllText(filename).Trim(HouseGenerator.charsToTrim);
            var routesDict = JsonConvert.DeserializeObject<Dictionary<string, RouteData>>(routesJson);
            if (routesDict is null)
                throw new LPGException($"Could not read routes from file {filename}");
            return routesDict.Values;
        }
    }
}
