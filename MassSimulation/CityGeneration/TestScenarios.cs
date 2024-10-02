using Automation;
using Automation.ResultFiles;
using Newtonsoft.Json;
using SimulationEngineLib.HouseJobProcessor;

namespace MassSimulation.CityGeneration
{
    public static class TestScenarios
    {
        /// <summary>
        /// Creates a scenario with many identical houses.
        /// </summary>
        /// <param name="houseJobFile">path to the house job definition file</param>
        /// <param name="numberOfHouseholds">number of copies of this house</param>
        /// <returns>scenario with identical copies of the defined house</returns>
        /// <exception cref="LPGException">if the house job file was invalid</exception>
        public static Scenario CreateDuplicateHousesScenario(string houseJobFile, int numberOfHouseholds)
        {
            // read house job file
            string houseJobStr = File.ReadAllText(houseJobFile).Trim(HouseGenerator.charsToTrim);
            HouseCreationAndCalculationJob? hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr);
            if (hcj == null)
                throw new LPGException("housejob was null");
            var calcSpec = hcj.CalcSpec ?? throw new LPGException("No CalcSpec was given in the input file");
            // TODO: calcspec should be complete and single-source-of-parameters
            // --> check and fill all missing values in the calcspec first, then move on

            // create result directory
            var resultDir = hcj.CalcSpec.OutputDirectory ??= HouseGenerator.DefaultResultDirectory;
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

            var calcObjectReference = houseGenerator.GetHouseReference(hcj, sim);

            // save settings to the database copy in the result directory
            JsonCalculator.SaveSettingsToDatabase(sim, hcj.CalcSpec);

            // create duplicates of the same house
            var targetReferences = new MassSimTargetReference[numberOfHouseholds];
            for (int i = 0; i < numberOfHouseholds; i++)
            {
                targetReferences[i] = new MassSimTargetReference("House " + i, calcObjectReference);
            }
            return new Scenario(newDbPath, calcSpec, targetReferences);
        }
    }
}