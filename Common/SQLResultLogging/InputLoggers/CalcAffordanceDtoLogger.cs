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
    public class CalcAffordanceDtoLogger : DataSaverBase
    {
        public CalcAffordanceDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcAffordanceDto), new ResultTableDefinition( "AffordanceDefinitions",ResultTableID.AffordanceDefinitions, "Json Specification of the Affordances", CalcOption.AffordanceDefinitions), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var affordances = objects.ConvertAll(x => (CalcAffordanceDto) x).ToList();
            foreach (var calcAffordanceDto in affordances) {
                se.AddRow(RowBuilder.Start("Name", calcAffordanceDto.Name)
                    .Add("Json", JsonConvert.SerializeObject(calcAffordanceDto, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcAffordanceDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcAffordanceDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
