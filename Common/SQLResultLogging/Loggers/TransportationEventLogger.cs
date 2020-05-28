using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class TransportationEventLogger : DataSaverBase {
        private const string TableName = "TransportationEvents";

        public TransportationEventLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(TransportationEventEntry),  new ResultTableDefinition(TableName,ResultTableID.TransportationEvents, "Transportation Events", CalcOption.TransportationStatistics), srls)
        {
        }

        [ItemNotNull]
        [NotNull]
        public List<TransportationEventEntry> Load([NotNull] HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var res = Srls.ReadFromJson<TransportationEventEntry>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
            return res;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var locationEntries = objects.ConvertAll(x => (TransportationEventEntry)x).ToList();
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("TimeStep", SqliteDataType.Text);
            se.AddField("PersonName", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            foreach (var locationEntry in locationEntries) {
                se.AddRow(RowBuilder.Start("PersonName", locationEntry.PersonName)
                    .Add("TimeStep", locationEntry.Timestep)
                    .Add("Json", JsonConvert.SerializeObject(locationEntry, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}