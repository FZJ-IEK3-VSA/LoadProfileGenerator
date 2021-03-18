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
using System.Collections.ObjectModel;
using System.Globalization;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class HolidayDate : DBBase, IComparable<HolidayDate> {
        public const string TableName = "tblHolidayDates";
        private DateTime _dateAndTime;
        [NotNull] private readonly StrGuid _guid;

        public HolidayDate(DateTime dt, int holidayID, [JetBrains.Annotations.NotNull] string connectionString, [CanBeNull] int? pID, [NotNull] StrGuid guid)
            : base(dt.ToLongDateString(), pID, TableName, connectionString, guid)
        {
            _dateAndTime = dt;
            _guid = guid;
            HolidayID = holidayID;
            TypeDescription = "Holiday Date";
        }

        [UsedImplicitly]
        public DateTime DateAndTime {
            get => _dateAndTime;
            set {
                SetValueWithNotify(value, ref _dateAndTime, nameof(DateAndTime));
                OnPropertyChanged(nameof(DateAndTimeString));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string DateAndTimeString {
            get => _dateAndTime.ToString(CultureInfo.CurrentCulture);
            set {
                var success = DateTime.TryParse(value, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                SetValueWithNotify(dt, ref _dateAndTime, nameof(DateAndTime));
                OnPropertyChanged(nameof(DateAndTimeString));
            }
        }

        public int HolidayID { get; }

        public int CompareTo([CanBeNull] HolidayDate other)
        {
            if (other == null) {
                return 0;
            }
            return DateAndTime.CompareTo(other.DateAndTime);
        }

        [JetBrains.Annotations.NotNull]
        private static HolidayDate AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var holidayDateID = dr.GetIntFromLong("ID");
            var holidayID = dr.GetIntFromLong("HolidayID");
            var time = dr.GetDateTime("DateAndTime");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HolidayDate(time, holidayID, connectionString, holidayDateID, guid);
        }

        public override int CompareTo(object obj)
        {
            if (!(obj is HolidayDate other))
            {
                return 0;
            }
            return _dateAndTime.CompareTo(other._dateAndTime);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HolidayDate> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("HolidayID", HolidayID);
            cmd.AddParameter("DateAndTime", DateAndTime);
        }

        public override string ToString() => DateAndTime.ToLongDateString();
    }
}