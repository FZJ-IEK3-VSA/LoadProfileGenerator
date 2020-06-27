using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace Common {
    public class WorkingDir: IDisposable {
        private int _filecount;
        [NotNull]
        private readonly string _lastDirectory;

        [NotNull]
        public string PreviousCurrentDir { get; }

        // ReSharper disable once NotNullMemberIsNotInitialized
        public WorkingDir([NotNull] string testname, bool useRamdisk = true)
        {
            PreviousCurrentDir = Environment.CurrentDirectory;
            _lastDirectory= InitializeWorkingDirectory(testname, useRamdisk);
            Logger.Get().ClearOldErrors();
            Logger.SetLogFilePath( Path.Combine(_lastDirectory, "Log.Unittest.txt"));
            Logger.Threshold = Severity.Debug;
            InitializeDataLogging();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void InitializeDataLogging()
        {
            SqlResultLoggingService  = new SqlResultLoggingService(_lastDirectory);
            List<IDataSaverBase> loggers = new List<IDataSaverBase>
            {
                new CalcAutoDevDtoLogger(SqlResultLoggingService),
                new CalcDeviceDtoLogger(SqlResultLoggingService)
            };
            InputDataLogger = new InputDataLogger(loggers.ToArray());
        }

        [NotNull]
        public InputDataLogger InputDataLogger { get; private set; }
        [NotNull]
        public SqlResultLoggingService SqlResultLoggingService { get; private  set; }

        [NotNull]
        public string WorkingDirectory => _lastDirectory;

        public bool SkipCleaning {
            get => _skipCleaning;
            set {
                if (value == true && Environment.MachineName.ToLower() != "i5home")
                {
                    throw new LPGException("trying to skip cleaning on a non-dev-pc");
                }
                _skipCleaning = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ClearDirectory()
        {
            var di = new DirectoryInfo(_lastDirectory);
            foreach (DirectoryInfo subdir in di.GetDirectories()) {
                subdir.Delete(true);
            }

            foreach (FileInfo file in di.GetFiles()) {
                file.Delete();
            }
        }

        private bool _isClean;
        private bool _skipCleaning;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void CleanUp(int numberOfFilesToTolerate = 0, bool throwAllErrors = true) {
            if (_isClean) {
                return;
            }

            if (SkipCleaning) {
                return;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var di = new DirectoryInfo(_lastDirectory);
            var fis = di.GetFiles("Log.Unittest.Error.txt", SearchOption.AllDirectories);
            if (fis.Length > 0) {
                string s;
                using (var sr = new StreamReader(fis[0].FullName)) {
                    s = sr.ReadToEnd();
                }
                throw new LPGException(s);
            }
            if (Logger.Get().Errors.Count > 0 && throwAllErrors) {
                Logger.Get().ThrowAllErrors();
            }
            var tryCount = 0;
            Exception lastException = null;
            _isClean = true;
            while (tryCount < 300) {
                try {
                    if (tryCount > 0) {
                        _filecount = 0;
                        RecursiveDelete(new DirectoryInfo(_lastDirectory));
                        Logger.ImportantInfo("Remaining file count:" + _filecount);
                        if (_filecount < numberOfFilesToTolerate) {
                            return;
                        }
                    }
                    if (_lastDirectory.Length != 0) {
                        if (Directory.Exists(_lastDirectory)) {
                            Directory.Delete(_lastDirectory, true);
                            Thread.Sleep(1000);
                        }
                    }
                    return;
                }
                catch (Exception e) {
                    lastException = e;
                    tryCount++;
                    bool logtoFile = Logger.LogToFile;
                    Logger.LogToFile = false;
                    Logger.Error("File blocked for " + tryCount + "s... waiting 1s and trying again. File" + e.Message);
                    Logger.LogToFile = logtoFile;
                    Thread.Sleep(1000);
                }
            }
            if (lastException != null) {
                Logger.Exception(lastException);
                throw lastException;
            }

        }

        [NotNull]
        public string Combine([NotNull] string name) => Path.Combine(_lastDirectory, name);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [NotNull]
        private static string InitializeWorkingDirectory([NotNull] string testname, bool useRamdisk) {
            var baseWorkingDir = DetermineBaseWorkingDir(useRamdisk);
            var resultdir = Path.Combine(baseWorkingDir, testname);
            try {
                if (Directory.Exists(resultdir)) {
                    Directory.Delete(resultdir, true);
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e) {
                Logger.Error("Error cleaning/creating:" + resultdir + Environment.NewLine + e.Message);
                Logger.Exception(e);
            }
            try
            {
                Directory.CreateDirectory(resultdir);
                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Logger.Error("Error cleaning/creating:" + resultdir + Environment.NewLine + e.Message);
                Logger.Exception(e);
            }
            Logger.Info("using:" + resultdir);
            return resultdir;
        }

        [NotNull]
        public static string DetermineBaseWorkingDir(bool useRamdisk)
        {
            var myDrives = DriveInfo.GetDrives();
            var path = Directory.GetCurrentDirectory();
#pragma warning disable S1075 // URIs should not be hardcoded
            if (Directory.Exists("c:\\work"))
#pragma warning restore S1075 // URIs should not be hardcoded
            {
                path = @"c:\work\";
            }

            if (useRamdisk) {
                foreach (var drive in myDrives) {
                    try {
                        if (drive.DriveType != DriveType.Network && drive.IsReady && drive.VolumeLabel == "RamDisk") {
                            path = drive.RootDirectory.FullName;
                        }
                    }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                    catch (Exception) {
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                        // ignored because isready might throw
                    }
                }
            }
            string resultdir = Path.Combine(path, "LPGUnitTest");
            if (!Directory.Exists(resultdir)) {
                try {
                    Directory.CreateDirectory(resultdir);
                    Thread.Sleep(500);
                }
                catch (Exception e) {
                    Logger.Error("Error cleaning/creating:" + resultdir + Environment.NewLine + e.Message);
                    Logger.Exception(e);
                }
            }

            return resultdir;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void RecursiveDelete([NotNull] DirectoryInfo di) {
            var subdirs = di.GetDirectories();
            foreach (var info in subdirs) {
                RecursiveDelete(info);
            }

            var fis = di.GetFiles();
            foreach (var fi in fis) {
                try {
                    fi.Delete();
                }
                catch (Exception e) {
                    Logger.Exception(e);
                    _filecount++;
                }
            }
        }

        public void Dispose()
        {
            CleanUp(1);
        }
    }
}