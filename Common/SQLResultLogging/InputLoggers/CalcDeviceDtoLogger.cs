using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcDeviceDtoLogger : DataSaverBase
    {
        public CalcDeviceDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcDeviceDto),  new ResultTableDefinition("DevicesDefinitions",ResultTableID.DeviceDefinitions,"Json Specification of the Devices"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var persons = objects.ConvertAll(x => (CalcDeviceDto) x).ToList();
            foreach (var calcPersonDto in persons) {
                se.AddRow(RowBuilder.Start("Name", calcPersonDto.Name)
                    .Add("Json", JsonConvert.SerializeObject(calcPersonDto, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcDeviceDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
