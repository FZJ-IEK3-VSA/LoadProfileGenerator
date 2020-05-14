using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CommonDataWPF.Tests;
using NUnit.Framework;
using Xunit;

namespace Common.Tests {
    [TestFixture]
    public static class CleanTestBase {
        [JetBrains.Annotations.NotNull]
        private static string GetPrettySize(long size) {
            if (size > 1024 * 1024 * 1024) {
                var gb = size / (1024.0 * 1024 * 1024);
                return gb.ToString("N2", CultureInfo.CurrentCulture) + " GB";
            }
            if (size > 1024 * 1024) {
                var gb = size / (1024.0 * 1024);
                return gb.ToString("N2", CultureInfo.CurrentCulture) + " MB";
            }
            if (size > 1024) {
                var gb = size / 1024.0;
                return gb.ToString("N2", CultureInfo.CurrentCulture) + " kB";
            }
            return size + " b";
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [TestCase(true)]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public static void RunAutomatically(bool throwExceptionOnLeftover) {
            Logger.Info("Starting to clean...");
            var myDrives = DriveInfo.GetDrives();
            string? path = null;
            foreach (var drive in myDrives) {
                if (drive.IsReady && drive.VolumeLabel == "RamDisk") {
                    path = drive.RootDirectory.FullName;
                }
            }
            if (path == null) {
                Logger.Info("No ramdrive found");
                return;
            }
            CleanFolder(throwExceptionOnLeftover,  path,false);
        }

        public static void CleanFolder(bool throwExceptionOnLeftover, [JetBrains.Annotations.NotNull] string path, bool throwExceptionOnPreviousLeftovers)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            DirectoryInfo di = new DirectoryInfo(path);
            DriveInfo drive = new DriveInfo(di.Root.FullName);
            Logger.Info("Free space at start of cleaning: " + GetPrettySize(drive.AvailableFreeSpace));

            var fis = di.GetFiles("*.*", SearchOption.AllDirectories);
            var size = fis.Select(x => x.Length).Sum();
            var s = string.Empty;
            foreach (var fi in fis) {
                s += fi.FullName + " (" + GetPrettySize(fi.Length) + ")" + Environment.NewLine;
            }

            if (throwExceptionOnPreviousLeftovers && size > 10000) {
                DirectoryInfo[] dirs = di.GetDirectories();
                string leftOvers = "";
                foreach (var mydir in dirs) {
                    leftOvers += mydir.FullName +"\\" + Environment.NewLine;
                }
                FileInfo[] fis1 = di.GetFiles("*.*",SearchOption.AllDirectories);
                long totalSize = 0;
                foreach (var fi in fis1)
                {
                    leftOvers += fi.FullName + "\\" + Environment.NewLine;
                    totalSize += fi.Length;
                }

                try {
                    di.Delete(true);
                }
                catch (Exception ex) {
                    Logger.Info(ex.Message);
                }
                throw new LPGException("Leftover from previous runs found:" + leftOvers + "\nTotalSize: " + GetPrettySize(totalSize));
            }
            if (size > 1000) {
                Logger.Info("found " + fis.Length + " files with " + GetPrettySize(size) + ": " + Environment.NewLine + s);
                try {
                    var sufos = di.GetDirectories();
                    foreach (var sufo in sufos) {
                        Logger.Info("Deleting " + sufo.FullName);
                        sufo.Delete(true);
                    }
                }
                catch (Exception ex) {
                    Logger.Info("While cleaning:" + ex.Message);
                    Logger.Exception(ex);
                }

                if (size > 100000) {
                    Thread.Sleep(2000);
                    fis = di.GetFiles("*.*", SearchOption.AllDirectories);
                    //try hard core delete to find the culprit
                    foreach (var fi in fis) {
                        try {
                            fi.Delete();
                        }
                        catch (Exception ex) {
                            Logger.Exception(ex);
                            var list = FileUtil.WhoIsLocking(fi.FullName);
                            foreach (var process in list) {
                                Logger.Error("Locked by " + process.ProcessName + " / " + process.MainWindowTitle);
                            }
                        }
                    }

                    fis = di.GetFiles("*.*", SearchOption.AllDirectories);
                    if (fis.Length > 0 && throwExceptionOnLeftover) {
                        throw new LPGException("files were leftover!" + Environment.NewLine + s);
                    }
                }
            }
            else {
                Logger.Info("Found nothing");
            }
        }
    }
}