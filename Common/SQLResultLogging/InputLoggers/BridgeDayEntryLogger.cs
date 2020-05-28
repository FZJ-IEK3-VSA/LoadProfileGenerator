using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    using JSON;

    public class BridgeDayEntryLogger : DataSaverBase {
        public const string TableName = "BridgeDays";
        public BridgeDayEntryLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(BridgeDayEntries),  new ResultTableDefinition(TableName, ResultTableID.BridgeDayEntries,"All the bridge days for this household", CalcOption.HouseholdContents), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var sets = (BridgeDayEntries)o;
            if (sets.Entries.Count == 0) {
                Logger.Info("No bridge days were found.");
                return;
            }
            SaveableEntry se = new SaveableEntry(key,ResultTableDefinition);
            se.AddField("BridgeDay", SqliteDataType.DateTime);
            se.AddField("BridgeDayJson", SqliteDataType.Text);

            foreach (var afftagset in sets.Entries) {
                se.AddRow(RowBuilder.Start("BridgeDay", afftagset)
                    .Add("BridgeDayJson", JsonConvert.SerializeObject(afftagset, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<BridgeDayEntries> Load([NotNull] HouseholdKey householdKey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<BridgeDayEntries>(ResultTableDefinition, householdKey, ExpectedResultCount.OneOrMore);
        }
    }
}
