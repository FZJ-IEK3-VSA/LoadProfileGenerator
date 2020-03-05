using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcPersonDtoLogger : DataSaverBase
    {
        public CalcPersonDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcPersonDto), new ResultTableDefinition("PersonDefinitions",ResultTableID.PersonDefinitions, "Json Specification of the Persons"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var persons = objects.ConvertAll(x => (CalcPersonDto) x).ToList();
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
        public List<CalcPersonDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcPersonDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
