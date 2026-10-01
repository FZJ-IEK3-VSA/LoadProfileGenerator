using MPI;

namespace CitySimulation
{
    internal static class Program
    {
        /// <summary>
        /// Main function of the city simulation.
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {
            var runWorkerAction = CreateMPIRunAction(args);
            MPI.Environment.Run(runWorkerAction);
        }

        /// <summary>
        /// Creates the action that can be passed to MPI.Environment.Run and
        /// that starts a city simulation worker. This is necessary to capture
        /// the command line arguments in the action.
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns>the action that runs a city simulation worker</returns>
        public static Action<Intracommunicator> CreateMPIRunAction(string[] args)
        {
            return comm => RunNewWorker(comm, args);
        }

        /// <summary>
        /// Initializes and runs a single MPI city simulation worker.
        /// </summary>
        /// <param name="comm">the MPI communicator object to use</param>
        /// <param name="args">command line arguments</param>
        static void RunNewWorker(Intracommunicator comm, string[] args)
        {
            // change to a JSON serializer instead of the default serializer
            // which uses the obsolete BinaryFormatter
            comm.Serialization.Serializer = MPIJsonSerializer.Default;
            // TODO: try MessagePack instead: https://steven-giesel.com/blogPost/4271d529-5625-4b67-bd59-d121f2d8c8f6
            //       seems to be faster and just as easy to use; other Alternative: protobuf

            Worker worker = new(comm, args);
            worker.Run();
        }
    }
}