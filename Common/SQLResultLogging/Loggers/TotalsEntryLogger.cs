using Automation.ResultFiles;
using Common.JSON;

namespace Common.SQLResultLogging.Loggers {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using SQLResultLogging;
    public class TotalsEntryLogger : DataSaverBase
    {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string TableName = "TotalsPerLoadtype";
        public TotalsEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(TotalsEntry), new ResultTableDefinition(TableName, ResultTableID.TotalsPerLoadtype, "Total Per Loadtype Entries"), srls)
        {
            _srls = srls;
        }

        public override void Run([NotNull] HouseholdKey key, [NotNull] object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (TotalsEntry)x).ToList();
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
        public List<TotalsEntry> Read([NotNull] HouseholdKey key)
        {
            var res =
                _srls.ReadFromJson<TotalsEntry>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
            //res.ForEach(x => x.HouseholdKey = hhkey);
            return res;
        }
    }
    public class PersonAffordanceInformationLogger : DataSaverBase
    {
        [NotNull] private readonly SqlResultLoggingService _srls;
        private const string TableName = "AffordanceTimeUse";
        public PersonAffordanceInformationLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(PersonAffordanceInformation), new ResultTableDefinition(TableName, ResultTableID.PersonAffordanceInformation, "Time Use Per Affordance Entries"), srls)
        {
            _srls = srls;
        }

        public override void Run([NotNull] HouseholdKey key, [NotNull] object o)
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