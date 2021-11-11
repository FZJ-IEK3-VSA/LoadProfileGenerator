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

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<GeographicLocHoliday> _holidays =
            new ObservableCollection<GeographicLocHoliday>();

        [CanBeNull] private TimeLimit _lightTimeLimit;
        [CanBeNull] private DateBasedProfile _solarRadiationProfile;
        private double _radiationThresholdForLight;

        /// <summary>
        /// The default value for the solar radiation threshold used for lighting simulation.
        /// A value of 50 is usually sensible for solar radiation profiles in W/m^2.
        /// This value is used e.g. for new geographic locations.
        /// </summary>
        private const double DefaultRadiationThreshold = 50;

        public GeographicLocation([JetBrains.Annotations.NotNull] string connectionString, [CanBeNull] TimeLimit lightTimeLimit, [CanBeNull] DateBasedProfile solarRadiationProfile, 
            [JetBrains.Annotations.NotNull] StrGuid guid) : base("Chemnitz", TableName, connectionString, guid) {
            _lightTimeLimit = lightTimeLimit;
            _solarRadiationProfile = solarRadiationProfile;
            _radiationThresholdForLight = DefaultRadiationThreshold;
            // dummy loc for unit tests
            TypeDescription = "Geographic Location";
        }

        public GeographicLocation([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string connectionString, [CanBeNull] TimeLimit lightTimeLimit, 
            DateBasedProfile radiationProfile, double radiationLimitForLight, [JetBrains.Annotations.NotNull] StrGuid guid,[CanBeNull] int? id = null) : base(
                name, id, TableName, connectionString, guid) {
            _lightTimeLimit = lightTimeLimit;
            _solarRadiationProfile = radiationProfile;
            _radiationThresholdForLight = radiationLimitForLight;
            TypeDescription = "Geographic Location";
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public ObservableCollection<GeographicLocHoliday> Holidays => _holidays;

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var holiday in _holidays)
            {
                holiday.SaveToDB();
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TimeLimit LightTimeLimit {
            get => _lightTimeLimit;
            set => SetValueWithNotify(value, ref _lightTimeLimit, false, nameof(LightTimeLimit));
        }

        /// <summary>
        /// The solar radiation profile for this geographic location that is used to simulate lighting
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public DateBasedProfile SolarRadiationProfile
        {
            get => _solarRadiationProfile;
            set => SetValueWithNotify(value, ref _solarRadiationProfile, false, nameof(SolarRadiationProfile));
        }

        /// <summary>
        /// The threshold that is used together with the solar radiation profile to simulate lighting.
        /// When the solar radiation is below this threshold, lighting is switched on.
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public double RadiationThresholdForLight
        {
            get => _radiationThresholdForLight;
            set => SetValueWithNotify(value, ref _radiationThresholdForLight, nameof(RadiationThresholdForLight));
        }

        public void AddHoliday([JetBrains.Annotations.NotNull] Holiday hd) {
            var ghd = new GeographicLocHoliday(null, hd, IntID,
                ConnectionString, hd.Name, System.Guid.NewGuid().ToStrGuid());
            Holidays.Add(ghd);
            Holidays.Sort();
            ghd.SaveToDB();
        }

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static GeographicLocation AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var name = dr.GetString("Name");
            var id = dr.GetIntFromLong("ID");
            var lightTimeLimitID = dr.GetIntFromLong("LightTimeLimitID", false, ignoreMissingFields, -1);
            if (lightTimeLimitID == -1 && ignoreMissingFields) {
                lightTimeLimitID = dr.GetIntFromLong("LightDeviceTimeID", false, ignoreMissingFields, -1);
            }
            var dt = aic.TimeLimits.FirstOrDefault(mydt => mydt.ID == lightTimeLimitID);
            var solarRadiationProfileID = dr.GetIntFromLong("SolarRadiationProfileID", false, ignoreMissingFields, -1);
            var solarRadiationProfile = aic.DateBasedProfiles.FirstOrDefault(profile => profile.ID == solarRadiationProfileID);
            var radiationThresholdForLight = dr.GetDouble("RadiationThresholdForLight", false, DefaultRadiationThreshold, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new GeographicLocation(name, connectionString, dt, solarRadiationProfile, radiationThresholdForLight, guid, id);
        }

        [JetBrains.Annotations.NotNull]
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

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) {
            var geoloc = new GeographicLocation(FindNewName(isNameTaken, "New Geographic Location "), connectionString, null, null, DefaultRadiationThreshold, System.Guid.NewGuid().ToStrGuid());
            return geoloc;
        }

        public override void DeleteFromDB() {
            base.DeleteFromDB();
            foreach (var locHoliday in _holidays) {
                locHoliday.DeleteFromDB();
            }
        }

        public void DeleteGeoHolidayFromDB([JetBrains.Annotations.NotNull] GeographicLocHoliday geoholi) {
            geoholi.DeleteFromDB();
            Holidays.Remove(geoholi);
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<DateTime, Holiday.HolidayType> GetHolidayDictWithBridge([JetBrains.Annotations.NotNull] Random random, [JetBrains.Annotations.NotNull] string key) {
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

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static GeographicLocation ImportFromItem([JetBrains.Annotations.NotNull] GeographicLocation toImport, [JetBrains.Annotations.NotNull] Simulator dstSim) {
            TimeLimit dt = null;
            if (toImport.LightTimeLimit != null) {
                dt = GetItemFromListByName(dstSim.TimeLimits.Items, toImport.LightTimeLimit.Name);
            }
            DateBasedProfile solarRadiationProfile = null;
            if (toImport.SolarRadiationProfile != null)
            {
                solarRadiationProfile = GetItemFromListByName(dstSim.DateBasedProfiles.Items, toImport.SolarRadiationProfile.Name);
            }
            var geoloc = new GeographicLocation(toImport.Name, dstSim.ConnectionString, dt, solarRadiationProfile, toImport.RadiationThresholdForLight, toImport.Guid);
            geoloc.SaveToDB();
            foreach (var geographicLocHoliday in toImport.Holidays) {
                var holiday = GetItemFromListByName(dstSim.Holidays.Items, geographicLocHoliday.Holiday.Name);
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

        private static bool IsCorrectParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child) {
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

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<GeographicLocation> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Holiday> holidays, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeLimit> timeLimits,
            [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<DateBasedProfile> dateBasedProfiles, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(timeLimits: timeLimits, dateBasedProfiles: dateBasedProfiles);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var geoholi = new ObservableCollection<GeographicLocHoliday>();
            GeographicLocHoliday.LoadFromDatabase(geoholi, connectionString, holidays, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(geoholi), IsCorrectParent, ignoreMissingTables);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);

            if (_lightTimeLimit != null) {
                cmd.AddParameter("LightTimeLimitID", _lightTimeLimit.IntID);
            }
            if (_solarRadiationProfile != null)
            {
                cmd.AddParameter("SolarRadiationProfileID", _solarRadiationProfile.IntID);
            }
            cmd.AddParameter("RadiationThresholdForLight", _radiationThresholdForLight);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((GeographicLocation)toImport,  dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}