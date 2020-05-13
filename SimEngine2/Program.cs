using JetBrains.Annotations;
using SimulationEngineLib;

namespace SimEngine2
{
    public static class Program
    {
        public static void Main([NotNull] [ItemNotNull] string[] args)
        {
            MainSimEngine.Run(args, "simengine2.exe");
        }

    }
}
