using System.Collections.Generic;
using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class DeviceTaggingSetLogger : DataSaverBase
    {
        public DeviceTaggingSetLogger([NotNull] SqlResultLoggingService srls): base(typeof(List<DeviceTaggingSetInformation>),new ResultTableDefinition(nameof(DeviceTaggingSetInformation),
            ResultTableID.DeviceTaggingSetInformation, "Device Tagging Sets"),srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se =  GetStandardSaveableEntry(key );
            var sets = (List<DeviceTaggingSetInformation>)o;
            foreach (var set in sets) {
                se.AddRow(RowBuilder.Start("Name", set.Name)
                    .Add("Json", JsonConvert.SerializeObject(set, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<DeviceTaggingSetInformation> Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<DeviceTaggingSetInformation>(ResultTableDefinition, Constants.GeneralHouseholdKey, ExpectedResultCount.OneOrMore);
        }
    }
}
