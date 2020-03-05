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
using System.Collections.ObjectModel;
using System.Globalization;
using Common;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicElements {
    public class TimeDataPoint : DBBase, IComparable<TimeDataPoint> {
        public const string TableName = "tblTimePoints";
        private TimeSpan _time;
        private double _value;

        public TimeDataPoint(TimeSpan ts, double pvalue, [CanBeNull]int? pID, int profileID, [NotNull] string connectionString, [NotNull] string guid)
            : base(ts.ToString(), pID, TableName, connectionString, guid) {
            TypeDescription = "Time Data Point";
            _time = ts;
            _value = pvalue;
            ProfileID = profileID;
        }

        public TimeDataPoint(DateTime dbtime, double pvalue, [CanBeNull]int? pID, int profileID, [NotNull] string connectionString, [NotNull] string guid)
            : base(dbtime.ToString(CultureInfo.InvariantCulture),
                pID, TableName, connectionString, guid) {
            TypeDescription = "Time Data Point";
            _time = dbtime - Config.DummyTime;
            _value = pvalue;
            ProfileID = profileID;
        }

        private DateTime DBTime => Config.DummyTime.Add(_time);

        public int ProfileID { get; }

        public TimeSpan Time {
            get => _time;
            [UsedImplicitly] set => SetValueWithNotify(value, ref _time, nameof(Time));
        }

        public double Value {
            get => _value;
            [UsedImplicitly] set => SetValueWithNotify(value, ref _value, nameof(Value));
        }

        public override int CompareTo([CanBeNull] object obj) {
            if (!(obj is TimeDataPoint other))
            {
                return 0;
            }
            return _time.CompareTo(other._time);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            message = "";
            return true;
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            cmd.AddParameter("TimeBasedProfileID", "@TimeBasedProfileID", ProfileID);
            cmd.AddParameter("Time", "@Time", DBTime);
            cmd.AddParameter("Value", "@Value", _value);
        }

        #region IComparable<TimeDataPoint> Members

        public int CompareTo([CanBeNull] TimeDataPoint other) {
            if (other == null) {
                return 0;
            }
            return Time.CompareTo(other.Time);
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TimeDataPoint> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        [NotNull]
        private static TimeDataPoint AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var datapointID = dr.GetIntFromLong("ID");
            var timeBasedProfileID = dr.GetInt("TimeBasedProfileID");
            var time = dr.GetDateTime("time");
            var value = dr.GetDouble("Value");
            var guid = GetGuid(dr, ignoreMissingFields);

            return new TimeDataPoint(time, value, datapointID, timeBasedProfileID, connectionString, guid);
        }

        #endregion
    }
}