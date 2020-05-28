using System;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.JSON;

namespace Common.SQLResultLogging.Loggers {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using SQLResultLogging;
    public class TotalsPerLoadtypeEntryLogger : DataSaverBase
    {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string TableName = "TotalsPerLoadtype";
        public TotalsPerLoadtypeEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(TotalsPerLoadtypeEntry), new ResultTableDefinition(TableName, ResultTableID.TotalsPerLoadtype, "Total Per Loadtype Entries", CalcOption.TotalsPerLoadtype), srls)
        {
            _srls = srls;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (TotalsPerLoadtypeEntry)x).ToList();
            var rowEntries = new List<Dictionary<string, object>>();
            foreach (var actionEntry in actionEntries)
            {
                rowEntries.Add(RowBuilder.Start("Name", actionEntry.Loadtype.Name)
                    .Add("Json", JsonConvert.SerializeObject(actionEntry, Formatting.Indented)).ToDictionary());
            }

            SaveableEntry se = GetStandardSaveableEntry(key);
            foreach (Dictionary<string, object> entry in rowEntries)
            {
                se.AddRow(entry);
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<TotalsPerLoadtypeEntry> Read([NotNull] HouseholdKey key)
        {
            var res =
                _srls.ReadFromJson<TotalsPerLoadtypeEntry>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
            //res.ForEach(x => x.HouseholdKey = hhkey);
            return res;
        }
    }
    public class TotalsPerDeviceEntry : IHouseholdKey
    {
        [Obsolete("for json only")]
        // ReSharper disable once NotNullMemberIsNotInitialized
        public TotalsPerDeviceEntry()
        {

        }
        public TotalsPerDeviceEntry([NotNull] HouseholdKey key, CalcDeviceDto device, [NotNull] CalcLoadTypeDto loadType)
        {
            HouseholdKey = key;
            Device = device;
            Loadtype = loadType;
        }

        public CalcDeviceDto Device { get; set; }

        [NotNull]
        [JsonProperty]
        public CalcLoadTypeDto Loadtype { get; set; }


        [JsonProperty]
        public double NegativeValues { get; set; }
        [JsonProperty]
        public double PositiveValues { get; set; }
        [JsonProperty]
        public double Value { get; set; }
        [JsonProperty]
        public Dictionary<int, double> ConsumptionPerMonth { get; set; } = new Dictionary<int, double>();

        [JsonProperty]
        public HouseholdKey HouseholdKey { get; private set; }

        public void AddConsumptionValue( int dtMonth, double activationEntryTotalEnergySum)
        {

            Value += activationEntryTotalEnergySum;
            if (activationEntryTotalEnergySum < 0) {
                NegativeValues += activationEntryTotalEnergySum;
            }
            else {
                PositiveValues += activationEntryTotalEnergySum;
            }


            if (!ConsumptionPerMonth.ContainsKey(dtMonth))
            {
                ConsumptionPerMonth.Add(dtMonth, 0);
            }
            ConsumptionPerMonth[dtMonth] += activationEntryTotalEnergySum;
        }
    }
    public class TotalsPerDeviceLogger : DataSaverBase
    {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string TableName = "TotalsPerDevice";
        public TotalsPerDeviceLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(TotalsPerDeviceEntry), new ResultTableDefinition(
                TableName, ResultTableID.TotalsPerDevice, "Total Per Device Entries", CalcOption.TotalsPerDevice), srls)
        {
            _srls = srls;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (TotalsPerDeviceEntry)x).ToList();
            var rowEntries = new List<Dictionary<string, object>>();
            foreach (var actionEntry in actionEntries)
            {
                rowEntries.Add(RowBuilder.Start("Name", actionEntry.Device?.Name)
                    .Add("Json", JsonConvert.SerializeObject(actionEntry, Formatting.Indented)).ToDictionary());
            }

            SaveableEntry se = GetStandardSaveableEntry(key);
            foreach (Dictionary<string, object> entry in rowEntries)
            {
                se.AddRow(entry);
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<TotalsPerDeviceEntry> Read([NotNull] HouseholdKey key)
        {
            var res =
                _srls.ReadFromJson<TotalsPerDeviceEntry>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
            //res.ForEach(x => x.HouseholdKey = hhkey);
            return res;
        }
    }
    public class PersonAffordanceInformationLogger : DataSaverBase
    {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string TableName = "AffordanceTimeUse";
        public PersonAffordanceInformationLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(PersonAffordanceInformation), new ResultTableDefinition(TableName, ResultTableID.PersonAffordanceInformation, "Time Use Per Affordance Entries", CalcOption.TimeOfUsePlot), srls)
        {
            _srls = srls;
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (PersonAffordanceInformation)x).ToList();
            var rowEntries = new List<Dictionary<string, object>>();
            foreach (var affordanceEnergyUseEntry in actionEntries)
            {
                rowEntries.Add(RowBuilder.Start( "PersonName", affordanceEnergyUseEntry.PersonName)
                    .Add("Json", JsonConvert.SerializeObject(affordanceEnergyUseEntry, Formatting.Indented)).ToDictionary());
            }

            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("PersonName", SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            foreach (Dictionary<string, object> entry in rowEntries)
            {
                se.AddRow(entry);
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<PersonAffordanceInformation> Read([NotNull] HouseholdKey key)
        {
            var res =
                _srls.ReadFromJson<PersonAffordanceInformation>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
            //res.ForEach(x => x.HouseholdKey = hhkey);
            return res;
        }
    }
}