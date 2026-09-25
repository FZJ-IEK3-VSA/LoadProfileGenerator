//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using Autofac;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using ChartCreator2;
using Common;
using Common.Enums;
using Common.JSON;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CalculationController.Queue
{
    public interface ILPGDispatcher
    {
        void BeginInvoke(Delegate method, object arg);

        bool IsCorrectThread();
    }

    /// <summary>
    /// Contains all parameters and objects that are required for the simulation,
    /// some of which depend on the specific house/household to simulate.
    /// </summary>
    public class CalcStartParameterSet
    {
        /// <summary>
        /// The CalcObject and all parameters depending on it.
        /// </summary>
        public CalcObjectParameters CalcObjectParams { get; }

        /// <summary>
        /// The common simulation parameters shared with other CalcObjects.
        /// </summary>
        public CalcParameters CalcParams { get; }

        /// <summary>
        /// Objects and callbacks for handling various tasks during the simulation.
        /// </summary>
        public CalculationHelpers Helpers { get; }

        /// <summary>
        /// Optional transportation objects. Only used by the GUI if a single household is simulated with
        /// transportation. Houses store dedicated transport objects for each contained household instead.
        /// </summary>
        public TransportObjects? Transport { get; }

        /// <summary>
        /// The seed for initializing the Random object for the simulation. Already contains the final
        /// seed to use. If, e.g., -1 was specified in the input, this returns a randomly chosen seed.
        /// </summary>
        public int RandomSeed { get; }

        public bool ResumeSettlement { get; }

        /// <summary>
        /// If true, keeps any log files from previous simulations while clearing the output directory.
        /// </summary>
        public bool PreserveLogfileWhileClearingFolder { get; }

        /// <summary>
        /// The version of the LoadProfileGenerator
        /// </summary>
        public string LPGVersion { get; } = Utili.GetCurrentAssemblyVersion();

        public CalcStartParameterSet(CalcObjectParameters objectsForCalc, CalcParameters parameters, CalculationHelpers helpers, int? userSelectedRandomSeed,
            TransportObjects? transport = null, bool resumeSettlement = false, bool preserveLogfile = false)
        {
            CalcObjectParams = objectsForCalc;
            CalcParams = parameters;
            Helpers = helpers;
            Transport = transport;
            RandomSeed = CalcParameters.GetActualRandomSeed(userSelectedRandomSeed, false);
            ResumeSettlement = resumeSettlement;
            PreserveLogfileWhileClearingFolder = preserveLogfile;

            if (TransportationEnabled && CalcTarget.CalcObjectType == CalcObjectType.ModularHousehold && Transport is null)
            {
                // when directly simulating a modular household without a house, transport parameters must be provided here
                throw new LPGException("Simulating a household with transportation enabled, but no transportation parameters were provided.");
            }

            EnableRequiredCalcOptions();
        }

        /// <summary>
        ///     starter for real calcs
        /// </summary>
        public CalcStartParameterSet(
            Func<bool, string, ObservableCollection<ResultFileEntry>?, bool> reportFinishFuncForHouseAndSettlement,
            Func<bool, string, string, bool>? reportFinishFuncForHousehold,
            Func<object, bool>? openTabFunc, ILPGDispatcher? dispatcher,
            GeographicLocation geographicLocation,
            TemperatureProfile temperatureProfile,
            ICalcObject calcTarget,
            EnergyIntensityType energyIntensity, [NotNull] Func<bool>? reportCancelFunc, bool resumeSettlement,
            DeviceSelection? deviceSelection, LoadTypePriority loadTypePriority,
            TransportationDeviceSet? transportationDeviceSet, TravelRouteSet? travelRouteSet,
            [NotNull] List<CalcOption> calcOptions,
            DateTime officialSimulationStartTime,
            DateTime officialSimulationEndTime,
            TimeSpan internalTimeResolution,
            [NotNull] string csvCharacter,
            int selectedRandomSeed,
            TimeSpan externalTimeResolution, bool writeExcelColumn, bool showSettlingPeriod, int settlingDays,
            int affordanceRepetitionCount, [NotNull] CalculationProfiler calculationProfiler, [CanBeNull] ChargingStationSet? chargingStationSet,
            List<string>? loadTypesToProcess,
            DeviceProfileHeaderMode deviceProfileHeaderMode,
            bool ignorePreviousActivitiesWhenNeeded,
            string resultPath, bool transportationEnabled, bool enableIdlemode, string decimalSeperator,
            bool flexibilityEnabled, bool citySimulationEnabled = false)
            : this(
                 new CalcObjectParameters(calcTarget, resultPath, temperatureProfile, geographicLocation, energyIntensity, deviceSelection),
                 new CalcParameters(calcOptions, officialSimulationStartTime, officialSimulationEndTime, internalTimeResolution, csvCharacter, externalTimeResolution,
                    writeExcelColumn, showSettlingPeriod, settlingDays, affordanceRepetitionCount, loadTypesToProcess, loadTypePriority, deviceProfileHeaderMode, ignorePreviousActivitiesWhenNeeded,
                    transportationEnabled, enableIdlemode, decimalSeperator, flexibilityEnabled, citySimulationEnabled),
                 new CalculationHelpers(calculationProfiler, dispatcher, reportFinishFuncForHouseAndSettlement, reportFinishFuncForHousehold, openTabFunc, reportCancelFunc), selectedRandomSeed,
                 new TransportObjects(travelRouteSet, transportationDeviceSet, chargingStationSet), resumeSettlement)
        { }

        /// <summary>
        ///     starter for unit tests
        /// </summary>
        public CalcStartParameterSet(
            [NotNull] GeographicLocation geographicLocation,
            [NotNull] TemperatureProfile temperatureProfile,
            [NotNull] ICalcObject calcTarget,
            EnergyIntensityType energyIntensity,
            bool resumeSettlement,
            [CanBeNull] DeviceSelection deviceSelection,
            LoadTypePriority loadTypePriority,
            TransportationDeviceSet? transportationDeviceSet, ChargingStationSet? chargingStationSet,
            TravelRouteSet? travelRouteSet, [NotNull] List<CalcOption> calcOptions,
            DateTime officialSimulationStartTime,
            DateTime officialSimulationEndTime,
            TimeSpan internalTimeResolution,
            [NotNull] string csvCharacter,
            int selectedRandomSeed,
            TimeSpan externalTimeResolution, bool writeExcelColumn, bool showSettlingPeriod, int settlingDays,
            int affordanceRepetitionCount, [NotNull] CalculationProfiler calculationProfiler, string resultPath, bool transportationEnabled,
            bool enableIdlemode, [NotNull] string decimalSeperator, bool flexibilityEnabled)
            : this(null, null, null, null, geographicLocation, temperatureProfile, calcTarget, energyIntensity, null, resumeSettlement, deviceSelection, loadTypePriority, transportationDeviceSet, travelRouteSet, calcOptions, officialSimulationStartTime,
                officialSimulationEndTime, internalTimeResolution, csvCharacter, selectedRandomSeed, externalTimeResolution, writeExcelColumn, showSettlingPeriod, settlingDays, affordanceRepetitionCount, calculationProfiler, chargingStationSet,
                [], DeviceProfileHeaderMode.Standard, false, resultPath, transportationEnabled, enableIdlemode, decimalSeperator, flexibilityEnabled)
        { }

        /// <summary>
        /// Enables CalcOptions that are required by the selected options, using dependencies of the Postprocessor and the ChartProcessor.
        /// </summary>
        private void EnableRequiredCalcOptions()
        {
            var fftd = new FileFactoryAndTrackerDummy();

            // check CalcOption dependencies from the ChartProcessor
            ChartProcessorManager.ChartingFunctionDependencySetter(ResultPath, CalculationProfiler, fftd, CalcOptions, false);

            // check CalcOption dependencies from the Postprocessor
            var container = PostProcessingManager.RegisterEverything(ResultPath, CalculationProfiler, fftd);
            using (var scope = container.BeginLifetimeScope())
            {
                var odm = scope.Resolve<OptionDependencyManager>();
                odm.EnableRequiredOptions(CalcOptions);
            }
        }


        // properties to directly access parameters, for compatibility with existing code
        public string ResultPath => CalcObjectParams.OutputDirectory;
        public CalculationProfiler? CalculationProfiler => Helpers.CalculationProfiler;
        public int AffordanceRepetitionCount => CalcParams.AffordanceRepetitionCount;
        public HashSet<CalcOption> CalcOptions => CalcParams.Options;
        public ICalcObject CalcTarget => CalcObjectParams.CalcObject;
        public string CsvCharacter => CalcParams.CSVCharacter;
        public string DecimalSeperator => CalcParams.DecimalSeperator;
        public DeviceSelection? DeviceSelection => CalcObjectParams.DeviceSelection;
        public ILPGDispatcher? Dispatcher => Helpers.Dispatcher;
        public EnergyIntensityType EnergyIntensity => CalcObjectParams.EnergyIntensity;
        public TimeSpan ExternalTimeResolution => CalcParams.ExternalStepsize;
        public GeographicLocation GeographicLocation => CalcObjectParams.GeographicLocation;
        public TimeSpan InternalTimeResolution => CalcParams.InternalStepsize;
        public LoadTypePriority LoadTypePriority => CalcParams.LoadTypePriority;
        // remark: CalcParameters adds one day to the passed end date, so this is one day later than the input end date
        public DateTime OfficialSimulationEndTime => CalcParams.OfficialEndTime;
        public DateTime OfficialSimulationStartTime => CalcParams.OfficialStartTime;
        public Func<object, bool>? OpenTabFunc => Helpers.OpenTabFunc;
        public Func<bool>? ReportCancelFunc => Helpers.ReportCancelFunc;
        public Func<bool, string, ObservableCollection<ResultFileEntry>, bool>? ReportFinishFuncForHouseAndSettlement
            => Helpers.ReportFinishFuncForHouseAndSettlement;
        public Func<bool, string, string, bool>? ReportFinishFuncForHousehold => Helpers.ReportFinishFuncForHousehold;
        public int SettlingDays => CalcParams.NumberOfSettlingDays;
        public bool ShowSettlingPeriod => CalcParams.ShowSettlingPeriodTime;
        public TemperatureProfile TemperatureProfile => CalcObjectParams.TemperatureProfile;
        public TransportationDeviceSet? TransportationDeviceSet => Transport?.TransportationDeviceSet;
        public TravelRouteSet? TravelRouteSet => Transport?.TravelRouteSet;
        public ChargingStationSet? ChargingStationSet => Transport?.ChargingStationSet;
        public bool WriteExcelColumn => CalcParams.WriteExcelColumn;
        public List<string> LoadTypesToProcess => CalcParams.LoadtypesToPostprocess;
        public DeviceProfileHeaderMode DeviceProfileHeaderMode => CalcParams.DeviceProfileHeaderMode;
        public bool IgnorePreviousActivitiesWhenNeeded => CalcParams.IgnorePreviousActivitesWhenNeeded;
        public bool TransportationEnabled => CalcParams.TransportationEnabled;
        public bool CitySimulationEnabled => CalcParams.CitySimulationEnabled;
        public bool EnableIdlemode => CalcParams.EnableIdlemode;
        public bool FlexibilityEnabled => CalcParams.FlexibilityEnabled;
    }


    /// <summary>
    /// Helper objects and functions for calculation.
    /// </summary>
    public record CalculationHelpers
    {
        public CalculationHelpers(CalculationProfiler? calculationProfiler = null, ILPGDispatcher? dispatcher = null,
        Func<bool, string, ObservableCollection<ResultFileEntry>, bool>? reportFinishFuncForHouseAndSettlement = null,
        Func<bool, string, string, bool>? reportFinishFuncForHousehold = null, Func<object, bool>? openTabFunc = null,
        Func<bool>? reportCancelFunc = null)
        {
            CalculationProfiler = calculationProfiler ?? new();
            Dispatcher = dispatcher;
            ReportFinishFuncForHouseAndSettlement = reportFinishFuncForHouseAndSettlement ?? ((_, _, _) => true);
            ReportFinishFuncForHousehold = reportFinishFuncForHousehold ?? ((_, _, _) => true);
            OpenTabFunc = openTabFunc ?? (_ => true);
            ReportCancelFunc = reportCancelFunc ?? (() => true);
        }

        public CalculationProfiler CalculationProfiler { get; }
        public ILPGDispatcher? Dispatcher { get; }
        public Func<bool, string, ObservableCollection<ResultFileEntry>, bool> ReportFinishFuncForHouseAndSettlement { get; }
        public Func<bool, string, string, bool> ReportFinishFuncForHousehold { get; }
        public Func<object, bool> OpenTabFunc { get; }
        public Func<bool> ReportCancelFunc { get; }
    }

    /// <summary>
    /// Set of parameters for calculation that depend on the CalcObject. Includes the main Calcobject (house or household) and
    /// all objects that can depend on the calcobject and might therefore differ.
    /// In a mass simulation, each house has its own CalcObjectParameters.
    /// </summary>
    public record CalcObjectParameters(ICalcObject CalcObject, string OutputDirectory, TemperatureProfile TemperatureProfile,
        GeographicLocation GeographicLocation, EnergyIntensityType EnergyIntensity, DeviceSelection? DeviceSelection = null);

    /// <summary>
    /// Objects for transport simulation. If transportation is enabled, all of them must be set.
    /// </summary>
    public record TransportObjects(TravelRouteSet TravelRouteSet, TransportationDeviceSet TransportationDeviceSet, ChargingStationSet ChargingStationSet);
}