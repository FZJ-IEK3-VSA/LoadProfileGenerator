using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcTransportationDeviceDtoLogger : DataSaverBase {
        private const string Tablename = "TransportationDevices";

        public CalcTransportationDeviceDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcTransportationDeviceDto), new ResultTableDefinition(Tablename, ResultTableID.TransportationDeviceDefinitions, "Json Specification of the transportation devices", CalcOption.HouseholdContents), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var transportationDeviceDtos = objects.ConvertAll(x => (CalcTransportationDeviceDto) x).ToList();
            foreach (var device in transportationDeviceDtos) {
                se.AddRow(RowBuilder.Start("Name", device.Name)
                    .Add("Json", JsonConvert.SerializeObject(device, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcTransportationDeviceDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcTransportationDeviceDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
