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
            // use a JSON serializer instead of the default serializer
            // which uses the obsolete BinaryFormatter
            comm.Serialization.Serializer = CustomJsonSerializer.Default;
            // TODO: try MessagePack instead: https://steven-giesel.com/blogPost/4271d529-5625-4b67-bd59-d121f2d8c8f6
            //       seems to be faster and just as easy to use; other Alternative: protobuf


            int numAgents = 10;
            List<AgentInfo> agents = InitSimulation(numAgents);

            Stopwatch watch = Stopwatch.StartNew();
            // main simulation loop
            int numTimesteps = 100;
            RunSimulation(numTimesteps, ref agents);
            watch.Stop();
            print(rank, "Number of agents: " + agents.Count);
            comm.Barrier();
            if (rank == 0) Console.WriteLine("Simulation time: " + Math.Round((double)watch.ElapsedMilliseconds / 1000, 2) + " s");

            CollectResults(agents);
        }

        public List<AgentInfo> InitSimulation(int numAgents)
        {
            AgentInfo[]? allAgents = null;
            if (rank == 0)
            {
                // initialize agents
                allAgents = CreateAgentPopulation(numAgents);
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

                //if (rank == 0)
                //{
                //    for (int j = 0; j < arrivingAgents.Length; j++)
                //    {
                //        print(0, "Received from " + j + ": " + arrivingAgents[j].Count);
                //    }
                //}
            }
        }

        private void CollectResults(List<AgentInfo> agents)
        {
            // collect all agents
            int[] agentCounts = comm.Gather(agents.Count, 0);
            var allAgents = comm.GatherFlattened(agents.ToArray(), agentCounts, 0);
            comm.Barrier();

            // result processing
            if (rank == 0)
            {
                //ProcessResults(allAgents);
            }
        }

        public void SimulateOneStep(int step, int rank, List<AgentInfo> agents)
        {
            foreach (AgentInfo agent in agents)
            {
                agent.Log += rank;
                Thread.Sleep(10);
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
                if (random.NextDouble() > 0.7)
                {
                    int target = random.Next(numWorkers);

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


        public static void ProcessResults(IEnumerable<AgentInfo> allAgents)
        {
            foreach (var agent in allAgents)
            {
                Console.WriteLine(agent.Name + " - " + agent.Log);
            }
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
            var content = RandomString(1000);
            return new AgentInfo(id, "Bob" + id, content);
        }


        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void print(int rank, string s)
        {
            Console.WriteLine(rank + ": " + s);
        }

        public static void print(int timestep, int rank, string s)
        {
            Console.WriteLine(timestep + ": " + rank + ": " + s);
        }
    }
}
