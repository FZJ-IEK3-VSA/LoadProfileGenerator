using System.Collections.Generic;
using System.IO;

namespace ReleaseMaker {
    public class SimEngineCopier : CopierBase {
        [JetBrains.Annotations.NotNull]
        public static List<string> CopySimEngineFiles([JetBrains.Annotations.NotNull] string src, [JetBrains.Annotations.NotNull] string dst)
        {
            List<string> programFiles = new List<string>();
            var srcDi = new DirectoryInfo(src);
            var dlls = srcDi.GetFiles("*.dll");
            foreach (var dll in dlls)
            {
                Copy(programFiles, srcDi, src, dst, dll.Name);
            }

            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.deps.json");
            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.exe");
            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.runtimeconfig.dev.json");
            Copy(programFiles, srcDi, src, dst, @"SimulationEngine.runtimeconfig.json");
            Copy(programFiles, srcDi, src, dst, @"xunit.runner.json");
            Copy(programFiles, srcDi, src, dst, @"ref\SimulationEngine.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-arm\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-arm\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-arm64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-arm64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-x64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-x64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-x64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-x64\native\SQLite.Interop.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx\native\libHarfBuzzSharp.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx\native\libSkiaSharp.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx-x64\native\libSystem.IO.Ports.Native.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx-x64\native\SQLite.Interop.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\tizen-armel\native\libHarfBuzzSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\tizen-x86\native\libHarfBuzzSharp.so");


            Copy(programFiles, srcDi, src, dst, @"runtimes\win-arm64\native\libSkiaSharp.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-arm64\native\sni.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x64\native\libHarfBuzzSharp.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x64\native\libSkiaSharp.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x64\native\sni.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x64\native\SQLite.Interop.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x86\native\libHarfBuzzSharp.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x86\native\libSkiaSharp.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x86\native\sni.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x86\native\SQLite.Interop.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\freebsd\lib\netcoreapp2.0\System.Data.Odbc.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux\lib\netcoreapp2.0\System.Data.Odbc.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux\lib\netcoreapp2.0\System.DirectoryServices.Protocols.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux\lib\netstandard2.0\System.IO.Ports.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx\lib\netcoreapp2.0\System.Data.Odbc.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx\lib\netcoreapp2.0\System.DirectoryServices.Protocols.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx\lib\netstandard2.0\System.IO.Ports.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\unix\lib\netcoreapp2.1\System.Data.SqlClient.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\unix\lib\netcoreapp3.0\System.Drawing.Common.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.Data.Odbc.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.Diagnostics.EventLog.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.Diagnostics.EventLog.Messages.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.Diagnostics.PerformanceCounter.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.DirectoryServices.AccountManagement.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.DirectoryServices.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.DirectoryServices.Protocols.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.0\System.Management.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.1\System.Data.SqlClient.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp2.1\System.Speech.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp3.0\Microsoft.Win32.SystemEvents.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp3.0\System.Drawing.Common.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp3.0\System.Security.Cryptography.Pkcs.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netcoreapp3.0\System.Windows.Extensions.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\Microsoft.Win32.Registry.AccessControl.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\System.Data.OleDb.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\System.IO.Ports.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\System.Runtime.Caching.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\System.Security.Cryptography.ProtectedData.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\System.ServiceProcess.ServiceController.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\netstandard2.0\System.Threading.AccessControl.dll");


            CheckIfFilesAreCompletelyCopied(src, programFiles);
            return programFiles;
        }
    }
}
