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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace CalculationController.Queue {
    public interface ILPGDispatcher {
        //(System.Windows.Threading.DispatcherPriority priority, Delegate method, object arg)
        void BeginInvoke(Delegate method, object arg);

        bool IsCorrectThread();
    }


    public class CalcStartParameterSet {
        public DateTime CalculationStartTime { get; set; }
        //public const string TableName = "CalcStartParameterSet";

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
            [CanBeNull] TransportationDeviceSet transportationDeviceSet, [CanBeNull] ChargingStationSet chargingStationSet,
            [CanBeNull] TravelRouteSet travelRouteSet, [NotNull] List<CalcOption> calcOptions,
            DateTime officialSimulationStartTime,
            DateTime officialSimulationEndTime,
            TimeSpan internalTimeResolution,
            [NotNull] string csvCharacter,
            int selectedRandomSeed,
            TimeSpan externalTimeResolution, bool deleteDatFiles, bool writeExcelColumn, bool showSettlingPeriod,
            int settlingDays, int affordanceRepetitionCount, [NotNull] CalculationProfiler calculationProfiler, string resultPath,
            bool transportationEnabled, bool enableIdlemode)
        {
            OfficialSimulationStartTime = officialSimulationStartTime;
            OfficialSimulationEndTime = officialSimulationEndTime;
            InternalTimeResolution = internalTimeResolution;
            CsvCharacter = csvCharacter;
            SelectedRandomSeed = selectedRandomSeed;
            ExternalTimeResolution = externalTimeResolution;
            DeleteDatFiles = deleteDatFiles;
            WriteExcelColumn = writeExcelColumn;
            ShowSettlingPeriod = showSettlingPeriod;
            SettlingDays = settlingDays;
            AffordanceRepetitionCount = affordanceRepetitionCount;
            CalculationProfiler = calculationProfiler;
            GeographicLocation = geographicLocation;
            TemperatureProfile = temperatureProfile;
            CalcTarget = calcTarget;
            EnergyIntensity = energyIntensity;
            ResumeSettlement = resumeSettlement;
            LPGVersion = Utili.GetCurrentAssemblyVersion();
            DeviceSelection = deviceSelection;
            LoadTypePriority = loadTypePriority;
            TransportationDeviceSet = transportationDeviceSet;
            TravelRouteSet = travelRouteSet;
            CalcOptions = calcOptions;
            ChargingStationSet = chargingStationSet;
            DeviceProfileHeaderMode = DeviceProfileHeaderMode.Standard;
            ResultPath = resultPath;
            CalculationStartTime = DateTime.Now;
            TransportationEnabled = transportationEnabled;
            EnableIdlemode = enableIdlemode;
        }

        /// <summary>
        ///     starter for real calcs
        /// </summary>
        public CalcStartParameterSet(
            [NotNull] Func<bool, string, ObservableCollection<ResultFileEntry>, bool>
                reportFinishFuncForHouseAndSettlement,
            [NotNull] Func<bool, string, string, bool> reportFinishFuncForHousehold,
            [NotNull] Func<object, bool> openTabFunc, [CanBeNull] ILPGDispatcher dispatcher,
            [NotNull] GeographicLocation geographicLocation,
            [NotNull] TemperatureProfile temperatureProfile,
            [NotNull] ICalcObject calcTarget,
            EnergyIntensityType energyIntensity, [NotNull] Func<bool> reportCancelFunc, bool resumeSettlement,
            [CanBeNull] DeviceSelection deviceSelection, LoadTypePriority loadTypePriority,
            [CanBeNull] TransportationDeviceSet transportationDeviceSet, [CanBeNull] TravelRouteSet travelRouteSet,
            [NotNull] List<CalcOption> calcOptions,
            DateTime officialSimulationStartTime,
            DateTime officialSimulationEndTime,
            TimeSpan internalTimeResolution,
            [NotNull] string csvCharacter,
            int selectedRandomSeed,
            TimeSpan externalTimeResolution, bool deleteDatFiles, bool writeExcelColumn, bool showSettlingPeriod,
            int settlingDays, int affordanceRepetitionCount, [NotNull] CalculationProfiler calculationProfiler,
            [CanBeNull] ChargingStationSet chargingStationSet,
            [CanBeNull][ItemNotNull] List<string> loadTypesToProcess,
            DeviceProfileHeaderMode deviceProfileHeaderMode,
            bool ignorePreviousActivitiesWhenNeeded, string resultPath, bool transportationEnabled, bool enableIdlemode)
        {
            IgnorePreviousActivitiesWhenNeeded = ignorePreviousActivitiesWhenNeeded;
            ResultPath = resultPath;
            LoadTypesToProcess = loadTypesToProcess;
            ExternalTimeResolution = externalTimeResolution;
            DeleteDatFiles = deleteDatFiles;
            WriteExcelColumn = writeExcelColumn;
            ShowSettlingPeriod = showSettlingPeriod;
            SettlingDays = settlingDays;
            AffordanceRepetitionCount = affordanceRepetitionCount;
            CalculationProfiler = calculationProfiler;
            SelectedRandomSeed = selectedRandomSeed;
            OfficialSimulationStartTime = officialSimulationStartTime;
            OfficialSimulationEndTime = officialSimulationEndTime;
            InternalTimeResolution = internalTimeResolution;
            CsvCharacter = csvCharacter;
            ReportFinishFuncForHouseAndSettlement = reportFinishFuncForHouseAndSettlement;
            ReportFinishFuncForHousehold = reportFinishFuncForHousehold;
            OpenTabFunc = openTabFunc;
            Dispatcher = dispatcher;
            GeographicLocation = geographicLocation;
            TemperatureProfile = temperatureProfile;
            CalcTarget = calcTarget;
            EnergyIntensity = energyIntensity;
            ReportCancelFunc = reportCancelFunc;
            ResumeSettlement = resumeSettlement;
            LPGVersion = Utili.GetCurrentAssemblyVersion();
            DeviceSelection = deviceSelection;
            LoadTypePriority = loadTypePriority;
            TransportationDeviceSet = transportationDeviceSet;
            TravelRouteSet = travelRouteSet;
            CalcOptions = calcOptions;
            ChargingStationSet = chargingStationSet;
            DeviceProfileHeaderMode = deviceProfileHeaderMode;
            CalculationStartTime = DateTime.Now;
            TransportationEnabled = transportationEnabled;
            EnableIdlemode = enableIdlemode;
        }

        public string ResultPath { get; set; }

        [NotNull]
        public CalculationProfiler CalculationProfiler { get; }
        public int AffordanceRepetitionCount { get; }

        [NotNull]
        public List<CalcOption> CalcOptions { get; }

        [NotNull]
        public ICalcObject CalcTarget { get; }

        [NotNull]
        public string CsvCharacter { get; }

        public bool DeleteDatFiles { get; }

        [CanBeNull]
        public DeviceSelection DeviceSelection { get; }

        [CanBeNull]
        public ILPGDispatcher Dispatcher { get; }

        public EnergyIntensityType EnergyIntensity { get; }
        public TimeSpan ExternalTimeResolution { get; }

        [NotNull]
        public GeographicLocation GeographicLocation { get; }

        public TimeSpan InternalTimeResolution { get; }
        public LoadTypePriority LoadTypePriority { get; }

        [NotNull]
        public string LPGVersion { get; }

        public DateTime OfficialSimulationEndTime { get; }
        public DateTime OfficialSimulationStartTime { get; }

        [CanBeNull]
        public Func<object, bool> OpenTabFunc { get; }

        [CanBeNull]
        public Func<bool> ReportCancelFunc { get; }

        [CanBeNull]
        public Func< bool, string, ObservableCollection<ResultFileEntry>, bool>
            ReportFinishFuncForHouseAndSettlement { get; }

        [CanBeNull]
        public Func< bool, string, string, bool> ReportFinishFuncForHousehold { get; }

        public bool ResumeSettlement { get; }
        public int SelectedRandomSeed { get; }


        public int SettlingDays { get; }
        public bool ShowSettlingPeriod { get; }

        [NotNull]
        public TemperatureProfile TemperatureProfile { get; }

        [CanBeNull]
        public TransportationDeviceSet TransportationDeviceSet { get; }

        [CanBeNull]
        public TravelRouteSet TravelRouteSet { get; }

        public bool WriteExcelColumn { get; }
        [CanBeNull]
        public ChargingStationSet ChargingStationSet { get; set; }

        public bool PreserveLogfileWhileClearingFolder { get; set; }
        [CanBeNull]
        [ItemNotNull]
        public List<string> LoadTypesToProcess { get; set; }
        public DeviceProfileHeaderMode DeviceProfileHeaderMode { get;  }
        public bool IgnorePreviousActivitiesWhenNeeded { get; set; }
        public bool TransportationEnabled { get; set; }
        public bool EnableIdlemode { get; }

        public JsonCalcSpecification CalcSpec { get; set; }
    }
}