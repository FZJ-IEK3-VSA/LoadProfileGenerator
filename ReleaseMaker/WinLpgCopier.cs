using System.Collections.Generic;
using System.IO;

namespace ReleaseMaker {
    public class WinLpgCopier:CopierBase {
        [JetBrains.Annotations.NotNull]
        public static List<string> CopyLpgFiles([JetBrains.Annotations.NotNull] string src, [JetBrains.Annotations.NotNull] string dst)
        {
            List<string> programFiles = [];
            var srcDi = new DirectoryInfo(src);
            var dlls = srcDi.GetFiles("*.dll");
            foreach (var dll in dlls)
            {
                Copy(programFiles, srcDi, src, dst, dll.Name);
            }

            Copy(programFiles, srcDi, src, dst,"LoadProfileGenerator.exe");
            //Copy(programFiles, srcDi, src, dst,"LoadProfileGenerator.exe.config", "4.0.3.0", "4.0.2.0");
            //Copy(programFiles, srcDi, src, dst,"SimulationEngine.exe");
            //Copy(programFiles, srcDi, src, dst,"libHarfBuzzSharp.dylib");
            //Copy(programFiles, srcDi, src, dst,"libSkiaSharp.dylib");

            Copy(programFiles, srcDi, src, dst, @"LoadProfileGenerator.deps.json");
            Copy(programFiles, srcDi, src, dst, @"LoadProfileGenerator.dll.config");
            Copy(programFiles, srcDi, src, dst, @"LoadProfileGenerator.runtimeconfig.json");
            Copy(programFiles, srcDi, src, dst, @"xunit.runner.json");
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

            // added for .net10.0
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx-arm64\native\libSystem.IO.Ports.Native.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-arm\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-arm\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-arm64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-arm64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-x64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-x64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-x86\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\android-x86\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\ios-arm\native\e_sqlite3.a");
            Copy(programFiles, srcDi, src, dst, @"runtimes\ios-arm64\native\e_sqlite3.a");
            Copy(programFiles, srcDi, src, dst, @"runtimes\iossimulator-arm64\native\e_sqlite3.a");
            Copy(programFiles, srcDi, src, dst, @"runtimes\iossimulator-x64\native\e_sqlite3.a");
            Copy(programFiles, srcDi, src, dst, @"runtimes\iossimulator-x86\native\e_sqlite3.a");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-arm\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-arm64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-armel\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-bionic-arm64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-bionic-arm64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-bionic-x64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-bionic-x64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-loongarch64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-mips64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-arm\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-arm\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-arm\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-arm64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-arm64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-arm64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-loongarch64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-riscv64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-riscv64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-s390x\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-x64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-musl-x64\native\libSystem.IO.Ports.Native.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-ppc64le\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-riscv64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-riscv64\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-s390x\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-x64\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-x86\native\libe_sqlite3.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux-x86\native\libSkiaSharp.so");
            Copy(programFiles, srcDi, src, dst, @"runtimes\maccatalyst-arm64\native\libe_sqlite3.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\maccatalyst-arm64\native\libSystem.IO.Ports.Native.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\maccatalyst-x64\native\libe_sqlite3.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\maccatalyst-x64\native\libSystem.IO.Ports.Native.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx-arm64\native\libe_sqlite3.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx-x64\native\libe_sqlite3.dylib");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-arm64\native\e_sqlite3.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-arm64\native\libHarfBuzzSharp.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x64\native\e_sqlite3.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win-x86\native\e_sqlite3.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\browser-wasm\nativeassets\net9.0\e_sqlite3.a");
            Copy(programFiles, srcDi, src, dst, @"runtimes\linux\lib\net10.0\System.DirectoryServices.Protocols.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\osx\lib\net10.0\System.DirectoryServices.Protocols.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\unix\lib\net10.0\System.Data.Odbc.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\unix\lib\net10.0\System.IO.Ports.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\unix\lib\net8.0\System.Data.SqlClient.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.Data.Odbc.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.Data.OleDb.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.DirectoryServices.AccountManagement.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.DirectoryServices.Protocols.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.IO.Ports.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.Management.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.Runtime.Caching.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.ServiceProcess.ServiceController.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net10.0\System.Speech.dll");
            Copy(programFiles, srcDi, src, dst, @"runtimes\win\lib\net8.0\System.Data.SqlClient.dll");

            CheckIfFilesAreCompletelyCopied(src, programFiles);

            return programFiles;
            //Copy(programFiles, srcDi, src, dst,"netstandard.dll");
        }
    }
}