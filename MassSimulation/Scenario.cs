using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using CalculationEngine;
using Common;
using Newtonsoft.Json;
using SimulationEngineLib.HouseJobProcessor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// Represents a simulation scenario with all objects that belong to that, including
    /// fully defined houses with households etc.
    /// </summary>
    internal class Scenario
    {
        public string DatabasePath { get; private set; }
        public JsonCalcSpecification CalcSpecification { get; private set; }
        public MassSimTargetReference[] TargetReferences { get; private set; }

        // TODO: also store SMEs here

        public Scenario(string databasePath, JsonCalcSpecification calcSpec, MassSimTargetReference[] targetReferences)
        {
            DatabasePath = databasePath;
            CalcSpecification = calcSpec;
            TargetReferences = targetReferences;
        }

        /// <summary>
        /// Creates a scenario with many identical houses.
        /// </summary>
        /// <param name="houseJobFile">path to the house job definition file</param>
        /// <param name="numberOfHouseholds">number of copies of this house</param>
        /// <returns>scenario with identical copies of the defined house</returns>
        /// <exception cref="LPGException">if the house job file was invalid</exception>
        public static Scenario CreateDuplicateHousesScenario(string databasePath, string houseJobFile, int numberOfHouseholds)
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
            houseGenerator.CleanupResultDir(resultDir);

            // copy DB file to result directory and open a connection to it
            var sim = houseGenerator.CopyAndOpenDatabase(hcj.PathToDatabase, resultDir);

            var calcObjectReference = houseGenerator.GetHouseReference(hcj, sim);

            // save settings to the database copy in the result directory
            JsonCalculator.SaveSettingsToDatabase(sim, hcj.CalcSpec);

            // create duplicates of the same house
            var targetReferences = new MassSimTargetReference[numberOfHouseholds];
            for (int i = 0; i < numberOfHouseholds; i++)
            {
                targetReferences[i] = new MassSimTargetReference("House " + i, calcObjectReference);
            }
            return new Scenario(databasePath, calcSpec, targetReferences);
        }

        /// <summary>
        /// Divide the scenario into scenario parts, one for each worker.
        /// </summary>
        /// <param name="numberOfParts"></param>
        /// <returns></returns>
        public ScenarioPart[] GetScenarioParts(int numberOfParts)
        {
            // divide the JsonReferences evenly
            var referencesSublists = TargetReferences.Split(numberOfParts);
            var parts = referencesSublists.Select(sublist => new ScenarioPart(DatabasePath, sublist.ToList(), CalcSpecification));
            return parts.ToArray();
        }

        public static AgentInfo[] CreateAgentPopulation(int size)
        {
            // initialize the agents
            var allAgents = new AgentInfo[size];
            for (int i = 0; i < size; i++)
            {
                allAgents[i] = CreateAgent(i);
            }
            return allAgents;
        }

        public static AgentInfo CreateAgent(int id)
        {
            var content = Utils.RandomString(1024 * 10);
            return new AgentInfo(id, "Bob" + id, content);
        }
    }
}
