using Automation;
using MassSimulation.Simulators;
using SimulationEngineLib.HouseJobProcessor;

namespace MassSimulation.CityGeneration
{

    /// <summary>
    /// Mockup for creation of a city scenario from external datasources (Builda etc.)
    /// </summary>
    internal class CityScenarioGenerator
    {
        public Scenario ImportCity(string inputFile)
        {
            HouseGenerator houseGenerator = new();
            List<MassSimTargetReference> targetReferences = [];

            // read the input file
            // iterate through all buildings
            object[] buildings = [];
            foreach (object buildingInfo in buildings)
            {
                if (IsResidential(buildingInfo))
                {
                    // create a house description out of the building data
                    HouseCreationAndCalculationJob? hcj = null;
                    
                    // chose or generate the corresponding LPG house and contained households
                    // remark: perhaps split here, so that not all households are generated in the same DB file
                    var calcObjectReference = houseGenerator.GetHouseReference(hcj, null);
                    var targetRef = new MassSimTargetReference("House 123", calcObjectReference);
                    targetReferences.Add(targetRef);

                    // I cannot modify a Persons Affordances until the CalcHousehold is created immediately before calculation
                    // Determine POI preferences here and save them in the MassSimTargetReference for later application in CalcPerson
                }
                else
                {
                    // determine building category, size/attractiveness
                    // map to LPG CalcLocation to determine available affordances
                    // create a set of available affordances including duration to simulate stay durations
                    // --> this might not be necessary if NewRemoteActivity messages contain a requested duration
                    // create point of interest
                    var poiSim = new PointOfInterestConfig(new("123"));
                }
            }
            return new Scenario(null, null, null, null);
        }

        private bool IsResidential(object buildingInfo)
        {
            throw new NotImplementedException();
        }
    }
}
