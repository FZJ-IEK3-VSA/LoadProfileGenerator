using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcLoadTypeDtoLogger : DataSaverBase
    {
        public const string TableName  = "CalcLoadTypeDto";
        public CalcLoadTypeDtoLogger( [NotNull] SqlResultLoggingService srls): base(typeof(List<CalcLoadTypeDto>),
            new ResultTableDefinition(TableName,ResultTableID.LoadTypeDefinitions,"All the load types", CalcOption.BasicOverview), srls)
        {
        }

        public override void Run(HouseholdKey key,object o)
        {
            if (key != Constants.GeneralHouseholdKey) {
                throw new LPGException("Trying to save load types not in the general file");
            }
            List<CalcLoadTypeDto> calcLoadTypeDtoDictionary = (List<CalcLoadTypeDto>)o;
            SaveableEntry se = GetStandardSaveableEntry(key);
            foreach (CalcLoadTypeDto dto in calcLoadTypeDtoDictionary) {
                se.AddRow(RowBuilder.Start("Name", dto.Name).Add("Json", JsonConvert.SerializeObject(dto, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcLoadTypeDto> Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcLoadTypeDto>(ResultTableDefinition, Constants.GeneralHouseholdKey, ExpectedResultCount.OneOrMore);
        }
    }
}
