using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    public class AgentInfo
    {
        public AgentInfo(int id, string name, string content)
        {
            Id = id;
            Name = name;
            Content = content;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }

        public string Log { get; set; } = "";
    }
}
