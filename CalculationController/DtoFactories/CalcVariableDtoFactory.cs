using System;
using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using Common.CalcDto;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcVariableDtoFactory {
        [NotNull]
        public Dictionary<string, CalcVariableDto> VariableDtos { get; } = new Dictionary<string, CalcVariableDto>();

    public bool IsKeyRegistered([NotNull] string key)
        {
            if (VariableDtos.ContainsKey(key))
            {
                return true;
            }

            return false;
        }

        [NotNull]
        public static string MakeKey([NotNull] Variable variable, [NotNull] Location location, [NotNull] HouseholdKey key)
        {
            return MakeKey(variable, location.PrettyName, key);
        }

        [NotNull]
        public static string MakeKey([NotNull] Variable variable, [NotNull] string locationName, [NotNull] HouseholdKey key)
        {
            return locationName + "#" + variable.PrettyName + "#" + key.Key;
        }

        [NotNull]
        public CalcVariableDto RegisterVariable([NotNull] Variable variable, [NotNull] string locationName, [NotNull] string locationGuid,
                                                [NotNull] HouseholdKey householdKey)
        {
            string variableGuid = Guid.NewGuid().ToString();
            CalcVariableDto cvd = new CalcVariableDto(variable.PrettyName, variableGuid, 0,
                locationName, locationGuid, householdKey);
            string key = MakeKey(variable, locationName, householdKey);
            VariableDtos.Add(key, cvd);
            return cvd;
        }

        [NotNull]
        public CalcVariableDto GetVariableByKey([NotNull] string key)
        {
            return VariableDtos[key];
        }
        /*
        private CalcVariableRepository _repository;

        [NotNull]
        public CalcVariableRepository GetRepository()
        {
            if (_repository != null) {
                return _repository;
            }

            _repository = new CalcVariableRepository();
            foreach (var variable in VariableDtos.Values) {
                _repository.RegisterVariable(new CalcVariable(
                    variable.Name,variable.Guid,variable.Value,variable.LocationName,variable.LocationGuid,variable.HouseholdKey));
            }

            return _repository;
        }*/
        [NotNull]
        public CalcVariableDto RegisterVariableIfNotRegistered([NotNull] Variable variable, [NotNull] string locationName,
                                                               [NotNull] string locationGuid,
                                                               [NotNull] HouseholdKey householdKey)
        {
            string key = MakeKey(variable, locationName, householdKey);
            if (!IsKeyRegistered(key))
            {
                return RegisterVariable(variable, locationName, locationGuid, householdKey);
            }

            return GetVariableByKey(key);
        }
        [NotNull]
        public CalcVariableDto RegisterVariableIfNotRegistered([NotNull] Variable variable, [NotNull] Location location,
                                                               [NotNull] HouseholdKey householdKey,
            [NotNull] LocationDtoDict locationDict)
        {
            string locGuid = locationDict.LocationDict[location].Guid;
            return RegisterVariableIfNotRegistered(variable, location.PrettyName, locGuid,
                householdKey);
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcVariableDto> GetAllVariableDtos()
        {
            int distinctCount = VariableDtos.Select(x => x.Value.Guid).Distinct().Count();
            int totalCount = VariableDtos.Count;
            if (distinctCount != totalCount) {
                throw new LPGException("duplicate variable registrations");
            }
            return VariableDtos.Values.ToList();
        }
    }
}
