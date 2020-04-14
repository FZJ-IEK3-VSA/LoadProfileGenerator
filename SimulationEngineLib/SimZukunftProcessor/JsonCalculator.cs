using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using ChartCreator2.OxyCharts;
using Common;
using Common.Enums;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SimulationEngineLib.SimZukunftProcessor {
    public class JsonCalculator {
        [NotNull] private readonly CalculationProfiler _calculationProfiler;

        public JsonCalculator() => _calculationProfiler = new CalculationProfiler();
        /*
        public bool Calculate([NotNull] JsonDirectoryOptions calcDirectoryOptions)
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

        //public bool CheckBasicSettingsForLoadingDatabase([NotNull] JsonCalcSpecification jcs)
        //{
        //    if (!string.IsNullOrWhiteSpace(jcs.PathToDatabase)) {
        //        var fi = new FileInfo(jcs.PathToDatabase);
        //        if (!fi.Exists) {
        //            throw new LPGException("Database was not found. Path supplied:" + fi.FullName);
        //        }
        //    }
        //    return true;
        //}

        public bool CheckSimulator([NotNull] JsonCalcSpecification jcs, [NotNull] Simulator sim)
        {
            if (jcs.TemperatureProfile == null) {
                jcs.TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference();
            }

            if (jcs.GeographicLocation == null) {
                jcs.GeographicLocation = sim.GeographicLocations[0].GetJsonReference();
            }
            if (jcs.StartDate == null)
            {
                jcs.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            }

            if (jcs.EndDate == null)
            {
                jcs.EndDate = new DateTime(DateTime.Now.Year, 12, 31);
            }
            //jcs.LoadTypePriorityEnum = GetLoadtypePriority(jcs.LoadTypePriority,calcObject);

            return true;
        }

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void StartHousehold([NotNull] Simulator sim, [NotNull] JsonCalcSpecification jcs, [NotNull] JsonReference calcObjectReference)
        {
            if(!CheckSimulator(jcs, sim))
            {
                throw new LPGPBadParameterException("Invalid simulation state");
            }

            var calcObject = GetCalcObject(sim, calcObjectReference);
            if (jcs.OutputDirectory == null)
            {
                jcs.OutputDirectory = AutomationUtili.CleanFileName(calcObject.Name) + " - " + calcObject;
            }
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            if (calcObjectReference == null) {
                throw new LPGException("No calculation object was selected.");
            }
            var calculationStartTime = DateTime.Now;
            if (calcObject == null) {
                throw new LPGException("Could not find the Calc Object with the guid " + calcObjectReference.Guid);
            }

            var generalResultsDirectory = new DirectoryInfo(jcs.OutputDirectory ?? throw new LPGException("Output directory was null."));
            var finishedFile = Path.Combine(generalResultsDirectory.FullName, Constants.FinishedFileFlag);
            if (Directory.Exists(generalResultsDirectory.FullName)) {
                if (jcs.SkipExisting) {
                    if (File.Exists(finishedFile)) {
                        Logger.Error("Directory already exists and calculation is finished. Exiting.");
                        _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
                        return;
                    }
                }

                Logger.Warning("Directory already exists, but calculation is not finished or skip existing is not specified. Deleting folder.");
                var files = generalResultsDirectory.GetFiles();
                foreach (FileInfo file in files) {
                    if (file.Name.StartsWith("Log.")) {
                        continue;
                    }
                    if (file.Name.EndsWith(".db3"))
                    {
                        continue;
                    }
                    file.Delete();
                }

                var directories = generalResultsDirectory.GetDirectories();
                foreach (DirectoryInfo info in directories) {
                    info.Delete(true);
                }

                Thread.Sleep(1000);
            }

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
            sim.MyGeneralConfig.StartDateUIString = jcs.StartDate.ToString();
            sim.MyGeneralConfig.EndDateUIString = jcs.EndDate.ToString();
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.DestinationPath = generalResultsDirectory.FullName;
            sim.MyGeneralConfig.ApplyOptionDefault(jcs.DefaultForOutputFiles);
            if (jcs.CalcOptions != null) {
                foreach (var option in jcs.CalcOptions) {
                    //var option = option;
                    /*if (option == null) {
                        throw  new LPGException("Could not identify Calc Option " + option + ". Stopping.");
                    }*/
                    Logger.Info("Enabling option " + option);
                    sim.MyGeneralConfig.Enable(option);
                }
            }

            if (jcs.DeleteDAT) {
                sim.MyGeneralConfig.DeleteDatFiles = "TRUE";
            }
            else {
                sim.MyGeneralConfig.DeleteDatFiles = "FALSE";
            }

            if (jcs.ExternalTimeResolution == null) {
                sim.MyGeneralConfig.ExternalTimeResolution = sim.MyGeneralConfig.InternalTimeResolution;
            }
            else {
                sim.MyGeneralConfig.ExternalTimeResolution = jcs.ExternalTimeResolution;
            }

            sim.MyGeneralConfig.RandomSeed = jcs.RandomSeed;
            var eit = jcs.EnergyIntensityType;
            if (eit == EnergyIntensityType.AsOriginal) {
                eit = calcObject.EnergyIntensityType;
            }

            var cs = new CalcStarter(sim);
            var temperatureProfile = sim.TemperatureProfiles.FindByJsonReference(jcs.TemperatureProfile);
            if (temperatureProfile == null) {
                throw new LPGException("Temperature Profile not found.");
            }

            var geographicLocation = sim.GeographicLocations.FindByJsonReference(jcs.GeographicLocation);
            if (geographicLocation == null) {
                throw new LPGException("Geographic location not found.");
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            DeviceSelection deviceSelection = null;
            if (jcs.DeviceSelection != null) {
                deviceSelection = sim.DeviceSelections.FindByJsonReference(jcs.DeviceSelection);
                if (deviceSelection == null) {
                    throw new LPGException("Unknown device selection \"" + jcs.DeviceSelection.Guid + "\"");
                }
            }

            TransportationDeviceSet transportationDeviceSet = sim.TransportationDeviceSets.FindByJsonReference(jcs.TransportationDeviceSet);

            TravelRouteSet travelRouteSet = sim.TravelRouteSets.FindByJsonReference(jcs.TravelRouteSet);

            ChargingStationSet chargingStationSet = sim.ChargingStationSets.FindByJsonReference(jcs.ChargingStationSet);

            if (jcs.EnableTransportation) {
                if (transportationDeviceSet == null) {
                    throw new LPGException("Transportation device set was not set or could not be found.");
                }

                if (travelRouteSet == null) {
                    throw new LPGException("Travel route set was not set or could not be found.");
                }

                if (chargingStationSet == null) {
                    throw new LPGException("Charging Station set was not set or could not be found.");
                }
            }

            if (jcs.LoadTypePriority == LoadTypePriority.Undefined) {
                if (calcObject.CalcObjectType == CalcObjectType.ModularHousehold) {
                    jcs.LoadTypePriority = LoadTypePriority.RecommendedForHouseholds;
                }
                else {
                    jcs.LoadTypePriority = LoadTypePriority.RecommendedForHouses;
                }
            }

            var options = sim.MyGeneralConfig.AllEnabledOptions();
            options.Add(CalcOption.OverallDats);
            var calcStartParameterSet = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                ReportFinishFuncForHousehold,
                OpenTabFunc,
                null,
                geographicLocation,
                temperatureProfile,
                calcObject,
                SetCalculationEntries,
                eit,
                ReportCancelFunc,
                false,
                version,
                deviceSelection,
                jcs.LoadTypePriority,
                transportationDeviceSet,
                travelRouteSet,
                options,
                sim.MyGeneralConfig.StartDateDateTime,
                sim.MyGeneralConfig.EndDateDateTime,
                sim.MyGeneralConfig.InternalStepSize,
                sim.MyGeneralConfig.CSVCharacter,
                jcs.RandomSeed,
                sim.MyGeneralConfig.ExternalStepSize,
                sim.MyGeneralConfig.DeleteDatFilesBool,
                sim.MyGeneralConfig.WriteExcelColumnBool,
                sim.MyGeneralConfig.ShowSettlingPeriodBool,
                3,
                sim.MyGeneralConfig.RepetitionCount,
                _calculationProfiler,
                chargingStationSet,
                jcs.LoadtypesForPostprocessing,
                sim.MyGeneralConfig.DeviceProfileHeaderMode,
                jcs.IgnorePreviousActivitiesWhenNeeded);
            calcStartParameterSet.PreserveLogfileWhileClearingFolder = true;
            cs.Start(calcStartParameterSet, jcs.OutputDirectory);
            var duration = DateTime.Now - calculationStartTime;
            if (jcs.DeleteAllButPDF) {
                var allFileInfos = generalResultsDirectory.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var fi in allFileInfos) {
                    if (fi.Name.ToUpperInvariant().EndsWith(".PDF", StringComparison.Ordinal)) {
                        continue;
                    }

                    if (fi.Name.ToUpperInvariant().StartsWith("SUMPROFILES.", StringComparison.Ordinal)) {
                        continue;
                    }

                    if (fi.Name.ToUpperInvariant().StartsWith("HOUSEHOLDNAME.", StringComparison.Ordinal)) {
                        continue;
                    }

                    fi.Delete();
                }
            }

            if (jcs.DeleteSqlite) {
                var allFileInfos = generalResultsDirectory.GetFiles("*.sqlite", SearchOption.AllDirectories);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                foreach (var fi in allFileInfos) {
                    try {
                        fi.Delete();
                    }
                    catch (Exception ex) {
                        Logger.Exception(ex);
                    }
                }
            }

            Logger.ImportantInfo("Calculation duration:" + duration);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            if (jcs.CalcOptions != null && jcs.CalcOptions.Contains(CalcOption.CalculationFlameChart)) {
                string targetfile = Path.Combine(generalResultsDirectory.FullName, Constants.CalculationProfilerJson);
                using (StreamWriter sw = new StreamWriter(targetfile)) {
                    _calculationProfiler.WriteJson(sw);
                    CalculationDurationFlameChart cdfc = new CalculationDurationFlameChart();
                    Thread t = new Thread(() => {
                        try {
                            cdfc.Run(_calculationProfiler, generalResultsDirectory.FullName, "JsonCommandLineCalc");
                        }
                        catch (Exception ex) {
                            Logger.Exception(ex);
                        }
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();
                }
            }

            //cleanup empty directories
            var subdirs = generalResultsDirectory.GetDirectories();
            foreach (var subdir in subdirs) {
                var files = subdir.GetFiles();
                var subsubdirs = subdir.GetDirectories();
                if (files.Length == 0 && subsubdirs.Length == 0) {
                    subdir.Delete();
                }
            }

            using (var sw = new StreamWriter(finishedFile)) {
                sw.WriteLine("Finished at " + DateTime.Now);
                sw.WriteLine("Duration in seconds:");
                sw.WriteLine(duration.TotalSeconds);
            }
        }

        [NotNull]
        private static ICalcObject GetCalcObject([NotNull] Simulator sim, [CanBeNull] JsonReference calcReference)
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

        private static bool OpenTabFunc([NotNull] object o) => true;

        private static bool ReportCancelFunc() => true;

        private static bool ReportFinishFuncForHouseAndSettlement(bool a2,
                                                                  [NotNull] string a3,
                                                                  [ItemCanBeNull] [CanBeNull] ObservableCollection<ResultFileEntry> a4) => true;

        private static bool ReportFinishFuncForHousehold(bool a2, [NotNull] string a3, [NotNull] string path) => true;

        private static bool SetCalculationEntries([NotNull] [ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries) => true;
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
        private LoadTypePriority GetLoadtypePriority([CanBeNull] string jcsLoadTypePriority, [NotNull] ICalcObject calcObject)
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