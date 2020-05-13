using System.Runtime.CompilerServices;
using ChartCreator2.OxyCharts;
using JetBrains.Annotations;
using SimulationEngineLib;

[assembly: InternalsVisibleTo("SimulationEngine.Tests")]

namespace SimulationEngine {
    internal static class Program {

        public static void Main([NotNull] [ItemNotNull] string[] args)
        {
            CommandProcessor.MakeFlameChart = ChartMaker.MakeFlameChart;
            CommandProcessor.Makeallthecharts = ChartMaker.MakeChartsAndPDF;
            MainSimEngine.Run(args,"simulationengine.exe");
        }
    }
}