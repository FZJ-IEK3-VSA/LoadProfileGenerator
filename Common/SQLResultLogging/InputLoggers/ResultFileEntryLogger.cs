using System.Collections.Generic;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class ResultFileEntryLogger : DataSaverBase {
        private bool _isTableCreated;
        private const string TableName = "ResultFileEntries";
        public ResultFileEntryLogger([NotNull] SqlResultLoggingService srls, bool isTableCreated=false): base(typeof(ResultFileEntry),
            new ResultTableDefinition(TableName,ResultTableID.ResultFileEntries, "Result files"),srls)
        {
            _isTableCreated = isTableCreated;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var hh = (ResultFileEntry)o;
            if (!_isTableCreated) {
                SaveableEntry se = GetStandardSaveableEntry(key);
                    se.AddRow(RowBuilder.Start("Name", Constants.GeneralHouseholdKey)
                        .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary());
                    if (Srls == null)
                    {
                        throw new LPGException("Data Logger was null.");
                    }
                Srls.SaveResultEntry(se);
                _isTableCreated = true;
            }
            else {
                var row = RowBuilder.Start("Name", Constants.GeneralHouseholdKey)
                    .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary();
                if (Srls == null)
                {
                    throw new LPGException("Data Logger was null.");
                }
                Srls.SaveDictionaryToDatabaseNewConnection(row, TableName, Constants.GeneralHouseholdKey);
            }
        }

        [ItemNotNull]
        [NotNull]
        public List<ResultFileEntry> Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<ResultFileEntry>(ResultTableDefinition, Constants.GeneralHouseholdKey,
                ExpectedResultCount.OneOrMore);
        }
    }
}
