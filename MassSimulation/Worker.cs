using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.JSON;
using MPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// MPI Worker class that is instantiated once per MPI process.
    /// </summary>
    internal class Worker
    {
        Intracommunicator comm;
        int rank;
        int numWorkers;
        string workerName;

        private List<ISimulator> simulators = [];
        private CalcParameters? calcParameters = null;
        private MPILogger logger;

        public Worker(Intracommunicator comm)
        {
            this.comm = comm;
            rank = comm.Rank;
            numWorkers = comm.Size;
            workerName = MPI.Environment.ProcessorName;
            logger = new MPILogger(true, rank);
        }

        public void Run()
        {
            int numAgents = 4;
            var databasePath = "profilegenerator-latest.db3";
            var calcSpecFile = @"D:\Home\Homeoffice\Arbeit FzJ\Projekte\Große Projekte\03 - LPG\test.json";

            logger.Info("Starting mass simulation with " + numWorkers + " workers.");

            Stopwatch watch = Stopwatch.StartNew();
            InitSimulation(databasePath, calcSpecFile, numAgents);


            // main simulation loop
            RunSimulation();
            watch.Stop();

            FinishSimulation();

            comm.Barrier();
            if (rank == 0)
                logger.Info("Simulation time: " + Math.Round((double)watch.ElapsedMilliseconds / 1000, 2) + " s");
        }

        public void InitSimulation(string databasePath, string houseJobFile, int numAgents)
        {
            // general settings
            // avoid MPI processes cluttering the console
            Config.OutputToConsole = false;

            // create scenario
            ScenarioPart[]? scenarioParts = null;
            if (rank == 0)
            {
                // determine simulation targets
                Scenario scenario = Scenario.CreateDuplicateHousesScenario(databasePath, houseJobFile, numAgents);
                scenarioParts = scenario.GetScenarioParts(numWorkers);
                int length = scenarioParts.Length;
                if (length < numWorkers)
                {
                    // not enough parts for all workers
                    throw new LPGException("Not enough work packages for all MPI processes (" + length + " work packages for " + numWorkers + " workers).");
                }
            }

            // distribute simulation targets
            ScenarioPart partForThisWorker = comm.Scatter(scenarioParts, 0);

            LPGMassSimulator lpgSimulator = new(rank, partForThisWorker);
            simulators.Add(lpgSimulator);
            calcParameters = lpgSimulator.CalcParameters;
        }

        private void RunSimulation()
        {
            if (calcParameters == null)
            {
                throw new LPGException("CalcParameters are not set");
            }
            var simulationTime = calcParameters.InternalStartTime;
            var timestep = new TimeStep(0, calcParameters);

            while (simulationTime < calcParameters.InternalEndTime)
            {
                SimulateOneStep(timestep, simulationTime);

                //var agentsByNewLocation = GetNextWorkerForAgents(rank, numWorkers, calcObjectReferences);

                // move agents to respective target worker
                //var arrivingAgents = comm.Alltoall(agentsByNewLocation);
                // reset list of local agents
                //calcObjectReferences = arrivingAgents.SelectMany(x => x).ToList();

                // TODO: this barrier simulates agent exchange
                comm.Barrier();

                // increment timestep
                simulationTime += calcParameters.InternalStepsize;
                timestep = timestep.AddSteps(1);
            }
        }

        private void FinishSimulation()
        {
            foreach (var simulator in simulators)
            {
                simulator.FinishSimulation();
            }
        }

        public void SimulateOneStep(TimeStep timeStep, DateTime simulationTime)
        {
            foreach (var simulator in simulators)
            {
                simulator.SimulateOneStep(timeStep, simulationTime);
            }
        }

        public List<AgentInfo>[] GetNextWorkerForAgents(int rank, int numWorkers, List<AgentInfo> agents)
        {
            List<AgentInfo>[] agentsByWorker = new List<AgentInfo>[numWorkers];
            for (int i = 0; i < numWorkers; i++)
            {
                agentsByWorker[i] = new List<AgentInfo>();
            }
            foreach (AgentInfo agent in agents)
            {
                // Determine if agent moves, and where
                if (Utils.random.NextDouble() > 0.8)
                {
                    int target = Utils.random.Next(numWorkers);

                    agentsByWorker[target].Add(agent);
                }
                else
                {
                    // agent stays on this worker
                    agentsByWorker[rank].Add(agent);
                }
            }
            return agentsByWorker;
        }


        private void CollectResults(List<AgentInfo> agents)
        {
            // collect all agents
            int[] agentCounts = comm.Gather(agents.Count, 0);
            if (rank == 0)
                Console.WriteLine("Agent counts: " + string.Join(", ", agentCounts));
            var allAgents = comm.GatherFlattened(agents.ToArray(), agentCounts, 0);
            comm.Barrier();

            // result processing
            if (rank == 0)
            {
                //ProcessResults(allAgents);
            }
        }

        public static void ProcessResults(IEnumerable<AgentInfo> allAgents)
        {
            foreach (var agent in allAgents)
            {
                Console.WriteLine(agent.Name + " - " + agent.Log);
            }
        }
    }
}
