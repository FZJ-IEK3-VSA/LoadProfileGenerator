using Automation.ResultFiles;

namespace Common.SQLResultLogging.InputLoggers {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using JSON;
    using Newtonsoft.Json;

    public class ColumnEntryLogger : DataSaverBase {
        [NotNull] private readonly SqlResultLoggingService _srls;

        public ColumnEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(List<ColumnEntry>), new ResultTableDefinition("ColumnEntry",ResultTableID.BinaryTempFileColumnDescriptions, "Description of the binary columns in the temporary result files"), srls) => _srls = srls;

        [ItemNotNull]
        [NotNull]
        public List<ColumnEntry> Read([NotNull] HouseholdKey hhkey)
        {
            var ce = _srls.ReadFromJson<ColumnEntry>(ResultTableDefinition,Constants.GeneralHouseholdKey, ExpectedResultCount.OneOrMore);
            if (hhkey != Constants.GeneralHouseholdKey) {
                return ce.Where(x => x.HouseholdKey == hhkey).ToList();
            }

            return ce;
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = new SaveableEntry(Constants.GeneralHouseholdKey,ResultTableDefinition);
            se.AddField("Name", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            se.AddField("LoadType", SqliteDataType.Text);
            se.AddField("Household", SqliteDataType.Text);

            var columns = (List<ColumnEntry>)o;
            foreach (var column in columns) {
                se.AddRow(RowBuilder.Start("Name", column.Name)
                    .Add("LoadType",column.LoadType.Name)
                    .Add("Household", column.HouseholdKey.Key)
                    .Add("Json", JsonConvert.SerializeObject(column, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}