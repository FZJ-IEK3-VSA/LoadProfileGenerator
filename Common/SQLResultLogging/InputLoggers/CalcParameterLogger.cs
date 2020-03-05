using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcParameterLogger : DataSaverBase
    {
        public const string TableName = "CalcParameters";

        public CalcParameterLogger( [NotNull] SqlResultLoggingService srls): base(typeof(CalcParameters),
             new ResultTableDefinition(TableName,ResultTableID.CalcParameters, "All the calculation parameters"), srls)
        {
        }

        public override void Run([NotNull] HouseholdKey key,[NotNull] object o)
        {
            CalcParameters calcParameters = (CalcParameters)o;
            SaveableEntry se = GetStandardSaveableEntry(key);
            se.AddRow(RowBuilder.Start("Name", "CalcParameters").Add("Json", JsonConvert.SerializeObject(calcParameters, Formatting.Indented)).ToDictionary());
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
        [NotNull]
        public CalcParameters Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcParameters>(ResultTableDefinition, Constants.GeneralHouseholdKey,ExpectedResultCount.One)[0];
        }
    }
}
