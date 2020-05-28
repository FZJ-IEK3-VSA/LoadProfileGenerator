using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class SingleTimestepActionEntryLogger : DataSaverBase
    {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string TableName = "ActionsEachTimestep";
        public SingleTimestepActionEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(SingleTimestepActionEntry), new ResultTableDefinition(TableName,
                ResultTableID.SingleTimeStepActionEntry, "Selected Action for each time step for each person", CalcOption.ActionsEachTimestep), srls)
        {
            _srls = srls;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (SingleTimestepActionEntry)x).ToList();
            var rowEntries = new List<Dictionary<string, object>>();
            foreach (var affordanceEnergyUseEntry in actionEntries)
            {
                rowEntries.Add(RowBuilder.Start("Timestep", affordanceEnergyUseEntry.TimeStep)
                    .Add("Json", JsonConvert.SerializeObject(affordanceEnergyUseEntry, Formatting.Indented)).ToDictionary());
            }

            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("Timestep", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);

            foreach (Dictionary<string, object> entry in rowEntries)
            {
                se.AddRow(entry);
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<SingleTimestepActionEntry> Read([NotNull] HouseholdKey key)
        {
            var res =
                _srls.ReadFromJson<SingleTimestepActionEntry>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
            return res;
        }
    }
}