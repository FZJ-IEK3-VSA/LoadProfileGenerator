using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using CalculationEngine;
using Common;
using Common.Enums;
using Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerArgs;

namespace SimulationEngineLib.HouseJobProcessor
{
    public class JsonCalculator
    {
        public const int SettlingDays = 3;
        public static readonly TimeSpan InternalTimeResolution = new(0, 1, 0);

        [JetBrains.Annotations.NotNull] private readonly CalculationProfiler _calculationProfiler = new();

        /*
        public bool Calculate([JetBrains.Annotations.NotNull] JsonDirectoryOptions calcDirectoryOptions)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            string jsonFileName = calcDirectoryOptions.Input;
            if (jsonFileName == null) {
                Logger.Error("No file was set.");
                return false;
            }

            var fi = new FileInfo(jsonFileName);
            if (!fi.Exists) {
                Logger.Error("File not found: " + fi.FullName);
            }

            string jsonContent = File.ReadAllText(fi.FullName);
            JsonCalcSpecification jcs = JsonConvert.DeserializeObject<JsonCalcSpecification>(jsonContent);


            if (!CheckBasicSettingsForLoadingDatabase(jcs)) {
                Logger.Error("Could not load the database ");
                return false;
            }

            //Logger.Info("Using the following configuration: \n" + JsonConvert.SerializeObject(jcs,Formatting.Indented)  );
            var myConnectionString = "Data Source=" + jcs.PathToDatabase;
            string targetDirectory = jcs.OutputDirectory;
            if (string.IsNullOrWhiteSpace(targetDirectory)) {
                targetDirectory = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(targetDirectory)) {
                Directory.CreateDirectory(targetDirectory);
                Thread.Sleep(500);
            }

            //Logger.LogFilePath = Path.Combine(targetDirectory, "mylog.txt");
            //Logger.LogToFile = true;
            //Config.ResetLogfileAtCalculationStart = false;
            Logger.Info("Loading...");
            var sim = new Simulator(myConnectionString);
            Logger.Info("Loading finished.");
            if (!CheckSimulator(jcs, sim)) {
                Logger.Error("Due to incorrect settings, calculation can not start.");
                return false;
            }

            StartHousehold(sim, jcs);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return true;
        }*/

        //public bool CheckBasicSettingsForLoadingDatabase([JetBrains.Annotations.NotNull] JsonCalcSpecification jcs)
        //{
        //    if (!string.IsNullOrWhiteSpace(jcs.PathToDatabase)) {
        //        var fi = new FileInfo(jcs.PathToDatabase);
        //        if (!fi.Exists) {
        //            throw new LPGException("Database was not found. Path supplied:" + fi.FullName);
        //        }
        //    }
        //    return true;
        //}

        public static TimeSpan ParseTimeResolution(string resolutionString, TimeSpan? defaultResolution = null)
        {
            defaultResolution ??= InternalTimeResolution;
            if (resolutionString.IsNullOrEmpty())
                return defaultResolution.Value;
            var success = TimeSpan.TryParse(resolutionString, out var result);
            if (!success)
                throw new LPGPBadParameterException("Invalid time resolution string: " + resolutionString);
            return result;
        }

        /// <summary>
        /// Generates the CalcStartParameterSet out of the JsonCalcSpecification, checks parameters and fills missing parameters with defaults from the
        /// CalcObject.
        /// </summary>
        /// <param name="sim">Simlator to read default values</param>
        /// <param name="calcSpec">The calculation specification to get parameters from</param>
        /// <param name="calcObjectReference">JsonReference of the object to simulate</param>
        /// <param name="profiler">optional profiler object to use in the calculation</param>
        /// <returns>The CalcStartParameterSet containing all specified parameters, and default values for anything not specified</returns>
        public static CalcStartParameterSet CreateCalcParametersFromCalcSpec(Simulator sim, JsonCalcSpecification calcSpec, JsonReference calcObjectReference, CalculationProfiler profiler = null)
        {
            // get the CalcObject from the JsonReference
            if (calcObjectReference == null)
            {
                throw new LPGException("No calculation object was selected.");
            }
            var calcObject = GetCalcObject(sim, calcObjectReference);

            // check if start and end date are set
            var startDate = calcSpec.StartDate ?? throw new LPGPBadParameterException("No StartDate specified.");
            var endDate = calcSpec.EndDate ?? throw new LPGPBadParameterException("No EndDate specified.");
            //jcs.StartDate ??= new DateTime(DateTime.Now.Year, 1, 1);
            //jcs.EndDate ??= new DateTime(DateTime.Now.Year, 12, 31);

            // parse time resolution parameters
            var internalResolution = ParseTimeResolution(calcSpec.InternalTimeResolution);
            var externalResolution = ParseTimeResolution(calcSpec.ExternalTimeResolution, internalResolution);

            // check if all required parameters are set and valid, or else choose default values
            if (calcSpec.OutputDirectory == null)
            {
                calcSpec.OutputDirectory = AutomationUtili.CleanFileName(calcObject.Name) + " - " + calcObject;
                if (calcSpec.OutputDirectory.Length > 50)
                {
                    calcSpec.OutputDirectory = calcSpec.OutputDirectory.Substring(0, 50);
                }
            }

            var energyIntensity = calcSpec.EnergyIntensityType;
            if (energyIntensity == EnergyIntensityType.AsOriginal)
            {
                energyIntensity = calcObject.EnergyIntensityType;
            }
            if (calcSpec.LoadTypePriority == LoadTypePriority.Undefined)
            {
                // set default LoadTypePriority depending on CalcObject type
                calcSpec.LoadTypePriority = (calcObject.CalcObjectType == CalcObjectType.ModularHousehold) ?
                    LoadTypePriority.RecommendedForHouseholds : LoadTypePriority.RecommendedForHouses;
            }

            // join the manually selected CalcOptions with the ones from the DefaultForOutputFiles setting
            var defaultCalcOptions = OutputFileDefaultHelper.GetOptionsForDefault(calcSpec.DefaultForOutputFiles);
            var mergedCalcOptions = calcSpec.CalcOptions.Union(defaultCalcOptions).ToList();

            // create a new profiler if none is passed
            profiler ??= new CalculationProfiler();

            // look up objects matching the specified JsonReferences
            // if no reference is provided, first fall back to the setting in the CalcObject, then to the general default object
            var temperatureProfile = sim.TemperatureProfiles.FindWithException(calcSpec.TemperatureProfile, true);
            temperatureProfile ??= calcObject.DefaultTemperatureProfile;
            temperatureProfile ??= sim.TemperatureProfiles.GetDefault();

            var geographicLocation = sim.GeographicLocations.FindWithException(calcSpec.GeographicLocation, true);
            geographicLocation ??= calcObject.DefaultGeographicLocation;
            geographicLocation ??= sim.GeographicLocations.GetDefault();

            var deviceSelection = sim.DeviceSelections.FindWithException(calcSpec.DeviceSelection, true);

            // fixed default values
            ILPGDispatcher lpgDispatcher = null;

            // Combine all settings in a CalcStartParameterSet object.
            // Note: settings taken from the Simulator are not part of the JsonCalcSpecification.
            return new CalcStartParameterSet(
                (_, _, _) => true,
                (_, _, _) => true,
                _ => true,
                lpgDispatcher,
                geographicLocation,
                temperatureProfile,
                calcObject,
                energyIntensity,
                () => true,
                false,
                deviceSelection,
                calcSpec.LoadTypePriority,
                transportationDeviceSet: null,
                travelRouteSet: null,
                mergedCalcOptions,
                startDate,
                endDate,
                internalResolution,
                sim.MyGeneralConfig.CSVCharacter,
                calcSpec.RandomSeed,
                externalResolution,
                sim.MyGeneralConfig.WriteExcelColumnBool,
                sim.MyGeneralConfig.ShowSettlingPeriodBool,
                SettlingDays,
                sim.MyGeneralConfig.RepetitionCount,
                calculationProfiler: profiler,
                chargingStationSet: null,
                calcSpec.LoadtypesForPostprocessing,
                sim.MyGeneralConfig.DeviceProfileHeaderMode,
                calcSpec.IgnorePreviousActivitiesWhenNeeded,
                calcSpec.OutputDirectory,
                calcSpec.EnableTransportation,
                calcSpec.EnableIdlemode,
                sim.MyGeneralConfig.DecimalSeperator,
                calcSpec.EnableFlexibility
            );
        }

        /// <summary>
        /// Stores the simulation settings in the JsonCalcSpecification to the database, so that
        /// subsequent simulations from the LPG GUI using this database use the same settings.
        /// </summary>
        /// <param name="sim">database access object</param>
        /// <param name="jcs">CalcSpec whose settings are to be saved</param>
        public void SaveSettingsToDatabase(Simulator sim, JsonCalcSpecification jcs)
        {
            sim.MyGeneralConfig.StartDateUIString = jcs.StartDate.ToString();
            sim.MyGeneralConfig.EndDateUIString = jcs.EndDate.ToString();
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.DestinationPath = new DirectoryInfo(jcs.OutputDirectory).FullName;
            sim.MyGeneralConfig.RandomSeed = jcs.RandomSeed;
            // merge the CalcOptions
            sim.MyGeneralConfig.ApplyOptionDefault(jcs.DefaultForOutputFiles);
            if (jcs.CalcOptions != null)
            {
                foreach (var option in jcs.CalcOptions)
                {
                    Logger.Info("Enabling option " + option);
                    sim.MyGeneralConfig.Enable(option);
                }
            }
            // if no external time resolution is set, use the internal resolution for that as well
            sim.MyGeneralConfig.ExternalTimeResolution = jcs.ExternalTimeResolution ?? sim.MyGeneralConfig.InternalTimeResolution;
        }

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void StartHousehold([JetBrains.Annotations.NotNull] Simulator sim, [JetBrains.Annotations.NotNull] JsonCalcSpecification jcs, [JetBrains.Annotations.NotNull] JsonReference calcObjectReference)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var calculationStartTime = DateTime.Now;

            // Check for already existing result files at the target location
            var generalResultsDirectory = new DirectoryInfo(jcs.OutputDirectory ?? throw new LPGException("Output directory was null."));
            var finishedFile = Path.Combine(generalResultsDirectory.FullName, Constants.FinishedFileFlag);
            if (Directory.Exists(generalResultsDirectory.FullName))
            {
                if (jcs.SkipExisting)
                {
                    if (File.Exists(finishedFile))
                    {
                        Logger.Error("Directory already exists and calculation is finished. Exiting.");
                        _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
                        return;
                    }
                }

                Logger.Warning("Directory already exists, but calculation is not finished or skip existing is not specified. Deleting folder.");
                var files = generalResultsDirectory.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Name.StartsWith("Log.", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (file.Name.EndsWith(".db3", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    file.Delete();
                }

                var directories = generalResultsDirectory.GetDirectories();
                foreach (DirectoryInfo info in directories)
                {
                    info.Delete(true);
                }

                Thread.Sleep(1000);
            }

            // initialize logfile and log the calcspec
            generalResultsDirectory.Create();
            Thread.Sleep(500);
            Logger.SetLogFilePath(Path.Combine(generalResultsDirectory.FullName, "Log.CommandlineCalculation.txt"));
            Logger.LogToFile = true;
            Logger.Get().FlushExistingMessages();
            Logger.Info("---------------------------");
            Logger.Info("Used calculation specification:");
            Logger.Info(JsonConvert.SerializeObject(jcs, Formatting.Indented), true);
            Logger.Info("---------------------------");
            Logger.Info("Directory: " + generalResultsDirectory.FullName);

            // save settings to the database copy in the result directory
            SaveSettingsToDatabase(sim, jcs);

            // create the CalcStartParameterSet containing all parameters for the calculation
            var calcStartParameterSet = CreateCalcParametersFromCalcSpec(sim, jcs, calcObjectReference, _calculationProfiler);
            calcStartParameterSet.PreserveLogfileWhileClearingFolder = true;

            // execute the simulation
            var cs = new CalcStarter(sim);
            cs.Start(calcStartParameterSet);

            if (jcs.CalcOptions != null && jcs.CalcOptions.Contains(CalcOption.CalculationFlameChart))
            {
                string targetfile = Path.Combine(generalResultsDirectory.FullName, Constants.CalculationProfilerJson);
                using (StreamWriter sw = new StreamWriter(targetfile))
                {
                    _calculationProfiler.WriteJson(sw);
                }
            }
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            // ChartMaker.MakeChartsAndPDF(_calculationProfiler,calcStartParameterSet.ResultPath);

            var duration = DateTime.Now - calculationStartTime;
            if (jcs.DeleteAllButPDF)
            {
                var allFileInfos = generalResultsDirectory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var fi in allFileInfos)
                {
                    if (fi.Name.ToUpperInvariant().EndsWith(".PDF", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (fi.Name.ToUpperInvariant().StartsWith("SUMPROFILES.", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (fi.Name.ToUpperInvariant().StartsWith("HOUSEHOLDNAME.", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    fi.Delete();
                }
            }

            if (jcs.DeleteSqlite)
            {
                var allFileInfos = generalResultsDirectory.GetFiles("*.sqlite", SearchOption.AllDirectories);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                foreach (var fi in allFileInfos)
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex);
                    }
                }
            }

            Logger.ImportantInfo("Calculation duration:" + duration);

            //cleanup empty directories
            var subdirs = generalResultsDirectory.GetDirectories();
            foreach (var subdir in subdirs)
            {
                var files = subdir.GetFiles();
                var subsubdirs = subdir.GetDirectories();
                if (files.Length == 0 && subsubdirs.Length == 0)
                {
                    subdir.Delete();
                }
            }

            using (var sw = new StreamWriter(finishedFile))
            {
                sw.WriteLine("Finished at " + DateTime.Now);
                sw.WriteLine("Duration in seconds:");
                sw.WriteLine(duration.TotalSeconds);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static ICalcObject GetCalcObject([JetBrains.Annotations.NotNull] Simulator sim, [CanBeNull] JsonReference calcReference)
        {
            if (calcReference == null)
            {
                throw new LPGException("No calc object was set. Can't continue.");
            }
            var house = sim.Houses.FindByJsonReference(calcReference);
            if (house != null)
            {
                return house;
            }

            throw new LPGException("Could not find the Calculation object with the guid " + calcReference.Guid);
        }

        private static bool OpenTabFunc([JetBrains.Annotations.NotNull] object o) => true;

        private static bool ReportCancelFunc() => true;

        private static bool ReportFinishFuncForHouseAndSettlement(bool a2,
                                                                  [JetBrains.Annotations.NotNull] string a3,
                                                                  [ItemCanBeNull][CanBeNull] ObservableCollection<ResultFileEntry> a4) => true;

        private static bool ReportFinishFuncForHousehold(bool a2, [JetBrains.Annotations.NotNull] string a3, [JetBrains.Annotations.NotNull] string path) => true;

        /*
        [CanBeNull]
        private CalcOption? GetCalcOption([CanBeNull] string strOption)
        {
            if (strOption == null) {
                return null;
            }

            CalcOption? co = null;
            foreach (CalcOption ofd in Enum.GetValues(typeof(CalcOption)))
            {
                if (ofd.ToString() == strOption)
                {
                    co = ofd;
                }
            }
            return co;
        }*/
        /*
        private LoadTypePriority GetLoadtypePriority([CanBeNull] string jcsLoadTypePriority, [JetBrains.Annotations.NotNull] ICalcObject calcObject)
        {
            LoadTypePriority ltp = LoadTypePriority.Undefined;
            foreach (LoadTypePriority ofd in Enum.GetValues(typeof(LoadTypePriority)))
            {
                if (ofd.ToString() == jcsLoadTypePriority)
                {
                    ltp =  ofd;
                }
            }
            if (ltp == LoadTypePriority.Undefined)
            {
                Logger.Info("LoadTypePriority was not set. Determing setting...");
                if (calcObject.CalcObjectType == CalcObjectType.ModularHousehold)
                {
                    ltp = LoadTypePriority.RecommendedForHouseholds;
                    Logger.Info("LoadTypePriority for households was selected");
                }
                else
                {
                    ltp = LoadTypePriority.RecommendedForHouses;
                    Logger.Info("LoadTypePriority for houses was selected");
                }
            }

            return ltp;
        }*/
        /*
        private EnergyIntensityType GetEnergyIntensity([CanBeNull] string copEnergyIntensityType)
        {
            if (string.IsNullOrWhiteSpace(copEnergyIntensityType))
            {
                return EnergyIntensityType.Random;
            }
            foreach (EnergyIntensityType ofd in Enum.GetValues(typeof(EnergyIntensityType)))
            {
                if (ofd.ToString() == copEnergyIntensityType)
                {
                    return ofd;
                }
            }
            throw new LPGException("Unrecognized default for energy intensity value");
        }*/
        /*
        private OutputFileDefault GetOutputFileDefaultsFromString([CanBeNull] string defaultForOutputFiles)
        {
            if (string.IsNullOrWhiteSpace(defaultForOutputFiles)) {
                return OutputFileDefault.ReasonableWithChartsAndPDF;
            }
            foreach (OutputFileDefault ofd in Enum.GetValues(typeof(OutputFileDefault))) {
                if (ofd.ToString() == defaultForOutputFiles) {
                    return ofd;
                }
            }
            throw new LPGException("Unrecognized default for output files value");
        }*/
    }
}
