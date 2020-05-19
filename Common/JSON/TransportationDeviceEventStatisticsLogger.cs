using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON
{
    public class TransportationDeviceEventStatistics : IHouseholdKey
    {
        public TransportationDeviceEventStatistics([NotNull] HouseholdKey householdKey, [NotNull] string transportationDevice)
        {
            HouseholdKey = householdKey;
            TransportationDevice = transportationDevice;
        }

        public double TotalDistance { get; set; }
        public double TotalTimeSteps { get; set; }

        [NotNull]
        public string TransportationDevice { get; }
        public HouseholdKey HouseholdKey { get; }
        public int NumberOfEvents { get; set; }
        [UsedImplicitly]
        public double AverageDistancePerEvent => TotalDistance / NumberOfEvents;

        [UsedImplicitly]
        public double AverageTimePerEvent => TotalTimeSteps / NumberOfEvents;
    }
    public class TransportationDeviceEventStatisticsLogger : DataSaverBase
    {
        private const string TableName = "TransportationDeviceEventStatistics";
        public TransportationDeviceEventStatisticsLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(List<TransportationDeviceEventStatistics>),
                new ResultTableDefinition(TableName, ResultTableID.TransportationDeviceEventStatistics, "Statistics about the transportation"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var actionEntries = (List<TransportationDeviceEventStatistics>)o;
            //var actionEntries = objects.ConvertAll(x => (TransportationDeviceStatisticsEntry)x).ToList();
            SaveableEntry se = GetStandardSaveableEntry(key);
            foreach (var actionEntry in actionEntries)
            {
                se.AddRow(RowBuilder.Start("Name", actionEntry.TransportationDevice)
                    .Add("Json", JsonConvert.SerializeObject(actionEntry, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

       /* [ItemNotNull]
        [NotNull]
        public List<ActionEntry> Read([NotNull]HouseholdKey hhkey)
        {
            var res = Srls.ReadFromJson<ActionEntry>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
            return res;
        }*/
    }
}
