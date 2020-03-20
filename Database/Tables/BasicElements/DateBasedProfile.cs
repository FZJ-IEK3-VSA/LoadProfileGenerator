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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Common;
using Database.Database;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class DateBasedProfile : DBBaseElement {
        public const string TableName = "tblDateBasedProfile";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<DateProfileDataPoint> _datapoints;
        [NotNull] private string _description;

        public DateBasedProfile([NotNull] string name, [NotNull] string description, [NotNull] string connectionString, [NotNull] string guid, [CanBeNull] int? pID = null) : base(name, TableName,
            connectionString, guid)
        {
            _datapoints = new ObservableCollection<DateProfileDataPoint>();
            ID = pID;
            TypeDescription = "Date based profile";
            _description = description;
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<DateProfileDataPoint> Datapoints => _datapoints;

        [UsedImplicitly]
        public int DatapointsCount => _datapoints.Count;

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [NotNull]
        public DateProfileDataPoint AddNewDatePoint(DateTime dt, double value, bool saveAndSort = true)
        {
            var tp = new DateProfileDataPoint(dt, value, null, IntID, ConnectionString, System.Guid.NewGuid().ToString());
            _datapoints.Add(tp);
            if (saveAndSort) {
                SaveToDB();
                _datapoints.Sort();
            }

            return tp;
        }

        [ItemNotNull]
        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            List<UsedIn> usedIns = new List<UsedIn>();
            foreach (var limit in sim.TimeLimits.It) {
                if (limit.TimeLimitEntries.Any(x => x.DateBasedProfile == this)) {
                    usedIns.Add(new UsedIn(limit, "Time Limit"));
                }
            }

            foreach (Generator generator in sim.Generators.It) {
                if (generator.DateBasedProfile == this) {
                    usedIns.Add(new UsedIn(generator, "Generator"));
                }
            }

            foreach (HouseholdTemplate template in sim.HouseholdTemplates.It) {
                if (template.TimeProfileForVacations == this) {
                    usedIns.Add(new UsedIn(template, "Household Template"));
                }
            }

            return usedIns;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) =>
            new DateBasedProfile(FindNewName(isNameTaken, "New profile "), "(no description)", connectionString, System.Guid.NewGuid().ToString());

        public void DeleteAllTimepoints()
        {
            using (var con = new Connection(ConnectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    cmd.ExecuteNonQuery("DELETE FROM " + DateProfileDataPoint.TableName + " WHERE DateBasedProfileID = " + IntID);
                }
            }

            _datapoints.Clear();
        }

        public void DeleteDatePoint([NotNull] DateProfileDataPoint tp)
        {
            tp.DeleteFromDB();
            _datapoints.Remove(tp);
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            if (_datapoints.Count > 0) {
                _datapoints[0].DeleteAllForOneProfile(IntID);
            }
        }

        [NotNull]
        public double[] GetValueArray(DateTime startDateTime, DateTime endDateTime, TimeSpan stepsize) => GetValueArray(startDateTime, endDateTime, stepsize, _datapoints);

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim) => ImportFromItem((DateBasedProfile)toImport, dstSim);

        [NotNull]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dstSim")]
        [UsedImplicitly]
        public static DateBasedProfile ImportFromItem([NotNull] DateBasedProfile toImport, [NotNull] Simulator dstSim)
        {
            var dbp = new DateBasedProfile(toImport.Name, toImport.Description, dstSim.ConnectionString, toImport.Guid);
            dbp.SaveToDB();
            using (var con = new Connection(dstSim.ConnectionString)) {
                con.Open();
                foreach (var tp in toImport.Datapoints) {
                    var dateProfileDataPoint = new DateProfileDataPoint(tp.DateAndTime, tp.Value, null, dbp.IntID, dstSim.ConnectionString, tp.Guid);
                    dateProfileDataPoint.ExecuteInsert(con);
                }
            }

            dbp.SaveToDB();
            return dbp;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DateBasedProfile> result, [NotNull] string connectionString, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var datapoints = new ObservableCollection<DateProfileDataPoint>();
            DateProfileDataPoint.LoadFromDatabase(datapoints, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(datapoints), IsCorrectParent, ignoreMissingTables);
            foreach (var profile in result) {
                profile._datapoints.Sort();
            }
        }

        /// <summary>
        ///     Saves to the db
        /// </summary>
        /// <param name="reportProgress"></param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public void SaveToDB([NotNull] Action<int> reportProgress)
        {
            base.SaveToDB();
            var i = 1;
            using (var con = new Connection(ConnectionString)) {
                con.Open();
                using (var tr = con.BeginTransaction()) {
                    foreach (var tp in _datapoints) {
                        if (tp.ID == null) {
                            if (i % 10 == 0) {
                                Logger.Debug("Saving data point #" + i);
                            }

                            tp.ExecuteInsert(con);
                            i++;
                            reportProgress(i);
                        }
                        else if (_GuidCreationCount > 0) {
                            tp.SaveToDB();
                        }
                    }

                    tr.Commit();
                }
            }

            if (i != 1) {
                Logger.Info("Finished saving " + i + " data points.");
            }
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            var i = 1;
            using (var con = new Connection(ConnectionString)) {
                con.Open();
                using (var tr = con.BeginTransaction()) {
                    foreach (var tp in _datapoints) {
                        if (tp.ID == null) {
                            if (i % 10 == 0) {
                                Logger.Debug("Saving data point #" + i);
                            }

                            tp.ExecuteInsert(con);
                            i++;
                        }
                    }

                    tr.Commit();
                }
            }

            if (_GuidCreationCount > 0) {
                foreach (var dp in _datapoints) {
                    if (GuidsToSave.Contains(dp.Guid)) {
                        dp.SaveToDB();
                    }
                }
            }

            if (i != 1) {
                Logger.Info("Finished saving " + i + " data points.");
            }
        }

        public override string ToString() => Name + "\t Datapoints:" + _datapoints.Count;

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
        }

        [NotNull]
        private static DateBasedProfile AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var description = dr.GetString("Description");
            var guid = GetGuid(dr, ignoreMissingFields);
            return new DateBasedProfile(name, description, connectionString, guid, id);
        }

        [NotNull]
        private static double[] GetValueArray(DateTime startDateTime, DateTime endDateTime, TimeSpan stepsize, [ItemNotNull] [NotNull] ObservableCollection<DateProfileDataPoint> dataPoints)
        {
            var tempvalues = new List<TempValue>();
            var yearsToMake = new List<int>();
            var startyear = startDateTime.Year;
            while (new DateTime(startyear, 1, 1) <= endDateTime) {
                yearsToMake.Add(startyear);
                startyear++;
            }

            foreach (var dateProfileDataPoint in dataPoints) {
                foreach (var year in yearsToMake) {
                    try {
                        if (DateTime.DaysInMonth(year, dateProfileDataPoint.DateAndTime.Month) >= dateProfileDataPoint.DateAndTime.Day) {
                            var newdate = new DateTime(year, dateProfileDataPoint.DateAndTime.Month, dateProfileDataPoint.DateAndTime.Day, dateProfileDataPoint.DateAndTime.Hour,
                                dateProfileDataPoint.DateAndTime.Minute, dateProfileDataPoint.DateAndTime.Second);
                            var tv = new TempValue(dateProfileDataPoint.Value, newdate);
                            tempvalues.Add(tv);
                        }
                    }
                    catch (ArgumentOutOfRangeException) {
                        Logger.Error("An invalid date had been found while trying to map the dates to the current year:" + year + " - " + dateProfileDataPoint.DateAndTime.Month + " - " +
                                     dateProfileDataPoint.DateAndTime.Day + " " + dateProfileDataPoint.DateAndTime.Hour + ":" + dateProfileDataPoint.DateAndTime.Minute + ":" +
                                     dateProfileDataPoint.DateAndTime.Second);
                    }
                }
            }

            tempvalues.Sort();
            // datearray creation
            var duration = endDateTime - startDateTime;
            var totalsteps = (int)(duration.TotalSeconds / stepsize.TotalSeconds);
            var dts = new DateTime[totalsteps];
            dts[0] = startDateTime;
            for (var i = 1; i < totalsteps; i++) {
                dts[i] = dts[i - 1] + stepsize;
            }

            // werte füllen
            var sourceTime = 0;
            var lastvalue = tempvalues[0].Value;
            var allValues = new double[totalsteps];
            for (var destinationTime = 0; destinationTime < dts.Length; destinationTime++) {
                // this makes potentially the first minute wrong && destinationTime > 0
                while (sourceTime < tempvalues.Count && tempvalues[sourceTime].Time <= dts[destinationTime]) {
                    sourceTime++;
                    lastvalue = tempvalues[sourceTime - 1].Value;
                }

                allValues[destinationTime] = lastvalue;
            }

            return allValues;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (DateProfileDataPoint)child;
            if (parent.ID == hd.ProfileID) {
                var tp = (DateBasedProfile)parent;
                tp._datapoints.Add(hd);
                return true;
            }

            return false;
        }

        // temporary values for mapping to the current year
        private class TempValue : IComparable<TempValue> {
            public TempValue(double value, DateTime time)
            {
                Value = value;
                Time = time;
            }

            public DateTime Time { get; }
            public double Value { get; }

            public int CompareTo([CanBeNull] TempValue other)
            {
                if (other == null) {
                    return 0;
                }

                return Time.CompareTo(other.Time);
            }

            public override bool Equals([CanBeNull] object obj)
            {
                if (obj == null) {
                    return false;
                }

                if (!(obj is TempValue other)) {
                    return false;
                }

                if (other == this) {
                    return true;
                }

                return false;
            }

            public override int GetHashCode() => Time.GetHashCode() * 17 + Value.GetHashCode();

            [NotNull]
            public override string ToString() => Time.ToShortDateString() + " " + Time.ToShortTimeString() + ":" + Value.ToString(CultureInfo.CurrentCulture);
        }
    }
}