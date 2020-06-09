using System;
using JetBrains.Annotations;
using Xunit.Abstractions;


namespace ReleaseBuilder {
    public static class Program {

        private class Teh : ITestOutputHelper {
            public void WriteLine(string message)
            {
                Console.WriteLine(message);
            }

            public void WriteLine(string format, params object[] args)
            {
                Console.WriteLine(format,args);
            }
        }
        public static void Main([NotNull] [ItemNotNull] string[] args)
        {
            ReleaseBuilderTests rb = new ReleaseBuilderTests(new Teh());
            rb.MakeRelease();
        }
    }
}