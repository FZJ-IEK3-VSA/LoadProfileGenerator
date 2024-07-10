using CalculationEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    internal class MassSimulationTarget
    {
        public readonly string Id;
        public readonly CalcManager CalcManager;
        public readonly string ResultDirectory;

        public MassSimulationTarget(string id, CalcManager calcManager, string resultDirectory)
        {
            Id = id;
            CalcManager = calcManager;
            ResultDirectory = resultDirectory;
        }
    }
}
