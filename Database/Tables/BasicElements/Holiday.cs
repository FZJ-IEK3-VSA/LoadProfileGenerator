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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Automation;
using Common;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class Holiday : DBBaseElement {
        public enum HolidayType {
            Holiday,
            BridgeDay
        }

        public const string TableName = "tblHolidays";
        [NotNull] private static readonly Dictionary<string, double> _randomValueDict = new Dictionary<string, double>();
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HolidayDate> _holidayDates;

        [NotNull] private readonly Dictionary<DayOfWeek, HolidayProbabilities> _probabilities =
            new Dictionary<DayOfWeek, HolidayProbabilities>();

        [NotNull] private string _description;

        public Holiday([NotNull] string name, [NotNull] string description, [NotNull] string connectionString, StrGuid guid, [CanBeNull] int? pID = null) : base(name,
            TableName, connectionString, guid) {
            _holidayDates = new ObservableCollection<HolidayDate>();
            ID = pID;
            TypeDescription = "Holiday";
            _description = description;
        }

        [NotNull]
        [UsedImplicitly]
        public string DateString {
            get {
                var builder = new StringBuilder();
                foreach (var date in _holidayDates) {
                    builder.Append(date.DateAndTime.ToLongDateString()).Append(", ");
                }
                var s = builder.ToString();
                return s.Substring(0, s.Length - 2);
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HolidayDate> HolidayDates => _holidayDates;

        [NotNull]
        [UsedImplicitly]
        public HolidayProbabilities ProbFriday => GetHolidayProbability(DayOfWeek.Friday);

        [NotNull]
        public HolidayProbabilities ProbMonday => GetHolidayProbability(DayOfWeek.Monday);

        [NotNull]
        [UsedImplicitly]
        public HolidayProbabilities ProbThursday => GetHolidayProbability(DayOfWeek.Thursday);

        [NotNull]
        [UsedImplicitly]
        public HolidayProbabilities ProbTuesday => GetHolidayProbability(DayOfWeek.Tuesday);

        [NotNull]
        [UsedImplicitly]
        public HolidayProbabilities ProbWednesday => GetHolidayProbability(DayOfWeek.Wednesday);

        public void AddNewDate(DateTime dt) {
            var tp = new HolidayDate(dt, IntID, ConnectionString, null, System.Guid.NewGuid().ToStrGuid());
            _holidayDates.Add(tp);
            _holidayDates.Sort();
            SaveToDB();
        }

        [NotNull]
        private static Holiday AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new Holiday(name, description, connectionString, guid,id);
        }

        [NotNull]
        public List<DateTime> CollectListOfHolidays() {
            var freedays = new List<DateTime>();

            foreach (var holidayDate in _holidayDates) {
                freedays.Add(holidayDate.DateAndTime);
            }
            return freedays;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new Holiday(
            FindNewName(isNameTaken, "New Holiday "), "(no description)", connectionString, System.Guid.NewGuid().ToStrGuid());

        public override void DeleteFromDB() {
            base.DeleteFromDB();
            foreach (var date in _holidayDates) {
                date.DeleteFromDB();
            }
            foreach (var value in _probabilities.Values) {
                value.DeleteFromDB();
            }
        }

        public void DeleteHoliday([NotNull] HolidayDate tp) {
            tp.DeleteFromDB();
            _holidayDates.Remove(tp);
        }

        [NotNull]
        private HolidayProbabilities GetHolidayProbability(DayOfWeek dayOfWeek) {
            if (!_probabilities.ContainsKey(dayOfWeek)) {
                var hp =
                    new HolidayProbabilities(IntID, null, dayOfWeek,
                        0, 0, 0, 0, 0, ConnectionString, System.Guid.NewGuid().ToStrGuid());
                _probabilities.Add(dayOfWeek, hp);
                hp.SaveToDB();
                return hp;
            }
            return _probabilities[dayOfWeek];
        }

        [NotNull]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public Dictionary<DateTime, HolidayType> GetListOfWorkFreeDates([NotNull] Random random, [NotNull] string genericKey) {
            var freedays = new Dictionary<DateTime, HolidayType>();
            double randomResult;
            var key = genericKey + "#" + IntID;
            if (_randomValueDict.ContainsKey(key)) {
                randomResult = _randomValueDict[key];
            }
            else {
                randomResult = random.NextDouble() * 99 + 1;
                _randomValueDict.Add(key, randomResult);
            }
            foreach (var holidayDate in _holidayDates) {
                freedays.Add(holidayDate.DateAndTime, HolidayType.Holiday);
                var hp = GetHolidayProbability(holidayDate.DateAndTime.DayOfWeek);
                if (holidayDate.DateAndTime.DayOfWeek == DayOfWeek.Monday) {
                    if (hp.Tuesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(1), HolidayType.BridgeDay);
                    }
                    if (hp.Wednesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(2), HolidayType.BridgeDay);
                    }
                    if (hp.Thursday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(3), HolidayType.BridgeDay);
                    }
                    if (hp.Friday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(4), HolidayType.BridgeDay);
                    }
                }
                if (holidayDate.DateAndTime.DayOfWeek == DayOfWeek.Tuesday) {
                    if (hp.Monday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-1), HolidayType.BridgeDay);
                    }
                    if (hp.Wednesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(1), HolidayType.BridgeDay);
                    }
                    if (hp.Thursday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(2), HolidayType.BridgeDay);
                    }
                    if (hp.Friday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(3), HolidayType.BridgeDay);
                    }
                }
                if (holidayDate.DateAndTime.DayOfWeek == DayOfWeek.Wednesday) {
                    if (hp.Monday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-2), HolidayType.BridgeDay);
                    }
                    if (hp.Tuesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-1), HolidayType.BridgeDay);
                    }
                    if (hp.Thursday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(1), HolidayType.BridgeDay);
                    }
                    if (hp.Friday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(2), HolidayType.BridgeDay);
                    }
                }
                if (holidayDate.DateAndTime.DayOfWeek == DayOfWeek.Thursday) {
                    if (hp.Monday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-3), HolidayType.BridgeDay);
                    }
                    if (hp.Tuesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-2), HolidayType.BridgeDay);
                    }
                    if (hp.Wednesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-1), HolidayType.BridgeDay);
                    }
                    if (hp.Friday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(1), HolidayType.BridgeDay);
                    }
                }
                if (holidayDate.DateAndTime.DayOfWeek == DayOfWeek.Friday) {
                    if (hp.Monday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-4), HolidayType.BridgeDay);
                    }
                    if (hp.Tuesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-3), HolidayType.BridgeDay);
                    }
                    if (hp.Wednesday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-2), HolidayType.BridgeDay);
                    }
                    if (hp.Thursday > randomResult) {
                        freedays.Add(holidayDate.DateAndTime.AddDays(-1), HolidayType.BridgeDay);
                    }
                }
            }
            return freedays;
        }

        [NotNull]
        [UsedImplicitly]
        public static Holiday ImportFromItem([NotNull] Holiday toImport, [NotNull] Simulator dstsim) {
            var hd = new Holiday(toImport.Name, toImport.Description,
                dstsim.ConnectionString, toImport.Guid);
            hd.SaveToDB();
            foreach (var holidayDate in toImport.HolidayDates) {
                hd.AddNewDate(holidayDate.DateAndTime);
            }
            foreach (var hpro in toImport._probabilities.Values) {
                var hp = new HolidayProbabilities(hd.IntID, null, hpro.DayOfWeek, hpro.Monday,
                    hpro.Tuesday, hpro.Wednesday,
                    hpro.Thursday, hpro.Friday, dstsim.ConnectionString, hpro.Guid);
                hd._probabilities.Add(hpro.DayOfWeek, hp);
            }
            hd.SaveToDB();
            return hd;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child) {
            var hd = (HolidayDate) child;
            if (parent.ID == hd.HolidayID) {
                var tp = (Holiday) parent;
                tp._holidayDates.Add(hd);
                return true;
            }
            return false;
        }

        private static bool IsCorrectProbabilityParent([NotNull] DBBase parent, [NotNull] DBBase child) {
            var hd = (HolidayProbabilities) child;
            if (parent.ID == hd.HolidayID) {
                var tp = (Holiday) parent;
                if(!tp._probabilities.ContainsKey(hd.DayOfWeek)) {
                    tp._probabilities.Add(hd.DayOfWeek, hd);
                }

                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<Holiday> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var holidayDates = new ObservableCollection<HolidayDate>();
            HolidayDate.LoadFromDatabase(holidayDates, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(holidayDates), IsCorrectParent, ignoreMissingTables);
            foreach (var holiday in result) {
                holiday.HolidayDates.Sort();
            }
            var probabilities = new ObservableCollection<HolidayProbabilities>();
            HolidayProbabilities.LoadFromDatabase(probabilities, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(probabilities), IsCorrectProbabilityParent,
                ignoreMissingTables);
        }

        public override void SaveToDB() {
            base.SaveToDB();
            foreach (var holidayDate in _holidayDates) {
                holidayDate.SaveToDB();
            }
            foreach (var probabilities in _probabilities.Values) {
                probabilities.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
        }

        public override string ToString() => Name + "\t Dates:" + _holidayDates.Count;
        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((Holiday) toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}