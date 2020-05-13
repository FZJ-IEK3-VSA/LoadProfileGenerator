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
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicElements {
    public class TemperatureValue : DBBase {
        public const string TableName = "tblTemperatures";
        private readonly int _tempProfileID;
        private readonly DateTime _time;
        private readonly double _value;

        public TemperatureValue(DateTime time, double value, int tempProfileID, [CanBeNull]int? id, [NotNull] string connectionString, [NotNull] StrGuid guid)
            : base(
                time.ToShortDateString() + " " + value.ToString(CultureInfo.InvariantCulture), id, TableName,
                connectionString, guid)
        {
            _time = time;
            _value = value;
            _tempProfileID = tempProfileID;
            TypeDescription = "Temperature Value";
        }

        [UsedImplicitly]
        public double DoubleValue => _value;

        public int TempProfileID => _tempProfileID;

        public DateTime Time => _time;

        [NotNull]
        [UsedImplicitly]
        public string TimeString => _time.ToString(CultureInfo.CurrentCulture);

        public double Value => _value;

        [NotNull]
        private static TemperatureValue AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var date = dr.GetDateTime("Date");
            var value = dr.GetDouble("Temperatur");
            var tempProfileID = dr.GetInt("tempProfileID");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TemperatureValue(date, value, tempProfileID, id, connectionString,guid );
        }

        #region IComparable Members

        public override int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is TemperatureValue other))
            {
                throw new LPGException("Invalid comparison");
            }
            return _time.CompareTo(other._time);
        }

        #endregion

        public override int CompareTo([CanBeNull] BasicElement other)
        {
            if (other is TemperatureValue tp) {
                return _time.CompareTo(tp._time);
            }
            return base.CompareTo(other);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TemperatureValue> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        public override void SaveToDB([NotNull] Connection con)
        {
            if (ID == null) {
                using (var cmd = new Command(con)) {
                    SetSqlParameters(cmd);
                    ID = cmd.ExecuteInsert(TableName);
                }
                NeedsUpdate = false;
            }

            if (GuidCreationCount > 0) {
                base.SaveToDB();
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Date", "@myDate", _time);
            cmd.AddParameter("Temperatur", "@myTemperatur", _value);
            cmd.AddParameter("TempProfileID", "@myTempProfileID", _tempProfileID);
        }

        public override string ToString() => _time.ToShortDateString() + " " + _time.ToShortTimeString() + ": " +
                                             _value.ToString(CultureInfo.CurrentCulture) + " °C";
    }
}