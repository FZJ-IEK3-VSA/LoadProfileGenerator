using System;
using System.Collections.Generic;
using System.Globalization;
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

    public class CalcParameters {
        private int _numberOfSettlingDays = 3;

        private CalcParameters()
        {
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceProfileHeaderMode DeviceProfileHeaderMode { get; set; }
        public int ActualRandomSeed { get; set; }
        public int AffordanceRepetitionCount { get; set; }

        public bool TransportationEnabled { get; set; }
        [NotNull]
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
        [ItemNotNull]
        public List<string> LoadtypesToPostprocess { get; set; } = new List<string>();
        public int OfficalTimesteps { get; set; }

        public DateTime OfficialEndTime { get; set; }
        public DateTime OfficialStartTime { get; set; }

        [NotNull]
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public HashSet<CalcOption> Options { get; } = new HashSet<CalcOption>();

        public bool ShowSettlingPeriodTime { get; set; }
        public int TimeStepsPerHour { get; set; }

        public int UserSelectedRandomSeed { get; set; }
        public bool WriteExcelColumn { get; set; }
        public bool IgnorePreviousActivitesWhenNeeded { get; set; }
        public bool EnableIdlemode { get; set; }

        //public void CheckDependenyOnOptions()
        //{
        //    if (!Config.ReallyMakeAllFilesIncludingBothSums) {
        //        if (IsSet(CalcOption.OverallSum) && IsSet(CalcOption.HouseSumProfilesFromDetailedDats)) {
        //            Logger.Error(
        //                "You have both individual and overall sums enabled. This is a waste of time. The overall sum files are just a quicker way of calculating the results when no detailed information is needed. Please only enable one of those.");
        //            Disable(CalcOption.OverallSum);
        //            Enable(CalcOption.OverallDats);
        //        }
        //    }

        //    // dependencies
        //    if (IsSet(CalcOption.OverallSum)) {
        //        Enable(CalcOption.OverallDats);
        //    }
        //    if (IsSet(CalcOption.JsonHouseSumFiles))
        //    {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.HouseSumProfilesFromDetailedDats)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.WeekdayProfiles)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.AffordanceEnergyUse)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.DeviceProfileExternalEntireHouse)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }
        //    if (IsSet(CalcOption.DeviceProfileExternalIndividualHouseholds))
        //    {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.TotalsPerLoadtype)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.DeviceProfiles)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.ActivationFrequencies)) {
        //        //Enable(CalcOption.ActionsLogfile);
        //        Enable(CalcOption.AffordanceEnergyUse);
        //    }

        //    if (IsSet(CalcOption.HouseholdPlan)) {
        //        Enable(CalcOption.ActivationFrequencies);
        //        //Enable(CalcOption.ActionsLogfile);
        //        Enable(CalcOption.AffordanceEnergyUse);
        //    }

        //    if (IsSet(CalcOption.ActivationsPerHour)) {
        //        //Enable(CalcOption.ActionsLogfile);
        //    }

        //    if (IsSet(CalcOption.TotalsPerDevice)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.DurationCurve)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.ActionCarpetPlot)) {
        //        //Enable(CalcOption.ActionsLogfile);
        //    }

        //    if (IsSet(CalcOption.TimeOfUsePlot)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.MakeGraphics)) {
        //        //WriteExcelColumn = false;
        //    }

        //    if (IsSet(CalcOption.PolysunImportFiles)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.MakePDF)) {
        //        Enable(CalcOption.MakeGraphics);
        //    }

        //    if (IsSet(CalcOption.BodilyActivityStatistics)) {
        //        Enable(CalcOption.ActionsEachTimestep);
        //    }
        //    /*  if (IsSet(CalcOption.SMAImportFiles))
        //      {
        //          Enable(CalcOption.DetailedDatFiles);
        //      }*/

        //    if (IsSet(CalcOption.SumProfileExternalEntireHouse)) {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }
        //    if (IsSet(CalcOption.SumProfileExternalIndividualHouseholds))
        //    {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }

        //    if (IsSet(CalcOption.HouseholdPlan)) {
        //        Enable(CalcOption.ActivationFrequencies);
        //        Enable(CalcOption.HouseholdPlan);
        //        //Enable(CalcOption.ActionsLogfile);
        //    }
        //    if (IsSet(CalcOption.SumProfileExternalIndividualHouseholdsAsJson))
        //    {
        //        Enable(CalcOption.DetailedDatFiles);
        //    }
        //    if (!IsSet(CalcOption.DetailedDatFiles) && !IsSet(CalcOption.OverallDats)) {
        //        //always enable at least some dat files.
        //        Enable(CalcOption.DetailedDatFiles);
        //    }
        //    if (!IsSet(CalcOption.DetailedDatFiles) && !IsSet(CalcOption.OverallDats)) {
        //        throw new LPGException("No dat file generation has been enabled. This is a bug. The workaround is to enable them manually "
        //                               + " by setting the appropriate option (set either DetailedDatFiles or OverallDats), but this should be fixed in the code. Please report.");
        //    }
        //}

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

        [NotNull]
        public static CalcParameters GetNew() => new CalcParameters();

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
        public CalcParameters SetDeleteDatFiles(bool deleteDatFiles)
        {
            DeleteDatFiles = deleteDatFiles;
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

        public void SetManyOptionsWithClear([NotNull] List<CalcOption> options)
        {
            foreach (var calcOption in options) {
                if (!Options.Contains(calcOption)) {
                    Options.Add(calcOption);
                }
            }

//            CheckDependenyOnOptions();
        }

        [NotNull]
        public CalcParameters SetRandomSeed(int randomSeed, bool forceRandom)
        {
            UserSelectedRandomSeed = randomSeed;
            ForceRandom = forceRandom;
            if (UserSelectedRandomSeed == -1 || forceRandom) {
                ActualRandomSeed = DateTime.Now.Millisecond + DateTime.Now.Second * 100;
            }
            else {
                ActualRandomSeed = randomSeed;
            }

            return this;
        }

        [NotNull]
        public CalcParameters SetSettlingDays(int numberOfDays)
        {
            _numberOfSettlingDays = numberOfDays;
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

        /*public void ClearOptions()
            {
                _options.Clear();
            }*/

        //private void Disable(CalcOption option)
        //{
        //    if (Options.Contains(option)) {
        //        Options.Remove(option);
        //    }

        //    //CheckDependenyOnOptions();
        //}

        private void InitializeTimeSteps()
        {
            if (_numberOfSettlingDays > 0) {
                _numberOfSettlingDays *= -1;
            }

            InternalStartTime = OfficialStartTime.AddDays(_numberOfSettlingDays);
            InternalEndTime = OfficialEndTime;
            //   if (OfficialEndTime.Hour == 0 && OfficialEndTime.Minute == 0 && OfficialEndTime.Second == 0)
            // {
            //   OfficialEndTime = OfficialEndTime.AddDays(1);
            //}

            var internalDuration = InternalEndTime - InternalStartTime;
            InternalTimesteps = (int)(internalDuration.TotalSeconds / InternalStepsize.TotalSeconds);

            var officialDuration = OfficialEndTime - OfficialStartTime;
            OfficalTimesteps = (int)(officialDuration.TotalSeconds / InternalStepsize.TotalSeconds);

            DummyCalcSteps = InternalTimesteps - OfficalTimesteps;
            TimeStepsPerHour = (int)(3600 / InternalStepsize.TotalSeconds);
        }
    }
}