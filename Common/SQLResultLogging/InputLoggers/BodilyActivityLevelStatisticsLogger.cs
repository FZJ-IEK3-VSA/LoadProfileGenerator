using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class BodilyActivityLevelStatistics : IHouseholdKey
    {
        [Obsolete("json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public BodilyActivityLevelStatistics()
        {}
        public BodilyActivityLevelStatistics([NotNull] HouseholdKey householdKey) => HouseholdKey = householdKey;

        public HouseholdKey HouseholdKey { get; set; }
        [NotNull]
        public Dictionary<BodilyActivityLevel, List<double>> ActivityLevels { get; set; } = new Dictionary<BodilyActivityLevel, List<double>>();
    }
    public class BodilyActivityLevelStatisticsLogger : DataSaverBase
    {
        public BodilyActivityLevelStatisticsLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(BodilyActivityLevelStatistics),
            new ResultTableDefinition("BodilyActivityLevelCount",
                ResultTableID.BodilyActivityLevelCount,
                "Json with the bodily activity level count per timestep per level", CalcOption.BodilyActivityStatistics), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var hh = (BodilyActivityLevelStatistics)o;
            se.AddRow(RowBuilder.Start("Name", hh.HouseholdKey.Key)
                .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary());
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
    }
}
