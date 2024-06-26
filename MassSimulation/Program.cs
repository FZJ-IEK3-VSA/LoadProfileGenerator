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
            // change to a JSON serializer instead of the default serializer
            // which uses the obsolete BinaryFormatter
            comm.Serialization.Serializer = MPIJsonSerializer.Default;
            // TODO: try MessagePack instead: https://steven-giesel.com/blogPost/4271d529-5625-4b67-bd59-d121f2d8c8f6
            //       seems to be faster and just as easy to use; other Alternative: protobuf

            Worker worker = new Worker(comm);
            worker.Run();
        }
    }
}