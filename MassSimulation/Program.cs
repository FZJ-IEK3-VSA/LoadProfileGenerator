using System;
using MPI;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace MassSimulation
{

    internal static class Program
    {

        public static void Main([NotNull] string[] args)
        {
            MPI.Environment.Run(WorkerMain);
        }

        public static void WorkerMain(Intracommunicator comm)
        {
            // use a JSON serializer instead of the default serializer
            // which uses the obsolete BinaryFormatter
            comm.Serialization.Serializer = CustomJsonSerializer.Default;
            // TODO: try MessagePack instead: https://steven-giesel.com/blogPost/4271d529-5625-4b67-bd59-d121f2d8c8f6
            //       seems to be faster and just as easy to use


            int rank = comm.Rank;
            int numWorkers = comm.Size;
            string workerName = MPI.Environment.ProcessorName;

            int numAgents = 0;
            AgentInfo[]? allAgents = null;
            if (rank == 0)
            {
                // initialization
                numAgents = 100;
                allAgents = CreateAgentPopulation(numAgents);
            }

            // distribute agents
            comm.Broadcast(ref numAgents, 0);
            AgentInfo[] agentsArray = comm.ScatterFromFlattened(allAgents, numAgents / numWorkers, 0);
            List<AgentInfo> agents = agentsArray.ToList();

            //print(rank, "Number of agents: " + agents.Count + ", starting with " + agents[0].Name);


            // main simulation loop
            int numTimesteps = 2;
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

            print(rank, "Number of agents: " + agents.Count + ", starting with " + agents[0].Name);


            // collect all agents
            var agentCounts = comm.Gather(agents.Count, 0);
            allAgents = comm.GatherFlattened(agents.ToArray(), agentCounts, 0);
            comm.Barrier();

            // result processing
            if (rank == 0)
            {
                ProcessResults(allAgents);
            }
        }


        public static void SimulateOneStep(int step, int rank, List<AgentInfo> agents)
        {
            foreach (AgentInfo agent in agents)
            {
                agent.Log += rank;
            }
        }

        public static List<AgentInfo>[] GetNextWorkerForAgents(int rank, int numWorkers, List<AgentInfo> agents)
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


        public static List<List<T>> Split<T>(this IList<T> source, int length)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / length)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
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