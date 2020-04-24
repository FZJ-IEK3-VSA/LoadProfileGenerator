using System;
using System.Collections.ObjectModel;
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

namespace SimulationEngineLib.Calculation {
    internal class CommandLineCalculator {
        [NotNull] private readonly CalculationProfiler _calculationProfiler;

        public CommandLineCalculator() => _calculationProfiler = new CalculationProfiler();

        public bool Calculate([NotNull] CalculationOptions calculationOptions, [NotNull] string connectionString)
        {
            _calculationProfiler.StartPart("CommandLineCalculator");
            if (!calculationOptions.CheckSettings(connectionString)) {
                return false;
            }

            if (calculationOptions.ConnectionString == null) {
                throw new LPGException("Connection string was null");
            }

            if (calculationOptions.Testing) {
                return true;
            }

            if (calculationOptions.OutputDirectory == null) {
                throw new LPGException("Output Directory was null.");
            }

            var di = new DirectoryInfo(calculationOptions.OutputDirectory);
            Logger.LogFileIndex = "Startup." + AutomationUtili.CleanFileName(di.Name);
            Logger.Info("Loading...");
            var sim = new Simulator(calculationOptions.ConnectionString);
            Logger.Info("Loading finished.");
            StartHousehold(sim, calculationOptions);
            _calculationProfiler.StopPart("CommandLineCalculator");
            return true;
        }

        [NotNull]
        private static ICalcObject GetCalcObject([NotNull] Simulator sim, CalcObjectType calcObjectType, int householdNumber)
        {
            ICalcObject calcObject;
            switch (calcObjectType) {
                case CalcObjectType.ModularHousehold:
                    if (sim.ModularHouseholds.MyItems.Count <= householdNumber) {
                        throw new LPGException("Invalid modular household number");
                    }

                    calcObject = sim.ModularHouseholds[householdNumber];
                    // Logger.ImportantInfo("Number of modular households:" + sim.ModularHouseholds.MyItems.Count);
                    break;
                case CalcObjectType.House:
                    if (sim.Houses.MyItems.Count <= householdNumber) {
                        throw new LPGException("Invalid house number");
                    }

                    calcObject = sim.Houses[householdNumber];
                    break;
                case CalcObjectType.Settlement:
                    if (sim.Settlements.MyItems.Count <= householdNumber) {
                        throw new LPGException("Invalid settlement number");
                    }

                    calcObject = sim.Settlements[householdNumber];
                    break;
                default: throw new LPGException("Unknown calcObjectType");
            }

            return calcObject;
        }

        private static bool OpenTabFunc([NotNull] object o) => true;

        private static bool ReportCancelFunc() => true;

        private static bool ReportFinishFuncForHouseAndSettlement(bool a2,
                                                                  [NotNull] string a3,
                                                                  [ItemCanBeNull] [CanBeNull] ObservableCollection<ResultFileEntry> a4) => true;

        private static bool ReportFinishFuncForHousehold(bool a2, [NotNull] string a3, [NotNull] string path) => true;

        private static bool SetCalculationEntries([NotNull] [ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries) => true;

        private void StartHousehold([NotNull] Simulator sim, [NotNull] CalculationOptions cop)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            if (cop.CalcObjectNumber == null) {
                throw new LPGException("cop was null");
            }

            var calculationStartTime = DateTime.Now;

            var calcObject = GetCalcObject(sim, cop.CalcObjectType, (int)cop.CalcObjectNumber);
            if (cop.OutputDirectory == null) {
                throw new LPGException("Output directory was null");
            }

            var di = new DirectoryInfo(cop.OutputDirectory);
            var finishedFile = Path.Combine(di.FullName, Constants.FinishedFileFlag);
            if (Directory.Exists(di.FullName)) {
                if (cop.SkipExisting) {
                    if (File.Exists(finishedFile)) {
                        Logger.Error("Directory already exists and calculation is finished. Exiting.");
                        _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
                        return;
                    }
                }

                Logger.Warning("Directory already exists, but calculation is not finished or skip existing is not specified. Deleting folder.");
                var files = di.GetFiles();
                foreach (FileInfo file in files) {
                    if (file.Name.ToUpperInvariant() != "LOGFILE.TXT") {
                        file.Delete();
                    }
                }

                var directories = di.GetDirectories();
                foreach (DirectoryInfo info in directories) {
                    info.Delete(true);
                }

                Directory.Delete(di.FullName, true);
                Thread.Sleep(1000);
            }

            di.Create();
            string lfpath = Path.Combine(di.FullName, "Logfile.CommandlineCalculation.txt");
            Logger.Info("Setting logfile path: " + lfpath);
            Logger.SetLogFilePath(lfpath);
            Logger.LogToFile = true;
            Logger.Info("Directory: " + di.FullName);
            sim.MyGeneralConfig.StartDateUIString = cop.StartDate.ToString();
            sim.MyGeneralConfig.EndDateUIString = cop.EndDate.ToString();
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.DestinationPath = di.FullName;
            sim.MyGeneralConfig.ApplyOptionDefault(cop.OutputFileDefault);
            if (cop.ForceCharts) {
                sim.MyGeneralConfig.Enable(CalcOption.MakeGraphics);
            }

            if (cop.ForcePDF) {
                sim.MyGeneralConfig.Enable(CalcOption.MakePDF);
            }

            if (cop.DeleteDAT) {
                sim.MyGeneralConfig.DeleteDatFiles = "True";
            }
            else {
                sim.MyGeneralConfig.DeleteDatFiles = "False";
            }

            Config.IsInUnitTesting = true;
            if (cop.ExternalTimeResolution == null) {
                sim.MyGeneralConfig.ExternalTimeResolution = sim.MyGeneralConfig.InternalTimeResolution;
            }
            else {
                sim.MyGeneralConfig.ExternalTimeResolution = cop.ExternalTimeResolution;
                sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalEntireHouse);
                sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalIndividualHouseholds);
            }

            sim.MyGeneralConfig.RandomSeed = cop.RandomSeed;
            var eit = cop.EnergyIntensityType;
            if (eit == EnergyIntensityType.AsOriginal) {
                eit = calcObject.EnergyIntensityType;
            }

            if (cop.ExcelColumn) {
                sim.MyGeneralConfig.WriteExcelColumn = "True";
            }

            var cs = new CalcStarter(sim);
            if (cop.TemperatureProfileIndex == null) {
                throw new LPGException("Temperature profile index was null");
            }

            var temps = sim.TemperatureProfiles[(int)cop.TemperatureProfileIndex];
            if (cop.GeographicLocationIndex == null) {
                throw new LPGException("GeographicLocationIndex was null");
            }

            var geographicLocation = sim.GeographicLocations[(int)cop.GeographicLocationIndex];
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            DeviceSelection deviceSelection = null;
            if (!string.IsNullOrWhiteSpace(cop.DeviceSelectionName)) {
                deviceSelection = sim.DeviceSelections.FindFirstByName(cop.DeviceSelectionName);
                if (deviceSelection == null) {
                    throw new LPGException("Unknown device selection \"" + cop.DeviceSelectionName + "\"");
                }
            }

            if (cop.CalcOption != null) {
                foreach (CalcOption option in cop.CalcOption) {
                    Logger.Info("Enabling option " + option);
                    sim.MyGeneralConfig.Enable(option);
                }
            }

            TransportationDeviceSet transportationDeviceSet = null;
            if (cop.TransportationDeviceSetIndex != null) {
                transportationDeviceSet = sim.TransportationDeviceSets.It[cop.TransportationDeviceSetIndex.Value];
            }

            TravelRouteSet travelRouteSet = null;
            if (cop.TravelRouteSetIndex != null) {
                travelRouteSet = sim.TravelRouteSets.It[cop.TravelRouteSetIndex.Value];
            }

            ChargingStationSet chargingStationSet = null;
            if (cop.ChargingStationSetIndex != null) {
                chargingStationSet = sim.ChargingStationSets[cop.ChargingStationSetIndex.Value];
            }

            var options = sim.MyGeneralConfig.AllEnabledOptions();
            options.Add(CalcOption.OverallDats);
            var calcStartParameterSet = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                ReportFinishFuncForHousehold,
                OpenTabFunc,
                null,
                geographicLocation,
                temps,
                calcObject,
                SetCalculationEntries,
                eit,
                ReportCancelFunc,
                false,
                version,
                deviceSelection,
                cop.LoadTypePriority,
                transportationDeviceSet,
                travelRouteSet,
                options,
                sim.MyGeneralConfig.StartDateDateTime,
                sim.MyGeneralConfig.EndDateDateTime,
                sim.MyGeneralConfig.InternalStepSize,
                sim.MyGeneralConfig.CSVCharacter,
                cop.RandomSeed,
                sim.MyGeneralConfig.ExternalStepSize,
                sim.MyGeneralConfig.DeleteDatFilesBool,
                sim.MyGeneralConfig.WriteExcelColumnBool,
                sim.MyGeneralConfig.ShowSettlingPeriodBool,
                3,
                sim.MyGeneralConfig.RepetitionCount,
                _calculationProfiler,
                chargingStationSet,
                null,
                sim.MyGeneralConfig.DeviceProfileHeaderMode,
                false);

            cs.Start(calcStartParameterSet, cop.OutputDirectory);
            var duration = DateTime.Now - calculationStartTime;
            if (cop.DeleteAllButPDF) {
                var allFileInfos = di.GetFiles("*.*", SearchOption.AllDirectories);
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

            Logger.ImportantInfo("Calculation duration:" + duration);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            if (cop.MeasureCalculationTimes) {
                string targetfile = Path.Combine(di.FullName, Constants.CalculationProfilerJson);
                using (StreamWriter sw = new StreamWriter(targetfile)) {
                    _calculationProfiler.WriteJson(sw);
                    CalculationDurationFlameChart cdfc = new CalculationDurationFlameChart();
                    Thread t = new Thread(() => {
                        try {
                            cdfc.Run(_calculationProfiler, di.FullName, "CommandlineCalc");
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

            using (var sw = new StreamWriter(finishedFile)) {
                sw.WriteLine("Finished at " + DateTime.Now);
                sw.WriteLine("Duration in seconds:");
                sw.WriteLine(duration.TotalSeconds);
            }
        }
    }
}