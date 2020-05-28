using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.InputLoggers;

namespace Common.SQLResultLogging.Loggers {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using SQLResultLogging;

    public class PersonStatusLogger : DataSaverBase {
        private const string TableName = "PersonStatus";

        public PersonStatusLogger([NotNull] SqlResultLoggingService srls) : base(typeof(PersonStatus),
            new ResultTableDefinition(TableName, ResultTableID.PersonStatus, "The status of each person for each timestep", CalcOption.PersonStatus),
            srls)
        {
        }

        [ItemNotNull]
        [NotNull]
        public List<PersonStatus> Read([NotNull] HouseholdKey hhkey)
        {
            if (Srls == null) {
                throw new LPGException("Data Logger was null.");
            }

            var res = Srls.ReadFromJson<PersonStatus>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
            return res;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (PersonStatus)x).ToList();
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("TimeStep", SqliteDataType.Text);
            se.AddField("PersonName", SqliteDataType.Text);
            se.AddField("AffordanceName", SqliteDataType.Text);
            se.AddField("LocationName", SqliteDataType.Text);
            se.AddField("SiteName", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            foreach (var actionEntry in actionEntries) {
                se.AddRow(RowBuilder.Start("PersonName", actionEntry.PersonName).Add("AffordanceName", actionEntry.ActiveAffordance)
                    .Add("TimeStep", actionEntry.TimeStep).Add("LocationName", actionEntry.LocationName).Add("SiteName", actionEntry.SiteName)
                    .Add("Json", JsonConvert.SerializeObject(actionEntry, Formatting.Indented)).ToDictionary());
            }

            if (Srls == null) {
                throw new LPGException("Data Logger was null.");
            }

            Srls.SaveResultEntry(se);
        }
    }
}