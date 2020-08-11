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
    public class CalcDeviceArchiveDtoLogger : DataSaverBase
    {
        public CalcDeviceArchiveDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcDeviceArchiveDto), new ResultTableDefinition("ArchiveOfAllDeviceDtos", ResultTableID.DeviceArchive, "Json Specification of all Devices, including charging stations", CalcOption.DeviceActivations), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var persons = objects.ConvertAll(x => (CalcDeviceArchiveDto)x).ToList();
            foreach (var devicedto in persons)
            {
                se.AddRow(RowBuilder.Start("Name", devicedto.Device.Name)
                    .Add("Json", JsonConvert.SerializeObject(devicedto, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceArchiveDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcDeviceArchiveDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }


        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceDto> Load([NotNull] List<HouseholdKeyEntry> keys)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }

            var alldevices = new List<CalcDeviceDto>();
            foreach (var key in keys)
            {
                alldevices.AddRange(Srls.ReadFromJson<CalcDeviceDto>(ResultTableDefinition, key.HHKey, ExpectedResultCount.OneOrMore));
            }

            return alldevices;
        }
    }
    public class CalcDeviceDtoLogger : DataSaverBase
    {
        public CalcDeviceDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcDeviceDto),  new ResultTableDefinition("DevicesDefinitions",ResultTableID.DeviceDefinitions,"Json Specification of the Devices", CalcOption.HouseholdContents), srls)
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


        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceDto> Load([NotNull] List<HouseholdKeyEntry> keys)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }

            var alldevices = new List<CalcDeviceDto>();
            foreach (var key in keys) {
                alldevices.AddRange(Srls.ReadFromJson<CalcDeviceDto>(ResultTableDefinition, key.HHKey, ExpectedResultCount.OneOrMore));
            }

            return alldevices;
        }
    }
}
