using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcVariableDtoLogger : DataSaverBase
    {
        public CalcVariableDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcVariableDto), new ResultTableDefinition("CalcVariableDefinition",ResultTableID.VariableDefinitions, "Json Specification of the Variables"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key );
            var objects = (List<IHouseholdKey>)o;
            var variableDefinitions = objects.ConvertAll(x => (CalcVariableDto) x).ToList();
            foreach (var calcPersonDto in variableDefinitions) {
                se.AddRow(RowBuilder.Start("Name", calcPersonDto.Name)
                    .Add("Json", JsonConvert.SerializeObject(calcPersonDto, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}
