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
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using PowerArgs;

namespace SimEngine2.SettlementCalculation {
    internal class ParallelLauncher {
        [ItemNotNull][JetBrains.Annotations.NotNull] private readonly ConcurrentQueue<string> _calculationsToProcess = new ConcurrentQueue<string>();
        [ItemNotNull][JetBrains.Annotations.NotNull] private readonly ConcurrentQueue<string> _foldersToArchive = new ConcurrentQueue<string>();

       [JetBrains.Annotations.NotNull] private readonly Random _r = new Random();

        private bool _continueArchiving = true;
        private bool _continueProcessing = true;

        private int _totalCalculations;

        private ParallelLauncher()
        {
        }

        public static int FindNumberOfCores()
        {
            var coreCount = Environment.ProcessorCount;
            return coreCount;
        }

        public static void LaunchParallel([JetBrains.Annotations.NotNull] ParallelLauncherOptions options)
        {
            var pl = new ParallelLauncher();
            pl.LaunchParallelInternal(options);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ArchiveEverything([JetBrains.Annotations.NotNull] ParallelLauncherOptions options)
        {
            try {
                if (!string.IsNullOrEmpty(options.ArchiveDirectory)) {
                    if (!Directory.Exists(options.ArchiveDirectory)) {
                        Directory.CreateDirectory(options.ArchiveDirectory);
                        Thread.Sleep(500);
                    }
                    while (_continueArchiving) {
                        Thread.Sleep(1000);
                        if (_foldersToArchive.Count > 0) {
                            var success = _foldersToArchive.TryDequeue(out var archivePath);
                            if (success) {
                                try {
                                    var oldPath = new DirectoryInfo(archivePath);
                                    var files = oldPath.GetFiles("*.*", SearchOption.AllDirectories);
                                    long totalSize = 0;
                                    foreach (var fileInfo in files) {
                                        totalSize += fileInfo.Length;
                                    }
                                    if (options.ArchiveDirectory?.Contains(":") == true) {
                                        var di = new DriveInfo(options.ArchiveDirectory.Substring(0, 1));
                                        while (di.AvailableFreeSpace * 1.1 < totalSize) {
                                            Logger.Info("Free Space on the Archive Drive: " +
                                                        di.AvailableFreeSpace / (1024.0 * 1024 * 1024) + " GB");
                                            Logger.Warning("Needed: " + totalSize * 1.1 / 1024 / 1024 / 1024 + " GB");
                                            Thread.Sleep(5000);
                                        }
                                    }
                                    if (options.ArchiveDirectory != null) {
                                        var newPath =
                                            new DirectoryInfo(Path.Combine(options.ArchiveDirectory, oldPath.Name));
                                        CopyAll(oldPath, newPath);
                                        oldPath.Delete(true);
                                        Logger.Info("finished archiving " + archivePath + ", calcs left:" +
                                                    _calculationsToProcess.Count);
                                    }
                                }
                                catch (Exception ex) {
                                    Logger.Error("Archiving failed: for " + archivePath);
                                    Logger.Exception(ex);
                                }
                            }
                        }
                    }
                }
                Logger.Error("Exiting Archive Thread");
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        private static void CopyAll([JetBrains.Annotations.NotNull] DirectoryInfo source,[JetBrains.Annotations.NotNull] DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles()) {
                Logger.Info(string.Format(CultureInfo.CurrentCulture, @"Copying {0}\{1}", target.FullName, fi.Name));
                /*if (string.Equals(fi.Name, Constants.ResultFileName, StringComparison.OrdinalIgnoreCase)) {
                    if (source.Parent == null) {
                        throw new LPGException("Directoryname was null");
                    }
                    var dirtoreplace = source.Parent.FullName;
                    if (!dirtoreplace.EndsWith("\\", StringComparison.Ordinal)) {
                        dirtoreplace += "\\";
                    }
                    using (var sr = new StreamReader(fi.FullName)) {
                        using (var sw = new StreamWriter(Path.Combine(target.FullName, fi.Name))) {
                            while (!sr.EndOfStream) {
                                var s = sr.ReadLine();
                                if (s == null) {
                                    throw new LPGException("Readline failed");
                                }
                                sw.WriteLine(s.Replace(dirtoreplace, string.Empty));
                            }
                        }
                    }
                }
                else*/ {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories()) {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Executor(int index,[JetBrains.Annotations.NotNull] ParallelLauncherOptions options)
        {
            try {
                Logger.Info("Starting thread " + index);
                var newfilename = "db" + index + ".db3";
                Logger.Info("Copying database to " + newfilename);
                if (File.Exists(newfilename)) {
                    File.Delete(newfilename);
                    Thread.Sleep(500 + _r.Next(1000));
                }
                File.Copy("profilegenerator.db3", newfilename);
                Thread.Sleep(500 + _r.Next(1000));
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
                        }
                    }
                    catch (Exception ex) {
                        Logger.Info(ex.Message);
                        Logger.Exception(ex);
                    }
                    var success = _calculationsToProcess.TryDequeue(out var path);
                    if (success && _continueProcessing) {
                        if (driveInfo != null) {
                            Logger.Info("Thread " + index + ": Free Space on drive " + driveInfo.RootDirectory + ": " +
                                        driveInfo.AvailableFreeSpace / 1024.0 / 1024 / 1024.0 + " GB");
                            var count = 0;
                            while (driveInfo.AvailableFreeSpace / 1024.0 / 1024 / 1024 < 5) {
                                count++;
                                Logger.Warning("Thread " + index + ": Please make at least 5GB space on " +
                                               driveInfo.RootDirectory);
                                Thread.Sleep(5000);
                                if (count > 5 && !Program.CatchErrors) {
                                    throw new LPGException("Not Enough space!");
                                }
                            }
                        }
                        while (_foldersToArchive.Count > 0 && !string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                            Logger.Warning("Thread " + index + ": Waiting for archiving to finish. " +
                                           _foldersToArchive.Count + " left.");
                            Thread.Sleep(5000);
                        }
                        if (!_continueProcessing) {
                            return;
                        }
                        var processesleft = _calculationsToProcess.Count;
                        var processesdone = _totalCalculations - processesleft;
                        var progress = " (" + processesdone + "/" + _totalCalculations + ")";
                        Logger.Info("Thread " + index + ": Processing " + path + progress);
                        var arguments = path.Substring("SimulationEngine.exe".Length);
                        var splitarguments = arguments.Split(' ');
                        if (splitarguments.Length < 2) {
                            throw new LPGException("could not split the arguments!");
                        }
                        string outputfolder = null;
                        for (var i = 0; i < splitarguments.Length; i++) {
                            if (splitarguments[i] == "-OutputDirectory") {
                                outputfolder = splitarguments[i + 1];
                            }
                        }
                        var startinfo = new ProcessStartInfo();
                        using (var process = new Process()) {
                            startinfo.Arguments = arguments + " -Database " + newfilename;
                            startinfo.UseShellExecute = true;
                            startinfo.WindowStyle = ProcessWindowStyle.Normal;
                            startinfo.FileName = "SimulationEngine.exe";
                            process.StartInfo = startinfo;
                            process.Start();
                            SetWindowText(process.MainWindowHandle, arguments);
                            process.WaitForExit();
                        }
                        Logger.Info("Thread " + index + ": Finished " + path);
                        Thread.Sleep(1000);
                        if (outputfolder != null) {
                            var fullOutputFolder = Path.Combine(Directory.GetCurrentDirectory(), outputfolder);
                            string finishedFile = Path.Combine(fullOutputFolder, "finished.flag");
                            string errorpath = Path.Combine(fullOutputFolder, "Logfile.Error.txt");
                            if (File.Exists(finishedFile) && !File.Exists(errorpath)) {
                                _foldersToArchive.Enqueue(fullOutputFolder);
                                Logger.Info("Thread " + index + ": Enqueued for moving: " + fullOutputFolder +
                                            progress);
                            }
                            else {
                                Logger.Info("Thread " + index + ": Not enqueued for moving due to missing finished flag: " + fullOutputFolder +
                                            progress);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Info("Thread " + index + ": Exception:" + Environment.NewLine + ex.Message);
                Logger.Exception(ex);
            }
        }

        private void LaunchParallelInternal([JetBrains.Annotations.NotNull] ParallelLauncherOptions options)
        {
            Logger.LogToFile = true;
            Logger.Info("Reading options");
            var startTime = DateTime.Now;

            if (string.IsNullOrEmpty(options.Batchfile)) {
                Logger.Error("No batch file was set!");
                return;
            }
            if (options.NumberOfCores == 0) {
                Logger.Error("No number of cores to use was set");
                return;
            }
            Logger.Info("Archiving folder is " + options.ArchiveDirectory);
            var outputDirectories = new List<string>();
            var batchFileLines = new List<string>();
            using (var sr = new StreamReader(options.Batchfile)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();

                    if (!string.IsNullOrWhiteSpace(s) ) {
                        batchFileLines.Add(s);
                    }
                }
            }
            if (options.MaximumCount > 0 && options.MaximumCount < batchFileLines.Count) {
                batchFileLines = batchFileLines.Take(options.MaximumCount).ToList();
            }
            for (var calcidx = 0; calcidx < batchFileLines.Count; calcidx++) {
                var addthis = true;
                if (!string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                    // find existing directories
                    var arrs = batchFileLines[calcidx].Split(' ');
                    var dstdirName = string.Empty;
                    for (var i = 0; i < arrs.Length; i++) {
                        if (arrs[i] == "-OutputDirectory") {
                            dstdirName = arrs[i + 1];
                            outputDirectories.Add(dstdirName);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(dstdirName)) {
                        Logger.Error(
                            "Could not identify the output directory parameter in the command line. Can't archive.");
                    }
                    else {
                        var dstFullName = Path.Combine(options.ArchiveDirectory, dstdirName);
                        if (Directory.Exists(dstFullName)) {
                            addthis = false;
                        }
                    }
                }
                if (addthis) {
                    _calculationsToProcess.Enqueue(batchFileLines[calcidx]);
                    _totalCalculations++;
                }
                else {
                    Logger.Warning("Skipping " + batchFileLines[calcidx]);
                }
            }

            if (!string.IsNullOrWhiteSpace(options.ArchiveDirectory) && Directory.Exists(options.ArchiveDirectory)) {
                var archiveDirectoryInfo = new DirectoryInfo(options.ArchiveDirectory);
                var subfos = archiveDirectoryInfo.GetDirectories();
                foreach (var subfo in subfos) {
                    if (!outputDirectories.Contains(subfo.Name)) {
                        if (archiveDirectoryInfo.Parent == null) {
                            throw new LPGException("Directory archive was null");
                        }
                        var oldarchivePath = Path.Combine(archiveDirectoryInfo.Parent.FullName,
                            "Old_" + archiveDirectoryInfo.Name);
                        if (!Directory.Exists(oldarchivePath)) {
                            Directory.CreateDirectory(oldarchivePath);
                            Thread.Sleep(500);
                        }
                        Logger.Info("Moving " + subfo.FullName + " to " + oldarchivePath);
                        var newpath = Path.Combine(oldarchivePath, subfo.Name);
                        subfo.MoveTo(newpath);
                    }
                }
            }

            Logger.Info("Read " + _calculationsToProcess.Count + " entries to calculate.");
            var threads = new List<Thread>();
            var totalCount = _calculationsToProcess.Count;
            int maxThreads = (int) Math.Floor(Environment.ProcessorCount / 2.0 * 0.95);
            if (options.NumberOfCores > 0) {
                maxThreads = options.NumberOfCores;
            }
            for (var i = 0; i < maxThreads && i < totalCount; i++) {
                var i1 = i;
                var t = new Thread(() => Executor(i1, options));
                t.Start();
                threads.Add(t);
                Thread.Sleep(500);
            }
            if (!string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                var archive = new Thread(() => ArchiveEverything(options));
                archive.Start();
            }
            foreach (var thread in threads) {
                thread.Join();
            }
            Logger.Info("All threads finished");
            if (!string.IsNullOrWhiteSpace(options.ArchiveDirectory)) {
                Thread.Sleep(10000);
                while (_foldersToArchive.Count > 0) {
                    Thread.Sleep(1000);
                }
            }
            _continueArchiving = false;
            var duration = DateTime.Now - startTime;
            Logger.Info("Calculation Duration: " + duration + " (" + duration.TotalMinutes + " minutes)");
            Logger.Info("Cleanup finished");
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd,[JetBrains.Annotations.NotNull] string title);

        [UsedImplicitly]
        public class ParallelLauncherOptions {
            [CanBeNull]
            [ArgDescription(
                "It is possible to automatically archive the results to a new directory, if you specify the target directory here")]
            [UsedImplicitly]
            [ArgShortcut(null)]
            public string ArchiveDirectory { get; set; }

            [CanBeNull]
            [ArgDescription("Sets the Batchfile to be used.")]
            [ArgShortcut(null)]
            [UsedImplicitly]
            public string Batchfile { get; set; }

            [ArgDescription("Limit to only the top x lines in the batch file (mostly for testing).")]
            [ArgShortcut(null)]
            [UsedImplicitly]
            [ArgDefaultValue(-1)]
            public int MaximumCount { get; set; } = -1;

            [ArgDescription("Sets the number of parallel processes to be launched.")]
            [ArgShortcut(null)]
            [UsedImplicitly]
            [ArgDefaultValue(-1)]
            public int NumberOfCores { get; set; } = Environment.ProcessorCount / 2;
        }
    }
}