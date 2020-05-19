using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class Vacation : DBBaseElement {
        public const string TableName = "tblVacations";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<VacationTime> _vacationTimes;

        private CreationType _creationType;
        private int _maximumAge;
        private int _minimumAge;

        public Vacation([NotNull] string name, [CanBeNull] int? pID, [NotNull] string connectionString, int minimumAge, int maximumAge,
            CreationType creationType, StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            _creationType = creationType;
            _minimumAge = minimumAge;
            _maximumAge = maximumAge;
            ID = pID;
            TypeDescription = "Vacation";
            AreNumbersOkInNameForIntegrityCheck = true;
            _vacationTimes = new ObservableCollection<VacationTime>();
        }

        [UsedImplicitly]
        public CreationType CreationType {
            get => _creationType;
            set => SetValueWithNotify(value, ref _creationType, nameof(CreationType));
        }

        public double DurationInH {
            get {
                double totalTime = 0;
                foreach (var vacationTime in VacationTimes) {
                    var timeInH = (vacationTime.End - vacationTime.Start).TotalHours;
                    totalTime += timeInH;
                }
                return totalTime;
            }
        }

        public int MaximumAge {
            get => _maximumAge;
            set => SetValueWithNotify(value, ref _maximumAge, nameof(MaximumAge));
        }

        public int MinimumAge {
            get => _minimumAge;
            set => SetValueWithNotify(value, ref _minimumAge, nameof(MinimumAge));
        }
        public override string PrettyName {
            get {
                var days = 0;
                foreach (var vacationTime in VacationTimes) {
                    days += vacationTime.Days;
                }
                var ages = " [Ages " + _minimumAge + " - " + _maximumAge + "]";
                return Name + " (" + days + " days)" + ages;
            }
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<VacationTime> VacationTimes => _vacationTimes;

        public void AddVacationTime(DateTime start, DateTime end, VacationType vacationType)
        {
            var name = start.ToLongDateString() + " - " + end.ToLongDateString();
            var vt = new VacationTime(name, null, start, end, IntID,
                ConnectionString, vacationType, System.Guid.NewGuid().ToStrGuid());
            vt.SaveToDB();
            Logger.Get().SafeExecute(() => {
                _vacationTimes.Add(vt);
                _vacationTimes.Sort();
            });
            OnPropertyChangedNoUpdate(nameof(PrettyName));
        }

        [NotNull]
        private static Vacation AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields);
            var name = dr.GetString("Name", false, string.Empty, ignoreMissingFields);
            var minimumAge = dr.GetIntFromLong("MinimumAge", false, ignoreMissingFields);
            var maximumAge = dr.GetIntFromLong("MaximumAge", false, ignoreMissingFields);
            var creationType = (CreationType) dr.GetIntFromLong("CreationType", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new Vacation(name, id, connectionString, minimumAge, maximumAge, creationType,guid);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new Vacation(
            FindNewName(isNameTaken, "New Vacation "), null, connectionString, 1, 99, CreationType.ManuallyCreated, System.Guid.NewGuid().ToStrGuid());

        public override void DeleteFromDB()
        {
            foreach (var vactime in _vacationTimes) {
                vactime.DeleteFromDB();
            }
            base.DeleteFromDB();
        }

        public void DeleteVacationTime([NotNull] VacationTime vactime)
        {
            _vacationTimes.Remove(vactime);
            vactime.DeleteFromDB();
        }

        [NotNull]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dstSim")]
        [UsedImplicitly]
        public static Vacation ImportFromItem([NotNull] Vacation toImport,  [NotNull] Simulator dstSim)
        {
            var p = new Vacation(toImport.Name, null, dstSim.ConnectionString, toImport.MinimumAge, toImport.MaximumAge,
                toImport._creationType, System.Guid.NewGuid().ToStrGuid());
            p.SaveToDB();
            foreach (var vactime in toImport._vacationTimes) {
                p.AddVacationTime(vactime.Start, vactime.End, vactime.VacationType);
            }
            return p;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (VacationTime) child;
            if (parent.ID == hd.VacationID) {
                var vacation = (Vacation) parent;
                vacation._vacationTimes.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<Vacation> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var vacationTimes = new ObservableCollection<VacationTime>();
            VacationTime.LoadFromDatabase(vacationTimes, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(vacationTimes), IsCorrectParent,
                ignoreMissingTables);
            result.Sort();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var vacationTime in _vacationTimes) {
                vacationTime.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("MinimumAge", _minimumAge);
            cmd.AddParameter("MaximumAge", _maximumAge);
            cmd.AddParameter("CreationType", _creationType);
        }

        public override string ToString() => Name;

        [ItemNotNull]
        [NotNull]
        public List<VacationTimeframe> VacationTimeframes()
        {
            var timeframes = new List<VacationTimeframe>();
            foreach (var vacationTime in _vacationTimes) {
                timeframes.Add(new VacationTimeframe(vacationTime.Start, vacationTime.End));
            }
            return timeframes;
        }

        public override DBBase ImportFromGenericItem(DBBase toImport,  Simulator dstSim)
            => ImportFromItem((Vacation)toImport,  dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}