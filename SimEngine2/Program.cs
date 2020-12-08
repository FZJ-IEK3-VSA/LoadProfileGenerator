using System;
using JetBrains.Annotations;
using SimulationEngineLib;

namespace SimEngine2
{
    public static class Program
    {
        public static void Main([JetBrains.Annotations.NotNull] [ItemNotNull] string[] args)
        {
            Console.WriteLine("Command line:");
            foreach (var arg in args) {
                Console.WriteLine(arg);
            }
            Console.WriteLine("Starting...");
            MainSimEngine.Run(args, "simengine2.exe");
        }

    }
}
