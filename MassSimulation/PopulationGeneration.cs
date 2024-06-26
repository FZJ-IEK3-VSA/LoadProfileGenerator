using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    internal static class PopulationGeneration
    {
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
