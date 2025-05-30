using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers
{
    public class TransportationDeviceChoiceLogger : DataSaverBase
    {
        private const string TableName = "TransportationDeviceChoices";

        public TransportationDeviceChoiceLogger(SqlResultLoggingService srls) :
            base(typeof(TransportationDeviceChoice), new ResultTableDefinition(TableName, ResultTableID.TransportationDeviceChoices, "Transportation Device Choices", CalcOption.TransportationDeviceChoices), srls)
        {
        }

        public List<TransportationDeviceChoice> Load(HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var res = Srls.ReadFromJson<TransportationDeviceChoice>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
            return res;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var choiceEntries = objects.Cast<TransportationDeviceChoice>();
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("TimeStep", SqliteDataType.Text);
            se.AddField("PersonName", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            foreach (var choiceEntry in choiceEntries)
            {
                var row = RowBuilder.Start("PersonName", choiceEntry.PersonName).Add("TimeStep", choiceEntry.Timestep)
                    .Add("Json", JsonConvert.SerializeObject(choiceEntry, Formatting.Indented)).ToDictionary();
                se.AddRow(row);
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}