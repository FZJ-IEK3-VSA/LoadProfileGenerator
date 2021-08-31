using System.Collections.Generic;
using System.IO;

namespace ReleaseMaker {
    public class SimEngine2Copier : CopierBase {
        public static void CopySimEngine2Files([JetBrains.Annotations.NotNull] string src, [JetBrains.Annotations.NotNull] string dst)
        {
            List<string> programFiles = new List<string>();
            var srcDi = new DirectoryInfo(src);
            var dlls = srcDi.GetFiles("*.dll");
            foreach (var dll in dlls)
            {
                Copy(programFiles, srcDi, src, dst, dll.Name);
            }

            Copy(programFiles, srcDi, src, dst, @"createdump.exe");
            Copy(programFiles, srcDi, src, dst, @"simengine2.deps.json");
            Copy(programFiles, srcDi, src, dst, @"simengine2.exe");
            Copy(programFiles, srcDi, src, dst, @"simengine2.runtimeconfig.json");

            CheckIfFilesAreCompletelyCopied(src, programFiles);
        }
    }
}