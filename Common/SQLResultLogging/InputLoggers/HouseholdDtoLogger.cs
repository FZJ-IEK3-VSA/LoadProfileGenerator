using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class HouseholdDtoLogger : DataSaverBase
    {
        public HouseholdDtoLogger([NotNull] SqlResultLoggingService srls): base(typeof(CalcHouseholdDto),
           new ResultTableDefinition("HouseholdDefinitions",ResultTableID.HouseholdDefinitions,  "Json Specification of the household"),srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se =  GetStandardSaveableEntry(key);
            var hh = (CalcHouseholdDto)o;
                se.AddRow(RowBuilder.Start("Name", hh.Name)
                    .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary());
                if (Srls == null)
                {
                    throw new LPGException("Data Logger was null.");
                }
            Srls.SaveResultEntry(se);
        }
    }
}
