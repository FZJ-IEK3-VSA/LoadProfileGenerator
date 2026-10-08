using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tables.Validation;
using Database.Tests;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;

namespace ReleaseMaker
{
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public class ReleaseBuilderTests
    {
        private const bool ThrowOnMissingOutcomes = false;
        private const bool ThrowOnUnusedDesires = true;

        private static void CheckForDevicesWithoutCategory([JetBrains.Annotations.NotNull] Simulator sim)
        {
            sim.DeviceCategories.DeviceCategoryNone.RefreshSubDevices();
            foreach (var device in sim.DeviceCategories.DeviceCategoryNone.SubDevicesWithoutRefresh)
            {
                Logger.Info(device.Name);
            }

            if (sim.DeviceCategories.DeviceCategoryNone.SubDevices.Count > 0)
            {
                throw new LPGException("There are devices in the none-category!");
            }
        }

        [SuppressMessage("ReSharper", "RedundantLogicalConditionalExpressionOperand")]
        private static void CheckForNewItems([JetBrains.Annotations.NotNull] Simulator sim)
        {
            var unusedDesires = FindUnusedDesires(sim);
            foreach (var unusedDesire in unusedDesires)
            {
                Logger.Error("unused desire:" + unusedDesire);
            }

            if (ThrowOnUnusedDesires && unusedDesires.Count > 0)
            {
                throw new LPGException(unusedDesires.Count + " unused desires found!");
            }
            foreach (var category in sim.Categories)
            {
                var thisType = category.GetType();
                var pi = thisType.GetProperty("MyItems");
                if (pi != null)
                {
                    var collection = pi.GetValue(category, null);
                    dynamic collection2 = collection;
                    var list = new List<DBBase>(collection2);
                    foreach (var dbBase in list)
                    {
                        if (dbBase.Name.ToLower(CultureInfo.CurrentCulture).StartsWith("new ", StringComparison.Ordinal)
                        )
                        {
                            throw new LPGException("Forgotten a new item:" + dbBase.Name);
                        }
                        if (dbBase.Name.ToLower(CultureInfo.CurrentCulture).Contains("diss") &&
                            dbBase.TypeDescription != "Settlement Template")
                        {
                            throw new LPGException("Forgotten a diss item: " + Environment.NewLine +
                                                   dbBase.TypeDescription + ":" + Environment.NewLine +
                                                   dbBase.Name);
                        }
                    }
                }
            }
        }


        /* 
         * //TODO: Fix all unused traits
        private static void FindUnusedTraits(Simulator sim)
        {
            List<HouseholdTrait> usedTraits = new List<HouseholdTrait>();
            foreach (ModularHousehold chh in sim.ModularHouseholds.MyItems)
                foreach (ModularHouseholdTrait trait in chh.Traits)
                    usedTraits.Add(trait.HouseholdTrait);
            List<HouseholdTrait> unusedTraits = new List<HouseholdTrait>();

            foreach (HouseholdTrait hht in sim.HouseholdTraits.MyItems)
            {
                bool found = usedTraits.Contains(hht);

                if (!found)
                    unusedTraits.Add(hht);
            }
            foreach (var device in unusedTraits)
            {
                Logger.Error("Found unused trait: " + device.Name);

            }
            if (unusedTraits.Count > 0)
                throw new LPGException("Unused traits!");
        }
        */
        /*
        private static void DeleteOldCalcOutcomes([JetBrains.Annotations.NotNull] DatabaseSetup db)
        {
            var sim = new Simulator(db.ConnectionString);
            var maxVersion = sim.CalculationOutcomes.It.Select(x => x.LPGVersion).Distinct()
                .OrderByDescending(x => x).First();
            var toDelete =
                sim.CalculationOutcomes.It.Where(x => x.LPGVersion != maxVersion).ToList();

            foreach (var simCalculationOutcome in toDelete)
            {
                simCalculationOutcome.DeleteFromDB();
            }
            Logger.Info("Deleted " + toDelete.Count + " items.");
        }
        */
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static void FindUnusedAffordance([JetBrains.Annotations.NotNull] Simulator sim)
        {
            var notFoundAffordances = new List<Affordance>();
            var affordances = new List<Affordance>(sim.Affordances.Items);
            var householdTraits = new List<HouseholdTrait>(sim.HouseholdTraits.Items);
            sim.DeviceCategories.Items.ToList().ForEach(dc => dc.RefreshSubDevices());
            // collect all household trait desires
            var hhtdesires = new Dictionary<HouseholdTrait, List<Desire>>();
            foreach (var householdTrait in householdTraits)
            {
                hhtdesires.Add(householdTrait, new List<Desire>());
                foreach (var desire in householdTrait.Desires)
                {
                    if (!hhtdesires[householdTrait].Contains(desire.Desire))
                    {
                        hhtdesires[householdTrait].Add(desire.Desire);
                    }
                }
            }
            foreach (var affordance in affordances)
            {
                var found = false;
                foreach (var householdTrait in sim.HouseholdTraits.Items)
                {
                    if (!found)
                    {
                        var isvalidinHousehold = false;
                        foreach (var affordanceDesire in affordance.AffordanceDesires)
                        {
                            if (hhtdesires[householdTrait].Contains(affordanceDesire.Desire))
                            {
                                isvalidinHousehold = true;
                            }
                        }
                        if (isvalidinHousehold)
                        {
                            foreach (var hhLocation in householdTrait.Locations)
                            {
                                if (hhLocation.AffordanceLocations.Any(x => x.Affordance == affordance))
                                {
                                    found = true;
                                }
                            }
                        }
                    }
                }
                if (!found)
                {
                    notFoundAffordances.Add(affordance);
                }
            }
            foreach (var notFoundAffordance in notFoundAffordances)
            {
                Logger.Error("Unused Affordance: " + notFoundAffordance);
            }
            if (notFoundAffordances.Count > 0)
            {
                throw new LPGException("too many unused affordances.");
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private static List<Desire> FindUnusedDesires([JetBrains.Annotations.NotNull] Simulator sim)
        {
            var desires = new List<Desire>();
            // collect all household trait desires
            foreach (var householdTrait in sim.HouseholdTraits.Items)
            {
                foreach (var desire in householdTrait.Desires)
                {
                    if (!desires.Contains(desire.Desire))
                    {
                        desires.Add(desire.Desire);
                    }
                }
            }
            var unusedDesires = new List<Desire>();
            foreach (var desire in sim.Desires.Items)
            {
                if (!desires.Contains(desire))
                {
                    unusedDesires.Add(desire);
                }
            }
            if (unusedDesires.Count > 0)
            {
                var desireNames = "";
                foreach (var unusedDesire in unusedDesires)
                {
                    desireNames = desireNames + Environment.NewLine + unusedDesire;
                }
                throw new LPGException("Unused desires: " + unusedDesires.Count + desireNames);
            }
            return unusedDesires;
        }

        private static void FindUnusedDevices([JetBrains.Annotations.NotNull] Simulator sim)
        {
            var usedDevices = new List<IAssignableDevice>();
            foreach (var affordance in sim.Affordances.Items)
            {
                foreach (var affordanceDevice in affordance.AffordanceDevices)
                {
                    if(affordanceDevice.Device != null) {
                        usedDevices.Add(affordanceDevice.Device);
                    }
                }
            }
            foreach (var location in sim.Locations.Items)
            {
                foreach (var locdev in location.LocationDevices)
                {
                    if (locdev.Device != null) {
                        usedDevices.Add(locdev.Device);
                    }
                }
            }
            foreach (var action in sim.DeviceActions.Items)
            {
                if(action.Device !=null) {
                    usedDevices.Add(action.Device);
                }
            }
            foreach (var dev in sim.RealDevices.Items)
            {
                if (dev.DeviceCategory != null) {
                    usedDevices.Add(dev.DeviceCategory);
                }
            }
            foreach (var hht in sim.HouseholdTraits.Items)
            {
                foreach (var autodev in hht.Autodevs)
                {
                    if (autodev.Device !=null) {
                        usedDevices.Add(autodev.Device);
                    }
                }
            }
            var devices = new List<IAssignableDevice>();
            devices.Clear();
            foreach (var rd in sim.RealDevices.Items)
            {
                if (rd.DeviceCategory != null) {
                    var found = usedDevices.Contains(rd) || usedDevices.Contains(rd.DeviceCategory);
                    if (!found) {
                        devices.Add(rd);
                    }
                }
            }
            foreach (var dc in sim.DeviceCategories.Items)
            {
                if (!usedDevices.Contains(dc) && dc.SubDevices.Count > 0)
                {
                    devices.Add(dc);
                }
            }
            foreach (var device in devices)
            {
                Logger.Error("Found unused device: " + device.Name);
            }
            if (devices.Count > 0)
            {
                throw new LPGException("Unused devices!");
            }
        }

        private void ReleaseCheck([JetBrains.Annotations.NotNull] string filename)
        {
            using (var db = new DatabaseSetup("CheckForNewLeftovers", filename))
            {
                var sim = new Simulator(db.ConnectionString);
                CheckForNewItems(sim);
                CheckForDevicesWithoutCategory(sim);
                FindUnusedAffordance(sim);
                FindUnusedDevices(sim);
                //TODO: Add the trait check back
                //FindUnusedTraits(sim);
                sim.MyGeneralConfig.PerformCleanUpChecks = "false";
                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                db.Cleanup();
            }
            CheckForCalculationOutcomeCompleteness();
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public void CheckForCalculationOutcomeCompleteness()
        {
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Logger.Info("Using file " + db.FileName);
            var sim = new Simulator(db.ConnectionString);
            var count = CalculationOutcomesPresenter.CountMissingEntries(sim);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (count != 0 && ThrowOnMissingOutcomes)
                // ReSharper disable once HeuristicUnreachableCode
            {
                throw new LPGException("Missing " + count + " calculation outcomes!");
            }
            db.Cleanup();
        }


        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void MakeRelease([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            // the .NET version is defined centrally in Directory.Build.props and embedded as assembly metadata
            var dotnetVersion = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == "LpgDotnetVersion")?.Value;
            if (string.IsNullOrEmpty(dotnetVersion))
                throw new LPGException("Could not determine the .NET version from the assembly metadata.");
            
            const string dbFilename = "profilegenerator-latest.db3";
            const bool cleanDatabase = true;
            const bool makeZip = true;
            const bool cleanCalcOutcomes = true;
            Logger.Info("### Starting Release");
            // get version number and remove the build number from that
            var fullVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            int lastDot = fullVersion.LastIndexOf('.');
            var releasename = fullVersion[..lastDot];
            if (string.IsNullOrEmpty(releasename))
                throw new LPGException("Could not determine proper release version number.");
            Logger.Info("Release name: " + releasename);

            // get the path to the root of the LPG repository, based on the location of this source file
            var lpgRepoPath = new FileInfo(sourceFilePath).Directory!.Parent;
            var baseReleasePath = lpgRepoPath!.CombineName("LPGRelease\\");
            var releaseDirectoriesPath = baseReleasePath + "release_directories\\";
            var dstWinFull = releaseDirectoriesPath + "windows";
            var dstLinuxSimEngine = releaseDirectoriesPath + "linux_simengine";
            var dstWinSimEngine = releaseDirectoriesPath + "windows_simengine";

            ClearDirectory(dstWinFull);
            ClearDirectory(dstLinuxSimEngine);
            ClearDirectory(dstWinSimEngine);

            // This source file (ReleaseBuilderTests.cs) is located in a subdirectory of the base development directory.
            // Use this to get the base development path from the file path
            var baseDevelopPath = Directory.GetParent(sourceFilePath)?.Parent;
            if (baseDevelopPath is null || !baseDevelopPath.Exists)
            {
                throw new LPGException("Could not find the base development path: " + baseDevelopPath);
            }
            Logger.Info($"Using base development path '{baseDevelopPath}'");

            // combine the main LPG GUI program and the SimulationEngine (with FlameChart function) in the same release folder
            Logger.Info("### Copying lpg files");
            string srcWinGUI = baseDevelopPath.CombineName($"LoadProfileGenerator\\bin\\Release\\{dotnetVersion}-windows\\publish");
            CopyDirectoryContents(srcWinGUI, dstWinFull);
            string srcWinSimengine = baseDevelopPath.CombineName($"SimulationEngine\\bin\\Release\\{dotnetVersion}\\win-x64\\publish");
            CopyDirectoryContents(srcWinSimengine, dstWinFull);

            // copy the SimEngine2 binaries (no GUI) for Windows and Linux to the respective release folders
            string simengine2Path = $"SimEngine2\\bin\\Release\\{dotnetVersion}\\";
            string srcWinSimEngine = baseDevelopPath.CombineName($"{simengine2Path}win-x64\\publish");
            CopyDirectoryContents(srcWinSimEngine, dstWinSimEngine);
            string srcsimLinux = baseDevelopPath.CombineName($"{simengine2Path}linux-x64\\publish");
            CopyDirectoryContents(srcsimLinux, dstLinuxSimEngine);
            Logger.Info("### Finished copying lpg files");

            Logger.Info("### Performing release checks");
            ReleaseCheck(dbFilename);

            // clean database
            using (var db = new DatabaseSetup("Release", dbFilename))
            {
                Logger.Info("Using database " + dbFilename);
                if (cleanDatabase)
                {
                    //DeleteOldCalcOutcomes(db);
                    Logger.Info("### cleaning database");
                    DissStuffDatabaseCleaner.Run(db.FileName);
                }
                if (cleanCalcOutcomes)
                {
                    Logger.Info("### cleaning calc outcomes");
                    CalculationOutcome.ClearTable(db.ConnectionString);
                }
                Logger.Info("### integrity check");
                var sim = new Simulator(db.ConnectionString);
                sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.ReasonableWithChartsAndPDF);
                sim.MyGeneralConfig.PerformCleanUpChecks = "True";
                sim.MyGeneralConfig.ShowSettlingPeriod = "False";
                sim.MyGeneralConfig.DestinationPath = "C:\\Work\\";
                sim.MyGeneralConfig.ImagePath = "C:\\Work\\";
                sim.MyGeneralConfig.RandomSeed = -1;
                sim.MyGeneralConfig.StartDateString = "01.01.2026";
                sim.MyGeneralConfig.EndDateString = "31.12.2026";
                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                sim.MyGeneralConfig.PerformCleanUpChecks = "False";
                sim.MyGeneralConfig.CSVCharacter = ";";
                var forgottenUpdates = false;
                Logger.Info("### updating estimates");
                foreach (var trait in sim.HouseholdTraits.Items)
                {
                    switch (trait.Name)
                    {
                        case "Cooking, average":
                        case "Cooking, maximum": continue;

                        default:

                            var count = trait.EstimatedTimeCount;
                            var tt = trait.EstimatedTimeType;
                            trait.CalculateEstimatedTimes();
                            if (Math.Abs(trait.EstimatedTimeCount - count) > 0.0000001 || trait.EstimatedTimeType != tt)
                            {
                                forgottenUpdates = true;
                                Logger.Error("seems you forgot to update the estimate for " + trait.PrettyName +
                                             Environment.NewLine + "Prev count: " + count + " curr: " +
                                             trait.EstimatedTimeCount + Environment.NewLine + "prev tt: " + tt +
                                             " curr tt: " + trait.EstimatedTimeType);
                            }
                            break;
                    }
                }
                if (forgottenUpdates)
                {
                    throw new LPGException("Forgotten updates!\n" + Logger.Get().ReturnAllLoggedErrors());
                }

                // get rid of all templated items
                if (cleanDatabase)
                {
                    Logger.Info("### deleting all templated items");
                    sim.FindAndDeleteAllTemplated();
                    var templatedItems = sim.FindAndDeleteAllTemplated();
                    if (templatedItems > 0)
                    {
                        throw new LPGException("Left templated items");
                    }
                }

                // copy cleaned database to release folders
                const string targetName = "profilegenerator.db3";
                File.Copy(db.FileName, Path.Combine(dstWinFull, targetName), true);
                File.Copy(db.FileName, Path.Combine(dstWinSimEngine, targetName), true);
                File.Copy(db.FileName, Path.Combine(dstLinuxSimEngine, targetName), true);
            }
            Thread.Sleep(1000);
            Logger.Info("### Finished copying all files");
            
            if (makeZip)
            {
                List<FileInfo> fileForUpload = [
                    MakeZipFile(releasename, dstWinFull),
                    MakeZipFile(releasename + "_windows_simengine", dstWinSimEngine),
                    MakeZipFile(releasename + "_linux_simengine", dstLinuxSimEngine)
                ];

                var zipFilesPath = $"{baseReleasePath}zip_files";
                ClearDirectory(zipFilesPath);
                foreach (FileInfo fi in fileForUpload) {
                    string dstName = Path.Combine(zipFilesPath, fi.Name);
                    fi.MoveTo(dstName, true);
                }
            }
        }

        /// <summary>
        /// Deletes the directory and creates it again, to ensure it is empty
        /// </summary>
        /// <param name="directory"></param>
        private static void ClearDirectory(string directory)
        {
            if (Directory.Exists(directory)) {
                try {
                    Directory.Delete(directory, true);
                }
                catch (Exception ex) {
                    Logger.Info(ex.Message);
                }

                Thread.Sleep(250);
            }

            Directory.CreateDirectory(directory);
            Thread.Sleep(250);
        }

        /// <summary>
        /// Copies all contents of one directory to another directory, including subdirectories and files.
        /// Keeps any existing contents in the destination directory, but overwrites files with the same name.
        /// </summary>
        /// <param name="sourcePath">the source directory</param>
        /// <param name="destinationPath">the destination directory</param>
        /// <exception cref="DirectoryNotFoundException">if the source directory does not exist</exception>
        private static void CopyDirectoryContents(string sourcePath, string destinationPath)
        {
            var sourceDir = new DirectoryInfo(sourcePath);

            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");

            // create directory if it doesn't exist
            Directory.CreateDirectory(destinationPath);

            // Copy files
            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationPath, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }

            // Recurse into subdirectories
            foreach (DirectoryInfo subDir in sourceDir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryContents(subDir.FullName, newDestinationDir);
            }
        }

        private static FileInfo MakeZipFile([JetBrains.Annotations.NotNull] string releaseName, [JetBrains.Annotations.NotNull] string dst)
        {
            using (var process = new Process()) {
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = @"C:\Program Files\7-Zip\7z.exe";
                process.StartInfo.Arguments = "a -tzip -mx9 LPG" + releaseName + ".zip  *";
                Logger.Info(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.StartInfo.WorkingDirectory = dst;
                process.Start();
                process.WaitForExit(); // Waits here for the process to exit.
            }
            return new FileInfo( Path.Combine( dst, "LPG"+releaseName + ".zip"));
        }
    }
}
