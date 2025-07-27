using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using Common;
using Common.Enums;
using Common.JSON;
using Database;
using JetBrains.Annotations;
using Newtonsoft.Json;
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
        /// Generates the CalcStartParameterSet from the JsonCalcSpecification, checks parameters and fills missing parameters with defaults from the
        /// CalcObject. Missing values are also set in the passed JsonCalcSpecification object.
        /// </summary>
        /// <param name="sim">Simlator to read default values</param>
        /// <param name="calcSpec">the calculation specification to get parameters from</param>
        /// <param name="calcObjectReference">JsonReference of the object to simulate</param>
        /// <param name="profiler">optional profiler object to use in the calculation</param>
        /// <param name="citySimulationEnabled">whether city simulation is enabled or not</param>
        /// <param name="preserveLogfile">if true, keeps any logfile of a previous simulation in the same output directory</param>
        /// <returns>the CalcStartParameterSet containing all specified parameters, and default values for anything not specified</returns>
        public static CalcStartParameterSet CreateCalcParametersFromJsonCalcSpec(Simulator sim, JsonCalcSpecification calcSpec, JsonReference calcObjectReference,
            CalculationProfiler? profiler = null, bool citySimulationEnabled = false, bool preserveLogfile=false)
        {
            CalcParameters parameters = CreateCalcParameters(sim, calcSpec, citySimulationEnabled);
            CalcObjectParameters calcObjectParams = CreateCalcObjectParams(sim, calcSpec, calcObjectReference);

            CalculationHelpers helpers = new(profiler);

            // Combine all settings in a CalcStartParameterSet object.
            return new CalcStartParameterSet(calcObjectParams, parameters, helpers, calcSpec.RandomSeed, null, preserveLogfile);
        }

        /// <summary>
        /// Generates the CalcObjectParameters for a specific CalcObject from the JsonCalcSpecification.
        /// </summary>
        /// <param name="sim">Simlator to read default values</param>
        /// <param name="calcSpec">the calculation specification to get parameters from</param>
        /// <param name="calcObjectReference">JsonReference of the CalcObject to simulate</param>
        /// <param name="outputDirectory">can be used to overwrite the output directory from the calcspec</param>
        /// <returns>the CalcObjectParameters for the CalcObject </returns>
        /// <exception cref="LPGException">if the CalcObject was null</exception>
        public static CalcObjectParameters CreateCalcObjectParams(Simulator sim, JsonCalcSpecification calcSpec, JsonReference calcObjectReference, string outputDirectory = "")
        {
            var deviceSelection = sim.DeviceSelections.FindWithException(calcSpec.DeviceSelection, true);

            // get the CalcObject from the JsonReference
            if (calcObjectReference == null)
            {
                throw new LPGException("No calculation object was selected.");
            }
            var calcObject = GetCalcObject(sim, calcObjectReference);

            // check if all required parameters are set and valid, or else choose default values
            if (outputDirectory.IsNullOrEmpty())
            {
                calcSpec.OutputDirectory ??= SelectDefaultResultDirectory(calcObject);
            } else
            {
                calcSpec.OutputDirectory = outputDirectory;
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

            // look up objects matching the specified JsonReferences
            // if no reference is provided, first fall back to the setting in the CalcObject, then to the general default object
            var temperatureProfile = sim.TemperatureProfiles.FindWithException(calcSpec.TemperatureProfile, true);
            temperatureProfile ??= calcObject.DefaultTemperatureProfile;
            temperatureProfile ??= sim.TemperatureProfiles.GetDefault();

            var geographicLocation = sim.GeographicLocations.FindWithException(calcSpec.GeographicLocation, true);
            geographicLocation ??= calcObject.DefaultGeographicLocation;
            geographicLocation ??= sim.GeographicLocations.GetDefault();
            return new(calcObject, calcSpec.OutputDirectory, temperatureProfile, geographicLocation, energyIntensity, calcSpec.LoadTypePriority, deviceSelection);
        }

        /// <summary>
        /// Generates the CalcParameters from the JsonCalcSpecification.
        /// </summary>
        /// <param name="sim">Simlator to read default values</param>
        /// <param name="calcSpec">the calculation specification to get parameters from</param>
        /// <param name="citySimulationEnabled">whether city simulation is enabled or not</param>
        /// <returns>the generated CalcParameters</returns>
        /// <exception cref="LPGPBadParameterException">if start or end date are missing</exception>
        public static CalcParameters CreateCalcParameters(Simulator sim, JsonCalcSpecification calcSpec, bool citySimulationEnabled = false)
        {
            // check if start and end date are set
            var startDate = calcSpec.StartDate ?? throw new LPGPBadParameterException("No StartDate specified.");
            var endDate = calcSpec.EndDate ?? throw new LPGPBadParameterException("No EndDate specified.");

            // parse time resolution parameters
            var internalResolution = ParseTimeResolution(calcSpec.InternalTimeResolution);
            var externalResolution = ParseTimeResolution(calcSpec.ExternalTimeResolution, internalResolution);

            // join the manually selected CalcOptions with the ones from the DefaultForOutputFiles setting
            var defaultCalcOptions = OutputFileDefaultHelper.GetOptionsForDefault(calcSpec.DefaultForOutputFiles);
            var mergedCalcOptions = calcSpec.CalcOptions.Union(defaultCalcOptions).ToList();

            // combine settings from the JsonCalcSpecification and the Simulator
            return new(
                mergedCalcOptions,
                startDate,
                endDate,
                internalResolution,
                sim.MyGeneralConfig.CSVCharacter,
                externalResolution,
                sim.MyGeneralConfig.WriteExcelColumnBool,
                sim.MyGeneralConfig.ShowSettlingPeriodBool,
                SettlingDays,
                sim.MyGeneralConfig.RepetitionCount,
                calcSpec.LoadtypesForPostprocessing,
                sim.MyGeneralConfig.DeviceProfileHeaderMode,
                calcSpec.IgnorePreviousActivitiesWhenNeeded,
                calcSpec.EnableTransportation,
                calcSpec.EnableIdlemode,
                sim.MyGeneralConfig.DecimalSeperator,
                calcSpec.EnableFlexibility,
                citySimulationEnabled: citySimulationEnabled
            );
        }

        /// <summary>
        /// Choose a default result directory in case none was specified.
        /// The directory is a subdirectory within the current working directory, with its name
        /// based on the CalcObject.
        /// </summary>
        /// <param name="calcObject">the object to simulate</param>
        /// <returns>the path for the result directory</returns>
        private static string SelectDefaultResultDirectory(ICalcObject calcObject)
        {
            var resultPath = AutomationUtili.CleanFileName(calcObject.Name) + " - " + calcObject;
            if (resultPath.Length > 50)
            {
                resultPath = resultPath[..50];
            }
            var resultDir = new DirectoryInfo(resultPath);
            // The system might automatically adapt illegal paths, e.g., with a trailing dot. Make sure to save the actual path.
            return resultDir.FullName;
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
        /// Initializes the logger and sets the log file path
        /// </summary>
        /// <param name="resultDirectory">result directory for the log file</param>
        /// <param name="logFileName">name of the log file to write to</param>
        public static void InitLogger(DirectoryInfo resultDirectory, string logFileName = "Log.CommandlineCalculation.txt")
        {
            resultDirectory.Create();
            Logger.SetLogFilePath(Path.Combine(resultDirectory.FullName, logFileName));
            Logger.LogToFile = true;
            Logger.Get().FlushExistingMessages();
            Logger.Info("Directory: " + resultDirectory.FullName);
        }

        /// <summary>
        /// Logs the JsonCalcSpecification
        /// </summary>
        /// <param name="jcs">the calcspec to log</param>
        public static void LogCalcSpec(JsonCalcSpecification jcs)
        {
            Logger.Info("---------------------------");
            Logger.Info("Used calculation specification:");
            Logger.Info(JsonConvert.SerializeObject(jcs, Formatting.Indented), true);
            Logger.Info("---------------------------");
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

            // create the CalcStartParameterSet containing all parameters for the calculation
            var calcStartParameterSet = CreateCalcParametersFromJsonCalcSpec(sim, jcs, calcObjectReference, _calculationProfiler, preserveLogfile: true);

            // initialize logfile and log the calcspec
            var resultDirectory = new DirectoryInfo(jcs.OutputDirectory ?? throw new LPGException("Output directory was null."));
            InitLogger(resultDirectory);
            LogCalcSpec(jcs);

            // save settings to the database copy in the result directory
            SaveSettingsToDatabase(sim, jcs);

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
