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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Automation;
using Common;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicElements {
    public class TemperatureProfile : DBBaseElement {
        public const string TableName = "tblTemperatureProfiles";
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<TemperatureValue> _temperatureValues;
        [JetBrains.Annotations.NotNull] private string _description;
        public TemperatureProfile([JetBrains.Annotations.NotNull] string name,[CanBeNull] int? id, [JetBrains.Annotations.NotNull] string description, [JetBrains.Annotations.NotNull] string connectionString,
                                  [NotNull] StrGuid guid) : base(name,
            TableName, connectionString, guid) {
            _description = description;
            _temperatureValues = new ObservableCollection<TemperatureValue>();
            TypeDescription = "Temperature Profile";
            ID = id;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TemperatureValue> TemperatureValues => _temperatureValues;

        public void AddTemperature(DateTime time, double value, [CanBeNull]int? id = null, bool sort = true, bool save = true) {
            var tv = new TemperatureValue(time, value, IntID, id, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            _temperatureValues.Add(tv);
            if (id == null && save) {
                SaveToDB();
            }
            if (sort) {
                _temperatureValues.Sort();
            }
        }

        [JetBrains.Annotations.NotNull]
        private static TemperatureProfile AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TemperatureProfile(name, id, description, connectionString, guid);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) {
            var house = new TemperatureProfile(FindNewName(isNameTaken, "New Temperature Profile "),
                null, "(no description)", connectionString, System.Guid.NewGuid().ToStrGuid());
            return house;
        }

        public void DeleteAllTemperatures() {
            using (var con = new Connection(ConnectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    cmd.ExecuteNonQuery("DELETE FROM " + TemperatureValue.TableName + " WHERE TempProfileID = " +
                                        IntID);
                }
                _temperatureValues.Clear();
            }
        }

        public override void DeleteFromDB() {
            DeleteAllTemperatures();
            base.DeleteFromDB();
        }

        public void DeleteOneTemperatur([JetBrains.Annotations.NotNull] TemperatureValue tv) {
            if (tv.ID != null) {
                using (var con = new Connection(ConnectionString)) {
                    con.Open();
                    using (var cmd = new Command(con)) {
                        cmd.DeleteByID(TemperatureValue.TableName, (int) tv.ID);
                        _temperatureValues.Remove(tv);
                    }
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        public double[] GetTemperatureArray(DateTime startDateTime, DateTime endDateTime, TimeSpan stepsize) =>
            GetTemperatureArray(startDateTime, endDateTime, stepsize, _temperatureValues);

        [JetBrains.Annotations.NotNull]
        public static double[] GetTemperatureArray(DateTime startDateTime, DateTime endDateTime, TimeSpan stepsize,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TemperatureValue> temperatureValues) {
            var tempvalues = new List<TempValue>();
            var yearsToMake = new List<int>();
            var startyear = startDateTime.Year;
            while (new DateTime(startyear, 1, 1) <= endDateTime) {
                yearsToMake.Add(startyear);
                startyear++;
            }
            foreach (var temperatureValue in temperatureValues) {
                foreach (var year in yearsToMake) {
                    try {
                        if (DateTime.DaysInMonth(year, temperatureValue.Time.Month) >= temperatureValue.Time.Day) {
                            var newdate = new DateTime(year, temperatureValue.Time.Month, temperatureValue.Time.Day,
                                temperatureValue.Time.Hour, temperatureValue.Time.Minute, temperatureValue.Time.Second);
                            var tv = new TempValue(temperatureValue.Value, newdate);
                            tempvalues.Add(tv);
                        }
                    }
                    catch (ArgumentOutOfRangeException) {
                        Logger.Error(
                            "An invalid date had been found while trying to map the dates to the current year:" +
                            year + " - " + temperatureValue.Time.Month + " - " + temperatureValue.Time.Day + " " +
                            temperatureValue.Time.Hour + ":" + temperatureValue.Time.Minute + ":" +
                            temperatureValue.Time.Second);
                    }
                }
            }
            tempvalues.Sort();
            // datearray aufbauen
            var duration = endDateTime - startDateTime;
            var totalsteps = (int) (duration.TotalSeconds / stepsize.TotalSeconds);
            var dts = new DateTime[totalsteps];
            dts[0] = startDateTime;
            for (var i = 1; i < totalsteps; i++) {
                dts[i] = dts[i - 1] + stepsize;
            }
            // temperaturen f�llen
            var srctime = 0;
            var lasttemp = tempvalues[0].Value;
            var alltemperatures = new double[totalsteps];
            for (var dsttime = 0; dsttime < dts.Length; dsttime++) {
                // this makes potenially the first minute wrong && dsttime > 0
                while (srctime < tempvalues.Count && tempvalues[srctime].Time <= dts[dsttime]) {
                    srctime++;
                    lasttemp = tempvalues[srctime - 1].Value;
                }
                alltemperatures[dsttime] = lasttemp;
            }
            return alltemperatures;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static TemperatureProfile ImportFromItem([JetBrains.Annotations.NotNull] TemperatureProfile toImport, [JetBrains.Annotations.NotNull] Simulator dstSim) {
            var tp = new TemperatureProfile(toImport.Name, null, toImport.Description,
                dstSim.ConnectionString, toImport.Guid);
            tp.SaveToDB();
            foreach (var temperatureValue in toImport.TemperatureValues) {
                tp.AddTemperature(temperatureValue.Time, temperatureValue.Value, null, false, false);
            }
            tp.SaveToDB(true);
            return tp;
        }

        private static bool IsCorrectParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child) {
            var hd = (TemperatureValue) child;
            if (parent.ID == hd.TempProfileID) {
                var tp = (TemperatureProfile) parent;
                tp.TemperatureValues.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TemperatureProfile> temperatureProfiles,
            [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(temperatureProfiles, connectionString, TableName, AssignFields, aic,
                ignoreMissingTables, true);
            var temperatureValues = new ObservableCollection<TemperatureValue>();
            TemperatureValue.LoadFromDatabase(temperatureValues, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(temperatureProfiles), new List<DBBase>(temperatureValues), IsCorrectParent,
                ignoreMissingTables);
            foreach (var temperatureProfile in temperatureProfiles) {
                temperatureProfile._temperatureValues.Sort();
            }
            temperatureProfiles.Sort();
        }

        public override void SaveToDB() {
            SaveToDB(false);
        }

        public void SaveToDB(bool showProgress)
        {
            base.SaveToDB();
            var itemsToSave = 0;
            var itemsSaved = 0;
            if (showProgress) {
                foreach (var temperatureValue in _temperatureValues) {
                    if (temperatureValue.ID == null) {
                        itemsToSave++;
                    }
                }
            }

            using (var con = new Connection(ConnectionString)) {
                con.Open();
                using (var ta = con.Sqlcon.BeginTransaction()) {
                    foreach (var temperatureValue in _temperatureValues) {
                        if (temperatureValue.ID == null) {
                            itemsSaved++;
                            if (showProgress && itemsSaved % 100 == 0) {
                                Logger.Info("Saved " + itemsSaved + " out of " + itemsToSave + ".");
                            }
                            temperatureValue.SaveToDB(con);
                        }
                    }
                    ta.Commit();
                }
            }

            if (GuidCreationCount > 0) {
                foreach (var temperatureValue in _temperatureValues) {
                    if (GuidsToSave.Contains(temperatureValue.Guid)) {
                        temperatureValue.SaveToDB();
                    }
                }
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", "@myDescription", _description);
        }

        public override string ToString() => Name;

        // temporary values for mapping to the current year
        private class TempValue : IComparable<TempValue> {
            public  DateTime Time { get; }
            public  double Value { get; }

            public TempValue(double value, DateTime time) {
                Value = value;
                Time = time;
            }

            public int CompareTo([CanBeNull] TempValue other) {
                if (other == null) {
                    return 0;
                }
                return Time.CompareTo(other.Time);
            }

            [JetBrains.Annotations.NotNull]
            public override string ToString() => Time.ToShortDateString() + " " + Time.ToShortTimeString() + ":" +
                                                 Value.ToString(CultureInfo.CurrentCulture);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TemperatureProfile)toImport,  dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}