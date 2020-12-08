using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcObjectInformation{
        public CalcObjectInformation(CalcObjectType calcObjectType, [JetBrains.Annotations.NotNull] string calcObjectName, [JetBrains.Annotations.NotNull] string basePath)
        {
            CalcObjectType = calcObjectType;
            CalcObjectName = calcObjectName;
            BasePath = basePath;
        }

        public CalcObjectType CalcObjectType { get; }
        [JetBrains.Annotations.NotNull]
        public string CalcObjectName { get; }
        [JetBrains.Annotations.NotNull]
        public string BasePath { get; }
    }

    public class CalcObjectInformationLogger: DataSaverBase
    {
        public const string TableName = "CalcObjectInformation";
        public CalcObjectInformationLogger([JetBrains.Annotations.NotNull] SqlResultLoggingService srls)
        : base(typeof(CalcObjectInformation),
            new ResultTableDefinition(TableName,ResultTableID.CalcObjectInformation, "Additional information about the calc object to help with further processing", CalcOption.HouseholdContents), srls)
    {
    }

    public override void Run(HouseholdKey key, object o)
    {
        CalcObjectInformation calcObjectInformation = (CalcObjectInformation)o;
        SaveableEntry se = GetStandardSaveableEntry(key);
        se.AddRow(RowBuilder.Start("Name", "CalcObjectInformation").Add("Json", JsonConvert.SerializeObject(calcObjectInformation, Formatting.Indented)).ToDictionary());
        if (Srls == null)
        {
            throw new LPGException("Data Logger was null.");
        }
            Srls.SaveResultEntry(se);
    }

        [JetBrains.Annotations.NotNull]
        public CalcObjectInformation Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcObjectInformation>(ResultTableDefinition, Constants.GeneralHouseholdKey,
                ExpectedResultCount.One)[0];
        }
    }
}
