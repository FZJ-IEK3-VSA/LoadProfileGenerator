using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcSiteDtoLogger : DataSaverBase {
        private const string Tablename = "SiteDefinitions";

        public CalcSiteDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcSiteDto), new ResultTableDefinition(Tablename,ResultTableID.SiteDefinitions, "Json Specification of the Sites"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var sites = objects.ConvertAll(x => (CalcSiteDto) x).ToList();
            foreach (var site in sites) {
                se.AddRow(RowBuilder.Start("Name", site.Name)
                    .Add("Json", JsonConvert.SerializeObject(site, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcSiteDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcSiteDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
