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
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Helpers;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class GeographicLocation : DBBaseElement {
        public const string TableName = "tblGeographicLocations";

        [ItemNotNull] [NotNull] private readonly ObservableCollection<GeographicLocHoliday> _holidays =
            new ObservableCollection<GeographicLocHoliday>();

        private CalcSunriseTimes.LatitudeCoords.Direction _latDirection;
        private int _latHour;
        private int _latMinute;
        private int _latSecond;

        [CanBeNull] private TimeLimit _lightTimeLimit;

        private CalcSunriseTimes.LongitudeCoords.Direction _longDirection;
        private int _longHour;
        private int _longMinute;
        private int _longSecond;

        public GeographicLocation([NotNull] string connectionString, [CanBeNull] TimeLimit lightTimeLimit, StrGuid guid) : base("Chemnitz",
            TableName, connectionString, guid) {
            _lightTimeLimit = lightTimeLimit;
            // dummy loc for unit tests
            _latDirection = CalcSunriseTimes.LatitudeCoords.Direction.North;
            _latHour = 50;
            _latMinute = 49;
            _latSecond = 21;

            _longDirection = CalcSunriseTimes.LongitudeCoords.Direction.East;
            _longHour = 12;
            _longMinute = 56;
            _longSecond = 16;
            TypeDescription = "Geographic Location";
        }

        public GeographicLocation([NotNull] string name, int slathour, int slatMinute, int slatSecond, int slongHour,
            int slongMinute, int slongSecond, [NotNull] string slongDir, [NotNull] string slatDir, [NotNull] string connectionString,
            [CanBeNull] TimeLimit lightTimeLimit,StrGuid guid,[CanBeNull] int? id = null) : base(name, id, TableName, connectionString, guid) {
            _latHour = slathour;
            _latMinute = slatMinute;
            _latSecond = slatSecond;
            _longHour = slongHour;
            _longMinute = slongMinute;
            _longSecond = slongSecond;
            _lightTimeLimit = lightTimeLimit;
            TypeDescription = "Geographic Location";
            if (slatDir.ToUpperInvariant().Trim() == "NORTH") {
                LatDirectionEnum = CalcSunriseTimes.LatitudeCoords.Direction.North;
            }
            else {
                LatDirectionEnum = CalcSunriseTimes.LatitudeCoords.Direction.South;
            }
            if (slongDir.ToUpperInvariant().Trim() == "EAST") {
                _longDirection = CalcSunriseTimes.LongitudeCoords.Direction.East;
            }
            else {
                _longDirection = CalcSunriseTimes.LongitudeCoords.Direction.West;
            }
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<GeographicLocHoliday> Holidays => _holidays;

        [NotNull]
        [UsedImplicitly]
        public string LatDirection {
            get => LatDirectionEnum.ToString();
            set {
                switch (value.ToUpperInvariant().Trim()) {
                    case "NORTH":
                        LatDirectionEnum = CalcSunriseTimes.LatitudeCoords.Direction.North;
                        break;
                    case "SOUTH":
                        LatDirectionEnum = CalcSunriseTimes.LatitudeCoords.Direction.South;
                        break;
                    default:
                        Logger.Error("Try either north or south as latitude direction.");
                        LatDirectionEnum = CalcSunriseTimes.LatitudeCoords.Direction.North;
                        break;
                }
                OnPropertyChanged(nameof(LatDirection));
                NeedsUpdate = true;
            }
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var holiday in _holidays)
            {
                holiday.SaveToDB();
            }
        }
        [UsedImplicitly]
        public CalcSunriseTimes.LatitudeCoords.Direction LatDirectionEnum {
            get => _latDirection;
            set => SetValueWithNotify(value, ref _latDirection, nameof(LatDirectionEnum));
        }

        [UsedImplicitly]
        public int LatHour {
            get => _latHour;
            set => SetValueWithNotify(value, ref _latHour, nameof(LatHour));
        }

        [UsedImplicitly]
        public int LatMinute {
            get => _latMinute;
            set => SetValueWithNotify(value, ref _latMinute, nameof(LatMinute));
        }

        [UsedImplicitly]
        public int LatSecond {
            get => _latSecond;
            set => SetValueWithNotify(value, ref _latSecond, nameof(LatSecond));
        }

        [CanBeNull]
        [UsedImplicitly]
        public TimeLimit LightTimeLimit {
            get => _lightTimeLimit;
            set => SetValueWithNotify(value, ref _lightTimeLimit, false, nameof(LightTimeLimit));
        }

        [NotNull]
        [UsedImplicitly]
        public string LongDirection {
            get => LongDirectionEnum.ToString();
            set {
                switch (value.ToUpperInvariant().Trim()) {
                    case "EAST":
                        _longDirection = CalcSunriseTimes.LongitudeCoords.Direction.East;
                        break;
                    case "WEST":
                        _longDirection = CalcSunriseTimes.LongitudeCoords.Direction.West;
                        break;
                    default:
                        Logger.Error("Longitude direction \"" + value + "\" is not recognized. Try east or west");
                        break;
                }
                OnPropertyChanged(nameof(LongDirection));
                NeedsUpdate = true;
            }
        }

        public CalcSunriseTimes.LongitudeCoords.Direction LongDirectionEnum => _longDirection;

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        [UsedImplicitly]
        public int LongHour {
            get => _longHour;
            set => SetValueWithNotify(value, ref _longHour, nameof(LongHour));
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        [UsedImplicitly]
        public int LongMinute {
            get => _longMinute;
            set => SetValueWithNotify(value, ref _longMinute, nameof(LongMinute));
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        [UsedImplicitly]
        public int LongSecond {
            get => _longSecond;
            set => SetValueWithNotify(value, ref _longSecond, nameof(LongSecond));
        }

        public void AddHoliday([NotNull] Holiday hd) {
            var ghd = new GeographicLocHoliday(null, hd, IntID,
                ConnectionString, hd.Name, System.Guid.NewGuid().ToStrGuid());
            Holidays.Add(ghd);
            Holidays.Sort();
            ghd.SaveToDB();
        }

        [NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static GeographicLocation AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var name = dr.GetString("Name");
            var id = dr.GetIntFromLong("ID");
            var lathour = dr.GetIntFromLong("LatHour");
            var latMinute = dr.GetIntFromLong("LatMinute");
            var latSecond = dr.GetIntFromLong("LatSecond");
            var longHour = dr.GetIntFromLong("LongHour");
            var longMinute = dr.GetIntFromLong("LongMinute");
            var longSecond = dr.GetIntFromLong("LongSecond");
            var longDir = dr.GetString("LongDir");
            var latDir = dr.GetString("LatDir");
            var lightTimeLimitID = dr.GetIntFromLong("LightTimeLimitID", false, ignoreMissingFields, -1);
            if (lightTimeLimitID == -1 && ignoreMissingFields) {
                lightTimeLimitID = dr.GetIntFromLong("LightDeviceTimeID", false, ignoreMissingFields, -1);
            }
            var dt = aic.TimeLimits.FirstOrDefault(mydt => mydt.ID == lightTimeLimitID);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new GeographicLocation(name, lathour, latMinute, latSecond, longHour, longMinute, longSecond,
                longDir, latDir, connectionString, dt,guid, id);
        }

        [NotNull]
        public Dictionary<DateTime, bool> CalculatePureHolidayDict() {
            var dict = new Dictionary<DateTime, bool>();
            foreach (var geographicLocHoliday in _holidays) {
                var dts = geographicLocHoliday.Holiday.CollectListOfHolidays();
                foreach (var dateTime in dts) {
                    if (!dict.ContainsKey(dateTime)) {
                        dict.Add(dateTime, true);
                    }
                }
            }
            return dict;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) {
            var geoloc = new GeographicLocation(FindNewName(isNameTaken, "New Geographic Location "), 1,
                1, 1, 1, 1, 1,
                "East", "North", connectionString,null,System.Guid.NewGuid().ToStrGuid());
            return geoloc;
        }

        public override void DeleteFromDB() {
            base.DeleteFromDB();
            foreach (var locHoliday in _holidays) {
                locHoliday.DeleteFromDB();
            }
        }

        public void DeleteGeoHolidayFromDB([NotNull] GeographicLocHoliday geoholi) {
            geoholi.DeleteFromDB();
            Holidays.Remove(geoholi);
        }

        [NotNull]
        public Dictionary<DateTime, Holiday.HolidayType> GetHolidayDictWithBridge([NotNull] Random random, [NotNull] string key) {
            var dict = new Dictionary<DateTime, Holiday.HolidayType>();
            foreach (var geographicLocHoliday in _holidays) {
                var dts =
                    geographicLocHoliday.Holiday.GetListOfWorkFreeDates(random, key);
                foreach (var dateTime in dts) {
                    if (!dict.ContainsKey(dateTime.Key)) {
                        dict.Add(dateTime.Key, dateTime.Value);
                    }
                }
            }
            return dict;
        }

        [NotNull]
        [UsedImplicitly]
        public static GeographicLocation ImportFromItem([NotNull] GeographicLocation toImport, [NotNull] Simulator dstSim) {
            TimeLimit dt = null;
            if (toImport.LightTimeLimit != null) {
                dt = GetItemFromListByName(dstSim.TimeLimits.MyItems, toImport.LightTimeLimit.Name);
            }
            var geoloc = new GeographicLocation(toImport.Name, toImport._latHour, toImport.LatMinute,
                toImport.LatSecond, toImport.LongHour, toImport.LongMinute, toImport.LongSecond, toImport.LongDirection,
                toImport.LatDirection,dstSim.ConnectionString, dt, toImport.Guid);
            geoloc.SaveToDB();
            foreach (var geographicLocHoliday in toImport.Holidays) {
                var holiday = GetItemFromListByName(dstSim.Holidays.MyItems, geographicLocHoliday.Holiday.Name);
                if (holiday == null) {
                    throw new LPGException("holiday could not be found.");
                }
                if (holiday.ConnectionString != dstSim.ConnectionString) {
                    throw new LPGException("Added wrong element from the wrong database. This is a bug.");
                }
                geoloc.AddHoliday(holiday);
            }
            geoloc.SaveToDB();
            return geoloc;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child) {
            var hd = (GeographicLocHoliday) child;

            if (parent.ID == hd.GeographicLocationID) {
                var geographicLoc = (GeographicLocation) parent;
                geographicLoc.Holidays.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<GeographicLocation> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Holiday> holidays, [ItemNotNull] [NotNull] ObservableCollection<TimeLimit> timeLimits,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections(timeLimits: timeLimits);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var geoholi = new ObservableCollection<GeographicLocHoliday>();
            GeographicLocHoliday.LoadFromDatabase(geoholi, connectionString, holidays, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(geoholi), IsCorrectParent, ignoreMissingTables);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("LatHour", _latHour);
            cmd.AddParameter("latMinute", _latMinute);
            cmd.AddParameter("latSecond", _latSecond);

            cmd.AddParameter("longHour", _longHour);
            cmd.AddParameter("longMinute", _longMinute);
            cmd.AddParameter("longSecond", _longSecond);

            cmd.AddParameter("longDir", LongDirectionEnum.ToString());
            cmd.AddParameter("latDir", LatDirectionEnum.ToString());
            if (_lightTimeLimit != null) {
                cmd.AddParameter("LightTimeLimitID", _lightTimeLimit.IntID);
            }
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((GeographicLocation)toImport,  dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}