using Automation;
using Common.Extensions;
using CitySimulation.SimulationTargets;
using Automation.ResultFiles;

namespace CitySimulation.Scenarios
{
    /// <summary>
    /// Represents a simulation scenario with all objects that belong to that, including
    /// fully defined houses with households etc.
    /// </summary>
    /// <param name="databasePath">path of the new database file to use in the target directory</param>
    /// <param name="calcSpec">calc specification with basic simulation parameters for all houses</param>
    /// <param name="targetReferences">the residential building configurations</param>
    /// <param name="pointsOfInterest">the poin of interest configurations</param>
    /// <param name="cityData">the city object with all points of interest and travel information</param>
    /// <param name="scenarioPath">the path of the scenario directory</param>
    public class Scenario(string databasePath, JsonCalcSpecification calcSpec, IEnumerable<ResidentialBuildingConfig> targetReferences, IEnumerable<PointOfInterestConfig> pointsOfInterest, CityData cityData, string scenarioPath)
    {
        public string DatabasePath { get; } = databasePath;
        public JsonCalcSpecification CalcSpecification { get; private set; } = calcSpec;
        public IEnumerable<ResidentialBuildingConfig> TargetReferences { get; private set; } = targetReferences;
        public IEnumerable<PointOfInterestConfig> PointsOfInterest { get; private set; } = pointsOfInterest;
        public CityData CityData { get; set; } = cityData;
        public string ScenarioPath { get; } = scenarioPath;

        /// <summary>
        /// Divide the scenario into scenario parts, one for each worker.
        /// </summary>
        /// <param name="numberOfParts"></param>
        /// <returns></returns>
        public ScenarioPart[] GetScenarioParts(int numberOfParts)
        {
            // distribute the houses to get similar total person counts
            var targetSublists = SplitHousesForFairPersonCounts(numberOfParts);
            // distribute the POIs evenly
            var poiSublists = PointsOfInterest.Split(numberOfParts);

            var poiRegister = BuildPointOfInterestRegister(poiSublists);

            // create the list of scenario part objects, each with its own share of households and POIs
            var parts = targetSublists.ZipLongest(poiSublists, [], []).Select(listPair => new ScenarioPart(listPair.Item1.ToList(), listPair.Item2.ToList(), DatabasePath, CalcSpecification, poiRegister, CityData));
            return parts.ToArray();
        }

        /// <summary>
        /// Attempts to fairly split up the list of houses, so that each worker ends up with a similar
        /// number of persons in total. Uses a greedy approach which does not guarantee optimal results.
        /// </summary>
        /// <param name="numberOfParts">the number of sublists to split the houses into</param>
        /// <returns>an array of sublists containing the houses</returns>
        private List<ResidentialBuildingConfig>[] SplitHousesForFairPersonCounts(int numberOfParts)
        {
            // load number of persons per house from the scenario statistics
            var personNumberFile = new DirectoryInfo(ScenarioPath).CombineName("statistics/persons_per_house.json");
            if (!File.Exists(personNumberFile))
                throw new LPGException($"File with persons per house does not exist: {personNumberFile}");
            var personNumbers = AutomationUtili.ParseJsonFile<Dictionary<string, int>>(personNumberFile);
            if (!TargetReferences.All(t => personNumbers.ContainsKey(t.Id)))
                throw new LPGPBadParameterException($"Person number file does not contain all houses: {personNumberFile}");

            // order the houses descendingly by the number of persons
            var orderedTargets = TargetReferences.OrderByDescending(t => personNumbers[t.Id]);

            // init an array of empty lists and sums starting with 0
            var targetSublists = Enumerable.Range(0, numberOfParts).Select(_ => new List<ResidentialBuildingConfig>()).ToArray();
            var sums = new int[numberOfParts];

            // assign each house to the list with the currently smallest sum
            foreach (var target in orderedTargets)
            {
                int indexLowest = Array.IndexOf(sums, sums.Min());
                targetSublists[indexLowest].Add(target);
                sums[indexLowest] += personNumbers[target.Id];
            }
            return targetSublists;
        }

        private PointOfInterestRegister BuildPointOfInterestRegister(IEnumerable<IEnumerable<PointOfInterestConfig>> poiSublists)
        {
            Dictionary<string, int> poiMapping = [];
            int workerId = 0;
            foreach (var sublist in poiSublists)
            {
                foreach (var poi in sublist)
                {
                    poiMapping.Add(poi.Id.Id, workerId);
                }
                workerId++;
            }
            return new PointOfInterestRegister(poiMapping);
        }
    }
}
