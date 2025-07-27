using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Automation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.JSON {
    public enum DeviceProfileHeaderMode {
        Standard,
        OnlyDeviceCategories
    }

    public static class DeviceProfileHeaderModeHelper {
        [NotNull]
        public static Dictionary<DeviceProfileHeaderMode, string> DeviceProfileHeaderModeDict { get; } = new Dictionary<DeviceProfileHeaderMode, string> {
            [DeviceProfileHeaderMode.Standard] = "Standard mode (Household - Location - Device [Unit])",
            [DeviceProfileHeaderMode.OnlyDeviceCategories] = "Reduced (Device Category [Unit])",
        };
    }

    /// <summary>
    /// Stores all general, CalcObject-independent parameters for a simulation. In a mass simulation,
    /// all house/household simulations use the same CalcParameters.
    /// </summary>
    public class CalcParameters
    {
        [JsonConstructor]
        private CalcParameters(DateTime startDate, DateTime endDate)
        {
            OfficialStartTime = startDate;
            OfficialEndTime = endDate.AddDays(1);
        }

        public CalcParameters(List<CalcOption> calcOptions, DateTime startDate, DateTime endDate, TimeSpan internalResolution, string cSVCharacter,
            TimeSpan externalResolution, bool writeExcelColumnBool, bool showSettlingPeriodBool, int settlingDays, int repetitionCount, List<string> loadtypesForPostprocessing,
            DeviceProfileHeaderMode deviceProfileHeaderMode, bool ignorePreviousActivitiesWhenNeeded, bool enableTransportation,
            bool enableIdlemode, string decimalSeperator, bool enableFlexibility, bool citySimulationEnabled = false) : this(startDate, endDate)
        {
            Options = calcOptions?.ToHashSet() ?? [];
            InternalStepsize = internalResolution;
            ExternalStepsize = externalResolution;
            CSVCharacter = cSVCharacter;
            WriteExcelColumn = writeExcelColumnBool;
            ShowSettlingPeriodTime = showSettlingPeriodBool;
            NumberOfSettlingDays = settlingDays;
            AffordanceRepetitionCount = repetitionCount;
            LoadtypesToPostprocess = loadtypesForPostprocessing;
            DeviceProfileHeaderMode = deviceProfileHeaderMode;
            IgnorePreviousActivitesWhenNeeded = ignorePreviousActivitiesWhenNeeded;
            TransportationEnabled = enableTransportation;
            EnableIdlemode = enableIdlemode;
            DecimalSeperator = decimalSeperator;
            FlexibilityEnabled = enableFlexibility;
            CitySimulationEnabled = citySimulationEnabled;

            InitializeTimeSteps();
            CheckSettings();
        }

        public static CalcParameters CreateDefaultParamsForTesting()
        {
            var cp = new CalcParameters(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31));
            cp.SetInternalTimeResolution(new TimeSpan(0, 1, 0)).SetExternalTimeResolution(new TimeSpan(0, 1, 0));
            cp.SetCsvCharacter(";");
            cp.SetLoadTypePriority(LoadTypePriority.RecommendedForHouses);
            cp.DisableShowSettlingPeriod();
            cp.SetSettlingDays(3);
            cp.SetWriteExcelColumn(false);
            cp.SetAffordanceRepetitionCount(3);
            cp.CheckSettings();
            return cp;
        }

        public int NumberOfSettlingDays { get; set; } = 3;

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceProfileHeaderMode DeviceProfileHeaderMode { get; set; }
        public int AffordanceRepetitionCount { get; set; }

        public bool TransportationEnabled { get; set; }

        /// <summary>
        /// Specifies whether this LPG simulation is part of a city simulation. In a city simulation,
        /// travels and remote activities (affordances that don't take place at home) are simulated dynamically,
        /// outside of the LPG household.
        /// </summary>
        public bool CitySimulationEnabled { get; set; }

        public bool FlexibilityEnabled { get; set; }
        public string CSVCharacter { get; set; } = ";";

        public string DecimalSeperator { get; set; }
        public bool DeleteDatFiles { get; set; }
        public int DummyCalcSteps { get; set; }
        public TimeSpan ExternalStepsize { get; set; }

        public bool ForceRandom { get; private set; }
        public DateTime InternalEndTime { get; set; }
        public DateTime InternalStartTime { get; set; }
        public TimeSpan InternalStepsize { get; set; }
        public int InternalTimesteps { get; set; }

        public LoadTypePriority LoadTypePriority { get; set; } = LoadTypePriority.All;
        [CanBeNull]
        public List<string> LoadtypesToPostprocess { get; set; } = [];
        public int OfficalTimesteps { get; set; }

        public DateTime OfficialEndTime { get; set; }
        public DateTime OfficialStartTime { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public HashSet<CalcOption> Options { get; } = [];

        public bool ShowSettlingPeriodTime { get; set; }
        public int TimeStepsPerHour { get; set; }

        public bool WriteExcelColumn { get; set; }
        public bool IgnorePreviousActivitesWhenNeeded { get; set; }
        public bool EnableIdlemode { get; set; }

        public void CheckSettings()
        {
            if (InternalStepsize.TotalSeconds < 1) {
                throw new DataIntegrityException("A time resolution of less than one second isn't possible.");
            }

            if (string.IsNullOrWhiteSpace(CSVCharacter)) {
                throw new DataIntegrityException("You dont have your csv separator set. Please fix.");
            }

            if (CSVCharacter == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) {
                throw new DataIntegrityException("You have set your decimal separator to the same character as the CSV separator (" + CSVCharacter +
                                                 "). That will make it impossible to read the result files. Please change the CSV separator in the settings to something else.");
            }

            if (CSVCharacter == CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator) {
                throw new DataIntegrityException("You have set your date separator on your system to the same character as the CSV separator (" + CSVCharacter +
                                                 "). That will make it impossible to read the result files. Please change the CSV separator in the settings to something else.");
            }

            if (OfficialStartTime > OfficialEndTime) {
                throw new DataIntegrityException("The calculation end date needs to be after the start date." + " The entered values were: Start:" + OfficialStartTime + " End:" + OfficialEndTime);
            }

            if (InternalTimesteps > 40_000_000) {
                throw new DataIntegrityException("You are trying to simulate more than 40 million time steps. That is not possible.");
            }

            if (InternalTimesteps < 0) {
                throw new DataIntegrityException("You have negative time steps. That is not possible and a bug. Please report.");
            }

            if (OfficalTimesteps == 0) {
                throw new DataIntegrityException("Total time steps was calculated to be zero. This is not possible." + " Try changing the start or end time or the time resolution.");
            }

            if (OfficalTimesteps < 0) {
                throw new DataIntegrityException("Total time steps was calculated to be negative. This is not possible." + " Try changing the start or end time or the time resolution.");
            }

            if (CitySimulationEnabled && !TransportationEnabled)
                throw new DataIntegrityException("City simulation can only be enabled if transport is enabled.");
        }

        [NotNull]
        public CalcParameters DisableShowSettlingPeriod()
        {
            ShowSettlingPeriodTime = false;
            return this;
        }

        public void Enable(CalcOption option)
        {
            if (!Options.Contains(option)) {
                Options.Add(option);
//                CheckDependenyOnOptions();
            }
        }

        [NotNull]
        public CalcParameters EnableShowSettlingPeriod()
        {
            ShowSettlingPeriodTime = true;
            return this;
        }

        public bool IsSet(CalcOption option)
        {
            if (Options.Contains(option)) {
                return true;
            }

            return false;
        }

        [NotNull]
        public CalcParameters SetAffordanceRepetitionCount(int count)
        {
            AffordanceRepetitionCount = count;
            return this;
        }

        [NotNull]
        public CalcParameters SetCsvCharacter([NotNull] string csvCharacter)
        {
            CSVCharacter = csvCharacter;
            return this;
        }


        [NotNull]
        public CalcParameters SetDecimalSeperator([NotNull] string decimalSeperator)
        {
            DecimalSeperator = decimalSeperator;
            return this;
        }

        [NotNull]
        public CalcParameters SetDummyTimeSteps(int timesteps)
        {
            DummyCalcSteps = timesteps;
            return this;
        }

        [NotNull]
        public CalcParameters SetEndDate(int year, int month, int day) => SetEndDate(new DateTime(year, month, day));

        [NotNull]
        public CalcParameters SetEndDate(DateTime enddate)
        {
            OfficialEndTime = enddate;
            InitializeTimeSteps();
            return this;
        }

        [NotNull]
        public CalcParameters SetExternalTimeResolution(TimeSpan externalTimeResolution)
        {
            ExternalStepsize = externalTimeResolution;
            return this;
        }

        [NotNull]
        public CalcParameters SetInternalTimeResolution(TimeSpan internalStepSize)
        {
            InternalStepsize = internalStepSize;
            InitializeTimeSteps();
            return this;
        }

        [NotNull]
        public CalcParameters SetLoadTypePriority(LoadTypePriority loadTypePriority)
        {
            LoadTypePriority = loadTypePriority;
            return this;
        }

        /// <summary>
        /// Determines the actual random seed to use, depending on what the user specified.
        /// If the user specified -1 or nothing at all, a random seed is chosen, otherwise
        /// the seed specified by the user is used directly.
        /// </summary>
        /// <param name="randomSeed">the user-specified seed</param>
        /// <param name="forceRandom">if true, always determine a new random seed to use</param>
        /// <returns>the random seed to use</returns>
        public static int GetActualRandomSeed(int? randomSeed, bool forceRandom = false)
        {
            int selectedSeed;
            if (randomSeed is null || randomSeed == -1 || forceRandom)
            {
                // use a new Random object to generate a random seed
                selectedSeed = new Random().Next();
            } else
            {
                selectedSeed = randomSeed.Value;
            }
            Logger.Info($"Using RNG seed {selectedSeed}");
            return selectedSeed;
        }

        [NotNull]
        public CalcParameters SetSettlingDays(int numberOfDays)
        {
            NumberOfSettlingDays = numberOfDays;
            InitializeTimeSteps();
            return this;
        }

        [NotNull]
        public CalcParameters SetShowSettlingPeriod(bool showSettlingPeriod)
        {
            ShowSettlingPeriodTime = showSettlingPeriod;
            return this;
        }

        [NotNull]
        public CalcParameters SetStartDate(DateTime startdate)
        {
            OfficialStartTime = startdate;
            InitializeTimeSteps();
            return this;
        }

        [NotNull]
        public CalcParameters SetStartDate(int year, int month, int day) => SetStartDate(new DateTime(year, month, day));


        [NotNull]
        public CalcParameters SetWriteExcelColumn(bool writeExcelColumn)
        {
            WriteExcelColumn = writeExcelColumn;
            return this;
        }

        private void InitializeTimeSteps()
        {
            if (NumberOfSettlingDays > 0) {
                NumberOfSettlingDays *= -1;
            }

            InternalStartTime = OfficialStartTime.AddDays(NumberOfSettlingDays);
            InternalEndTime = OfficialEndTime;

            var internalDuration = InternalEndTime - InternalStartTime;
            InternalTimesteps = (int)(internalDuration.TotalSeconds / InternalStepsize.TotalSeconds);

            var officialDuration = OfficialEndTime - OfficialStartTime;
            OfficalTimesteps = (int)(officialDuration.TotalSeconds / InternalStepsize.TotalSeconds);

            DummyCalcSteps = InternalTimesteps - OfficalTimesteps;
            TimeStepsPerHour = (int)(3600 / InternalStepsize.TotalSeconds);
        }
    }
}