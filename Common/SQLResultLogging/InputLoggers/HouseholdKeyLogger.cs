using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class HouseholdKeyLogger : DataSaverBase {
        private const string TableName = "HouseholdKeys";
        private bool _isTableCreated;
        [ItemNotNull] [NotNull] private readonly HashSet<HouseholdKey> _savedKeys = new HashSet<HouseholdKey>();
        public HouseholdKeyLogger([NotNull] SqlResultLoggingService srls):
            base(typeof(HouseholdKeyEntry),new ResultTableDefinition(TableName,ResultTableID.HouseholdKeys, "All Householdkeys", CalcOption.BasicOverview),srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var hh = (HouseholdKeyEntry)o;
            //check for duplicates
            if (!_savedKeys.Add(hh.HouseholdKey)) {
                return;
                //throw new LPGException("Householdkey already existed");
            }
            if (!_isTableCreated) {
                SaveableEntry se = GetStandardSaveableEntry(key);
                    se.AddRow(RowBuilder.Start("Name", hh.HouseholdKey)
                        .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary());
                    if (Srls == null)
                    {
                        throw new LPGException("Data Logger was null.");
                    }
                Srls.SaveResultEntry(se);
                _isTableCreated = true;
                return;
            }

            var dict = RowBuilder.Start("Name", hh.HouseholdKey)
                .Add("Json", JsonConvert.SerializeObject(hh, Formatting.Indented)).ToDictionary();
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveDictionaryToDatabaseNewConnection(dict,TableName,Constants.GeneralHouseholdKey);
        }

        [ItemNotNull]
        [NotNull]
        public List<HouseholdKeyEntry> Load()
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<HouseholdKeyEntry>(ResultTableDefinition, Constants.GeneralHouseholdKey,
                ExpectedResultCount.OneOrMore);
        }

        [ItemNotNull]
        [NotNull]
        public static List<HouseholdKeyEntry>  Load([NotNull] SqlResultLoggingService srls)
        {
            HouseholdKeyLogger hhkl = new HouseholdKeyLogger(srls);
            return hhkl.Load();
        }
    }
}
