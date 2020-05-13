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
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class HolidayProbabilities : DBBase {
        public const string TableName = "tblHolidayProbabilities";
        private DayOfWeek _dayOfWeek;
        private double _friday;
        private double _monday;
        private double _thursday;
        private double _tuesday;
        private double _wednesday;

        public HolidayProbabilities(int holidayID,[CanBeNull] int? pID, DayOfWeek dayOfWeek, double monday, double tuesday,
            double wednesday, double thursday, double friday, [NotNull] string connectionString, [NotNull] StrGuid guid)
            : base(dayOfWeek.ToString(), pID, TableName, connectionString, guid)
        {
            HolidayID = holidayID;
            _dayOfWeek = dayOfWeek;
            _monday = monday;
            _tuesday = tuesday;
            _wednesday = wednesday;
            _thursday = thursday;
            _friday = friday;
            TypeDescription = "Holiday Probabilities";
        }

        [UsedImplicitly]
        public DayOfWeek DayOfWeek {
            get => _dayOfWeek;
            set => SetValueWithNotify(value, ref _dayOfWeek, nameof(DayOfWeek));
        }

        [UsedImplicitly]
        public double Friday {
            get => _friday;
            set {
                var myvalue = value;
                if (value > 100) {
                    myvalue = 100;
                }
                if (value < 0) {
                    myvalue = 0;
                }
                SetValueWithNotify(myvalue, ref _friday, nameof(Friday));
            }
        }

        public int HolidayID { get; }

        [UsedImplicitly]
        public double Monday {
            get => _monday;
            set {
                var myvalue = value;
                if (value > 100) {
                    myvalue = 100;
                }
                if (value < 0) {
                    myvalue = 0;
                }
                SetValueWithNotify(myvalue, ref _monday, nameof(Monday));
            }
        }

        [UsedImplicitly]
        public double Thursday {
            get => _thursday;
            set {
                var myvalue = value;
                if (value > 100) {
                    myvalue = 100;
                }
                if (value < 0) {
                    myvalue = 0;
                }
                SetValueWithNotify(myvalue, ref _thursday, nameof(Thursday));
            }
        }

        public double Tuesday {
            get => _tuesday;
            set {
                var myvalue = value;
                if (value > 100) {
                    myvalue = 100;
                }
                if (value < 0) {
                    myvalue = 0;
                }
                SetValueWithNotify(myvalue, ref _tuesday, nameof(Tuesday));
            }
        }

        public double Wednesday {
            get => _wednesday;
            set {
                var myvalue = value;
                if (value > 100) {
                    myvalue = 100;
                }
                if (value < 0) {
                    myvalue = 0;
                }
                SetValueWithNotify(myvalue, ref _wednesday, nameof(Wednesday));
            }
        }

        [NotNull]
        private static HolidayProbabilities AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var holidayID = dr.GetIntFromLong("HolidayID");
            var dayofWeek = (DayOfWeek) dr.GetIntFromLong("DayOfWeek");
            var monday = dr.GetDouble("Monday");
            var tuesday = dr.GetDouble("Tuesday");
            var wednesday = dr.GetDouble("Wednesday");
            var thursday = dr.GetDouble("Thursday");
            var friday = dr.GetDouble("Friday");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HolidayProbabilities(holidayID, id, dayofWeek, monday,
                tuesday, wednesday, thursday, friday,
                connectionString, guid);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([NotNull] [ItemNotNull] ObservableCollection<HolidayProbabilities> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("HolidayID", HolidayID);
            cmd.AddParameter("DayOfWeek", _dayOfWeek);
            cmd.AddParameter("Monday", Monday);
            cmd.AddParameter("Tuesday", Tuesday);
            cmd.AddParameter("Wednesday", Wednesday);
            cmd.AddParameter("Thursday", Thursday);
            cmd.AddParameter("Friday", Friday);
        }

        public override string ToString() => _dayOfWeek + " " + Monday + "% " + Tuesday + "% " + Wednesday + "% " +
                                             Thursday + "% " + Friday;
    }
}