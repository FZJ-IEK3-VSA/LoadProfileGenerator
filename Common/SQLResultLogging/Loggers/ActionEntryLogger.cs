using Automation;
using Automation.ResultFiles;

namespace Common.SQLResultLogging.Loggers {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using SQLResultLogging;

    public class ActionEntryLogger : DataSaverBase {
        private const string TableName = "PerformedActions";
        public ActionEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(ActionEntry), new ResultTableDefinition(TableName,ResultTableID.PerformedActions, "Action Entries", CalcOption.ActionEntries), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (ActionEntry)x).ToList();
                SaveableEntry se = GetStandardSaveableEntry(key);
                foreach (var actionEntry in actionEntries) {
                    se.AddRow(RowBuilder.Start("Name", actionEntry.AffordanceName)
                        .Add("Json", JsonConvert.SerializeObject(actionEntry, Formatting.Indented)).ToDictionary());
                }
                if (Srls == null)
                {
                    throw new LPGException("Data Logger was null.");
                }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<ActionEntry> Read([NotNull]HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var res = Srls.ReadFromJson<ActionEntry>(ResultTableDefinition, hhkey,ExpectedResultCount.OneOrMore);
            return res;
        }
    }
}