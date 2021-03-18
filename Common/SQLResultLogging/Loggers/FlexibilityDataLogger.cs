using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class FlexibilityDataLogger : DataSaverBase
    {
        private const string TableName = "FlexibilityData";
        public FlexibilityDataLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(TimeShiftableDeviceActivation), new ResultTableDefinition(TableName, ResultTableID.FlexibilityInformation,
                "Flexibility Data", CalcOption.FlexibilityEvents), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var timeShiftableDeviceActivations = objects.ConvertAll(x => (TimeShiftableDeviceActivation)x).ToList();
            SaveableEntry se = GetStandardSaveableEntry(key);
            //se.AddField("Timestep", SqliteDataType.Integer);
            foreach (var tsda in timeShiftableDeviceActivations)
            {
                se.AddRow(RowBuilder.Start("Name", tsda.Device.Name)
                    //.Add("Timestep", tsda.EarliestStart.ExternalStep)
                    .Add("Json", JsonConvert.SerializeObject(tsda, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<TimeShiftableDeviceActivation> Read([NotNull] HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var res = Srls.ReadFromJson<TimeShiftableDeviceActivation>(ResultTableDefinition, hhkey, ExpectedResultCount.OneOrMore);
            return res;
        }
    }
}