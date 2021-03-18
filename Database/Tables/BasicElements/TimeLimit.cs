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

#region import

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicElements {
    public class TimeLimit : DBBaseElement {
        private const string OldTableName = "tblDeviceTimes";
        public const string TableName = "tblTimeLimits";

        [CanBeNull] private TimeLimitEntry _rootEntry;

        public TimeLimit([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] string connectionString,[JetBrains.Annotations.NotNull] StrGuid guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            if (guid == null) {
                throw new ArgumentNullException(nameof(guid));
            }

            if (guid == null) {
                throw new ArgumentNullException(nameof(guid));
            }

            ID = pID;
            TypeDescription = "Time Limit";
        }

        [CanBeNull]
        [UsedImplicitly]
        public TimeLimitEntry RootEntry {
            get => _rootEntry;
            set => SetValueWithNotify(value, ref _rootEntry, false, nameof(RootEntry));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TimeLimitEntry> TimeLimitEntries { get; } =
            new ObservableCollection<TimeLimitEntry>();

        [JetBrains.Annotations.NotNull]
        public TimeLimitEntry AddTimeLimitEntry([CanBeNull] TimeLimitEntry parent,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles, bool savetoDB = true)
        {
            int? parentID;
            if (parent == null) {
                parentID = null;
            }
            else {
                parentID = parent.IntID;
            }

            var dte =
                TimeLimitEntry.CreateDefaultEntry(IntID, parentID, ConnectionString, dateBasedProfiles);
            dte.ParentEntry = parent;
            TimeLimitEntries.Add(dte);

            // set either root entry or add to parent entry
            if (parent == null) {
                if (_rootEntry == null) {
                    _rootEntry = dte;
                }
                else {
                    throw new LPGException("Time limit " + Name +
                                           " already has a root entry! you can't add a second one.");
                }
            }
            else {
                parent.Subentries.Add(dte);
            }

            if (savetoDB) {
                dte.SaveToDB();
            }

            return dte;
        }

        [JetBrains.Annotations.NotNull]
        public TimeLimitEntry
            AddTimeLimitEntry([CanBeNull] TimeLimitEntry parent, [JetBrains.Annotations.NotNull] TimeLimitEntry fc,
                [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles, bool savetoDB = true) => AddTimeLimitEntry(
            parent, fc.StartTimeDateTime, fc.EndTimeDateTime, fc.RepeaterType, fc.DailyDayCount, fc.WeeklyWeekCount,
            fc.WeeklyMonday, fc.WeeklyTuesday, fc.WeeklyWednesday, fc.WeeklyThursday, fc.WeeklyFriday,
            fc.WeeklySaturday, fc.WeeklySunday, fc.MonthlyMonthCount, fc.MonthlyDay, fc.YearlyDay, fc.YearlyMonth,
            fc.StartWeek, fc.MinTemperature, fc.MaxTemperature, fc.NeedsLight, fc.NeedsDarkness, fc.AnyAll,
            fc.DateRangeStart, fc.DateRangeEnd, fc.DateProfileID, fc.MaxDateProfileValue, fc.MinDateProfileValue,
            fc.DuringVacation, fc.DuringNotVacation, fc.DuringVacationLongerThan, fc.DuringVacationShorterThan,
            fc.VacationDurationLimit, fc.DuringHoliday, dateBasedProfiles, fc.RandomizeTimeAmount,
            fc.DateProfileMinVariation, fc.DateProfileMaxVariation, savetoDB);

        [JetBrains.Annotations.NotNull]
        public TimeLimitEntry AddTimeLimitEntry([CanBeNull] TimeLimitEntry parent, DateTime startTime, DateTime endTime,
            PermissionMode repeaterType, int dailyDaycount, int weeklyWeekCount, bool weeklyMonday, bool weeklyTuesday,
            bool weeklyWednesday, bool weeklyThursday, bool weeklyFriday, bool weeklySaturday, bool weeklySunday,
            int monthlyMonthCount, int monthlyDay, int yearlyDay, int yearlyMonth, int startWeek, double minTemperature,
            double maxTemperature, bool needsLight, bool needsDarkness, AnyAllTimeLimitCondition anyall,
            DateTime dateRangeStart, DateTime dateRangeEnd, [CanBeNull] int? dateProfileID, double maxDateValue,
            double minDateValue, bool duringVacation, bool duringNotVacation, bool duringVacationLongerThan,
            bool duringVacationShorterThan, int vacationDurationLimit, bool duringHoliday,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles, int randomizeTimeAmount,
            double dateBasedMinVariation, double dateBasedMaxVariation, bool savetoDB = true)
        {
            int? parentID;
            if (parent == null) {
                parentID = null;
            }
            else {
                parentID = parent.IntID;
            }

            var dte = new TimeLimitEntry(null, IntID, startTime, endTime, repeaterType, dailyDaycount,
                weeklyWeekCount, weeklyMonday, weeklyTuesday, weeklyWednesday, weeklyThursday, weeklyFriday,
                weeklySaturday, weeklySunday, monthlyMonthCount, monthlyDay, yearlyDay, yearlyMonth, startWeek,
                minTemperature, maxTemperature, needsLight, needsDarkness, parentID, anyall, dateRangeStart,
                dateRangeEnd, dateProfileID, maxDateValue, minDateValue, duringVacation, duringNotVacation,
                duringVacationLongerThan, duringVacationShorterThan, vacationDurationLimit, duringHoliday,
                dateBasedProfiles, ConnectionString, randomizeTimeAmount, dateBasedMinVariation, dateBasedMaxVariation,
                System.Guid.NewGuid().ToStrGuid())
            {
                ParentEntry = parent
            };
            TimeLimitEntries.Add(dte);

            // set either root entry or add to parent entry
            if (parent == null) {
                if (RootEntry == null) {
                    RootEntry = dte;
                }
                else {
                    throw new LPGException("Time limit " + Name +
                                           " already has a root entry! you can't add a second one.");
                }
            }
            else {
                parent.Subentries.Add(dte);
            }

            if (savetoDB) {
                dte.SaveToDB();
            }

            return dte;
        }

        [JetBrains.Annotations.NotNull]
        public string CombineCompleteString()
        {
            var s = Name + Environment.NewLine;
            s += RootEntry?.GetRecursiveDescription(1);
            return s;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString)
        {
            var dc = new TimeLimit(FindNewName(isNameTaken, "New time limit "),
                connectionString, System.Guid.NewGuid().ToStrGuid());
            return dc;
        }

        public void DeleteTimeLimitEntryFromDB([JetBrains.Annotations.NotNull] TimeLimitEntry db)
        {
            if (db.ParentEntry != null) {
                foreach (var subEntry in db.Subentries) {
                    subEntry.ParentEntry = db.ParentEntry;
                    db.ParentEntry.Subentries.Add(subEntry);
                }
            }

            var entriesToDelete = new List<TimeLimitEntry>();

            foreach (var entry in TimeLimitEntries) {
                if (entry == db) {
                    entriesToDelete.Add(entry);
                }
            }

            foreach (var timeLimitEntry in entriesToDelete) {
                timeLimitEntry.DeleteFromDB();
                TimeLimitEntries.Remove(timeLimitEntry);
            }

            var parent = db.ParentEntry;
            parent?.Subentries.Remove(db);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport,Simulator dstSim)
            => ImportFromItem((TimeLimit)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();
            foreach (var affordance in sim.Affordances.Items) {
                if (affordance.TimeLimit == this) {
                    used.Add(new UsedIn(affordance, "Affordance"));
                }
            }

            foreach (var hh in sim.HouseholdTraits.Items) {
                foreach (var autodev in hh.Autodevs) {
                    if (autodev.TimeLimit == this) {
                        used.Add(new UsedIn(hh, "Household Trait"));
                    }
                }

                foreach (HHTLocation hhtLocation in hh.Locations) {
                    foreach (HHTAffordance affordanceLocation in hhtLocation.AffordanceLocations) {
                        if (affordanceLocation.TimeLimit == this) {
                            used.Add(new UsedIn(hh, "Household Trait Affordance Time Limit"));
                        }
                    }
                }
            }

            foreach (var hh in sim.HouseTypes.Items) {
                foreach (var autodev in hh.HouseDevices) {
                    if (autodev.TimeLimit == this) {
                        used.Add(new UsedIn(hh, "Housetype"));
                    }
                }
            }

            foreach (var geoloc in sim.GeographicLocations.Items) {
                if (geoloc.LightTimeLimit == this) {
                    used.Add(new UsedIn(geoloc, "Geographic Location"));
                }
            }

            return used;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static TimeLimit ImportFromItem([JetBrains.Annotations.NotNull] TimeLimit toImport,  [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            var dt = new TimeLimit(toImport.Name, dstSim.ConnectionString,
                toImport.Guid);

            dt.SaveToDB();
            if(toImport.RootEntry == null) {
                throw new LPGException("Rootentry was null");
            }

            dt.RootEntry = RecursiveImport(dt, toImport.RootEntry, null, dstSim.DateBasedProfiles.Items);
            dt.SaveToDB();
            return dt;
        }

        public void ImportFromOtherTimeLimit([JetBrains.Annotations.NotNull] TimeLimit other, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles)
        {
            if (_rootEntry == null) {
                AddTimeLimitEntry(null, dateBasedProfiles);
            }

            if (RootEntry != null && other.RootEntry != null) {
                RootEntry.CopyEverything(other.RootEntry);
                RecursiveCopy(RootEntry, other.RootEntry, dateBasedProfiles);
            }
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeLimit> result,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();

            var loadresult = LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic,
                ignoreMissingTables, true);
            if (ignoreMissingTables && loadresult == LoadResults.TableNotFound) {
                LoadAllFromDatabase(result, connectionString, OldTableName, AssignFields, aic, ignoreMissingTables,
                    true);
            }

            var boolentries = new ObservableCollection<TimeLimitEntry>();
            TimeLimitEntry.LoadFromDatabase(boolentries, connectionString, dateBasedProfiles, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(boolentries), IsCorrectBoolEntry,
                ignoreMissingTables);
            // set rootelement
            var items2Delete = new List<TimeLimitEntry>();
            foreach (var timeLimit in result) {
                foreach (var timeLimitEntry in timeLimit.TimeLimitEntries) {
                    if (timeLimitEntry.ParentEntry == null) {
                        if (timeLimit.RootEntry != null) {
                            Logger.Error("Two root entries found while loading a device time: " + timeLimit.Name +
                                         " deleting one.");
                            items2Delete.Add(timeLimit.RootEntry);
                        }

                        timeLimit._rootEntry = timeLimitEntry;
                    }

                    if (timeLimitEntry.ParentEntry != null &&
                        timeLimitEntry.ParentEntry.TimeLimitID != timeLimitEntry.TimeLimitID) {
                        Logger.Error("Found a subentry from different time limits mixed up: " + timeLimit.Name +
                                     " deleting the subentry..");
                        items2Delete.Add(timeLimitEntry);
                    }
                }

                foreach (var timeLimitEntry in items2Delete) {
                    timeLimit.DeleteTimeLimitEntryFromDB(timeLimitEntry);
                }
            }
        }

        public override void SaveToDB()
        {
            base.SaveToDB();

            foreach (var boolEntry in TimeLimitEntries) {
                boolEntry.SaveToDB();
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
        }

        [JetBrains.Annotations.NotNull]
        private static TimeLimit AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name");
            var id = dr.GetIntFromLong("ID");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TimeLimit(name, connectionString,guid, id);
        }

        private static bool IsCorrectBoolEntry([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (TimeLimitEntry) child;

            if (parent.ID == hd.TimeLimitID) {
                var dt = (TimeLimit) parent;
                dt.TimeLimitEntries.Add(hd);
                return true;
            }

            return false;
        }

        private void RecursiveCopy([JetBrains.Annotations.NotNull] TimeLimitEntry newparent, [JetBrains.Annotations.NotNull] TimeLimitEntry oldParent,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles)
        {
            foreach (var entry in oldParent.Subentries) {
                AddTimeLimitEntry(newparent, entry, dateBasedProfiles);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static TimeLimitEntry RecursiveImport([JetBrains.Annotations.NotNull] TimeLimit newTimeLimit, [JetBrains.Annotations.NotNull] TimeLimitEntry oldEntry,
            [CanBeNull] TimeLimitEntry newParent, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles)
        {
            var newEntry = newTimeLimit.AddTimeLimitEntry(newParent, oldEntry, dateBasedProfiles);
            foreach (var boolEntry in oldEntry.Subentries) {
                RecursiveImport(newTimeLimit, boolEntry, newEntry, dateBasedProfiles);
            }

            return newEntry;
        }
    }
}