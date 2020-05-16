using System;
using System.IO;
using System.Threading;
using Common;
using Database.Tests;


namespace SimulationEngine.Tests {
    internal class SimulationEngineTestPreparer :  IDisposable {
        [JetBrains.Annotations.NotNull] private static string _lastDirectory = "c:\\";

        [JetBrains.Annotations.NotNull] private readonly WorkingDir _wd;

        public SimulationEngineTestPreparer([JetBrains.Annotations.NotNull] string name) {
            Logger.Info(Environment.CurrentDirectory);
            Config.CatchErrors = false;
            var di = new DirectoryInfo(Environment.CurrentDirectory);
            var fis = di.GetFiles("*.*");
            var db3Path = DatabaseSetup.GetSourcepath(null);
            _wd = new WorkingDir(name);
            Thread.Sleep(1000);
            _lastDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_wd.WorkingDirectory);
            foreach (var fi in fis) {
                switch (fi.Extension.ToUpperInvariant()) {
                    case "CMD": continue;
                    case "LOG": continue;
                    case "TXT": continue;
                    case "PDF": continue;
                    default:
                        var dstpath = Path.Combine(_wd.WorkingDirectory, fi.Name);
                        fi.CopyTo(dstpath);
                        break;
                }
            }
            if (File.Exists("profilegenerator.db3")) {
                File.Delete("profilegenerator.db3");
            }
            File.Copy(db3Path, "profilegenerator.db3");
        }

        [JetBrains.Annotations.NotNull]
        public string WorkingDirectory => _wd.WorkingDirectory;

        public void Clean() {
            Directory.SetCurrentDirectory(_lastDirectory);
            _wd.CleanUp();
        }

        public void Dispose()
        {
            _wd.Dispose();
        }
    }
}