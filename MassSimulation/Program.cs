using System.Diagnostics.CodeAnalysis;

namespace MassSimulation
{
    internal static class Program
    {

        public static void Main([NotNull] string[] args)
        {
            MPI.Environment.Run(ref args, communicator =>
            {
                Console.WriteLine("Hello, from process number "
                                         + communicator.Rank + " of "
                                         + communicator.Size);
            });
        }
    }
}