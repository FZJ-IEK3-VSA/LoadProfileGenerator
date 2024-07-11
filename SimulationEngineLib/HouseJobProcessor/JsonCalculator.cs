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

        /// <summary>
        /// Parses a time resolution for the simulation from a settings string. Applies a default if 
        /// the string is empty, and throws an error if it is invalid.
        /// </summary>
        /// <param name="resolutionString">string specifying a time resolution in format hh:mm:ss</param>
        /// <param name="defaultResolution">default resolution to use in case of an empty string</param>
        /// <returns>the parsed resolution</returns>
        /// <exception cref="LPGPBadParameterException">if an invalid string is passed</exception>
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
        public static void SaveSettingsToDatabase(Simulator sim, JsonCalcSpecification jcs)
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

        /// <summary>
        /// Initialize the logger and log the JsonCalcSpecification
        /// </summary>
        /// <param name="resultDirectory">result directory for the log file</param>
        /// <param name="jcs">JsonCalcSpecification of the simulation job, which will be logged</param>
        /// <param name="logFileName">name of the log file to write to</param>
        public static void InitLoggerAndLogCalcSpec(DirectoryInfo resultDirectory, JsonCalcSpecification jcs, string logFileName = "Log.CommandlineCalculation.txt")
        {
            Logger.SetLogFilePath(Path.Combine(resultDirectory.FullName, logFileName));
            Logger.LogToFile = true;
            Logger.Get().FlushExistingMessages();
            Logger.Info("---------------------------");
            Logger.Info("Used calculation specification:");
            Logger.Info(JsonConvert.SerializeObject(jcs, Formatting.Indented), true);
            Logger.Info("---------------------------");
            Logger.Info("Directory: " + resultDirectory.FullName);
        }


        /// <summary>
        /// Deletes all but PDF files (and some other relevant files) in the result directory.
        /// </summary>
        /// <param name="resultDirectory">the result directory to clean up</param>
        public static void DeleteAllButPDF(DirectoryInfo resultDirectory)
        {
            var allFileInfos = resultDirectory.GetFiles("*.*", SearchOption.AllDirectories);
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

        /// <summary>
        /// Deletes all .sqlite files in the result directory.
        /// </summary>
        /// <param name="resultDirectory">the result directory to delete sqlite files in</param>
        public static void DeleteSqlite(DirectoryInfo resultDirectory)
        {
            var allFileInfos = resultDirectory.GetFiles("*.sqlite", SearchOption.AllDirectories);
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

        /// <summary>
        /// Deletes a directoy if it is empty or, if recursive is true, it it only contains
        /// empty subdirectories.
        /// </summary>
        /// <param name="directory">the directory to clean up</param>
        /// <param name="recursive">whether to recursively check subdirectories as well</param>
        public static void DeleteDirectoryIfEmpty(DirectoryInfo directory, bool recursive = true)
        {
            var subdirs = directory.GetDirectories();
            if (recursive)
            {
                // recursively clear empty subdirectories
                foreach (var subsubdir in subdirs)
                {
                    DeleteDirectoryIfEmpty(subsubdir, recursive);
                }
                // get an updated list of subdirectories
                subdirs = directory.GetDirectories();
            }
            var files = directory.GetFiles();
            if (files.Length == 0 && subdirs.Length == 0)
            {
                directory.Delete();
            }
        }

        /// <summary>
        /// Cleans up the result directory after the calculation.
        /// </summary>
        /// <param name="calcSpec">the result directory to clean up</param>
        public static void CleanUpResultDirectory(JsonCalcSpecification calcSpec)
        {
            var resultDirectory = new DirectoryInfo(calcSpec.OutputDirectory);
            if (calcSpec.DeleteAllButPDF)
            {
                DeleteAllButPDF(resultDirectory);
            }
            if (calcSpec.DeleteSqlite)
            {
                DeleteSqlite(resultDirectory);
            }
            DeleteDirectoryIfEmpty(resultDirectory);
        }

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void StartHousehold([JetBrains.Annotations.NotNull] Simulator sim, [JetBrains.Annotations.NotNull] JsonCalcSpecification jcs, [JetBrains.Annotations.NotNull] JsonReference calcObjectReference)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var calculationStartTime = DateTime.Now;

            var resultDirectory = new DirectoryInfo(jcs.OutputDirectory ?? throw new LPGException("Output directory was null."));

            // initialize logfile and log the calcspec
            InitLoggerAndLogCalcSpec(resultDirectory, jcs);

            // save settings to the database copy in the result directory
            SaveSettingsToDatabase(sim, jcs);

            // create the CalcStartParameterSet containing all parameters for the calculation
            var calcStartParameterSet = CreateCalcParametersFromCalcSpec(sim, jcs, calcObjectReference, _calculationProfiler);
            calcStartParameterSet.PreserveLogfileWhileClearingFolder = true;

            // execute the simulation
            var cs = new CalcStarter(sim);
            cs.Start(calcStartParameterSet);

            // write profiler results to JSON file
            if (jcs.CalcOptions != null && jcs.CalcOptions.Contains(CalcOption.CalculationFlameChart))
            {
                string targetfile = Path.Combine(resultDirectory.FullName, Constants.CalculationProfilerJson);
                using (StreamWriter sw = new StreamWriter(targetfile))
                {
                    _calculationProfiler.WriteJson(sw);
                }
            }
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());

            var duration = DateTime.Now - calculationStartTime;
            Logger.ImportantInfo("Calculation duration:" + duration);

            // remove unneeded files and subdirectories
            CleanUpResultDirectory(jcs);
        }

        /// <summary>
        /// Returns a CalcObject given its JsonReference.
        /// </summary>
        /// <param name="sim">database access object</param>
        /// <param name="calcReference">the reference of the CalcObject to get</param>
        /// <returns>the CalcObject</returns>
        /// <exception cref="LPGException">if the reference was null or no object with this reference was found</exception>
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
    }
}
