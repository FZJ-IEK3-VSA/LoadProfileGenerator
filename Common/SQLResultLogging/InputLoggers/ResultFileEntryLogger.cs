using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class ResultFileEntryLogger : DataSaverBase
    {
        private const string TableName = "ResultFileEntries";
        public ResultFileEntryLogger([NotNull] IResultLoggingService srls) : base(typeof(ResultFileEntry),
            new ResultTableDefinition(TableName, ResultTableID.ResultFileEntries, "Result files", CalcOption.BasicOverview), srls)
        { }

        public override void Run(HouseholdKey key, object o)
        {
            if (Srls == null)
                throw new LPGException("Data Logger was null.");

            var hh = (ResultFileEntry)o;
            var row = BuildRow(hh);

            SaveableEntry se = GetStandardSaveableEntry(key);
            se.AddRow(row);
            Srls.SaveResultEntry(se);
        }

        private static Dictionary<string, object> BuildRow(ResultFileEntry hh)
        {
            return RowBuilder.Start("Name", Constants.GeneralHouseholdKey).Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary();
        }

        /// <summary>
        /// Deletes entries from the result file list in the database
        /// </summary>
        /// <param name="rfes">The entries to delete</param>
        public void DeleteEntries(IEnumerable<ResultFileEntry> rfes)
        {
            // create rows as the ones already in the database that should be deleted
            var rows = rfes.Select(rfe => BuildRow(rfe));
            Srls.DeleteEntries(rows, TableName, Constants.GeneralHouseholdKey);
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
