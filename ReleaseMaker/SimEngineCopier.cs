using System.Collections.Generic;
using System.IO;

namespace ReleaseMaker {
    public class SimEngineCopier : CopierBase {
        [JetBrains.Annotations.NotNull]
        public static List<string> CopySimEngineFiles([JetBrains.Annotations.NotNull] string src, [JetBrains.Annotations.NotNull] string dst)
        {
            List<string> programFiles = [];
            var srcDi = new DirectoryInfo(src);
            var dlls = srcDi.GetFiles("*.dll");
            foreach (var dll in dlls)
            {
                Copy(programFiles, srcDi, src, dst, dll.Name);
            }

            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.deps.json");
            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.exe");
            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.runtimeconfig.json");
            Copy(programFiles, srcDi, src, dst, @"xunit.runner.json");

            //Copy(programFiles, srcDi, src, dst, @"createdump.exe");


            CheckIfFilesAreCompletelyCopied(src, programFiles);
            return programFiles;
        }
    }
}
