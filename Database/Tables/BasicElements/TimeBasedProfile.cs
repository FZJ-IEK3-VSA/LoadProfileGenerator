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

#region assign

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicElements {
    public enum TimeProfileType {
        Relative = 0,
        Absolute = 1
    }

    public class TimeBasedProfile : DBBaseElement {
        public const string TableName = "tblTimeBasedProfile";
        [NotNull] private string _dataSource;
        private TimeProfileType _timeProfileType;

        public TimeBasedProfile([NotNull] string name, [CanBeNull] int? pID, [NotNull] string connectionString, TimeProfileType timeProfileType,
            [NotNull] string dataSource, StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            _timeProfileType = timeProfileType;
            ObservableDatapoints = new ObservableCollection<TimeDataPoint>();
            ID = pID;
            _dataSource = dataSource;
            TypeDescription = "Time profile";
        }

        public int DatapointsCount => ObservableDatapoints.Count;

        [NotNull]
        [UsedImplicitly]
        public string DataSource {
            get => _dataSource;
            set => SetValueWithNotify(value, ref _dataSource, nameof(DataSource));
        }

        public TimeSpan Duration {
            get {
                var ts = new TimeSpan(0);
                foreach (var timeDataPoint in ObservableDatapoints) {
                    if (timeDataPoint.Time > ts) {
                        ts = timeDataPoint.Time;
                    }
                }
                return ts;
            }
        }

        public double Maxiumum {
            get {
                var maxval = double.MinValue;
                foreach (var point in ObservableDatapoints) {
                    if (point.Value > maxval) {
                        maxval = point.Value;
                    }
                }
                return maxval;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string NameWithTime => Name + " [" + GetMaximumTime() + "]";

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TimeDataPoint> ObservableDatapoints { get; }

        [UsedImplicitly]
        public TimeProfileType TimeProfileType {
            get => _timeProfileType;
            set {
                SetValueWithNotify(value, ref _timeProfileType, nameof(TimeProfileType));
                OnPropertyChanged(nameof(ValueTypeLabel));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string ValueTypeLabel => CalculateValueTypeLabel();

        public void AddNewTimepoint(TimeSpan ts, double value, bool saveAndSort = true)
        {
            var tp = new TimeDataPoint(ts, value, null, IntID, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            lock (ObservableDatapoints) {
                ObservableDatapoints.Add(tp);
            }
            if (saveAndSort) {
                SaveToDB();
                lock (ObservableDatapoints) {
                    ObservableDatapoints.Sort();
                }
                OnPropertyChanged(nameof(Duration));
            }
        }

        [NotNull]
        private static TimeBasedProfile AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name");
            var timeprofiletypeid = dr.GetIntFromLong("TimeProfileType", false, ignoreMissingFields);
            var dataSource = dr.GetString("DataSource", false, string.Empty, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TimeBasedProfile(name, id, connectionString,
                (TimeProfileType) timeprofiletypeid, dataSource,guid);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public static TimeSpan CalculateMinimumTimespan() => new TimeSpan(0, 1, 0);

        // no need anymore for the entire minimum-timespan-thing since
        // there is the function to go both to finer and coarser time resolutions

        public double CalculateSecondsPercent()
        {
            double value = 0;
            for (var i = 1; i < ObservableDatapoints.Count; i++) {
                var ts = ObservableDatapoints[i].Time - ObservableDatapoints[i - 1].Time;
                value += ts.TotalSeconds * ObservableDatapoints[i - 1].Value / 100;
            }
            return value;
        }

        public double CalculateSecondsSum()
        {
            double value = 0;
            for (var i = 1; i < ObservableDatapoints.Count; i++) {
                var ts = ObservableDatapoints[i].Time - ObservableDatapoints[i - 1].Time;
                value += ts.TotalSeconds * ObservableDatapoints[i - 1].Value;
            }
            return value;
        }

        [NotNull]
        private string CalculateValueTypeLabel()
        {
            switch (TimeProfileType) {
                case TimeProfileType.Relative: return "Value [%]";
                case TimeProfileType.Absolute: return "Value";
                default: throw new LPGException("Forgotten Time Profile Type. Please report");
            }
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            TimeBasedProfile(FindNewName(isNameTaken, "New profile "), null, connectionString,
                TimeProfileType.Relative,
                "unknown", System.Guid.NewGuid().ToStrGuid());

        public void DeleteAllTimepoints()
        {
            DeleteAllForOneParent(IntID, "TimeBasedProfileID", TimeDataPoint.TableName, ConnectionString);
            lock (ObservableDatapoints) {
                ObservableDatapoints.Clear();
            }
            OnPropertyChanged(nameof(Duration));
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            DeleteAllTimepoints();
        }

        public void DeleteTimepoint([NotNull] TimeDataPoint tp)
        {
            tp.DeleteFromDB();
            lock (ObservableDatapoints) {
                ObservableDatapoints.Remove(tp);
            }
            OnPropertyChanged(nameof(Duration));
        }

        [NotNull]
        private string GetMaximumTime() => Duration.ToString();

        public override DBBase ImportFromGenericItem(DBBase toImport,  Simulator dstSim)
            => ImportFromItem((TimeBasedProfile)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var result = new List<UsedIn>();
            foreach (var action in sim.DeviceActions.Items) {
                foreach (var profile in action.Profiles) {
                    if (profile.Timeprofile == this) {
                        result.Add(new UsedIn(action, "Device Action - " + action.Name));
                    }
                }
            }
            foreach (var affordance in sim.Affordances.Items) {
                foreach (var affordanceDevice in affordance.AffordanceDevices) {
                    if (affordanceDevice.TimeProfile == this) {
                        result.Add(new UsedIn(affordance, "Affordance - " + affordanceDevice.Name));
                    }
                }
                if (affordance.PersonProfile == this) {
                    result.Add(new UsedIn(affordance, "Affordance - Person Profile"));
                }
            }
            foreach (var hht in sim.HouseholdTraits.Items) {
                foreach (var dev in hht.Autodevs) {
                    if (dev.TimeProfile == this) {
                        result.Add(new UsedIn(hht, "Household Trait - " + dev.Name));
                    }
                }
            }
            return result;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] TimeBasedProfile item, [NotNull] Simulator dstSimulator)
        {
            var tbp = new TimeBasedProfile(item.Name, null,
                dstSimulator.ConnectionString, item.TimeProfileType,
                item.DataSource,item.Guid);
            tbp.SaveToDB();
            foreach (var point in item.ObservableDatapoints) {
                tbp.AddNewTimepoint(point.Time, point.Value, false);
            }

            return tbp;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (TimeDataPoint) child;
            if (parent.ID == hd.ProfileID) {
                var tp = (TimeBasedProfile) parent;
                lock (tp.ObservableDatapoints) {
                    tp.ObservableDatapoints.Add(hd);
                }
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TimeBasedProfile> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var timeDataPoints = new ObservableCollection<TimeDataPoint>();
            TimeDataPoint.LoadFromDatabase(timeDataPoints, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(timeDataPoints), IsCorrectParent,
                ignoreMissingTables);

            foreach (var profile in result) {
                lock (profile.ObservableDatapoints) {
                    profile.ObservableDatapoints.Sort();
                }
            }
            // cleanup
        }

        public override void SaveToDB()
        {
            base.SaveToDB();

            foreach (var tp in ObservableDatapoints) {
                tp.SaveToDB();
            }
        }

        public void SaveToDB([NotNull] Action<int> reportProgress)
        {
            var i = 0;
            base.SaveToDB();
            lock (ObservableDatapoints) {
                foreach (var tp in ObservableDatapoints) {
                    tp.SaveToDB();
                    i++;
                    reportProgress(i);
                }
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("TimeProfileType", _timeProfileType);
            cmd.AddParameter("DataSource", _dataSource);
        }

        public override string ToString() => Name + "\t Datapoints:" + ObservableDatapoints.Count;
    }
}