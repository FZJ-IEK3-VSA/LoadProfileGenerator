using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcAffordanceTaggingSetDtoLogger : DataSaverBase {
        private const string TableName = "AffordanceTaggingSets";
        public CalcAffordanceTaggingSetDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(List<CalcAffordanceTaggingSetDto>),  new ResultTableDefinition(TableName,ResultTableID.AffordanceTaggingSets, "Json Specification of the Affordances Tagging Sets", CalcOption.AffordanceTaggingSets), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var sets = (List<CalcAffordanceTaggingSetDto>)o;
            foreach (var afftagset in sets) {
                se.AddRow(RowBuilder.Start("Name", afftagset.Name)
                    .Add("Json", JsonConvert.SerializeObject(afftagset, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcAffordanceTaggingSetDto> Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcAffordanceTaggingSetDto>(ResultTableDefinition, Constants.GeneralHouseholdKey, ExpectedResultCount.OneOrMore);
        }
    }
}
