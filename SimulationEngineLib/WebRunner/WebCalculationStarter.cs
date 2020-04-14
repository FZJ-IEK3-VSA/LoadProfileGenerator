using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using Common;
using Common.JSON;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace SimulationEngineLib.WebRunner
{
    internal static class WebCalculationStarter
    {
        public static void StartHousehold([NotNull] Simulator sim, [NotNull] ModularHousehold chh, [NotNull] string dstPathBase,
            [NotNull] FileReader.AspHousehold aspHh)
        {
            string dstPath = Path.Combine(dstPathBase, "Calc");

            DirectoryInfo di = new DirectoryInfo(dstPath);
            if (Directory.Exists(di.FullName))
            {
                Directory.Delete(di.FullName, true);
                Thread.Sleep(1000);
            }
            Logger.Info("Directory: " + di.FullName);
            DateTime now = DateTime.Now;

            sim.MyGeneralConfig.DestinationPath = di.FullName;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.OverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            Logger.LogToFile = true;
            Config.IsInUnitTesting = true;
            sim.MyGeneralConfig.ExternalTimeResolution = sim.MyGeneralConfig.InternalTimeResolution;
            sim.MyGeneralConfig.StartDateUIString = new DateTime(now.Year, 1, 1).ToString(CultureInfo.CurrentCulture);
            sim.MyGeneralConfig.EndDateUIString = new DateTime(now.Year, 12, 31).ToString(CultureInfo.CurrentCulture);
            EnergyIntensityType eit = aspHh.EnergyIntensity;

            CalcStarter cs = new CalcStarter(sim);
            Logger.Info("first temperature profile ID: " + sim.TemperatureProfiles.It.First().IntID);
            TemperatureProfile temps = sim.TemperatureProfiles.It.First(x => x.IntID == aspHh.TemperatureProfileID);
            GeographicLocation geographicLocation =
                sim.GeographicLocations.It.First(x => x.IntID == aspHh.GeographicLocationID);
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet calcStartParameterSet =
                new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                    ReportFinishFuncForHousehold, OpenTabFunc, null, geographicLocation, temps, chh,
                    SetCalculationEntries, eit, ReportCancelFunc, false, version, null, LoadTypePriority.Mandatory,null,null, sim.MyGeneralConfig.AllEnabledOptions(),
                     new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31),
                    new TimeSpan(0,1,0),";",-1,
                    new TimeSpan(0,1,0),false,false,false,3,3,calculationProfiler,null,null,
                    DeviceProfileHeaderMode.Standard,false);
            cs.Start(calcStartParameterSet, dstPath);
        }

        private static bool OpenTabFunc([CanBeNull] object o) => true;

        private static bool ReportCancelFunc() => true;

        private static bool ReportFinishFuncForHouseAndSettlement( bool a2, [CanBeNull] string a3,
            [ItemNotNull][CanBeNull] ObservableCollection<ResultFileEntry> a4) => true;

        private static bool ReportFinishFuncForHousehold(bool a2, [CanBeNull] string a3, [CanBeNull] string a4) => true;

        private static bool SetCalculationEntries([ItemNotNull] [NotNull] ObservableCollection<CalculationEntry> calculationEntries) => true;
    }
}