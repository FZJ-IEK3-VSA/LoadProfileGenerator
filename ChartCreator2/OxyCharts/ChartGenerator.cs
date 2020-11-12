using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace ChartCreator2.OxyCharts {
    public enum FileProcessingResult {
        ShouldCreateFiles,
        NoFilesTocreate
    }


    public static class ChartMaker {
        public static void MakeFlameChart([NotNull] DirectoryInfo di, [NotNull] CalculationProfiler calculationProfiler)
        {
            string targetfile = Path.Combine(di.FullName, Constants.CalculationProfilerJson);
            using StreamWriter sw = new StreamWriter(targetfile);
            calculationProfiler.WriteJson(sw);
            CalculationDurationFlameChart cdfc = new CalculationDurationFlameChart();
            Thread t = new Thread(() => {
                try {
                    cdfc.Run(calculationProfiler, di.FullName, "CommandlineCalc");
                }
                catch (Exception ex) {
                    Logger.Exception(ex);
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        public static void MakeChartsAndPDF([NotNull] CalculationProfiler calculationProfiler, string resultPath)
        {
            try {
                SqlResultLoggingService srls = new SqlResultLoggingService(resultPath);
                CalcParameterLogger cpl = new CalcParameterLogger(srls);
                InputDataLogger idl = new InputDataLogger(Array.Empty<IDataSaverBase>());
                var calcParameters = cpl.Load();
                Logger.Info("Checking for charting parameters");
                if (!calcParameters.IsSet(CalcOption.MakePDF) && !calcParameters.IsSet(CalcOption.MakeGraphics)) {
                    Logger.Info("No charts wanted");
                    return;
                }

                calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Charting");
                using (FileFactoryAndTracker fileFactoryAndTracker = new FileFactoryAndTracker(resultPath, "Name", idl)) {
                    fileFactoryAndTracker.ReadExistingFilesFromSql();
                    calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Chart Generator RunAll");
                    try {
                        ChartCreationParameters ccp = new ChartCreationParameters(144, 1600, 1000, false, calcParameters.CSVCharacter,
                            new DirectoryInfo(resultPath));
                        var cg = new ChartGeneratorManager(calculationProfiler, fileFactoryAndTracker, ccp);
                        cg.Run(resultPath);
                    }
                    finally {
                        calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Chart Generator RunAll");
                    }

                    if (calcParameters.IsSet(CalcOption.MakePDF)) {
                        try {
                            calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - PDF Creation");
                            Logger.ImportantInfo("Creating the PDF. This will take a really long time without any progress report...");

                            //MigraPDFCreator mpc = new MigraPDFCreator(calculationProfiler);
                            //mpc.MakeDocument(resultPath, "", false, false, calcParameters.CSVCharacter, fileFactoryAndTracker);
                        }
                        finally {
                            calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - PDF Creation");
                        }
                    }
                }

                Logger.Info("Finished making the charts");

                // CalculationDurationFlameChart cdfc = new CalculationDurationFlameChart();
                //cdfc.Run(calculationProfiler, resultPath, "CalcManager");
            }
            catch (FileLoadException flex) {
                try {
                    ErrorReporter er = new ErrorReporter();
                    er.Run("Caught exception:" + flex.Message, flex.StackTrace);
                }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                catch (Exception) {
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                    // ignored
                }

                Logger.Error(
                    "Failed to enable the charting library due to missing dependencies on your computer. No chart creation is possible. Everything else worked though. The exact error message is: " +
                    flex.Message);
            }
            finally {
                calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Charting");
            }
        }

        //var t = new Thread(a);
// t.SetApartmentState(ApartmentState.STA);
        //t.Start();
        //t.Join();

    }

    public class ChartGeneratorManager {
        [NotNull] private readonly ICalculationProfiler _calculationProfiler;
        [NotNull] private readonly FileFactoryAndTracker _fft;

        [NotNull] private readonly ChartCreationParameters _chartCreationParameters;
        [NotNull] private readonly SqlResultLoggingService _srls;

        public ChartGeneratorManager([NotNull] ICalculationProfiler calculationProfiler, [NotNull] FileFactoryAndTracker fft,
                                     [NotNull] ChartCreationParameters chartCreationParameters)
        {
            _calculationProfiler = calculationProfiler;
            _fft = fft;
            _chartCreationParameters = chartCreationParameters;
            _srls = new SqlResultLoggingService(_chartCreationParameters.BaseDirectory.FullName);
        }

        public void Run([NotNull] string resultPath)
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - Post Processing");
            try {
                CalcDataRepository cdr = new CalcDataRepository(_srls);
                var builder = new ContainerBuilder();
                builder.Register(_ => new SqlResultLoggingService(resultPath)).As<SqlResultLoggingService>().SingleInstance();
                //builder.Register(c =>_logFile).As<ILogFile>().SingleInstance();
                builder.Register(_ => _calculationProfiler).As<ICalculationProfiler>().SingleInstance();
                builder.Register(_ => _fft).As<FileFactoryAndTracker>().SingleInstance();
                builder.Register(_ => _srls).As<SqlResultLoggingService>().SingleInstance();
                builder.Register(_ => _chartCreationParameters).As<ChartCreationParameters>().SingleInstance();
                builder.Register(_ => cdr.CalcParameters).As<CalcParameters>().SingleInstance();
                builder.RegisterType<ChartGenerator>().As<ChartGenerator>().SingleInstance();
                builder.RegisterType<CalcDataRepository>().As<CalcDataRepository>().SingleInstance();

                //general file processing steps
                builder.RegisterType<ActivityFrequenciesPerMinute>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<ActivityPercentage>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<AffordanceEnergyUse>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<AffordanceTaggingSet>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<AffordanceTimeUse>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<CriticalThresholdViolations>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<Desires>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<DeviceDurationCurves>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<DeviceProfiles>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<DeviceProfilesExternal>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<DeviceSums>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<DeviceTaggingSet>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<DurationCurve>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<ExecutedActionsOverviewCount>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<LocationStatistics>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<HouseholdPlan>().As<IChartMakerStep>().SingleInstance();
                //builder.RegisterType<SumProfiles>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<SumProfilesExternal>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<Temperatures>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<TimeOfUse>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<VariableLogFileChart>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<WeekdayProfiles>().As<IChartMakerStep>().SingleInstance();
                builder.RegisterType<NoOpChart>().As<IChartMakerStep>().SingleInstance();

                //sql steps
                builder.RegisterType<AffordanceEnergyUsePerPerson>().As<ChartBaseSqlStep>().SingleInstance();
                var container = builder.Build();
                using var scope = container.BeginLifetimeScope();
                ChartGenerator ps = scope.Resolve<ChartGenerator>();
                ps.RunAll();
            }
            finally {
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - Post Processing");
            }
        }
    }

    public class ChartCreationParameters {
        public int Dpi { get; }
        public int Height { get; }
        public int Width { get; }
        public bool ShowTitle { get; }

        public ChartCreationParameters(int dpi, int imgWidth, int imgHeight, bool showTitle, [NotNull] string csvCharacter,
                                       [NotNull] DirectoryInfo baseDirectory)
        {
            Dpi = dpi;
            CSVCharacter = csvCharacter;
            BaseDirectory = baseDirectory;
            Height = imgHeight;
            Width = imgWidth;
            ShowTitle = showTitle;
            _csvCharacterArr = new[] {csvCharacter};
        }

        [NotNull]
        public DirectoryInfo BaseDirectory { get; }

        [NotNull]
        public string CSVCharacter { get; }

        [ItemNotNull]
        [NotNull]
        public string[] CSVCharacterArr => _csvCharacterArr;

        public int PDFFontSize { get; } = 30;

        [NotNull] [ItemNotNull] private readonly string[] _csvCharacterArr;
    }

    public class ChartGenerator {
        [NotNull]
        public ChartCreationParameters GeneralParameters { get; }

        [NotNull] private readonly ICalculationProfiler _calculationProfiler;
        [NotNull] private readonly FileFactoryAndTracker _fft;
        [ItemNotNull] [NotNull] private readonly IChartMakerStep[] _chartMakerSteps;
        [ItemNotNull] [NotNull] private readonly ISqlChartMakerStep[] _sqlChartMakerSteps;
        [NotNull] private readonly SqlResultLoggingService _srls;
        [NotNull] private readonly CalcDataRepository _repository;

        public ChartGenerator([NotNull] ICalculationProfiler calculationProfiler, [NotNull] ChartCreationParameters generalParameters,
                              [NotNull] FileFactoryAndTracker fft, [ItemNotNull] [NotNull] IChartMakerStep[] chartMakerSteps,
                              [ItemNotNull] [NotNull] ISqlChartMakerStep[] sqlChartMakerSteps, [NotNull] SqlResultLoggingService srls,
                              [NotNull] CalcDataRepository repository)
        {
            GeneralParameters = generalParameters;
            _fft = fft;
            _chartMakerSteps = chartMakerSteps;
            _sqlChartMakerSteps = sqlChartMakerSteps;
            _srls = srls;
            _repository = repository;
            _calculationProfiler = calculationProfiler;
            HashSet<ResultFileID> fileIDs = new HashSet<ResultFileID>();
            foreach (var step in chartMakerSteps) {
                foreach (ResultFileID id in step.ResultFileIDs) {
                    if (!fileIDs.Add(id)) {
                        throw new LPGException("Duplicate file id added:" + id);
                    }
                }
            }

            HashSet<ResultTableID> tableIDs = new HashSet<ResultTableID>();
            foreach (var step in sqlChartMakerSteps) {
                foreach (var id in step.ValidResultTableIds) {
                    if (!tableIDs.Add(id)) {
                        throw new LPGException("Duplicate table id added:" + id);
                    }
                }
            }
        }

        /*
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        private FileProcessingResult ProcessFile([JetBrains.Annotations.NotNull] ResultFileEntry entry, DirectoryInfo basisPath, string csv,
            FileFactoryAndTracker fft, )
        {
            if (entry.LoadTypeInformation != null) {
                if (!entry.LoadTypeInformation.ShowInCharts) {
                    return FileProcessingResult.NoFilesTocreate; //discard everything where the loadtype shouldnt be shown.
                }
            }

            switch (entry.ResultFileID)
            {
                
                
                _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.DeviceProfileCSV);
                var dp = new DeviceProfiles(parameters);
                if (entry.LoadTypeInformation == null)
                {
                    throw new LPGException("Entry was null");
                }
                var fpr = dp.MakePlot(entry, "Device Profiles " + entry.HouseholdNumberString + " " + entry.LoadTypeInformation.Name, basisPath);
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.DeviceProfileCSV);
                return fpr;
                
                
                
                
         
                
                
                case ResultFileID.TimeOfUseEnergy:
                    _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.TimeOfUseEnergy);
                    var touEnergy = new TimeOfUse(parameters,true);
                    if (entry.LoadTypeInformation == null)
                    {
                        throw new LPGException("LoadTypeInformation was null");
                    }
                    touEnergy.MakePlot(entry, "Time of Energy Use " + entry.HouseholdNumberString + " " + entry.LoadTypeInformation.Name, basisPath);
                    _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.TimeOfUseEnergy);
                    return FileProcessingResult.ShouldCreateFiles;
               
               
        
                
               
                case ResultFileID.DeviceSumsPerMonth:
                    _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.DeviceSumsPerMonth);
                    var ds = new DeviceSums(parameters);
                    if (entry.LoadTypeInformation == null)
                    {
                        throw new LPGException("LoadTypeInformation was null");
                    }
                    ds.MakePlotMonthly(entry, "Device Sums Monthly" + entry.HouseholdNumberString + " " + entry.LoadTypeInformation.Name, basisPath);
                    _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.DeviceSumsPerMonth);
                    return FileProcessingResult.ShouldCreateFiles;
              
                case ResultFileID.SumProfileForHouseholds:
                    _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.SumProfileForHouseholds);
                    var sumProfileForHouseholds = new SumProfiles(parameters);
                    if (entry.LoadTypeInformation == null)
                    {
                        throw new LPGException("LoadTypeInformation was null");
                    }
                    sumProfileForHouseholds.MakePlot(entry, "Sum Profile for " + entry.HouseholdNumberString + " " + entry.LoadTypeInformation.Name, basisPath);
                    _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.SumProfileForHouseholds);
                    return FileProcessingResult.ShouldCreateFiles;
                case ResultFileID.TotalsPerHousehold:
                    return FileProcessingResult.NoFilesTocreate;
                case ResultFileID.ExternalSumsForHouseholds:
                    _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.ExternalSumsForHouseholds);
                    var sumProfilesExternal = new SumProfilesExternal(parameters);
                    if (entry.LoadTypeInformation == null)
                    {
                        throw new LPGException("LoadTypeInformation was null");
                    }
                    sumProfilesExternal.MakePlot(entry, "Sum Profile for " + entry.HouseholdNumberString + " " + entry.LoadTypeInformation.Name, basisPath);
                    _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass() + " - " + ResultFileID.ExternalSumsForHouseholds);
                    return FileProcessingResult.ShouldCreateFiles;
                
                default:
                    throw new LPGException("Failure to process file " + entry.ResultFileID + ": " + entry.FileName);
            }
        }*/

        public void RunAll()
        {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            try {
                List<ResultFileEntry> entriesToProcess = _fft.ResultFileList.ResultFiles.Values.ToList();
                if (entriesToProcess.Count == 0) {
                    throw new LPGException("Not a single file to process was found.");
                }

                foreach (var entry in entriesToProcess) {
                    var start = DateTime.Now;
                    int prevCount = _fft.ResultFileList.ResultFiles.Count;
                    // find the proper chart maker step, making sure only a single chart maker applies
                    var makerStep = _chartMakerSteps.FirstOrDefault(x => x.IsEnabled(entry));
                    if (makerStep == null) {
                        continue;
                    }

                    //make the chart
                    var result = makerStep.MakePlot(entry);
                    //var processingResult =  ProcessFile(entry, di, csvCharacter,fft);
                    int newcount = _fft.ResultFileList.ResultFiles.Count;
                    if (result == FileProcessingResult.ShouldCreateFiles && prevCount == newcount) {
                        throw new LPGException("No pictures created:" + Environment.NewLine + entry.ResultFileID + Environment.NewLine +
                                               entry.FullFileName);
                    }

                    if (entry.Name == null) {
                        throw new LPGException("Name was null");
                    }

                    Logger.Info("Chart for " + entry.Name.PadRight(60) + "(" +
                                (DateTime.Now - start).TotalSeconds.ToString("N1", CultureInfo.CurrentCulture) + "s)");
                }

                var databases = _srls.LoadDatabases();
                var householdKeys = _repository.HouseholdKeys;
                foreach (var db in databases) {
                    var tables = _srls.LoadTables(db.Key);
                    var hhentry = householdKeys.Single(x => x.HHKey == db.Key);
                    foreach (var table in tables) {
                        foreach (ISqlChartMakerStep chartMakerStep in _sqlChartMakerSteps) {
                            if (chartMakerStep.IsEnabled(hhentry, table)) {
                                chartMakerStep.MakePlot(hhentry);
                            }
                        }
                    }
                }
            }
            finally {
                _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            }
        }
    }
}

