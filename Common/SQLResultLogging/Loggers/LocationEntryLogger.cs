using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class LocationEntryLogger : DataSaverBase {
        private const string TableName = "LocationEntries";

        public LocationEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(LocationEntry), new ResultTableDefinition(TableName,ResultTableID.LocationEntries, "Location Entries"), srls)
        {
        }

        [ItemNotNull]
        [NotNull]
        public List<LocationEntry> Load([NotNull] HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var res = Srls.ReadFromJson<LocationEntry>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
            return res;
        }

        public override void Run([NotNull] HouseholdKey key, [NotNull] object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var locationEntries = objects.ConvertAll(x => (LocationEntry)x).ToList();
            SaveableEntry se = new SaveableEntry(key,ResultTableDefinition);
            se.AddField("TimeStep", SqliteDataType.Text);
            se.AddField("PersonName", SqliteDataType.Text);
            se.AddField("LocationName", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            foreach (var locationEntry in locationEntries) {
                se.AddRow(RowBuilder.Start("PersonName", locationEntry.PersonName)
                    .Add("LocationName", locationEntry.LocationName)
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