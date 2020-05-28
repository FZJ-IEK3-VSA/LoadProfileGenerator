using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class ChargingStationStateLogger : DataSaverBase {
        private const string Tablename = "ChargingStationStates";

        public ChargingStationStateLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(ChargingStationState), new ResultTableDefinition(Tablename,ResultTableID.ChargingeStationState, "State of the charging stations", CalcOption.TransportationStatistics), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var transportationDeviceStateEntries = objects.ConvertAll(x => (ChargingStationState)x).ToList();
            SaveableEntry se = new SaveableEntry(key,ResultTableDefinition);
            se.AddField("Time", SqliteDataType.Integer);
            se.AddField("ChargingStationName", SqliteDataType.Text);
            se.AddField("IsAvailable", SqliteDataType.Bit);
            se.AddField("CarName", SqliteDataType.Text);
            se.AddField("ChargingPower", SqliteDataType.Double);
            se.AddField("Json", SqliteDataType.JsonField);
            foreach (var ae in transportationDeviceStateEntries)
            {
                se.AddRow(RowBuilder.Start("Time", ae.TimeStep)
                    .Add("ChargingStationName", ae.ChargingStationName)
                    .Add("IsAvailable", ae.IsAvailable?1:0)
                    .Add("CarName", ae.ConnectedCarName)
                    .Add("ChargingPower", ae.ChargingPower)
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
        public List<ChargingStationState> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<ChargingStationState>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
