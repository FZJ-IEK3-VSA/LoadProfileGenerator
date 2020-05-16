using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PowerArgs;

namespace SimulationEngineLib.SimZukunftProcessor {
    public class ParallelJsonLauncher {
        private class CompletedCalc {
            public string FileName { get; set; }
            public TimeSpan Duration { get; set; }
            public bool IsSuccess { get; set; }
            public string  Status { get; set; }
        }

        //[NotNull] private readonly CalculationProfiler _calculationProfiler;
        [ItemNotNull] [NotNull] private readonly ConcurrentQueue<CalcJobQueueEntry> _calculationsToProcess = new ConcurrentQueue<CalcJobQueueEntry>();
        [ItemNotNull] [NotNull] private readonly ConcurrentQueue<string> _foldersToArchive = new ConcurrentQueue<string>();
        [CanBeNull] public DirectoryInfo BaseDirectoryInfo { get; set; }
        private bool _continueArchiving = true;
        private bool _continueProcessing = true;

        private int _totalCalculations;

        private readonly List<CompletedCalc> _completedCalcs  = new List<CompletedCalc>();

        public static bool ThrowOnInvalidFile { get; set; } = false;
        public static int FindNumberOfCores()
        {
            var coreCount = Environment.ProcessorCount;
            return coreCount;
        }

        public static void LaunchParallel([NotNull] ParallelJsonLauncherOptions options)
        {
            Logger.LogFileIndex = "JsonParallelCalculationStarter";
            Logger.LogToFile = true;
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var pl = new ParallelJsonLauncher();
            pl.LaunchParallelInternal(options);
            calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ArchiveEverythingThread([NotNull] ParallelJsonLauncherOptions options)
        {
            if (options.ArchiveDirectory == null || string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                Logger.Info("archive directory was not set, quitting archiving thread");
                return;
            }

            Logger.Info("Archive directory is " + options.ArchiveDirectory);


            try {
                if (!Directory.Exists(options.ArchiveDirectory)) {
                    Directory.CreateDirectory(options.ArchiveDirectory);
                    Thread.Sleep(500);
                }

                while (_continueArchiving) {
                    if (_foldersToArchive.Count > 0) {
                        var success = _foldersToArchive.TryDequeue(out var calculationDirectory);
                        if (success) {
                            try {
                                var oldPath = new DirectoryInfo(calculationDirectory);
                                var files = oldPath.GetFiles("*.*", SearchOption.AllDirectories);
                                long totalSize = 0;
                                foreach (var fileInfo in files) {
                                    totalSize += fileInfo.Length;
                                }

                                if (options.ArchiveDirectory?.Contains(":") == true) {
                                    var di = new DriveInfo(options.ArchiveDirectory.Substring(0, 1));
                                    while (di.AvailableFreeSpace * 1.1 < totalSize) {
                                        Logger.Info("Free Space on the Archive Drive: " + di.AvailableFreeSpace / (1024.0 * 1024 * 1024) + " GB");
                                        Logger.Warning("Needed: " + totalSize * 1.1 / 1024 / 1024 / 1024 + " GB");
                                        Thread.Sleep(5000);
                                    }
                                }

                                var newPath = GetArchiveDirectory(oldPath, options);
                                if (!options.TestArchiving) {
                                    CopyAll(oldPath, new DirectoryInfo(newPath));
                                    oldPath.Delete(true);
                                }

                                Logger.Info("finished archiving " + calculationDirectory + " to " + newPath + ", calcs left:" + _calculationsToProcess.Count + ", directories to archive left right now: " + _foldersToArchive.Count );
                            }
                            catch (Exception ex) {
                                Logger.Error("Archiving failed: for " + calculationDirectory);
                                Logger.Exception(ex);
                            }
                        }
                        else {
                            Thread.Sleep(1000);
                        }
                    }
                }

                Logger.Error("Exiting Archive Thread");
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        private static void CheckForSufficentFreeSpace(int index, [CanBeNull] DriveInfo driveInfo)
        {
            if (Config.SkipFreeSpaceCheckForCalculation) {
                return;
            }

            if (driveInfo != null) {
                Logger.Info("Thread " + index + ": Free Space on drive " + driveInfo.RootDirectory + ": " + driveInfo.AvailableFreeSpace / 1024.0 / 1024 / 1024.0 + " GB");
                var count = 0;
                while (driveInfo.AvailableFreeSpace / 1024.0 / 1024 / 1024 < 5) {
                    count++;
                    Logger.Warning("Thread " + index + ": Please make at least 5GB space on " + driveInfo.RootDirectory);
                    Thread.Sleep(5000);
                    if (count > 5 && !Config.CatchErrors) {
                        throw new LPGException("Not Enough space!");
                    }
                }
            }
        }

        public static void CopyAll([NotNull] string source, [NotNull] string target)
        {
            CopyAll(new DirectoryInfo(source), new DirectoryInfo(target));
        }


        public static void CopyAll([NotNull] DirectoryInfo source, [NotNull] DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles()) {
                Logger.Info(string.Format(CultureInfo.CurrentCulture, @"Copying {0}\{1}", target.FullName, fi.Name));
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories()) {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        [CanBeNull]
        public static HouseCreationAndCalculationJob LoadFromFile([NotNull] string inputFile)
        {
            string s = File.ReadAllText(inputFile);
            Logger.Info("Reading " + inputFile);
            try {
                var jcs = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(s);
                return jcs;
            }
            catch (Exception) {
                Logger.Error("Could not read file "+ inputFile + ". Skipping this file.");
                return null;
            }

        }
        private void FillCalculationQueue([NotNull] ParallelJsonLauncherOptions options)
        {Logger.Info("Starting to look for all the json files to calculate...");
            if (options.JsonDirectory == null) {
                throw new LPGCommandlineException("Json directory was not set.");
            }

            DirectoryInfo di = new DirectoryInfo(options.JsonDirectory);
            if (!di.Exists) {
                throw new LPGCommandlineException("Directory " + di.FullName + " was not found.");
            }

            var inputFiles = di.GetFiles("*.json").ToList();
            if (options.SearchSecondLevel) {
                foreach (DirectoryInfo subdir in di.GetDirectories()) {
                    inputFiles.AddRange(subdir.GetFiles("*.json"));
                }
            }

            if (inputFiles.Count == 0) {
                throw new LPGCommandlineException("No input files were found.");
            }

            if (options.MaximumNumberOfCalculations > 0 && options.MaximumNumberOfCalculations < inputFiles.Count) {
                inputFiles = inputFiles.Take(options.MaximumNumberOfCalculations).ToList();
            }

            for (var calcidx = 0; calcidx < inputFiles.Count; calcidx++) {
                HouseCreationAndCalculationJob jcs = LoadFromFile(inputFiles[calcidx].FullName);
                if(jcs == null) {
                    continue;
                }

                //check if there even is an output directory
                if (string.IsNullOrWhiteSpace(jcs.CalcSpec?.OutputDirectory)) {
                    Logger.Error("Skipping file: No output directory set in the file " + inputFiles[calcidx].Name);
                    if (ThrowOnInvalidFile) {
                        throw new LPGException("No output file was set: " + jcs.CalcSpec?.OutputDirectory);
                    }
                    continue;
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                DirectoryInfo outputDir = new DirectoryInfo(jcs.CalcSpec.OutputDirectory);
                //check the archive directory if the results already exist
                if (!string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                    //make archive path
                    string archivePath = GetArchiveDirectory(outputDir, options);
                    string finishedFlagPath = Path.Combine(archivePath, "finished.flag");
                    if (File.Exists(finishedFlagPath)) {
                        Logger.Info("Found finished folder in archive: " + archivePath);
                        continue;
                    }
                }

                //check current directory
                    if (outputDir.Exists) {
                        var calculatedFiles = outputDir.GetFiles("finished.flag");
                        if (calculatedFiles.Length == 1) {
                            Logger.Warning(outputDir.Name + " seems finished. Enqueueing for archiving");
                            _foldersToArchive.Enqueue(outputDir.FullName);
                        }
                        else {
                            Logger.Warning(outputDir.Name + " had files left over. Deleting and trying again.");
                            outputDir.Delete(true);
                        }
                    }

                    Logger.Info("Enqueuing file " + inputFiles[calcidx].Name);
                    _calculationsToProcess.Enqueue(new CalcJobQueueEntry(inputFiles[calcidx], calcidx));
                    _totalCalculations++;
            }
        }

        [NotNull]
        private string GetArchiveDirectory([NotNull] DirectoryInfo outputDirectory, [NotNull] ParallelJsonLauncherOptions pjl)
        {
            if (BaseDirectoryInfo == null) {
                throw new LPGException("BaseDirectory was not set.");
            }
            var relativePath = outputDirectory.FullName.Replace(BaseDirectoryInfo.FullName, "");
            if (pjl.ArchiveDirectory == null || string.IsNullOrWhiteSpace(pjl.ArchiveDirectory)) {
                throw new Exception("Output directory was null");
            }

            if (relativePath.StartsWith("\\", StringComparison.OrdinalIgnoreCase)) {
                relativePath = relativePath.Substring(1);
            }

            var newPath = Path.Combine(pjl.ArchiveDirectory, relativePath);
            Logger.Info("Archiving new path is " + newPath + ", archive base: " + pjl.ArchiveDirectory);
            return newPath;
        }

        private void LaunchParallelInternal([NotNull] ParallelJsonLauncherOptions options)
        {
            Logger.LogToFile = true;
            Logger.Info("Reading options");
            Logger.Info("Json Directory: " + options.JsonDirectory);
            Logger.Info("Maximum number of calculations to perform before quitting: " + options.MaximumNumberOfCalculations);
            Logger.Info("Number of cores to use: " + options.NumberOfCores);
            Logger.Info("Search in the subdirectories for files: " + options.SearchSecondLevel);
            Logger.Info("Archiving folder is " + options.ArchiveDirectory);
            BaseDirectoryInfo = new DirectoryInfo(Environment.CurrentDirectory);
            var startTime = DateTime.Now;

            if (string.IsNullOrEmpty(options.JsonDirectory)) {
                Logger.Error("No JSON directory file was set!");
                return;
            }

            if (options.NumberOfCores == 0) {
                Logger.Error("Number of cores to use was set to 0. Not very useful.");
                return;
            }

            if (!File.Exists("simulationengine.exe")) {
                throw new LPGException("simulationengine.exe was not found in the directory " + Environment.CurrentDirectory);
            }

            FillCalculationQueue(options);
            Logger.Info("Read " + _calculationsToProcess.Count + " entries to calculate.");
            LaunchWorkerThreads(options, startTime);
            Logger.Info("Cleanup finished");
            foreach (var calc in _completedCalcs) {
                Logger.Info("\""+ calc.FileName + "\": " + calc.Duration.ToString() + " Status: "   + calc.Status);
            }
            int successCount = _completedCalcs.Count(x => x.IsSuccess);
            Logger.Info("Finished " + _completedCalcs.Count + " calculations, " + successCount+  " successfully");
        }

        private readonly List<Thread> _threads = new List<Thread>();

        private void LaunchWorkerThreads([NotNull] ParallelJsonLauncherOptions options, DateTime startTime)
        {
            _threads.Clear();
            Thread archiveThread = null;
            //archiving Thread
            if (!string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                archiveThread = new Thread(() => ArchiveEverythingThread(options));
                archiveThread.Start();
            }

            if (!options.ArchiveOnly) {
                var totalCount = _calculationsToProcess.Count;
                int maxThreads = (int)Math.Floor(Environment.ProcessorCount / 2.0 * 0.95);
                if (options.NumberOfCores > 0) {
                    maxThreads = options.NumberOfCores;
                }

                for (var i = 0; i < maxThreads && i < totalCount; i++) {
                    var i1 = i;
                    var t = new Thread(() => SingleThreadExecutor(i1));
                    t.Start();
                    t.Name = "Executor " + i;
                    _threads.Add(t);
                    Thread.Sleep(500);
                }
            }

            foreach (var thread in _threads) {
                thread.Join();
            }

            if (archiveThread != null) {
                Logger.Info("Waiting for archiving to finish, " + _foldersToArchive.Count + " folders left");
                while (_foldersToArchive.Count > 0 && archiveThread.IsAlive) {
                    Logger.Info("Waiting for archiving to finish, " + _foldersToArchive.Count + " folders left");
                    Thread.Sleep(1000);
                }

                _continueArchiving = false;
                archiveThread.Join();
            }

            Logger.Info("All threads finished");
            var duration = DateTime.Now - startTime;
            Logger.Info("Calculation Duration: " + duration + " (" + duration.TotalMinutes.ToString("F1", CultureInfo.InvariantCulture) + " minutes)");
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd, [NotNull] string title);

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void SingleThreadExecutor(int index)
        {
            try {
                Logger.Info("Starting thread " + index);
                var di = new DirectoryInfo(Directory.GetCurrentDirectory());
                DriveInfo driveInfo = null;
                if (di.FullName.Contains(":")) {
                    var drive = di.FullName.Substring(0, 1);
                    driveInfo = new DriveInfo(drive);
                }

                while (!_calculationsToProcess.IsEmpty && _continueProcessing) {
                    try {
                        if (Console.KeyAvailable) {
                            if (Console.ReadKey(true).Key == ConsoleKey.Q) {
                                _continueProcessing = false;
                                while (!_calculationsToProcess.IsEmpty) {
                                    _calculationsToProcess.TryDequeue(out var _);
                                }

                                Logger.Info("Thread: " + index + "Q was pressed. Quitting soon.");
                            }
                            if (Console.ReadKey(true).Key == ConsoleKey.T)
                            {
                                foreach (Thread thread in _threads) {
                                    Logger.Info(thread.Name +  ": " + thread.IsAlive);
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        Logger.Info(ex.Message);
                        //Logger.Exception(ex);
                    }

                    try {
                        var success = _calculationsToProcess.TryDequeue(out var path);
                        if (success && _continueProcessing) {
                            CheckForSufficentFreeSpace(index, driveInfo);
                            if (!_continueProcessing) {
                                return;
                            }
                            DateTime startTime = DateTime.Now;
                            var processesleft = _calculationsToProcess.Count;
                            var processesdone = _totalCalculations - processesleft;
                            var progress = " (" + processesdone + "/" + _totalCalculations + ")";
                            Logger.Info("Thread " + index + ": Processing " + path.JsonFile.Name + progress);
                            var startinfo = new ProcessStartInfo();
                            string arguments = "processhousejob -j \"" + path.JsonFile.FullName + "\"";

                            Logger.Info("Launching simulationengine.exe " + arguments);
                            using (var process = new Process()) {
                                startinfo.Arguments = arguments;
                                startinfo.UseShellExecute = true;
                                startinfo.WindowStyle = ProcessWindowStyle.Normal;
                                startinfo.FileName = "SimulationEngine.exe";
                                process.StartInfo = startinfo;
                                process.Start();
                                SetWindowText(process.MainWindowHandle, path.JsonFile.FullName);
                                process.WaitForExit();
                            }

                            Logger.Info("Thread " + index + ": Finished \"" + path.JsonFile.Name + "\"");
                            CompletedCalc completedCalc = new CompletedCalc();
                            _completedCalcs.Add(completedCalc);
                            completedCalc.FileName = path.JsonFile.Name;
                            Thread.Sleep(1000);
                            string jsonStr = File.ReadAllText(path.JsonFile.FullName);
                            HouseCreationAndCalculationJob jcs = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(jsonStr);
                            completedCalc.Status = "No finished.flag found";
                            if (jcs.CalcSpec.OutputDirectory != null) {
                                DirectoryInfo outputFolder = new DirectoryInfo(jcs.CalcSpec.OutputDirectory);
                                Logger.Info("Checking output folder " + outputFolder.FullName);
                                if (outputFolder.Exists) {
                                    string finishedFile = Path.Combine(outputFolder.FullName, "finished.flag");

                                    if (File.Exists(finishedFile)) {
                                        completedCalc.Status = "finished.flag found";
                                        completedCalc.IsSuccess = true;
                                        _foldersToArchive.Enqueue(outputFolder.FullName);
                                        Logger.Info("Thread " + index + ": Enqueued for moving: " + outputFolder.FullName + progress);
                                    }
                                    else {
                                        completedCalc.Status = "No folder and no finished.flag found";
                                        Logger.Info("Thread " + index + ": Not enqueued for moving due to missing finished flag: " + outputFolder.FullName + progress);
                                    }

                                    if (jcs.CalcSpec.DeleteDAT) {
                                        var dats = outputFolder.GetFiles("*.dat", SearchOption.AllDirectories);
                                        foreach (var info in dats) {
                                            info.Delete();
                                        }
                                    }
                                }
                                else {
                                    completedCalc.Status = "No folder found";
                                }
                            }
                            else {
                                Logger.Info("No output folder found for " + path.JsonFile.Name);
                            }

                            completedCalc.Duration = (DateTime.Now - startTime);
                        }

                    }
                    catch (Exception ex) {
                        Logger.Exception(ex);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Info("Thread " + index + ": Exception:" + Environment.NewLine + ex.Message);
                Logger.Exception(ex);
            }
        }

        [UsedImplicitly]
        public class ParallelJsonLauncherOptions {
            [CanBeNull]
            [ArgDescription("ArchiveDirectory. All successfull calculations will be moved to this directory. " +
                            "This is intended to make it possible to for example run the calculations on a fast local drive but to move the results " +
                            "to a big network drive, if you need to run thousands of calculations.")]
            [ArgShortcut("ar")]
            [UsedImplicitly]
            public string ArchiveDirectory { get; set; }

            [ArgDescription("Archive only. This will only try to archive existing files without starting any new calculations")]
            [UsedImplicitly]
            public bool ArchiveOnly { get; set; }

            [CanBeNull]
            [ArgDescription("Sets the directory where all the JSON Calculation definition files are stored.")]
            [ArgShortcut("d")]
            [UsedImplicitly]
            public string JsonDirectory { get; set; }

            [ArgDescription("Limit to only the first x files (mostly for testing).")]
            [ArgShortcut("mc")]
            [UsedImplicitly]
            [ArgDefaultValue(-1)]
            public int MaximumNumberOfCalculations { get; set; } = -1;

            [ArgDescription("Sets the number of parallel processes to be launched. Defaults to half of the reported cores (including hyperthreading).")]
            [ArgShortcut("cores")]
            [UsedImplicitly]
            [ArgDefaultValue(-1)]
            public int NumberOfCores { get; set; } = Environment.ProcessorCount / 2;

            [ArgDescription("If you enable this, then the direct subdirectories of the target directory will also be searched " +
                            "for json files.")]
            [ArgShortcut("2nd")]
            [UsedImplicitly]
            [ArgDefaultValue(false)]
            public bool SearchSecondLevel { get; set; }

            [ArgDescription("Test Archiving. This will display what the archiver would do, without actually doing anything.")]
            [UsedImplicitly]
            public bool TestArchiving { get; set; }
        }
    }
}