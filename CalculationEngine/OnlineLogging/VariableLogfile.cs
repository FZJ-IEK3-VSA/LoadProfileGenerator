/*
using System.Collections.Generic;
using System.Linq;
using CalculationEngine.HouseholdElements;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineLogging {
    public class VariableLogfile {
        [NotNull]
        private readonly SqlResultLoggingService _sqlResultLoggingService;
        [NotNull]
        private readonly CalcVariableRepository _variableRepository;
        public const string TableName = "VariableValues";
        //private readonly CalcParameters _calcParameters;
        //private readonly Dictionary<string, int> _columns = new Dictionary<string, int>();
        //private readonly bool _showNegativeTime;
        //private readonly StreamWriter _sw;
        //private bool _headerWritten;
        //private DateStampCreator dsc;
        public VariableLogfile([NotNull] SqlResultLoggingService sqlResultLoggingService, [NotNull] CalcVariableRepository variableRepository)
        {
            _sqlResultLoggingService = sqlResultLoggingService;
            _variableRepository = variableRepository;
            var vr = variableRepository.GetAllVariables();
            var householdKeys = vr.Select(x => x.HouseholdKey).Distinct().ToList();

            foreach (var householdKey in householdKeys) {
                List<SqlResultLoggingService.FieldDefinition> fields = new List<SqlResultLoggingService.FieldDefinition>();
                SqlResultLoggingService.FieldDefinition timefield = new SqlResultLoggingService.FieldDefinition("Time", "int");
                fields.Add(timefield);
                foreach (CalcVariable calcVariable in vr)
                {
                    if (calcVariable.HouseholdKey == householdKey) {
                        SqlResultLoggingService.FieldDefinition field =
                            new SqlResultLoggingService.FieldDefinition(calcVariable.SqlName, "Double");
                        fields.Add(field);
                    }
                }
                sqlResultLoggingService.MakeTableForListOfFields(fields, householdKey,TableName);
            }
//            variableRepository.Get
            //throw new NotImplementedException("xxx");
          /*  _calcParameters = calcParameters;
            dsc = new DateStampCreator(_calcParameters);
            _showNegativeTime = _calcParameters.ShowSettlingPeriodTime;
            _sw = lf.FileFactoryAndTracker.MakeFile<StreamWriter>("Variablelogfile." + householdKey + ".csv",
                "Logfile with all the variable values", true, ResultFileID.VariableLogfile, householdKey,
                TargetDirectory.Reports, _calcParameters.InternalStepsize);
        }

        public void LogValues(int timestamp, [NotNull] HouseholdKey key)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("Time",timestamp);
            var allVariables = _variableRepository.GetAllVariables().Where(x=> x.HouseholdKey == key).ToList();
            foreach (var variable in allVariables) {
                values.Add(variable.SqlName,variable.Value);
            }
            _sqlResultLoggingService.SaveDictionaryToDatabaseNewConnection(values, TableName,key);
        }
        /*
        public void WriteLine(int time, List<CalcLocation> locations) {
            foreach (var location in locations) {
                foreach (var pair in location.Variables) {
                    var trig = location.Name + " - " + pair.Key;
                    if (!_columns.ContainsKey(trig)) {
                        _columns.Add(trig, _columns.Count);
                    }
                }
            }

            if (!_headerWritten) {
                var s = dsc.GenerateDateStampHeader();
                foreach (var pair in _columns) {
                    s += pair.Key + _calcParameters.CSVCharacter;
                }
                _sw.WriteLine(s);
                _headerWritten = true;
            }
            if (time < _calcParameters.DummyCalcSteps && !_showNegativeTime) {
                return;
            }
            var values = new string[_columns.Count];
            foreach (var location in locations) {
                foreach (var d in location.Variables) {
                    var trig = location.Name + " - " + d.Key;
                    var columnnumber = _columns[trig];
                    values[columnnumber] = d.Value.ToString(Config.CultureInfo);
                }
            }
            var sb = new StringBuilder();
            dsc.GenerateDateStampForTimestep(time, sb, true);

            foreach (var s in values) {
                sb.Append(s);
                sb.Append(_calcParameters.CSVCharacter);
            }
            _sw.WriteLine(sb.ToString());
        }
    }
}*/