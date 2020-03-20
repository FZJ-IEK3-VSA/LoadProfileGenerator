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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class TimeLimitEntry : DBBase {
        public const string TableName = "tblTimeLimitEntries";
        private const string TableNameOld = "tblDeviceTimeBoolEntries";

        [NotNull] private readonly object _bitmapLock = new object();
        [ItemNotNull] [NotNull] private readonly ObservableCollection<TimeLimitEntry> _subentries;
        private AnyAllTimeLimitCondition _anyAll;
        private int _dailyDaycount;
        [CanBeNull]
        private int? _dateProfileID;
        private double _dateProfileMaxVariation;
        private double _dateProfileMinVariation;
        private DateTime _dateRangeEnd;
        private DateTime _dateRangeStart;
        private bool _duringHoliday;
        private bool _duringNotVacation;
        private bool _duringVacation;
        private bool _duringVacationLongerThan;
        private bool _duringVacationShorterThan;
        private DateTime _endTime;
        private double _maxDateProfileValue;
        private double _maxTemperature;
        private double _minDateProfileValue;
        private double _minTemperature;
        private int _monthlyDay;
        private int _monthlyMonthCount;
        private bool _needsDarkness;
        private bool _needsLight;
        [CanBeNull]
        private int? _parentEntryID;
        private int _randomizeTimeAmount;
        private PermissionMode _repeaterType;
        private DateTime _startTime;
        private int _startWeek;
        private int _vacationDurationLimit;
        private bool _weeklyFriday;
        private bool _weeklyMonday;
        private bool _weeklySaturday;
        private bool _weeklySunday;
        private bool _weeklyThursday;
        private bool _weeklyTuesday;
        private bool _weeklyWednesday;
        private int _weeklyWeekCount;
        private int _yearlyDay;
        private int _yearlyMonth;

        public TimeLimitEntry([CanBeNull]int? id, int timeLimitID, DateTime startTime, DateTime endTime,
            PermissionMode repeaterType, int dailyDaycount, int weeklyWeekCount, bool weeklyMonday, bool weeklyTuesday,
            bool weeklyWednesday, bool weeklyThursday, bool weeklyFriday, bool weeklySaturday, bool weeklySunday,
            int monthlyMonthCount, int monthlyDay, int yearlyDay, int yearlyMonth, int startWeek, double minTemperature,
            double maxTemperature, bool needsLight, bool needsDarkness, [CanBeNull] int? parentEntryID,
            AnyAllTimeLimitCondition anyAllTimeLimitCondition, DateTime dateRangeStart, DateTime dateRangeEnd,
            [CanBeNull]int? dateprofileID, double maxDateProfileValue, double minDateProfileValue, bool duringVacation,
            bool duringNotVacation, bool duringVacationLongerThan, bool duringVacationShorterThan,
            int vacationDurationLimit, bool duringHoliday, [ItemNotNull] [NotNull] ObservableCollection<DateBasedProfile> allDateProfiles,
            [NotNull] string connectionString, int randomizeTimeAmount, double dateProfileMinVariation,
            double dateProfileMaxVariation, [NotNull] string guid) : base(startTime + " " + endTime, TableName, connectionString, guid)
        {
            ID = id;
            AllDateProfiles = allDateProfiles;
            _startTime = startTime;
            _endTime = endTime;
            _duringVacationLongerThan = duringVacationLongerThan;
            _duringVacationShorterThan = duringVacationShorterThan;
            _vacationDurationLimit = vacationDurationLimit;
            _duringNotVacation = duringNotVacation;
            _duringVacation = duringVacation;
            _repeaterType = repeaterType;
            _subentries = new ObservableCollection<TimeLimitEntry>();
            _duringHoliday = duringHoliday;
            _dailyDaycount = dailyDaycount;
            _randomizeTimeAmount = randomizeTimeAmount;
            _dateProfileMinVariation = dateProfileMinVariation;
            _dateProfileMaxVariation = dateProfileMaxVariation;
            if (_dailyDaycount == 0) {
                _dailyDaycount = 1;
            }
            _weeklyWeekCount = weeklyWeekCount;
            if (_weeklyWeekCount == 0) {
                _weeklyWeekCount = 1;
            }
            _weeklyMonday = weeklyMonday;
            _weeklyTuesday = weeklyTuesday;
            _weeklyWednesday = weeklyWednesday;
            _weeklyThursday = weeklyThursday;
            _weeklyFriday = weeklyFriday;
            _weeklySaturday = weeklySaturday;
            _weeklySunday = weeklySunday;
            _monthlyMonthCount = monthlyMonthCount;
            TimeLimitID = timeLimitID;
            if (_monthlyMonthCount == 0) {
                _monthlyMonthCount = 1;
            }
            _monthlyDay = monthlyDay;
            if (_monthlyDay == 0) {
                _monthlyDay = 1;
            }
            _yearlyDay = yearlyDay;
            if (_yearlyDay == 0) {
                _yearlyDay = 1;
            }
            _yearlyMonth = yearlyMonth;
            if (_yearlyMonth == 0) {
                _yearlyMonth = 1;
            }
            _startWeek = startWeek;
            _maxTemperature = maxTemperature;
            _needsLight = needsLight;
            _needsDarkness = needsDarkness;
            _parentEntryID = parentEntryID;
            _anyAll = anyAllTimeLimitCondition;
            _minTemperature = minTemperature;
            _dateRangeStart = dateRangeStart;
            _dateRangeEnd = dateRangeEnd;
            _dateProfileID = dateprofileID;
            _maxDateProfileValue = maxDateProfileValue;
            _minDateProfileValue = minDateProfileValue;
            if (!_needsLight && !_needsDarkness) {
                _needsLight = true;
            }
            TypeDescription = "Time Limit Entry";
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DateBasedProfile> AllDateProfiles { get; }

        public AnyAllTimeLimitCondition AnyAll {
            get => _anyAll;
            set => SetValueWithNotify(value, ref _anyAll, nameof(AnyAll));
        }

        [UsedImplicitly]
        public int DailyDayCount {
            get => _dailyDaycount;
            set {
                if (value != 0) {
                    SetValueWithNotify(value, ref _dailyDaycount, nameof(DailyDayCount));
                }
                else {
                    SetValueWithNotify(1, ref _dailyDaycount, nameof(DailyDayCount));
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public DateBasedProfile DateBasedProfile {
            get {
                var profile = AllDateProfiles.FirstOrDefault(dbp => dbp.ID == DateProfileID);
                return profile;
            }
            set {
                var id = -1;
                if (value != null) {
                    id = value.IntID;
                }
                DateProfileID = id;
                OnPropertyChanged(nameof(DateBasedProfile));
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public int? DateProfileID {
            get => _dateProfileID;
            set => SetValueWithNotify(value, ref _dateProfileID, nameof(DateProfileID));
        }

        [UsedImplicitly]
        public double DateProfileMaxVariation {
            get => _dateProfileMaxVariation;
            set => SetValueWithNotify(value, ref _dateProfileMaxVariation, nameof(DateProfileMaxVariation));
        }

        [UsedImplicitly]
        public double DateProfileMinVariation {
            get => _dateProfileMinVariation;
            set => SetValueWithNotify(value, ref _dateProfileMinVariation, nameof(DateProfileMinVariation));
        }

        [UsedImplicitly]
        public DateTime DateRangeEnd {
            get => _dateRangeEnd;
            set => SetValueWithNotify(value, ref _dateRangeEnd, nameof(DateRangeEnd));
        }

        [UsedImplicitly]
        public DateTime DateRangeStart {
            get => _dateRangeStart;
            set => SetValueWithNotify(value, ref _dateRangeStart, nameof(DateRangeStart));
        }

        [UsedImplicitly]
        public bool DuringHoliday {
            get => _duringHoliday;
            set => SetValueWithNotify(value, ref _duringHoliday, nameof(DuringHoliday));
        }

        [UsedImplicitly]
        public bool DuringNotVacation {
            get => _duringNotVacation;
            set => SetValueWithNotify(value, ref _duringNotVacation, nameof(DuringNotVacation));
        }

        [UsedImplicitly]
        public bool DuringVacation {
            get => _duringVacation;
            set => SetValueWithNotify(value, ref _duringVacation, nameof(DuringVacation));
        }

        [UsedImplicitly]
        public bool DuringVacationLongerThan {
            get => _duringVacationLongerThan;
            set => SetValueWithNotify(value, ref _duringVacationLongerThan, nameof(DuringVacationLongerThan));
        }

        [UsedImplicitly]
        public bool DuringVacationShorterThan {
            get => _duringVacationShorterThan;
            set => SetValueWithNotify(value, ref _duringVacationShorterThan, nameof(DuringVacationShorterThan));
        }

        [UsedImplicitly]
        public DateTime EndTimeDateTime {
            get => _endTime;
            set => SetValueWithNotify(value, ref _endTime, nameof(EndTimeTimeSpan));
        }

        [UsedImplicitly]
        public TimeSpan EndTimeTimeSpan {
            get => _endTime - Config.DummyTime;
            set => SetValueWithNotify(Config.DummyTime.Add(value), ref _endTime, nameof(EndTimeTimeSpan));
        }

        [UsedImplicitly]
        public double MaxDateProfileValue {
            get => _maxDateProfileValue;
            set => SetValueWithNotify(value, ref _maxDateProfileValue, nameof(MaxDateProfileValue));
        }

        [UsedImplicitly]
        public double MaxTemperature {
            get => _maxTemperature;
            set => SetValueWithNotify(value, ref _maxTemperature, nameof(MaxTemperature));
        }

        [UsedImplicitly]
        public double MinDateProfileValue {
            get => _minDateProfileValue;
            set => SetValueWithNotify(value, ref _minDateProfileValue, nameof(MinDateProfileValue));
        }

        [UsedImplicitly]
        public double MinTemperature {
            get => _minTemperature;
            set => SetValueWithNotify(value, ref _minTemperature, nameof(MinTemperature));
        }

        [UsedImplicitly]
        public int MonthlyDay {
            get => _monthlyDay;
            set => SetValueWithNotify(value, ref _monthlyDay, nameof(MonthlyDay));
        }

        [UsedImplicitly]
        public int MonthlyMonthCount {
            get => _monthlyMonthCount;
            set => SetValueWithNotify(value, ref _monthlyMonthCount, nameof(MonthlyMonthCount));
        }

        [UsedImplicitly]
        public bool NeedsDarkness {
            get => _needsDarkness;
            set => SetValueWithNotify(value, ref _needsDarkness, nameof(NeedsDarkness));
        }

        [UsedImplicitly]
        public bool NeedsLight {
            get => _needsLight;
            set => SetValueWithNotify(value, ref _needsLight, nameof(NeedsLight));
        }

        [CanBeNull]
        public TimeLimitEntry ParentEntry { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public int? ParentEntryID {
            get => _parentEntryID;
            set => SetValueWithNotify(value, ref _parentEntryID, nameof(ParentEntryID));
        }

        [UsedImplicitly]
        public int RandomizeTimeAmount {
            get => _randomizeTimeAmount;
            set => SetValueWithNotify(value, ref _randomizeTimeAmount, nameof(RandomizeTimeAmount));
        }

        [NotNull]
        [UsedImplicitly]
        public string RepeaterString => _repeaterType.ToString();

        [UsedImplicitly]
        public PermissionMode RepeaterType {
            get => _repeaterType;
            set => SetValueWithNotify(value, ref _repeaterType, nameof(RepeaterType));
        }

        [UsedImplicitly]
        public DateTime StartTimeDateTime {
            get => _startTime;
            set => SetValueWithNotify(value, ref _startTime, nameof(StartTimeTimeSpan));
        }

        [UsedImplicitly]
        public TimeSpan StartTimeTimeSpan {
            get => _startTime - Config.DummyTime;
            set => SetValueWithNotify(Config.DummyTime.Add(value), ref _startTime, nameof(StartTimeTimeSpan));
        }

        [UsedImplicitly]
        public int StartWeek {
            get => _startWeek;
            set => SetValueWithNotify(value, ref _startWeek, nameof(StartWeek));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TimeLimitEntry> Subentries => _subentries;

        public int TimeLimitID { get; }

        [UsedImplicitly]
        public int VacationDurationLimit {
            get => _vacationDurationLimit;
            set => SetValueWithNotify(value, ref _vacationDurationLimit, nameof(VacationDurationLimit));
        }

        [UsedImplicitly]
        public bool WeeklyFriday {
            get => _weeklyFriday;
            set => SetValueWithNotify(value, ref _weeklyFriday, nameof(WeeklyFriday));
        }

        [UsedImplicitly]
        public bool WeeklyMonday {
            get => _weeklyMonday;
            set => SetValueWithNotify(value, ref _weeklyMonday, nameof(WeeklyMonday));
        }

        [UsedImplicitly]
        public bool WeeklySaturday {
            get => _weeklySaturday;
            set => SetValueWithNotify(value, ref _weeklySaturday, nameof(WeeklySaturday));
        }

        [UsedImplicitly]
        public bool WeeklySunday {
            get => _weeklySunday;
            set => SetValueWithNotify(value, ref _weeklySunday, nameof(WeeklySunday));
        }

        [UsedImplicitly]
        public bool WeeklyThursday {
            get => _weeklyThursday;
            set => SetValueWithNotify(value, ref _weeklyThursday, nameof(WeeklyThursday));
        }

        [UsedImplicitly]
        public bool WeeklyTuesday {
            get => _weeklyTuesday;
            set => SetValueWithNotify(value, ref _weeklyTuesday, nameof(WeeklyTuesday));
        }

        [UsedImplicitly]
        public bool WeeklyWednesday {
            get => _weeklyWednesday;
            set => SetValueWithNotify(value, ref _weeklyWednesday, nameof(WeeklyWednesday));
        }

        [UsedImplicitly]
        public int WeeklyWeekCount {
            get => _weeklyWeekCount;
            set => SetValueWithNotify(value, ref _weeklyWeekCount, nameof(WeeklyWeekCount));
        }

        [UsedImplicitly]
        public int YearlyDay {
            get => _yearlyDay;
            set => SetValueWithNotify(value, ref _yearlyDay, nameof(YearlyDay));
        }

        [UsedImplicitly]
        public int YearlyMonth {
            get => _yearlyMonth;
            set => SetValueWithNotify(value, ref _yearlyMonth, nameof(YearlyMonth));
        }

        [NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static TimeLimitEntry AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var timeLimitID = dr.GetIntFromLong("TimeLimitID", false, ignoreMissingFields, -1);
            if (ignoreMissingFields && timeLimitID == -1) {
                timeLimitID = dr.GetIntFromLong("DeviceTimeID", false, ignoreMissingFields, -1);
            }
            var startTime = dr.GetDateTime("StartTime");
            var endTime = dr.GetDateTime("EndTime");
            var repeaterType = (PermissionMode) dr.GetIntFromLong("repeaterType");
            var anyAllTimeLimitCondition = (AnyAllTimeLimitCondition) dr.GetIntFromLong("AnyAll");
            var dailyDayCount = dr.GetIntFromLong("DailyDayCount");
            var weeklyWeekCount = dr.GetIntFromLong("WeeklyWeekCount");
            var weeklyMonday = dr.GetBool("WeeklyMonday");
            var weeklyTuesday = dr.GetBool("WeeklyTuesday");
            var weeklyWednesday = dr.GetBool("WeeklyWednesday");
            var weeklyThursday = dr.GetBool("WeeklyThursday");
            var weeklyFriday = dr.GetBool("WeeklyFriday");
            var weeklySaturday = dr.GetBool("WeeklySaturday");
            var weeklySunday = dr.GetBool("WeeklySunday");
            var monthlyMonthCount = dr.GetIntFromLong("MonthlyMonthCount");
            var monthlyDay = dr.GetIntFromLong("MonthlyDay");
            var yearlyDay = dr.GetIntFromLong("YearlyDay");
            var yearlyMonth = dr.GetIntFromLong("YearlyMonth");
            var parentID = dr.GetNullableIntFromLong("ParentEntryID", false);
            var startWeek = 0;

            if (dr["StartWeek"] != DBNull.Value) {
                startWeek = dr.GetIntFromLong("StartWeek");
            }
            var minTemperature = dr.GetDouble("MinTemperature", false, -100.0);
            var maxTemperature = dr.GetDouble("MaxTemperature", false, 100.0);
            var needsLight = dr.GetBool("NeedsLight", false);
            var needsDarkness = dr.GetBool("NeedsDarkness", false);
            var dateRangeStart = dr.GetDateTime("DateRangeStart", false);
            var dateRangeEnd = dr.GetDateTime("DateRangeEnd", false);
            var dateProfileID = dr.GetIntFromLong("DateProfileID", false, ignoreMissingFields);
            var maxDateProfileValue = dr.GetDouble("MaxDateProfileValue", false, 0, ignoreMissingFields);
            var minDateProfileValue = dr.GetDouble("MinDateProfileValue", false, 0, ignoreMissingFields);
            var duringVacation = dr.GetBool("DuringVacation", false, false, ignoreMissingFields);
            var duringNotVacation = dr.GetBool("DuringNotVacation", false, false, ignoreMissingFields);
            var duringVacationLongerThan = dr.GetBool("DuringVacationLongerThan", false, false, ignoreMissingFields);
            var duringVacationShorterThan = dr.GetBool("DuringVacationShorterThan", false, false, ignoreMissingFields);
            var vacationDurationLimit = dr.GetIntFromLong("VacationDurationLimit", false, ignoreMissingFields);
            var duringHoliday = dr.GetBool("DuringHoliday", false, false, ignoreMissingFields);
            var randomizeTimeAmount = dr.GetIntFromLong("RandomizeTimeAmount", false, ignoreMissingFields);
            var dateProfileMinVariation = dr.GetDouble("DateProfileMinVariation", false, 0, ignoreMissingFields);
            var dateProfileMaxVariation = dr.GetDouble("DateProfileMaxVariation", false, 0, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var pte = new TimeLimitEntry(id, timeLimitID, startTime, endTime, repeaterType, dailyDayCount,
                weeklyWeekCount, weeklyMonday, weeklyTuesday, weeklyWednesday, weeklyThursday, weeklyFriday,
                weeklySaturday, weeklySunday, monthlyMonthCount, monthlyDay, yearlyDay, yearlyMonth, startWeek,
                minTemperature, maxTemperature, needsLight, needsDarkness, parentID, anyAllTimeLimitCondition,
                dateRangeStart, dateRangeEnd, dateProfileID, maxDateProfileValue, minDateProfileValue, duringVacation,
                duringNotVacation, duringVacationLongerThan, duringVacationShorterThan, vacationDurationLimit,
                duringHoliday, aic.DateBasedProfiles, connectionString, randomizeTimeAmount, dateProfileMinVariation,
                dateProfileMaxVariation, guid);
            return pte;
        }

        public void CopyEverything([NotNull] TimeLimitEntry otherEntry)
        {
            _startTime = otherEntry._startTime;
            _endTime = otherEntry._endTime;
            _duringVacationLongerThan = otherEntry._duringVacationLongerThan;
            _duringVacationShorterThan = otherEntry._duringVacationShorterThan;
            _vacationDurationLimit = otherEntry._vacationDurationLimit;
            _duringNotVacation = otherEntry._duringNotVacation;
            _duringVacation = otherEntry._duringVacation;
            _repeaterType = otherEntry._repeaterType;

            _dailyDaycount = otherEntry._dailyDaycount;
            if (_dailyDaycount == 0) {
                _dailyDaycount = 1;
            }
            _weeklyWeekCount = otherEntry._weeklyWeekCount;
            if (_weeklyWeekCount == 0) {
                _weeklyWeekCount = 1;
            }
            _weeklyMonday = otherEntry._weeklyMonday;
            _weeklyTuesday = otherEntry._weeklyTuesday;
            _weeklyWednesday = otherEntry._weeklyWednesday;
            _weeklyThursday = otherEntry._weeklyThursday;
            _weeklyFriday = otherEntry._weeklyFriday;
            _weeklySaturday = otherEntry._weeklySaturday;
            _weeklySunday = otherEntry._weeklySunday;
            _monthlyMonthCount = otherEntry._monthlyMonthCount;

            if (_monthlyMonthCount == 0) {
                _monthlyMonthCount = 1;
            }
            _monthlyDay = otherEntry._monthlyDay;
            if (_monthlyDay == 0) {
                _monthlyDay = 1;
            }
            _yearlyDay = otherEntry._yearlyDay;
            if (_yearlyDay == 0) {
                _yearlyDay = 1;
            }
            _yearlyMonth = otherEntry._yearlyMonth;
            if (_yearlyMonth == 0) {
                _yearlyMonth = 1;
            }
            _startWeek = otherEntry._startWeek;
            _maxTemperature = otherEntry._maxTemperature;
            _needsLight = otherEntry._needsLight;
            _needsDarkness = otherEntry._needsDarkness;
            AnyAll = otherEntry.AnyAll;
            _minTemperature = otherEntry._minTemperature;
            _dateRangeStart = otherEntry._dateRangeStart;
            _dateRangeEnd = otherEntry._dateRangeEnd;
            _dateProfileID = otherEntry._dateProfileID;
            _maxDateProfileValue = otherEntry._maxDateProfileValue;
            _minDateProfileValue = otherEntry._minDateProfileValue;
            _dateProfileMinVariation = otherEntry.DateProfileMinVariation;
            _dateProfileMaxVariation = otherEntry.DateProfileMaxVariation;
            if (!_needsLight && !_needsDarkness) {
                _needsLight = true;
            }
        }

        [NotNull]
        public static TimeLimitEntry
            CreateDefaultEntry(int timeLimitID, [CanBeNull]int? parentEntryID, [NotNull] string connectionString,
                [ItemNotNull] [NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles) => new TimeLimitEntry(null, timeLimitID,
            new DateTime(1900, 1, 1, 0, 0, 0), new DateTime(1900, 1, 1, 20, 0, 0), PermissionMode.EveryXDay, 1, 1,
            true,
            true, true, true, true, true, true, 1, 1, 1, 1, 1, -100, 100, true, false, parentEntryID,
            AnyAllTimeLimitCondition.All, new DateTime(2014, 12, 1), new DateTime(2014, 12, 31), -1, 0, 0, false,
            false,
            false, false, 5, true, dateBasedProfiles, connectionString, 0, 0, 0,
            System.Guid.NewGuid().ToString());

        [ItemNotNull]
        [NotNull]
        private List<Tuple<DateTime, DateTime>> GetDateRanges([NotNull] List<int> years)
        {
            var ranges = new List<Tuple<DateTime, DateTime>>();
            foreach (var year in years) {
                var startTime = new DateTime(year, DateRangeStart.Month, DateRangeStart.Day);
                var endTime = new DateTime(year, DateRangeEnd.Month, DateRangeEnd.Day);
                var tup = new Tuple<DateTime, DateTime>(startTime, endTime);
                ranges.Add(tup);
            }
            return ranges;
        }

        [NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private string GetDescription()
        {
            string s;
            switch (_repeaterType) {
                case PermissionMode.EveryXDay:
                    s = "Every " + _dailyDaycount + " day";
                    break;
                case PermissionMode.EveryWorkday:
                    s = "Every work day";
                    break;
                case PermissionMode.EveryXWeeks:
                    s = "Every " + _weeklyWeekCount + " weeks on ";
                    if (_weeklyMonday) {
                        s += "Monday, ";
                    }
                    if (_weeklyTuesday) {
                        s += "Tuesday, ";
                    }
                    if (_weeklyWednesday) {
                        s += "Wednesday, ";
                    }
                    if (_weeklyThursday) {
                        s += "Thursday, ";
                    }
                    if (_weeklyFriday) {
                        s += "Friday, ";
                    }
                    if (_weeklySaturday) {
                        s += "Saturday, ";
                    }
                    if (_weeklySunday) {
                        s += "Sunday,";
                    }
                    s += " starting week " + _startWeek;
                    s = s.Trim();
                    if (s.EndsWith(",", StringComparison.Ordinal)) {
                        s = s.Substring(0, s.Length - 1);
                    }
                    break;
                case PermissionMode.EveryXMonths:
                    s = "Every " + _monthlyMonthCount + " month on the " + _monthlyDay + " day.";
                    break;
                case PermissionMode.Yearly:
                    s = "Every year on the " + _yearlyDay + " day of the month " + _yearlyMonth;
                    break;
                case PermissionMode.Temperature:
                    s = "Whenever temperatures are between " + _minTemperature + " �C and " + MaxTemperature + " �C";
                    break;
                case PermissionMode.LightControlled:
                    s = string.Empty;
                    if (NeedsLight) {
                        s += "Whenever there is daylight.";
                    }
                    if (NeedsDarkness) {
                        s += "Whenever there is darkness.";
                    }
                    break;
                case PermissionMode.ControlledByDateProfile:
                    if (DateBasedProfile != null) {
                        s = "Whenever the value in " + DateBasedProfile.Name + " is between " + _minDateProfileValue +
                            " and " + _maxDateProfileValue;
                    }
                    else {
                        s = "Whenever the value in (undetermined profile) is between " + _minDateProfileValue +
                            " (+- " + _dateProfileMinVariation + " and " + _maxDateProfileValue + " (+-" +
                            _dateProfileMaxVariation + ")";
                    }
                    break;
                case PermissionMode.VacationControlled:
                    s = "Vacation controlled;";
                    if (DuringVacation) {
                        s = "During vacations;";
                    }
                    if (DuringNotVacation) {
                        s = "During not vacations";
                    }
                    break;
                case PermissionMode.EveryDateRange:
                    s = "During Date Range " + DateRangeStart + " to " + _dateRangeEnd;
                    break;
                case PermissionMode.HolidayControlled:
                    if (DuringHoliday) {
                        s = "During Holiday";
                    }
                    else {
                        s = "Not during holiday";
                    }
                    break;
                default: throw new LPGException("Unknown repeater type:" + _repeaterType);
            }

            if (s == "unknown") {
                throw new LPGException("Unknown repeater type:" + _repeaterType);
            }
            return s;
        }

        public static int GetLevel([NotNull] TimeLimitEntry de)
        {
            var dtbe = de;
            var level = 0;
            while (dtbe.ParentEntry != null) {
                level++;
                dtbe = dtbe.ParentEntry;
            }
            return level;
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public BitArray GetOneYearArray(TimeSpan stepsize, DateTime startDateTime, DateTime endDateTime,
            [NotNull] TemperatureProfile tempProfile, [NotNull] GeographicLocation geoloc, [NotNull] Random random,
            [ItemNotNull] [NotNull] List<VacationTimeframe> vacationTimeframes, [NotNull] string holidayKey, [NotNull] out List<DateTime> bridgeDates,
            int startMinusMinutes, int startPlusMinutes, int endMinusMinutes, int endPlusMinutes)
        {
            if (_subentries.Count > 0) {
                BitArray ar = null;
                var bridgeDateTimes = new List<DateTime>();
                foreach (var timeLimitEntry in _subentries) {
                    var subarr = timeLimitEntry.GetOneYearArray(stepsize, startDateTime, endDateTime, tempProfile,
                        geoloc, random, vacationTimeframes, holidayKey, out var subBridgeDates, startMinusMinutes, startPlusMinutes,
                        endMinusMinutes, endPlusMinutes);
                    bridgeDateTimes.AddRange(subBridgeDates);
                    if (ar == null && (AnyAll == AnyAllTimeLimitCondition.All ||
                                       AnyAll == AnyAllTimeLimitCondition.Any)) {
                        ar = subarr;
                    }
                    else if (ar == null && AnyAll == AnyAllTimeLimitCondition.None) {
                        ar = subarr.Not();
                    }
                    else if (ar == null) {
                        throw new LPGException("Unknown AnyAllCollectionValue");
                    }
                    else {
                        switch (AnyAll) {
                            case AnyAllTimeLimitCondition.All:
                                ar = subarr.And(ar);
                                break;
                            case AnyAllTimeLimitCondition.Any:
                                ar = subarr.Or(ar);
                                break;
                            case AnyAllTimeLimitCondition.None:
                                ar = ar.And(subarr.Not());
                                break;
                            default: throw new LPGException("Unknown AnyAllCollectionValue");
                        }
                    }
                }
                bridgeDates = bridgeDateTimes;
                if (ar == null) {
                    throw new LPGException("Could not calculate a bit array.");
                }
                return ar;
            }
            bridgeDates = new List<DateTime>();
            lock (_bitmapLock) {
                // cleanup of data
                if (_dailyDaycount == 0) {
                    _dailyDaycount = 1;
                }
                int actualStartMinutes = 0;
                    int actualEndMinutes=0;
                if (Config.AdjustTimesForSettlement) {
                    actualStartMinutes = random.Next(startMinusMinutes + startPlusMinutes) - startMinusMinutes;
                    actualEndMinutes = random.Next(endMinusMinutes + endPlusMinutes) - endMinusMinutes;
                }
                if (startMinusMinutes != 0 || startPlusMinutes != 0 || endMinusMinutes != 0 || endPlusMinutes != 0)
                {
                    Logger.Info("Adjusting start by " + actualStartMinutes + " and end by " + actualEndMinutes);
                }
                var mystarttime = _startTime - new DateTime(1900, 1, 1) - TimeSpan.FromMinutes(startMinusMinutes);
                var myendtime = _endTime - new DateTime(1900, 1, 1) - TimeSpan.FromMinutes(endMinusMinutes);
                var duration = endDateTime - startDateTime;
                var totalsteps = (int) (duration.TotalSeconds / stepsize.TotalSeconds);
                var dts = new DateTime[totalsteps];
                dts[0] = startDateTime;
                for (var i = 1; i < totalsteps; i++) {
                    dts[i] = dts[i - 1] + stepsize;
                }
                var br = new BitArray(totalsteps);
                br.SetAll(false);
                var repeatertypeFound = false;
                // daily
                if (_repeaterType == PermissionMode.EveryXDay) {
                    repeatertypeFound = SetEveryDay(totalsteps, dts, mystarttime, myendtime, br, random);
                }

                // workday
                if (_repeaterType == PermissionMode.EveryWorkday) {
                    repeatertypeFound = SetWorkday(totalsteps, dts, geoloc, random, mystarttime, myendtime, br,
                        holidayKey, out bridgeDates);
                }
                // weekly
                if (_repeaterType == PermissionMode.EveryXWeeks) {
                    repeatertypeFound = SetWeekly(startDateTime, totalsteps, dts, mystarttime, myendtime, br, random);
                }
                // monthly
                if (_repeaterType == PermissionMode.EveryXMonths) {
                    repeatertypeFound = SetEveryXMonths(totalsteps, dts, mystarttime, myendtime, br, random);
                }
                // yearly
                if (_repeaterType == PermissionMode.Yearly) {
                    repeatertypeFound = SetYearly(totalsteps, dts, mystarttime, myendtime, br, random);
                }
                // temperature controlled
                if (_repeaterType == PermissionMode.Temperature) {
                    repeatertypeFound = SetTemperature(stepsize, startDateTime, endDateTime, tempProfile, totalsteps,
                        br);
                }
                // light controlled
                if (_repeaterType == PermissionMode.LightControlled) {
                    repeatertypeFound = SetLightControlled(stepsize, startDateTime, endDateTime, geoloc, totalsteps,
                        br);
                }

                // Date Range controlled
                if (_repeaterType == PermissionMode.EveryDateRange) {
                    repeatertypeFound = SetDateRangeControlled(startDateTime, endDateTime, totalsteps, dts, br);
                }

                // date profile
                if (_repeaterType == PermissionMode.ControlledByDateProfile) {
                    repeatertypeFound =
                        SetDateProfileControlled(stepsize, startDateTime, endDateTime, totalsteps, br, random);
                }
                // Holiday controlled
                if (_repeaterType == PermissionMode.HolidayControlled) {
                    repeatertypeFound = SetHolidayControlled(geoloc, totalsteps, dts, br);
                }

                // vacation
                if (_repeaterType == PermissionMode.VacationControlled) {
                    repeatertypeFound =
                        SetVacationControlled(startDateTime, endDateTime, vacationTimeframes, totalsteps, dts, br);
                }
                if (!repeatertypeFound) {
                    throw new LPGException("Unknown repeater type");
                }

                return br;
            }
        }

        [ItemNotNull]
        [NotNull]
        public BitArray GetOneYearHourArray([NotNull] TemperatureProfile temperatureProfile, [NotNull] GeographicLocation geoloc,
            [NotNull] Random random, [ItemNotNull] [NotNull] List<VacationTimeframe> vacationTimeframes, [NotNull] string holidayKey,
            [NotNull] out List<DateTime> bridgeDays)
        {
            var year = DateTime.Now.Year;
            var stepsize = new TimeSpan(1, 0, 0);
            var startDateTime = new DateTime(year, 1, 1);
            var endDateTime = new DateTime(year + 1, 1, 1);
            var arr = GetOneYearArray(stepsize, startDateTime, endDateTime, temperatureProfile, geoloc, random,
                vacationTimeframes, holidayKey, out bridgeDays,0,0,0,0);
            return arr;
        }

        [NotNull]
        public string GetRecursiveDescription(int level)
        {
            if (level > 15) {
                throw new LPGException("something went wrong!");
            }
            var s = GetDescription();
            var builder = new StringBuilder();
            builder.Append(s);
            foreach (var timeLimitEntry in _subentries) {
                builder.Append(Environment.NewLine + string.Empty.PadLeft(level) + timeLimitEntry.GetRecursiveDescription(level + 1));
            }
            s = builder.ToString();
            return s;
        }

        [NotNull]
        private static List<int> GetYears(DateTime startDate, DateTime endDate)
        {
            var years = new List<int>();

            var startyear = startDate.Year;
            var endyear = endDate.Year;
            while (startyear <= endyear) {
                years.Add(startyear++);
            }
            return years;
        }

        private static bool IsDayAHoliday([NotNull] Dictionary<DateTime, Holiday.HolidayType> holidays, DateTime date)
        {
            var dt = new DateTime(date.Year, date.Month, date.Day);
            if (holidays.ContainsKey(dt)) {
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TimeLimitEntry> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(dateBasedProfiles: dateBasedProfiles);
            var loadresult = LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic,
                ignoreMissingTables, false);
            if (ignoreMissingTables && loadresult == LoadResults.TableNotFound) {
                LoadAllFromDatabase(result, connectionString, TableNameOld, AssignFields, aic, ignoreMissingTables,
                    false);
            }
            SetTree(result);
        }
        private void RandomizeTime(TimeSpan start, TimeSpan end, out TimeSpan startout, out TimeSpan endout, [NotNull] Random r)
        {
            if (_randomizeTimeAmount == 0) {
                startout = start;
                endout = end;
                return;
            }
            var minutes = (int) (r.Next((int) (_randomizeTimeAmount * 2.0)) - _randomizeTimeAmount / 2.0);
            var ts1 = new TimeSpan(0, minutes, 0);
            startout = start.Add(ts1);
            var minutes2 = (int) (r.Next((int) (_randomizeTimeAmount * 2.0)) - _randomizeTimeAmount / 2.0);
            var ts2 = new TimeSpan(0, minutes2, 0);
            endout = end.Add(ts2);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var boolEntry in _subentries) {
                boolEntry.SaveToDB();
            }
        }

        private bool SetDateProfileControlled(TimeSpan stepsize, DateTime startDateTime, DateTime endDateTime,
            int totalsteps, [ItemNotNull] [NotNull] BitArray br, [NotNull] Random r)
        {
            if (DateBasedProfile != null) {
                var valueArray = DateBasedProfile.GetValueArray(startDateTime, endDateTime, stepsize);
                for (var i = 0; i < totalsteps; i++) {
                    var minvariation = (r.NextDouble() - 0.5) * _dateProfileMinVariation * 2;
                    var maxvariation = (r.NextDouble() - 0.5) * _dateProfileMaxVariation * 2;
                    if (valueArray[i] >= _minDateProfileValue - minvariation &&
                        valueArray[i] <= _maxDateProfileValue + maxvariation) {
                        br[i] = true;
                    }
                }
            }
            return true;
        }

        private bool SetDateRangeControlled(DateTime startDateTime, DateTime endDateTime, int totalsteps,
            [NotNull] DateTime[] dts, [ItemNotNull] [NotNull] BitArray br)
        {
            var dateRanges = GetDateRanges(GetYears(startDateTime, endDateTime));

            for (var i = 0; i < totalsteps; i++) {
                foreach (var dateRange in dateRanges) {
                    if (dts[i] > dateRange.Item1 && dts[i] < dateRange.Item2) {
                        br[i] = true;
                    }
                }
            }
            return true;
        }

        private bool SetEveryDay(int totalsteps, [NotNull] DateTime[] dts, TimeSpan mystarttime, TimeSpan myendtime, [ItemNotNull] [NotNull] BitArray br,
            [NotNull] Random r)
        {
            for (var i = 0; i < totalsteps; i++) {
                RandomizeTime(mystarttime, myendtime, out var start, out var end, r);
                if (dts[i].DayOfYear % _dailyDaycount == 0 && dts[i].TimeOfDay >= start &&
                    dts[i].TimeOfDay < end) {
                    br[i] = true;
                }
            }

            return true;
        }

        private bool SetEveryXMonths(int totalsteps, [NotNull] DateTime[] dts, TimeSpan mystarttime, TimeSpan myendtime,
            [ItemNotNull] [NotNull] BitArray br, [NotNull] Random r)
        {
            for (var i = 0; i < totalsteps; i++) {
                var month = dts[i].Month;
                var day = dts[i].Day;
                RandomizeTime(mystarttime, myendtime, out var start, out var end, r);
                if (month % _monthlyMonthCount == 0 && day == _monthlyDay && dts[i].TimeOfDay >= start &&
                    dts[i].TimeOfDay < end) {
                    br[i] = true;
                }
            }
            return true;
        }

        private bool SetHolidayControlled([NotNull] GeographicLocation geoloc, int totalsteps, [NotNull] DateTime[] dts, [ItemNotNull] [NotNull] BitArray br)
        {
            var holidaydict = geoloc.CalculatePureHolidayDict();
            for (var i = 0; i < totalsteps; i++) {
                var isholiday = false;
                foreach (var keyValuePair in holidaydict) {
                    var dt = keyValuePair.Key;
                    if (dts[i].Year == dt.Year && dts[i].Month == dt.Month && dts[i].Day == dt.Day) {
                        isholiday = true;
                        break;
                    }
                }
                if (isholiday && _duringHoliday) {
                    br[i] = true;
                }
            }
            return true;
        }

        private bool SetLightControlled(TimeSpan stepsize, DateTime startDateTime, DateTime endDateTime,
            [NotNull] GeographicLocation geoloc, int totalsteps, [ItemNotNull] [NotNull] BitArray br)
        {
            var st = new SunriseTimes(geoloc);
            var sunlightarr = st.MakeArray(totalsteps, startDateTime, endDateTime, stepsize);
            for (var i = 0; i < totalsteps; i++) {
                if (_needsLight) {
                    br[i] = sunlightarr[i];
                }
                else {
                    br[i] = !sunlightarr[i];
                }
            }

            return true;
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("StartTime", "@StartTime", _startTime);
            cmd.AddParameter("EndTime", "@EndTime", _endTime);
            cmd.AddParameter("RepeaterType", _repeaterType);
            cmd.AddParameter("DailyDaycount", _dailyDaycount);
            cmd.AddParameter("WeeklyWeekCount", _weeklyWeekCount);
            cmd.AddParameter("WeeklyMonday", _weeklyMonday);
            cmd.AddParameter("WeeklyTuesday", _weeklyTuesday);
            cmd.AddParameter("WeeklyWednesday", _weeklyWednesday);
            cmd.AddParameter("WeeklyThursday", _weeklyThursday);
            cmd.AddParameter("WeeklyFriday", _weeklyFriday);
            cmd.AddParameter("WeeklySaturday", _weeklySaturday);
            cmd.AddParameter("WeeklySunday", _weeklySunday);
            cmd.AddParameter("MonthlyMonthCount", _monthlyMonthCount);
            cmd.AddParameter("MonthlyDay", _monthlyDay);
            cmd.AddParameter("YearlyDay", _yearlyDay);
            cmd.AddParameter("YearlyMonth", _yearlyMonth);
            cmd.AddParameter("StartWeek", _startWeek);
            cmd.AddParameter("MinTemperature", _minTemperature);
            cmd.AddParameter("MaxTemperature", MaxTemperature);
            cmd.AddParameter("NeedsLight", _needsLight);
            cmd.AddParameter("NeedsDarkness", _needsDarkness);
            cmd.AddParameter("TimeLimitID", TimeLimitID);
            if (_parentEntryID != null) {
                cmd.AddParameter("ParentEntryID", _parentEntryID);
            }
            cmd.AddParameter("DateRangeStart", _dateRangeStart);
            cmd.AddParameter("DateRangeEnd", _dateRangeEnd);
            cmd.AddParameter("AnyAll", AnyAll);
            if (DateProfileID != null) {
                cmd.AddParameter("DateProfileID", DateProfileID);
            }
            cmd.AddParameter("MaxDateProfileValue", MaxDateProfileValue);
            cmd.AddParameter("MinDateProfileValue", MinDateProfileValue);
            cmd.AddParameter("DuringVacation", DuringVacation);
            cmd.AddParameter("DuringNotVacation", DuringNotVacation);
            cmd.AddParameter("DuringVacationLongerThan", DuringVacationLongerThan);
            cmd.AddParameter("DuringVacationShorterThan", DuringVacationShorterThan);
            cmd.AddParameter("VacationDurationLimit", VacationDurationLimit);
            cmd.AddParameter("DuringHoliday", DuringHoliday);
            cmd.AddParameter("RandomizeTimeAmount", _randomizeTimeAmount);
            cmd.AddParameter("DateProfileMinVariation", DateProfileMinVariation);
            cmd.AddParameter("DateProfileMaxVariation", DateProfileMaxVariation);
        }

        private bool SetTemperature(TimeSpan stepsize, DateTime startDateTime, DateTime endDateTime,
            [NotNull] TemperatureProfile tempProfile, int totalsteps, [ItemNotNull] [NotNull] BitArray br)
        {
            var temperatureArray = tempProfile.GetTemperatureArray(startDateTime, endDateTime, stepsize);
            for (var i = 0; i < totalsteps; i++) {
                if (temperatureArray[i] >= _minTemperature &&
                    temperatureArray[i] <= _maxTemperature) {
                    br[i] = true;
                }
            }
            return true;
        }

        private static void SetTree([NotNull] [ItemNotNull] ObservableCollection<TimeLimitEntry> entries)
        {
            foreach (var subEntry in entries) {
                if (subEntry.ParentEntryID != null) {
                    foreach (var parententry in entries) {
                        if (parententry.ID == subEntry.ParentEntryID) {
                            subEntry.ParentEntry = parententry;
                            parententry.Subentries.Add(subEntry);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool SetVacationControlled(DateTime startDateTime, DateTime endDateTime,
            [ItemNotNull] [NotNull] List<VacationTimeframe> vacationTimeframes, int totalsteps, [NotNull] DateTime[] dts, [ItemNotNull] [NotNull] BitArray br)
        {
            var dateTimes = new List<Tuple<DateTime, DateTime>>();
            var datesToRemap = new List<Tuple<DateTime, DateTime>>();
            foreach (var timeframe in vacationTimeframes) {
                if (!timeframe.MapToOtherYears) {
                    dateTimes.Add(new Tuple<DateTime, DateTime>(timeframe.StartDate, timeframe.EndDate));
                }
                else {
                    datesToRemap.Add(new Tuple<DateTime, DateTime>(timeframe.StartDate, timeframe.EndDate));
                }
            }
            var years = GetYears(startDateTime, endDateTime);
            foreach (var tuple in datesToRemap) {
                foreach (var year in years) {
                    var success = false;
                    var daystoadd = 0; // for shifting dates like the 29.2 to the 1.3.
                    var beginVacationDateTime = DateTime.Now;
                    while (!success && daystoadd < 10) {
                        try {
                            var startTemp = tuple.Item1.AddDays(daystoadd);
                            beginVacationDateTime = new DateTime(year, startTemp.Month, startTemp.Day);
                            success = true;
                        }
                        catch (Exception e) {
                            Logger.Error("While mapping vacations the following happened:" + e.Message);
                            Logger.Exception(e);
                        }
                        daystoadd++;
                    }
                    var endVacationDateTime = DateTime.Now;
                    daystoadd = 0;
                    success = false;
                    while (!success && daystoadd < 10) {
                        var endTemp = tuple.Item2.AddDays(daystoadd);
                        try {
                            if (DateTime.DaysInMonth(year, endTemp.Month) >= endTemp.Day) {
                                endVacationDateTime = new DateTime(year, endTemp.Month, endTemp.Day);
                                success = true;
                            }
                        }
                        catch (Exception e) {
                            Logger.Error("While mapping vacations the following happened:" + e.Message);
                        }
                        daystoadd++;
                    }
                    dateTimes.Add(new Tuple<DateTime, DateTime>(beginVacationDateTime, endVacationDateTime));
                }
            }
            for (var i = 0; i < totalsteps; i++) {
                var isvacation = false;
                var vacationDuration = 0;
                foreach (var dateTime in dateTimes) {
                    if (dts[i] >= dateTime.Item1 && dts[i] <= dateTime.Item2) {
                        isvacation = true;
                        vacationDuration = (int) (dateTime.Item2 - dateTime.Item1).TotalDays;
                        break;
                    }
                }
                if (isvacation && _duringVacation) {
                    br[i] = true;
                }
                if (!isvacation && _duringNotVacation) {
                    br[i] = true;
                }
                if (isvacation && _duringVacationShorterThan &&
                    vacationDuration <= _vacationDurationLimit) {
                    br[i] = true;
                }
                if (isvacation && _duringVacationLongerThan &&
                    vacationDuration >= _vacationDurationLimit) {
                    br[i] = true;
                }
            }
            return true;
        }

        private bool SetWeekly(DateTime startDateTime, int totalsteps, [NotNull] DateTime[] dts, TimeSpan mystarttime,
            TimeSpan myendtime, [ItemNotNull] [NotNull] BitArray br, [NotNull] Random r)
        {
            Calendar c = new GregorianCalendar(GregorianCalendarTypes.USEnglish);
            for (var i = 0; i < totalsteps; i++) {
                var weekofyear = c.GetWeekOfYear(dts[i], CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                var weeksinceStart = (int) ((dts[i] - startDateTime).TotalDays / 7);
                var offsettedweek = weekofyear + _startWeek;
                var dw = dts[i].DayOfWeek;
                if (offsettedweek % _weeklyWeekCount == 0 && _startWeek < weeksinceStart) {
                    if (WeeklyMonday && dw == DayOfWeek.Monday || WeeklyTuesday && dw == DayOfWeek.Tuesday ||
                        WeeklyWednesday && dw == DayOfWeek.Wednesday ||
                        WeeklyThursday && dw == DayOfWeek.Thursday || WeeklyFriday && dw == DayOfWeek.Friday ||
                        WeeklySaturday && dw == DayOfWeek.Saturday || WeeklySunday && dw == DayOfWeek.Sunday) {
                        RandomizeTime(mystarttime, myendtime, out var start, out var end, r);

                        if (dts[i].TimeOfDay >= start && dts[i].TimeOfDay <= end) {
                            br[i] = true;
                        }
                    }
                }
            }
            return true;
        }

        private bool SetWorkday(int totalsteps, [NotNull] DateTime[] dts, [NotNull] GeographicLocation geoloc, [NotNull] Random r,
            TimeSpan mystarttime, TimeSpan myendtime, [ItemNotNull] [NotNull] BitArray br, [NotNull] string holidayKey, [NotNull] out List<DateTime> bridgeDays)
        {
            var holidays = geoloc.GetHolidayDictWithBridge(r, holidayKey);
            for (var i = 0; i < totalsteps; i++) {
                if ((dts[i].DayOfWeek == DayOfWeek.Monday || dts[i].DayOfWeek == DayOfWeek.Tuesday ||
                     dts[i].DayOfWeek == DayOfWeek.Wednesday || dts[i].DayOfWeek == DayOfWeek.Thursday ||
                     dts[i].DayOfWeek == DayOfWeek.Friday) && !IsDayAHoliday(holidays, dts[i])) {
                    RandomizeTime(mystarttime, myendtime, out var start, out var end, r);
                    if (dts[i].TimeOfDay >= start && dts[i].TimeOfDay <= end) {
                        br[i] = true;
                    }
                }
            }
            bridgeDays = new List<DateTime>();
            foreach (var holiday in holidays) {
                if (holiday.Value == Holiday.HolidayType.BridgeDay) {
                    bridgeDays.Add(holiday.Key);
                }
            }
            return true;
        }

        private bool SetYearly(int totalsteps, [NotNull] DateTime[] dts, TimeSpan mystarttime, TimeSpan myendtime, [ItemNotNull] [NotNull] BitArray br,
            [NotNull] Random r)
        {
            for (var i = 0; i < totalsteps; i++) {
                var month = dts[i].Month;
                var day = dts[i].Day;
                RandomizeTime(mystarttime, myendtime, out var start, out var end, r);
                if (month == _yearlyMonth && day == _yearlyDay && dts[i].TimeOfDay >= start &&
                    dts[i].TimeOfDay <= end) {
                    br[i] = true;
                }
            }
            return true;
        }
    }
}