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

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Database.Database;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace Database.Tables {
    public class CombinedSettings {

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceProfileHeaderMode DeviceProfileHeaderMode { get; set; }
    }
    public class GeneralConfig : DBBase {
        public const string TableName = "tblSettings";
        private TimeSpan _externalStepSize;
        private TimeSpan _internalStepSize;

        [NotNull] private Dictionary<string, SingleSetting> _settings = new Dictionary<string, SingleSetting>();

        public GeneralConfig([NotNull] string connectionString) : base("GeneralConfig", TableName, connectionString, "E5E37859-2ECB-4F62-A164-3ADD39300225".ToStrGuid()) {
            Options = new Dictionary<CalcOption, SingleOption>();
        }
        public int CarpetPlotWidth {
            get {
                var success = int.TryParse(_settings[nameof(CarpetPlotWidth)].SettingValue, out var i);
                if (!success) {
                    i = 5;
                }
                return i;
            }
            set => UpdateValue(nameof(CarpetPlotWidth), value.ToString(CultureInfo.InvariantCulture));
        }

        [NotNull]
        public new string ConnectionString => base.ConnectionString;

        [NotNull]
        [UsedImplicitly]
        public string CSVCharacter {
            get => _settings[nameof(CSVCharacter)].SettingValue;
            set => UpdateValue(nameof(CSVCharacter), value);
        }
        [NotNull]
        [UsedImplicitly]
        public CombinedSettings CombinedSettings {
            get => JsonConvert.DeserializeObject<CombinedSettings>(_settings[nameof(CombinedSettings)].SettingValue);
            set => UpdateValue(nameof(CombinedSettings), JsonConvert.SerializeObject(value, Formatting.Indented));
        }


        private void UpdateJsonSettings([NotNull] CombinedSettings settings)
        {
            UpdateValue(nameof(CombinedSettings), JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
        [UsedImplicitly]
        public DeviceProfileHeaderMode DeviceProfileHeaderMode {
            get => CombinedSettings.DeviceProfileHeaderMode;

            set {
                var settings = CombinedSettings;
                settings.DeviceProfileHeaderMode = value;
                UpdateJsonSettings(settings);
            }
        }

        [UsedImplicitly]
        public bool DeleteDatFilesBool => DeleteDatFiles=="TRUE";

        [NotNull]
        [UsedImplicitly]
        public string DeleteDatFiles {
            get => _settings[nameof(DeleteDatFiles)].SettingValue;
            set => UpdateValue(nameof(DeleteDatFiles), value);
        }

        [NotNull]
        [UsedImplicitly]
        public string DestinationPath {
            get => _settings[nameof(DestinationPath)].SettingValue;
            set => UpdateValue(nameof(DestinationPath), value);
        }

        [UsedImplicitly]
        public DateTime EndDateDateTime {
            get {
                var success = DateTime.TryParse(EndDateString, null, DateTimeStyles.RoundtripKind, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                return dt;
            }
            set => EndDateString = value.ToString("o", CultureInfo.InvariantCulture);
        }

        [NotNull]
        [UsedImplicitly]
        public string EndDateString {
            get => _settings["EndDate"].SettingValue;
            set {
                var success = DateTime.TryParse(value, null, DateTimeStyles.RoundtripKind, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                UpdateValue("EndDate", dt.ToString("o", CultureInfo.InvariantCulture));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string EndDateUIString {
            get => EndDateDateTime.ToString(CultureInfo.CurrentCulture);
            set {
                var success = DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                var newdt = new DateTime(dt.Year, dt.Month, dt.Day);
                OnPropertyChanged(nameof(EndDateUIString));
                OnPropertyChanged(nameof(EndDateDateTime));
                UpdateValue("EndDate", newdt.ToString("o", CultureInfo.InvariantCulture));
            }
        }

        public TimeSpan ExternalStepSize => _externalStepSize;

        [NotNull]
        public string ExternalTimeResolution {
            get => _externalStepSize.ToString();
            set {
                var success = TimeSpan.TryParse(value, out _externalStepSize);
                if (!success) {
                    _externalStepSize = new TimeSpan(0, 1, 0);
                }
                UpdateValue(nameof(ExternalTimeResolution), _externalStepSize.ToString());
            }
        }

        public int GeographicLocation {
            get {
                if (int.TryParse(_settings[nameof(GeographicLocation)].SettingValue, out var i)) {
                    return i;
                }
                return -1;
            }
            set => UpdateValue(nameof(GeographicLocation), value.ToString(CultureInfo.InvariantCulture));
        }

        [NotNull]
        [UsedImplicitly]
        public string ImagePath {
            get => _settings[nameof(ImagePath)].SettingValue;
            set {
                var s = value;
                if (!s.Contains(":")) {
                    if(Config.StartPath == null ) {
                        throw new LPGException("Path was null");
                    }

                    s = Path.Combine(Config.StartPath, s);
                }
                UpdateValue(nameof(ImagePath), s);
            }
        }

        public void SaveEverything()
        {
            foreach (var val in _settings.Values) {
                val.EnableNeedsUpdate();
            }

            foreach (SingleOption option in Options.Values) {
                option.EnableNeedsUpdate();
            }
            SaveToDB();
        }

        public TimeSpan InternalStepSize => _internalStepSize;

        [NotNull]
        public string InternalTimeResolution {
            get => _settings[nameof(InternalTimeResolution)].SettingValue;
            set {
                var success = TimeSpan.TryParse(value, out _internalStepSize);
                if (!success) {
                    _internalStepSize = new TimeSpan(0, 1, 0);
                }
                UpdateValue(nameof(InternalTimeResolution), _internalStepSize.ToString());
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string LastSelectedCalcObject {
            get => _settings[nameof(LastSelectedCalcObject)].SettingValue;
            set => UpdateValue(nameof(LastSelectedCalcObject), value);
        }

        [NotNull]
        [UsedImplicitly]
        public string LastSelectedTransportationSetting
        {
            get => _settings[nameof(LastSelectedTransportationSetting)].SettingValue;
            set => UpdateValue(nameof(LastSelectedTransportationSetting), value);
        }
        [CanBeNull]
        [UsedImplicitly]
        public string LastSelectedTransportationDeviceSet
        {
            get => _settings[nameof(LastSelectedTransportationDeviceSet)].SettingValue;
            set => UpdateValue(nameof(LastSelectedTransportationDeviceSet), value);
        }

        [CanBeNull]
        [UsedImplicitly]
        public string LastSelectedRouteSet
        {
            get => _settings[nameof(LastSelectedRouteSet)].SettingValue;
            set => UpdateValue(nameof(LastSelectedRouteSet),value);
        }

        [CanBeNull]
        [UsedImplicitly]
        public string LastSelectedChargingStationSet
        {
            get => _settings[nameof(LastSelectedChargingStationSet)].SettingValue;
            set => UpdateValue(nameof(LastSelectedChargingStationSet),value);
        }
        [NotNull]
        [UsedImplicitly]
        public string LastSelectedCalcType {
            get => _settings[nameof(LastSelectedCalcType)].SettingValue;
            set => UpdateValue(nameof(LastSelectedCalcType), value);
        }

        [NotNull]
        public Dictionary<CalcOption, SingleOption> Options { get; private set; }

        [NotNull]
        [UsedImplicitly]
        public string PerformCleanUpChecks {
            get => _settings[nameof(PerformCleanUpChecks)].SettingValue;
            set => UpdateValue(nameof(PerformCleanUpChecks), value);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool")]
        [UsedImplicitly]
        public bool PerformCleanUpChecksBool => _settings[nameof(PerformCleanUpChecks)].SettingValue
                                                    .ToUpperInvariant() == "TRUE";

        public int RandomSeed {
            get {
                var success = int.TryParse(_settings[nameof(RandomSeed)].SettingValue, out var i);
                if (!success) {
                    i = 5;
                }
                return i;
            }
            set => UpdateValue(nameof(RandomSeed), value.ToString(CultureInfo.InvariantCulture));
        }

        public int RepetitionCount {
            get {
                var success = int.TryParse(_settings[nameof(RepetitionCount)].SettingValue, out var repetitionCount);
                if (!success) {
                    repetitionCount = 2;
                }
                return repetitionCount;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string RepetitionCountString {
            get => _settings[nameof(RepetitionCount)].SettingValue;
            set {
                var success = int.TryParse(value, out var repetitionCount);
                if (!success) {
                    repetitionCount = 2;
                }
                UpdateValue(nameof(RepetitionCount), repetitionCount.ToString(CultureInfo.InvariantCulture));
            }
        }

        public EnergyIntensityType SelectedEnergyIntensity {
            get {
                if (int.TryParse(_settings[nameof(SelectedEnergyIntensity)].SettingValue, out var i)) {
                    return (EnergyIntensityType) i;
                }
                return EnergyIntensityType.AsOriginal;
            }
            set => UpdateValue(nameof(SelectedEnergyIntensity), ((int) value).ToString(CultureInfo.InvariantCulture));
        }

        [UsedImplicitly]
        public LoadTypePriority SelectedLoadTypePriority {
            get => Utili.ParseStringToEnum(_settings[nameof(SelectedLoadTypePriority)].SettingValue,
                (LoadTypePriority) (-1));
            set => UpdateValue(nameof(SelectedLoadTypePriority), value.ToString());
        }

        public int SelectedTemperatureProfile {
            get {
                if (int.TryParse(_settings[nameof(SelectedTemperatureProfile)].SettingValue, out var i)) {
                    return i;
                }
                return -1;
            }
            set => UpdateValue(nameof(SelectedTemperatureProfile), value.ToString(CultureInfo.InvariantCulture));
        }

        [NotNull]
        [UsedImplicitly]
        public string ShowSettlingPeriod {
            get => _settings[nameof(ShowSettlingPeriod)].SettingValue;
            set => UpdateValue(nameof(ShowSettlingPeriod), value);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool")]
        public bool ShowSettlingPeriodBool {
            get {
                if (_settings[nameof(ShowSettlingPeriod)].SettingValue.ToLower(CultureInfo.CurrentCulture) ==
                    "true") {
                    return true;
                }
                return false;
            }
            set => UpdateValue(nameof(ShowSettlingPeriod), value.ToString());
        }

        [UsedImplicitly]
        public DateTime StartDateDateTime {
            get {
                var success = DateTime.TryParse(StartDateString, null, DateTimeStyles.RoundtripKind, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                return dt;
            }
            set => StartDateString = value.ToString("o", CultureInfo.InvariantCulture);
        }

        [NotNull]
        [UsedImplicitly]
        public string StartDateString {
            get => _settings["StartDate"].SettingValue;
            set {
                var success = DateTime.TryParse(value, null, DateTimeStyles.RoundtripKind, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                UpdateValue("StartDate", dt.ToString("o", CultureInfo.InvariantCulture));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string StartDateUIString {
            get => StartDateDateTime.ToString(CultureInfo.CurrentCulture);
            set {
                var success = DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                var newdt = new DateTime(dt.Year, dt.Month, dt.Day);
                OnPropertyChanged(nameof(StartDateUIString));
                OnPropertyChanged(nameof(StartDateDateTime));
                UpdateValue("StartDate", newdt.ToString("o", CultureInfo.InvariantCulture));
            }
        }

        public int TimeStepsPerHour => (int) (60.0 * 60.0 / _internalStepSize.TotalSeconds);

        [NotNull]
        [UsedImplicitly]
        public string WriteExcelColumn {
            get => _settings[nameof(WriteExcelColumn)].SettingValue;
            set => UpdateValue(nameof(WriteExcelColumn), value);
        }

        public bool WriteExcelColumnBool => WriteExcelColumn.ToUpper() == "TRUE";


        [NotNull]
        [UsedImplicitly]
        public string EnableIdlemode
        {
            get => _settings[nameof(EnableIdlemode)].SettingValue;
            set => UpdateValue(nameof(EnableIdlemode), value);
        }

        public bool EnableIdlemodeBool {
            get => EnableIdlemode.ToUpper() == "TRUE";
            set => EnableIdlemode = value.ToString().ToUpper();
        }

        [NotNull]
        public List<CalcOption> AllEnabledOptions() {
            var options = new List<CalcOption>();
            foreach (var pair in Options) {
                if (pair.Value.SettingValue) {
                    options.Add(pair.Key);
                }
            }
            return options;
        }

        public void ApplyOptionDefault(OutputFileDefault selectedOptionDefault) {
            var selected = OutputFileDefaultHelper.GetOptionsForDefault(selectedOptionDefault);
            foreach (var option in Options) {
                if (selected.Contains(option.Key)) {
                    option.Value.SettingValue = true;
                }
                else {
                    option.Value.SettingValue = false;
                }
            }
        }

        private void CheckExistence([NotNull] string key, [NotNull] string defaultvalue, bool ignoreMissing) {
            if (!_settings.ContainsKey(key)) {
                var ss = new SingleSetting(key, defaultvalue, ConnectionString,System.Guid.NewGuid().ToStrGuid());
                if(!ignoreMissing) {
                    ss.SaveToDB();
                }

                _settings.Add(ss.Name, ss);
            }
        }

        public void Disable(CalcOption option) {
            Options[option].SettingValue = false;
            Options[option].SaveToDB();
        }

        public void Enable(CalcOption option) {
            Options[option].SettingValue = true;
            Options[option].SaveToDB();
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }
        /*
        public bool IsSet(CalcOption option) {
            if (Options[option].SettingValue) {
                return true;
            }
            return false;
        }*/

        [NotNull]
        public static GeneralConfig LoadFromDatabase([NotNull] string connectionString, bool ignoreMissing) {
            var gc = new GeneralConfig(connectionString)
            {
                _settings = SingleSetting.LoadFromDatabase(connectionString, ignoreMissing)
            };
            gc.CheckExistence(nameof(ImagePath), @"Images\", ignoreMissing);
            gc.CheckExistence(nameof(InternalTimeResolution), "00:00:10", ignoreMissing);
            gc.CheckExistence(nameof(ExternalTimeResolution), "00:00:10", ignoreMissing);
            gc.CheckExistence("StartDate", "01.01.2012", ignoreMissing);
            gc.CheckExistence("EndDate", "07.01.2012", ignoreMissing);
            gc.CheckExistence(nameof(DestinationPath), "c:\\work\\", ignoreMissing);
            gc.CheckExistence(nameof(LastSelectedCalcObject), string.Empty, ignoreMissing);
            gc.CheckExistence(nameof(GeographicLocation), "-1", ignoreMissing);
            gc.CheckExistence(nameof(RepetitionCount), "5", ignoreMissing);
            gc.CheckExistence(nameof(ShowSettlingPeriod), "false", ignoreMissing);
            gc.CheckExistence(nameof(RandomSeed), "-1", ignoreMissing);
            gc.CheckExistence(nameof(CSVCharacter), ";", ignoreMissing);
            gc.CheckExistence(nameof(SelectedTemperatureProfile), "1", ignoreMissing);
            gc.CheckExistence(nameof(LastSelectedCalcType), "Household", ignoreMissing);
            gc.CheckExistence(nameof(SelectedLoadTypePriority), "Mandatory", ignoreMissing);

            gc.CheckExistence(nameof(DeleteDatFiles), "False", ignoreMissing);
            gc.CheckExistence(nameof(CarpetPlotWidth), "7", ignoreMissing);
            gc.CheckExistence(nameof(SelectedEnergyIntensity), "0", ignoreMissing);
            gc.CheckExistence(nameof(WriteExcelColumn), "True", ignoreMissing);

            gc.CheckExistence(nameof(PerformCleanUpChecks), "True", ignoreMissing);
            gc.CheckExistence(nameof(EnableIdlemode), "True", ignoreMissing);

            gc.CheckExistence(nameof(LastSelectedTransportationSetting), "", ignoreMissing);
            gc.CheckExistence(nameof(LastSelectedTransportationDeviceSet), "", ignoreMissing);
            gc.CheckExistence(nameof(LastSelectedRouteSet), "", ignoreMissing);
            gc.CheckExistence(nameof(LastSelectedChargingStationSet), "", ignoreMissing);
            gc.CheckExistence(nameof(CombinedSettings), JsonConvert.SerializeObject(new CombinedSettings()), ignoreMissing);

            var success = TimeSpan.TryParse(gc._settings[nameof(InternalTimeResolution)].SettingValue,
                out gc._internalStepSize);
            if (!success) {
                gc._internalStepSize = new TimeSpan(0, 1, 0);
            }
            success = TimeSpan.TryParse(gc._settings[nameof(ExternalTimeResolution)].SettingValue,
                out gc._externalStepSize);
            if (!success) {
                gc._internalStepSize = new TimeSpan(0, 1, 0);
            }
            // init options and save all missing ones to db
            gc.Options = SingleOption.LoadFromDatabase(connectionString, ignoreMissing);
            if (!ignoreMissing)
            {
                var enumOptions = Enum.GetValues(typeof(CalcOption)).Cast<CalcOption>().ToList();
                foreach (var o in enumOptions) {
                    if (!gc.Options.ContainsKey(o)) {
                        var so = new SingleOption(o, false, connectionString, System.Guid.NewGuid().ToStrGuid());
                        so.SaveToDB();
                        gc.Options.Add(o, so);
                    }
                }
            }

            return gc;
        }

        public override void SaveToDB() {
            foreach (var singleSetting in _settings) {
                singleSetting.Value.SaveToDB();
            }

            foreach (var option in Options) {
                option.Value.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            throw new LPGException("Not Implemented!");
        }

        private void UpdateValue([NotNull] string key, [CanBeNull] string value) {
            if(value == null) {
                return;
            }
            if (_settings[key].SettingValue == value) {
                return;
            }
            _settings[key].SettingValue = value;
            _settings[key].SaveToDB();
            OnPropertyChanged(key);
        }
    }
}