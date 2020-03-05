using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcTravelRouteDtoLogger : DataSaverBase {
        private const string Tablename = "TravelRoutes";

        public CalcTravelRouteDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcTravelRouteDto), new ResultTableDefinition(Tablename,ResultTableID.TravelRouteDefinitions, "Json Specification of the Travel Routes"), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var transportationDeviceDtos = objects.ConvertAll(x => (CalcTravelRouteDto) x).ToList();
            foreach (var device in transportationDeviceDtos) {
                se.AddRow(RowBuilder.Start("Name", device.Name)
                    .Add("Json", JsonConvert.SerializeObject(device, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcTravelRouteDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcTravelRouteDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
