using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTemplateVacation : DBBase, IJSonSubElement<JsonReference> {
        public StrGuid RelevantGuid => Vacation.Guid;
        public const string TableName = "tblHHTemplateVacations";
        private readonly int _hhTemplateID;

        [CanBeNull] private readonly Vacation _vacation;

        public HHTemplateVacation([CanBeNull]int? pID, [CanBeNull] Vacation vacation, int hhTemplateID, [JetBrains.Annotations.NotNull] string name,
            [JetBrains.Annotations.NotNull] string connectionString, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            ID = pID;
            _vacation = vacation;
            _hhTemplateID = hhTemplateID;
            TypeDescription = "Household Template Vacations";
        }
        [CanBeNull]
        public int? HHTemplateID => _hhTemplateID;

        [JetBrains.Annotations.NotNull]
        public Vacation Vacation => _vacation ?? throw new InvalidOperationException();

        [JetBrains.Annotations.NotNull]
        private static HHTemplateVacation AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var hhpID = dr.GetIntFromLong("ID");
            var vacationID = dr.GetIntFromLong("VacationID", ignoreMissingField: ignoreMissingFields);
            var templateID = dr.GetIntFromLong("HHTemplateID");
            var v = aic.Vacations.FirstOrDefault(mypers => mypers.ID == vacationID);
            var name = "(no name)";
            if (v != null) {
                name = v.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhp = new HHTemplateVacation(hhpID, v, templateID, name, connectionString, guid);
            return hhp;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_vacation == null) {
                message = "Vacation is missing";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HHTemplateVacation> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Vacation> vacations, bool ignoreMissingTables) {
            var aic = new AllItemCollections(vacations: vacations);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_vacation != null) {
                cmd.AddParameter("VacationID", _vacation.IntID);
            }
            cmd.AddParameter("HHTemplateID", _hhTemplateID);
        }

        public JsonReference GetJson() => Vacation.GetJsonReference();

        public void SynchronizeDataFromJson(JsonReference json, Simulator sim)
        {
            if (_vacation?.Guid != json.Guid) {
                throw new LPGException("This should be impossible");
            }
        }

        public override string ToString() => Vacation.PrettyName;
    }
}