using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class AffordanceEnergyUseLogger : DataSaverBase {
        private const string Tablename = "AffordanceEnergyUses";

        public AffordanceEnergyUseLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(List<AffordanceEnergyUseEntry>), new ResultTableDefinition(Tablename,ResultTableID.AffordanceEnergyUse,"Json Summaries of all Energy uses", CalcOption.AffordanceEnergyUse), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<AffordanceEnergyUseEntry>)o;
            //var affordanceEnergyUseEntries = objects.ConvertAll(x => (AffordanceEnergyUseEntry) x).ToList();
            foreach (var ae in objects) {
                se.AddRow(RowBuilder.Start("Name", ae.AffordanceName)
                    .Add("Json", JsonConvert.SerializeObject(ae, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<AffordanceEnergyUseEntry> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<AffordanceEnergyUseEntry>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
