using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class HouseholdDtoLogger : DataSaverBase
    {
        public HouseholdDtoLogger([NotNull] SqlResultLoggingService srls) : base(typeof(CalcHouseholdDto),
            new ResultTableDefinition("HouseholdDefinitions", ResultTableID.HouseholdDefinitions, "Json Specification of the household", CalcOption.HouseholdContents), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var hh = (CalcHouseholdDto)o;
            se.AddRow(RowBuilder.Start("Name", hh.Name)
                .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary());
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        public CalcHouseholdDto Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var allhhs= Srls.ReadFromJson<CalcHouseholdDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
            return allhhs.Single(x => x.HouseholdKey == key);
        }
    }
}
