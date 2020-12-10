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
using Common;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DateProfileDataPoint : DBBase, IComparable<DateProfileDataPoint> {
        public const string TableName = "tblDateProfilePoints";
        private DateTime _dateAndTime;
        private double _value;

        public DateProfileDataPoint(DateTime dt,
                                    double pvalue,
                                    [CanBeNull]int? pID,
                                    int profileID,
                                    [NotNull] string connectionString,
                                    StrGuid guid)
            : base(dt.ToString(CultureInfo.InvariantCulture), pID, TableName, connectionString,
                guid) {
            _dateAndTime = dt;
            _value = pvalue;
            ProfileID = profileID;
            TypeDescription = "Date Profile Data Point";
        }

        [UsedImplicitly]
        public DateTime DateAndTime {
            get => _dateAndTime;
            set => SetValueWithNotify(value, ref _dateAndTime, nameof(DateAndTime));
        }

        [NotNull]
        [UsedImplicitly]
        public string DateAndTimeString {
            get => _dateAndTime.ToString(CultureInfo.CurrentCulture);
            set {
                var success = DateTime.TryParse(value, out var dt);
                if (!success) {
                    dt = DateTime.Now;
                }
                SetValueWithNotify(dt, ref _dateAndTime, nameof(DateAndTimeString));
                OnPropertyChanged(nameof(DateAndTime));
            }
        }

        public int ProfileID { get; }

        public double Value {
            get => _value;
            [UsedImplicitly] set => SetValueWithNotify(value, ref _value, nameof(Value));
        }

        public int CompareTo([CanBeNull] DateProfileDataPoint other) {
            if (other == null) {
                return 0;
            }
            return DateAndTime.CompareTo(other.DateAndTime);
        }

        [NotNull]
        private static DateProfileDataPoint AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var datapointID = dr.GetIntFromLong("ID");
            var timeBasedProfileID = dr.GetIntFromLong("DateBasedProfileID");
            var time = dr.GetDateTime("DateAndTime");
            var value = dr.GetDouble("Value");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new DateProfileDataPoint(time, value, datapointID, timeBasedProfileID, connectionString,guid);
        }

        public override int CompareTo(object obj) {
            if (!(obj is DateProfileDataPoint other))
            {
                return 0;
            }
            return _dateAndTime.CompareTo(other._dateAndTime);
        }

        public void DeleteAllForOneProfile(int profid) {
            if (ID != null) {
                using (var con = new Connection(ConnectionString)) {
                    con.Open();
                    using (var cmd = new Command(con)) {
                        const string cmdstring = "DELETE FROM " + TableName + " WHERE DateBasedProfileID = @profid";
                        cmd.AddParameter("profid", profid);
                        cmd.ExecuteNonQuery(cmdstring);
                    }
                    Logger.Info("Deleted all the entries for the profile " + profid + " from the data base.");
                }
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DateProfileDataPoint> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("DateBasedProfileID", ProfileID);
            cmd.AddParameter("DateAndTime", DateAndTime);
            cmd.AddParameter("Value", _value);
        }
    }
}