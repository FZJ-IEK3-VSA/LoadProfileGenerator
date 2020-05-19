using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class DeviceActivationEntryLogger : DataSaverBase {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string Tablename = "DeviceActivationEntries";

        public DeviceActivationEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(DeviceActivationEntry),  new ResultTableDefinition(Tablename,ResultTableID.DeviceActivationEntries, "Device Activation Entries"), srls) => _srls = srls;

        [ItemNotNull]
        [NotNull]
        public List<DeviceActivationEntry> Read([NotNull] HouseholdKey hhkey)
        {
            var res = _srls.ReadFromJson<DeviceActivationEntry>(ResultTableDefinition, hhkey,
                ExpectedResultCount.OneOrMore);
            return res;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var affordanceActivationEntries = objects.ConvertAll(x => (DeviceActivationEntry)x).ToList();
            SaveableEntry se = new SaveableEntry(key,ResultTableDefinition);
            se.AddField("AffordanceName", SqliteDataType.Text);
            se.AddField("PersonName", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            foreach (var affordanceActivationEntry in affordanceActivationEntries) {
                se.AddRow(RowBuilder.Start("AffordanceName", affordanceActivationEntry.AffordanceName)
                    .Add("PersonName", affordanceActivationEntry.ActivatorName)
                    .Add("Json", JsonConvert.SerializeObject(affordanceActivationEntry, Formatting.Indented))
                    .ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}