using MPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    internal class Worker
    {
        Intracommunicator comm;
        int rank;
        int numWorkers;
        string workerName;

        public Worker(Intracommunicator comm)
        {
            this.comm = comm;
            rank = comm.Rank;
            numWorkers = comm.Size;
            workerName = MPI.Environment.ProcessorName;
        }

        public void Run()
        {
            int numAgents = 1000;
            List<AgentInfo> agents = InitSimulation(numAgents);

            Stopwatch watch = Stopwatch.StartNew();
            // main simulation loop
            int numTimesteps = 10;
            RunSimulation(numTimesteps, ref agents);
            watch.Stop();
            comm.Barrier();
            if (rank == 0)
                Console.WriteLine("Simulation time: " + Math.Round((double)watch.ElapsedMilliseconds / 1000, 2) + " s");

            CollectResults(agents);
        }

        public List<AgentInfo> InitSimulation(int numAgents)
        {
            AgentInfo[]? allAgents = null;
            if (rank == 0)
            {
                // initialize agents
                allAgents = PopulationGeneration.CreateAgentPopulation(numAgents);
            }

            // distribute agents
            comm.Broadcast(ref numAgents, 0);
            AgentInfo[] agentsArray = comm.ScatterFromFlattened(allAgents, numAgents / numWorkers, 0);
            return agentsArray.ToList();
        }

        private void RunSimulation(int numTimesteps, ref List<AgentInfo> agents)
        {
            for (int i = 0; i < numTimesteps; i++)
            {
                SimulateOneStep(i, rank, agents);
                var agentsByNewLocation = GetNextWorkerForAgents(rank, numWorkers, agents);

                // move agents to respective target worker
                var arrivingAgents = comm.Alltoall(agentsByNewLocation);
                // reset list of local agents
                agents = arrivingAgents.SelectMany(x => x).ToList();
            }
        }

        public void SimulateOneStep(int step, int rank, List<AgentInfo> agents)
        {
            foreach (AgentInfo agent in agents)
            {
                agent.Log += rank;
                //Thread.Sleep(10);
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
