using Automation;
using CalculationEngine.CitySimulation;

namespace MassSimulation
{
    /// <summary>
    /// Represents a simulation scenario with all objects that belong to that, including
    /// fully defined houses with households etc.
    /// </summary>
    public class Scenario(string databasePath, JsonCalcSpecification calcSpec, IEnumerable<MassSimTargetReference> targetReferences, IEnumerable<PointOfInterestConfig> pointsOfInterest)
    {
        public string DatabasePath { get; private set; } = databasePath;
        public JsonCalcSpecification CalcSpecification { get; private set; } = calcSpec;
        public IEnumerable<MassSimTargetReference> TargetReferences { get; private set; } = targetReferences;
        public IEnumerable<PointOfInterestConfig> PointsOfInterest { get; private set; } = pointsOfInterest;

        /// <summary>
        /// Divide the scenario into scenario parts, one for each worker.
        /// </summary>
        /// <param name="numberOfParts"></param>
        /// <returns></returns>
        public ScenarioPart[] GetScenarioParts(int numberOfParts)
        {
            // divide the JsonReferences and POIs evenly
            // TODO: use a better split to put neighboring buildings and POIs to the same worker
            var referencesSublists = TargetReferences.Split(numberOfParts);
            var poiSublists = PointsOfInterest.Split(numberOfParts);

            var poiRegister = BuildPointOfInterestRegister(numberOfParts, poiSublists);

            // create the list of scenario part objects, each with its own share of households and POIs
            var parts = referencesSublists.Zip(poiSublists, (references, pois) => new ScenarioPart(references.ToList(), pois.ToList(), DatabasePath, CalcSpecification, poiRegister));
            return parts.ToArray();
        }

        private PointOfInterestRegister BuildPointOfInterestRegister(int numberOfWorkers, IEnumerable<IEnumerable<PointOfInterestConfig>> poiSublists)
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
