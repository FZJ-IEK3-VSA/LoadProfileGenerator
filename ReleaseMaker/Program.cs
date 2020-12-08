using Common;
using JetBrains.Annotations;

namespace ReleaseMaker
{
    public static class Program
    {
        public static void Main([JetBrains.Annotations.NotNull][ItemNotNull] string[] args)
        {
            Config.OutputToConsole = true;
            ReleaseBuilderTests rb = new ReleaseBuilderTests();
            rb.MakeRelease();
        }
    }
}
