using System.Collections.Generic;
using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class HouseDtoLogger : DataSaverBase
    {
        public HouseDtoLogger([NotNull] SqlResultLoggingService srls):
            base(typeof(CalcHouseDto), new ResultTableDefinition("HouseDefinition",ResultTableID.HouseDefinition, "Json Specification of the house"),srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se =  GetStandardSaveableEntry(key);
            var hh = (CalcHouseDto)o;
                se.AddRow(RowBuilder.Start("Name", hh.HouseName)
                    .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary());
                if (Srls == null)
                {
                    throw new LPGException("Data Logger was null.");
                }
            Srls.SaveResultEntry(se);
        }
        [ItemNotNull]
        [NotNull]
        public List<CalcHouseDto> Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcHouseDto>(ResultTableDefinition, Constants.GeneralHouseholdKey, ExpectedResultCount.OneOrMore);
        }
    }
}
