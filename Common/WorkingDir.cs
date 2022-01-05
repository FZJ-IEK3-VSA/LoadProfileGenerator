using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;

namespace Common {
    public class WorkingDir: IDisposable {
        private int _filecount;
        [JetBrains.Annotations.NotNull]
        private readonly string _lastDirectory;

        [JetBrains.Annotations.NotNull]
        public string PreviousCurrentDir { get; }

        // ReSharper disable once NotNullMemberIsNotInitialized
        public WorkingDir([JetBrains.Annotations.NotNull] string testname, bool useRamdisk = true)
        {
            PreviousCurrentDir = Environment.CurrentDirectory;
            _lastDirectory= InitializeWorkingDirectory(testname, useRamdisk);
            Logger.Get().ClearOldErrors();
            Logger.SetLogFilePath( Path.Combine(_lastDirectory, "Log.Unittest.txt"));
            //Logger.Threshold = Severity.Debug;
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

        [JetBrains.Annotations.NotNull]
        public InputDataLogger InputDataLogger { get; private set; }
        [JetBrains.Annotations.NotNull]
        public SqlResultLoggingService SqlResultLoggingService { get; private  set; }

        [JetBrains.Annotations.NotNull]
        public string WorkingDirectory => _lastDirectory;

        public bool SkipCleaning {
            get => _skipCleaning;
            set {
                if (value && Environment.MachineName.ToLower() != "i5home")
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
            long peakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64;
            Logger.Info("Peak workingset was : " + peakWorkingSet / 1024/1024 + " mb");
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

        [JetBrains.Annotations.NotNull]
        public string Combine([JetBrains.Annotations.NotNull] string name) => Path.Combine(_lastDirectory, name);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [JetBrains.Annotations.NotNull]
        private static string InitializeWorkingDirectory([JetBrains.Annotations.NotNull] string testname, bool useRamdisk) {
            var baseWorkingDir = DetermineBaseWorkingDir(useRamdisk);
            testname = ReplaceInvalidChars(testname);
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

        /// <summary>
        /// Replaces all characters that are not allowed in file names with a fixed replacement character.
        /// </summary>
        /// <param name="filename">The file name to adjust</param>
        /// <param name="replacementChar">The character to use instead of illegal characters</param>
        /// <returns>The adjusted file name without any illegal characters</returns>
        public static string ReplaceInvalidChars(string filename, char replacementChar = '_')
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (c == replacementChar)
                {
                    throw new LPGException("Used an illegal character as replacementChar: '" + replacementChar + "'");
                }
                filename = filename.Replace(c, replacementChar);
            }
            return filename;
        }

        /// <summary>
        /// Determines the base directory for any working directories used for tests.
        /// </summary>
        /// <param name="useRamdisk">Whether to choose a directory on a ramdisk, if available</param>
        /// <param name="createDefaultBaseDir">Whether the default base directory C:\Work\ should be created if it does not exist</param>
        /// <returns>The base path for working directories</returns>
        [JetBrains.Annotations.NotNull]
        public static string DetermineBaseWorkingDir(bool useRamdisk, bool createDefaultBaseDir = true)
        {
            var myDrives = DriveInfo.GetDrives();
            var path = Directory.GetCurrentDirectory();
            var defaultPath = @"C:\work\";
            if (Directory.Exists(defaultPath))
            {
                // use the default base directory if it already exists
                path = defaultPath;
            }
            else if (createDefaultBaseDir)
            {
                try
                {
                    // try to create the default base directory and use it
                    Directory.CreateDirectory(defaultPath);
                    path = defaultPath;
                }
                catch (Exception)
                {
                    Logger.Warning("Could not create the default base working directory: " + defaultPath);
                }
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
        private void RecursiveDelete([JetBrains.Annotations.NotNull] DirectoryInfo di) {
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