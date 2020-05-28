using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON
{
    public class TransportationRouteStatistics : IHouseholdKey
    {
        public TransportationRouteStatistics([NotNull] HouseholdKey householdKey, [NotNull] string transportationDevice,
                                             [NotNull] string route)
        {
            HouseholdKey = householdKey;
            TransportationDevice = transportationDevice;
            Route = route;
        }

        [NotNull]
        public string Route { get; }
        public double TotalDistance { get; set; }
        public double TotalTimeSteps { get; set; }

        [NotNull]
        public string TransportationDevice { get; }
        public HouseholdKey HouseholdKey { get; }
        public int NumberOfEvents { get; set; }
    }
    public class TransportationRouteStatisticsLogger : DataSaverBase
    {
        private const string TableName = "TransportationRouteStatistics";
        public TransportationRouteStatisticsLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(List<TransportationRouteStatistics>),
                new ResultTableDefinition(TableName, ResultTableID.TransportationRouteStatistics, "Statistics about the transportation", CalcOption.TransportationStatistics), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var actionEntries = (List<TransportationRouteStatistics>)o;
            //var actionEntries = objects.ConvertAll(x => (TransportationDeviceStatisticsEntry)x).ToList();
            SaveableEntry se = GetStandardSaveableEntry(key);
            foreach (var actionEntry in actionEntries)
            {
                se.AddRow(RowBuilder.Start("Name", actionEntry.Route)
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
