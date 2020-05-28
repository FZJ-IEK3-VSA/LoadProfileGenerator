using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON
{
    public class TransportationDeviceStatisticsLogger : DataSaverBase
    {
        private const string TableName = "TransportationDeviceStatistics";
        public TransportationDeviceStatisticsLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(List<TransportationDeviceStatisticsEntry>), new ResultTableDefinition(TableName, ResultTableID.TransportationDeviceStatistics, "Statistics about the transportation", CalcOption.TransportationStatistics), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var actionEntries = (List<TransportationDeviceStatisticsEntry>)o;
            //var actionEntries = objects.ConvertAll(x => (TransportationDeviceStatisticsEntry)x).ToList();
            SaveableEntry se = GetStandardSaveableEntry(key);
            foreach (var actionEntry in actionEntries)
            {
                se.AddRow(RowBuilder.Start("Name", actionEntry.TransportationDeviceName)
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

    public class TransportationDeviceStatisticsEntry:IHouseholdKey
        {
            public TransportationDeviceStatisticsEntry(StrGuid transportationDeviceGuid, [NotNull] string transportationDeviceName, [NotNull] HouseholdKey householdKey)
            {
                TransportationDeviceGuid = transportationDeviceGuid;
                TransportationDeviceName = transportationDeviceName;
                HouseholdKey = householdKey;
            }

            public StrGuid TransportationDeviceGuid { get; }
            [NotNull]
            public string TransportationDeviceName { get; }
            public double TotalDistanceTraveled { get; set; }
            public double TotalDistanceCharged { get; set; }
            [NotNull]
            public Dictionary<TransportationDeviceState, int> StepsPerState { get; } = new Dictionary<TransportationDeviceState, int>();
            [NotNull]
            public Dictionary<string, int> StepsPerSite { get; } = new Dictionary<string, int>();
            public double MaxRange { get; set; } = double.MinValue;
            public double MinRange { get; set; } = double.MaxValue;

            public void ProcessOneState([NotNull] TransportationDeviceStateEntry state)
            {
                string site = state.CurrentSite ?? "(no site)";
                if (!StepsPerSite.ContainsKey(site))
                {
                    StepsPerSite.Add(site, 0);
                }

                StepsPerSite[site]++;
                if (!StepsPerState.ContainsKey(state.TransportationDeviceStateEnum))
                {
                    StepsPerState.Add(state.TransportationDeviceStateEnum, 0);
                }

                StepsPerState[state.TransportationDeviceStateEnum]++;
                if (MaxRange < state.CurrentRange)
                {
                    MaxRange = state.CurrentRange;
                }
                if (MinRange > state.CurrentRange)
                {
                    MinRange = state.CurrentRange;
                }
            }

            public HouseholdKey HouseholdKey { get; }
        }
    }
