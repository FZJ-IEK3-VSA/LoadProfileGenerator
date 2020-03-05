using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Database;
using Database.Tables;
using Database.Tests;
using JetBrains.Annotations;

namespace ChartCreator2.Tests.Oxyplot
{
    internal class OxyCalculationSetup
    {
        [NotNull]
        private readonly string _directoryName;
        public OxyCalculationSetup([NotNull] string directoryName)
        {
            _directoryName = directoryName;
            string dstDirName = directoryName;
            //}
            _wd = new WorkingDir(dstDirName);
            DstDir = Wd.WorkingDirectory;
        }

        [CanBeNull] private DatabaseSetup _db;
        [NotNull]
        private readonly WorkingDir _wd;

        [NotNull]
        public string DstDir { get; }

        [NotNull]
        public WorkingDir Wd => _wd;

        public void CleanUp(int acceptableLeftoverFileCount = 0)
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
            _db?.Cleanup();
            Wd.CleanUp(acceptableLeftoverFileCount);
        }

        [NotNull]
        public FileFactoryAndTracker GetFileTracker()
        {
            if (Wd == null)
            {
                throw new LPGException("_wd was null");
            }
            var fft = new FileFactoryAndTracker(Wd.WorkingDirectory, "OxyplotTest", Wd.InputDataLogger);
            fft.ReadExistingFilesFromSql();
            return fft;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "filename")]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
#pragma warning disable RCS1163 // Unused parameter.
        public static void CopyImage([NotNull] string filename)
#pragma warning restore RCS1163 // Unused parameter.
        {
            /* if (!_copyImage)
                            return;
                       if (System.Environment.MachineName.ToLower() != "i5")
                            return;
                        const string dstpath = @"c:\work\UnitTestImages\";
                        if (!Directory.Exists(dstpath))
                            Directory.CreateDirectory(dstpath);
                        FileInfo fi = new FileInfo(filename);
                        string dstName = Path.Combine(dstpath, fi.Name);
                        if (File.Exists(dstName))
                            File.Delete(dstName);
                        File.Copy(filename, dstName);
                        // Thread.Sleep(1000);
                        Process.Start(dstName);*/
        }

        private static bool OpenTabFunc([CanBeNull] object o) => true;

        private static bool ReportCancelFunc() => true;

        private bool ReportFinishFuncForHouseAndSettlement( bool a2, [CanBeNull] string a3,
            [CanBeNull][ItemCanBeNull] ObservableCollection<ResultFileEntry> a4) => true;

        private bool ReportFinishFuncForHousehold( bool a2, [CanBeNull] string a3, [CanBeNull] string a4)
        {
            return true;
        }

        private bool SetCalculationEntries([NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries) => true;

        //private ObservableCollection<CalculationEntry> _calculationEntries;

#pragma warning disable RCS1141 // Add parameter to documentation comment.
        /// <summary>
        ///     calc year is 2012
        /// </summary>
        [CanBeNull]
        public CalcDataRepository StartHousehold(int householdNumber, [NotNull] string csvCharacter,

            LoadTypePriority priority = LoadTypePriority.Mandatory, [CanBeNull] DateTime? enddate = null,
            [CanBeNull] Action<GeneralConfig> configSetter = null,
            EnergyIntensityType energyIntensity = EnergyIntensityType.EnergyIntensive, bool useHouse = false)
#pragma warning restore RCS1141 // Add parameter to documentation comment.
        {
            Config.IsInUnitTesting = true;
            //string dstDirName;// = "Household" + householdNumber;
            //if (useHouse) {
            //  dstDirName = "House" + householdNumber;
            //}
            //if (directoryName != null) {

            _db = new DatabaseSetup("CalcStarterTests." + _directoryName, DatabaseSetup.TestPackage.ChartCreator);
            var sim = new Simulator(_db.ConnectionString) {MyGeneralConfig = {StartDateDateTime = new DateTime(2012, 1, 1), EndDateDateTime = new DateTime(2012, 1, 31)}};
            if (enddate != null)
            {
                sim.MyGeneralConfig.EndDateDateTime = enddate.Value;
            }
            sim.MyGeneralConfig.RandomSeed = -1;
            sim.MyGeneralConfig.CSVCharacter = csvCharacter;
            sim.MyGeneralConfig.ExternalTimeResolution = "00:15:00";
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            configSetter?.Invoke(sim.MyGeneralConfig);
            Logger.Info("Enabled options are:");
            foreach (var option in sim.MyGeneralConfig.AllEnabledOptions())
            {
                Logger.Info(option.ToString());
            }
            Logger.Info("External time resolution is: " + sim.MyGeneralConfig.ExternalTimeResolution);

            var cs = new CalcStarter(sim);
            Logger.Info("Number of modular households:" + sim.ModularHouseholds.MyItems.Count);
            if (sim.ModularHouseholds.MyItems.Count <= householdNumber)
            {
                return null;
            }
            if (
                sim.ModularHouseholds[householdNumber].Description.ToLower(CultureInfo.CurrentCulture)
                    .StartsWith("only for modular", StringComparison.Ordinal))
            {
                return null;
            }
            var lpgVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string workingDir = Wd.WorkingDirectory;
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            if (useHouse)
            {
                var house = sim.Houses.It[householdNumber];

                Logger.Info("CHH Device selection:" + house.Name);

                var cspsHouse = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                    ReportFinishFuncForHousehold, OpenTabFunc, null, sim.GeographicLocations[0],
                    sim.TemperatureProfiles[0], house, SetCalculationEntries, energyIntensity, ReportCancelFunc, false,
                    lpgVersion,null, priority, null, null,sim.MyGeneralConfig.AllEnabledOptions(),
                    sim.MyGeneralConfig.StartDateDateTime,sim.MyGeneralConfig.EndDateDateTime,sim.MyGeneralConfig.InternalStepSize,
                    ";",-1,new TimeSpan(0,15,0),false,false,false,3, 3,calculationProfiler,null,null,
                    DeviceProfileHeaderMode.Standard,false);
                var duration = cspsHouse.OfficialSimulationEndTime - cspsHouse.OfficialSimulationStartTime;
                if (duration.TotalDays > 370) {
                    throw new LPGException("Trying to test with more than 1 year");
                }
                cs.Start(cspsHouse, workingDir);
                return null;
            }
            var chh = sim.ModularHouseholds[householdNumber];

            Logger.Info("Modular Household Device selection:" + chh.DeviceSelection?.Name);
            var csps = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                 ReportFinishFuncForHousehold, OpenTabFunc, null, sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], chh, SetCalculationEntries, energyIntensity, ReportCancelFunc, false,
                lpgVersion, null, priority, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                sim.MyGeneralConfig.StartDateDateTime, sim.MyGeneralConfig.EndDateDateTime, sim.MyGeneralConfig.InternalStepSize,
                ";", -1, new TimeSpan(0, 15, 0),false,false,false,3,3,calculationProfiler,null,null,
                 DeviceProfileHeaderMode.Standard,false);
            var simduration = csps.OfficialSimulationEndTime - csps.OfficialSimulationStartTime;
            if (simduration.TotalDays > 370)
            {
                throw new LPGException("Trying to test with more than 1 year");
            }
            cs.Start(csps, workingDir);
            CalcDataRepository cdr = new CalcDataRepository(Wd.SqlResultLoggingService);
            //sim.ModularHouseholds[householdNumber].Name
            return cdr;
        }

        [NotNull]
        public ResultFileEntry GetRfeByFilename([NotNull] string filename)
        {
            ResultFileEntryLogger rfel = new ResultFileEntryLogger(_wd.SqlResultLoggingService);
            var rfes = rfel.Load();
            return rfes.Single(x => x.FileName == filename);
        }
    }
}