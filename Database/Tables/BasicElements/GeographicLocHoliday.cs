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
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class GeographicLocHoliday : DBBase {
        public const string TableName = "tblGeographicLocHolidays";

        [CanBeNull] private readonly Holiday _holiday;

        public GeographicLocHoliday([CanBeNull]int? pID, [CanBeNull] Holiday holiday, int geographicLocID, [JetBrains.Annotations.NotNull] string connectionString,
            [JetBrains.Annotations.NotNull] string holidayname, [NotNull] StrGuid guid) : base(holidayname, TableName, connectionString, guid)
        {
            ID = pID;
            _holiday = holiday;
            GeographicLocationID = geographicLocID;
            TypeDescription = "Geographic Location Holiday";
        }

        public int GeographicLocationID { get; }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public Holiday Holiday => _holiday ?? throw new InvalidOperationException();

        public override string Name {
            get {
                if (_holiday != null) {
                    return _holiday.Name;
                }
                return base.Name;
            }
        }

        [JetBrains.Annotations.NotNull]
        private static GeographicLocHoliday AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var holidayID = dr.GetIntFromLong("HolidayID");
            var geographicLocationID = dr.GetIntFromLong("GeographicLocationID");
            var holiday = aic.Holidays.FirstOrDefault(myhd => myhd.ID == holidayID);
            var holidayName = string.Empty;
            if (holiday != null) {
                holidayName = holiday.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var tdlt = new GeographicLocHoliday(id, holiday, geographicLocationID, connectionString,
                holidayName, guid);
            return tdlt;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_holiday == null) {
                message = "Holiday not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<GeographicLocHoliday> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Holiday> holidays, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(holidays: holidays);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new List<GeographicLocHoliday>();
            foreach (var geographicLocHoliday in result) {
                if (geographicLocHoliday._holiday == null) {
                    items2Delete.Add(geographicLocHoliday);
                }
            }
            foreach (var geographicLocHoliday in items2Delete) {
                geographicLocHoliday.DeleteFromDB();
                result.Remove(geographicLocHoliday);
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_holiday != null) {
                cmd.AddParameter("HolidayID", _holiday.IntID);
            }
            cmd.AddParameter("GeographicLocationID", GeographicLocationID);
        }

        public override string ToString() => Name;
    }
}