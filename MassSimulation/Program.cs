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
            MPI.Environment.Run(RunNewWorker);
        }

        static void RunNewWorker(Intracommunicator comm)
        {
            Worker worker = new Worker(comm);
            worker.Run();
        }
    }
}