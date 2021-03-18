using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public enum VacationType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TurnOff")]
        GoAwayAndTurnOffElectricity,
        GoAway,
        StayAtHomeVacation
    }

    public static class VacationTypeHelper {
        [JetBrains.Annotations.NotNull]
        public static Dictionary<VacationType, string> VacationTypeDictionary { get; } =
            new Dictionary<VacationType, string> {
                {VacationType.GoAwayAndTurnOffElectricity, "Vacation with all devices at home turned off"},
                {VacationType.GoAway, "Vacation, Standby devices are not turned off"},
                {VacationType.StayAtHomeVacation, "Stay-at-Home Vacation"}
            };
    }

    public class VacationTime : DBBase {
        public const string TableName = "tblVacationTimes";
        private readonly DateTime _end;
        private readonly DateTime _start;
        [CanBeNull]
        private readonly int? _vacationID;

        private VacationType _vacationType;

        public VacationTime([JetBrains.Annotations.NotNull] string name, [CanBeNull] int? pID, DateTime start, DateTime end, int vacationID,
            [JetBrains.Annotations.NotNull] string connectionString, VacationType vacationType, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            _vacationType = vacationType;
            TypeDescription = "Person Desire";
            ID = pID;
            _start = start;
            _end = end;
            _vacationID = vacationID;
        }

        public int Days => (int) (_end - _start).TotalDays;

        public DateTime End => _end;

        public DateTime Start => _start;
        [CanBeNull]
        public int? VacationID => _vacationID;

        [UsedImplicitly]
        public VacationType VacationType {
            get => _vacationType;
            set => SetValueWithNotify(value, ref _vacationType, nameof(VacationType));
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string VacationTypeName => VacationTypeHelper.VacationTypeDictionary[_vacationType];

        [JetBrains.Annotations.NotNull]
        private static VacationTime AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLongOrInt("ID", false, ignoreMissingFields);
            var start = dr.GetDateTime("Start", false);
            var end = dr.GetDateTime("End", false);
            var vacationID = dr.GetIntFromLong("VacationID", false, ignoreMissingFields);
            var name = start.ToLongDateString() + " - " + end.ToLongDateString();
            var vactype = (VacationType) dr.GetIntFromLong("VacationType", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var pd = new VacationTime(name, id, start, end, vacationID, connectionString, vactype,
                guid);
            return pd;
        }

        public override int CompareTo(BasicElement other)
        {
            if (other is VacationTime pd)
            {
                return _start.CompareTo(pd.Start);
            }
            return base.CompareTo(other);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_vacationID == null) {
                message = "Vacation not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<VacationTime> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_vacationID != null) {
                cmd.AddParameter("VacationID", _vacationID);
            }
            cmd.AddParameter("Start", _start);
            cmd.AddParameter("End", _end);
            cmd.AddParameter("VacationType", (int) _vacationType);
        }

        public override string ToString() => _start.ToLongDateString() + " - " + _end.ToLongDateString() + " (" +
                                             VacationTypeHelper.VacationTypeDictionary[VacationType] + ")";
    }
}