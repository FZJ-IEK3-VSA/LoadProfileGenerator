using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class TransportationStateEntryLogger : DataSaverBase {
        private const string Tablename = "TransportationDeviceStates";

        public TransportationStateEntryLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(TransportationDeviceStateEntry), new ResultTableDefinition(Tablename,ResultTableID.TransportationDeviceStates, "All Transportation Device States"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var transportationDeviceStateEntries = objects.ConvertAll(x => (TransportationDeviceStateEntry) x).ToList();
            SaveableEntry se = new SaveableEntry( key, ResultTableDefinition);
            se.AddField("Time", SqliteDataType.Integer);
            se.AddField("DateTime", SqliteDataType.Integer);
            se.AddField("DeviceName", SqliteDataType.Text);
            se.AddField("User", SqliteDataType.Text);
            se.AddField("DeviceState", SqliteDataType.Text);
            se.AddField("NumericDeviceState", SqliteDataType.Integer);
            se.AddField("CurrentRange", SqliteDataType.Double);
            se.AddField("CurrentSite", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.JsonField);
            foreach (var ae in transportationDeviceStateEntries) {
                se.AddRow(RowBuilder.Start("Time", ae.TimeStep)
                    .Add("DateTime",ae.DateTime)
                    .Add("DeviceName", ae.TransportationDeviceName)
                    .Add("User", ae.CurrentUser)
                    .Add("DeviceState", ae.TransportationDeviceState)
                    .Add("NumericDeviceState",(int) ae.TransportationDeviceStateEnum)
                    .Add("CurrentRange", ae.CurrentRange)
                    .Add("CurrentSite", ae.CurrentSite)
                    .Add("Json", JsonConvert.SerializeObject(ae, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<TransportationDeviceStateEntry> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<TransportationDeviceStateEntry>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
