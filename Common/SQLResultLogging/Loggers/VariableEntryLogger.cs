using Automation;
using Automation.ResultFiles;

namespace Common.SQLResultLogging.Loggers {
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using SQLResultLogging;

    public class CalcVariableEntry :IHouseholdKey{
        public CalcVariableEntry([NotNull] string name, StrGuid guid, double value,
                                 [NotNull] string locationName, StrGuid locationGuid,
                                 [NotNull] HouseholdKey householdKey, [NotNull] TimeStep timeStep)
        {
            Name = name;
            Guid = guid;
            Value = value;
            LocationName = locationName;
            LocationGuid = locationGuid;
            HouseholdKey = householdKey;
            TimeStep = timeStep;
            SqlName = (householdKey + "_" + locationName + "_" + Name).Replace(" ", "");
        }

        [NotNull]
        public string Name { get; set; }
        public StrGuid Guid { get; set; }
        public double Value { get; set; }
        [NotNull]
        public string LocationName { get; set; }
        public StrGuid LocationGuid { get; set; }
        public HouseholdKey HouseholdKey { get; set; }

        [NotNull]
        public TimeStep TimeStep { get; }

        [NotNull]
        public string SqlName { get; set; }
    }
    public class VariableEntryLogger : DataSaverBase {
        private const string TableName = "VariableValues";
        public VariableEntryLogger([NotNull] SqlResultLoggingService srls) :
            base(typeof(CalcVariableEntry), new ResultTableDefinition(TableName,ResultTableID.VariableValues, "Variable Values", CalcOption.VariableLogFile), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var calcVarEntries = objects.ConvertAll(x => (CalcVariableEntry)x).ToList();
                SaveableEntry se = GetStandardSaveableEntry(key);
                se.AddField("TimeStep",SqliteDataType.Integer);
                foreach (var actionEntry in calcVarEntries) {
                    se.AddRow(RowBuilder.Start("Name", actionEntry.Name).Add("Timestep",actionEntry.TimeStep)
                        .Add("Json", JsonConvert.SerializeObject(actionEntry, Formatting.Indented)).ToDictionary());
                }
                if (Srls == null)
                {
                    throw new LPGException("Data Logger was null.");
                }
            Srls.SaveResultEntry(se);
        }

        [ItemNotNull]
        [NotNull]
        public List<CalcVariableEntry> Read([NotNull]HouseholdKey hhkey)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            var res = Srls.ReadFromJson<CalcVariableEntry>(ResultTableDefinition, hhkey,ExpectedResultCount.OneOrMore);
            return res;
        }
    }
}