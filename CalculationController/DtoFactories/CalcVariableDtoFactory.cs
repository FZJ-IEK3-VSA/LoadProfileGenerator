using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using Common;
using Common.CalcDto;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcVariableDtoFactory {
        [JetBrains.Annotations.NotNull]
        public Dictionary<string, CalcVariableDto> VariableDtos { get; } = new Dictionary<string, CalcVariableDto>();

    public bool IsKeyRegistered([JetBrains.Annotations.NotNull] string key)
        {
            if (VariableDtos.ContainsKey(key))
            {
                return true;
            }

            return false;
        }

        [JetBrains.Annotations.NotNull]
        public static string MakeKey([JetBrains.Annotations.NotNull] Variable variable, [JetBrains.Annotations.NotNull] Location location, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            return MakeKey(variable, location.PrettyName, key);
        }

        [JetBrains.Annotations.NotNull]
        public static string MakeKey([JetBrains.Annotations.NotNull] Variable variable, [JetBrains.Annotations.NotNull] string locationName, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            return locationName + "#" + variable.PrettyName + "#" + key.Key;
        }

        [JetBrains.Annotations.NotNull]
        public CalcVariableDto RegisterVariable([JetBrains.Annotations.NotNull] Variable variable, [JetBrains.Annotations.NotNull] string locationName, StrGuid locationGuid,
                                                [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            StrGuid variableGuid = Guid.NewGuid().ToStrGuid();
            CalcVariableDto cvd = new CalcVariableDto(variable.PrettyName, variableGuid, 0,
                locationName, locationGuid, householdKey);
            string key = MakeKey(variable, locationName, householdKey);
            VariableDtos.Add(key, cvd);
            return cvd;
        }

        [JetBrains.Annotations.NotNull]
        public CalcVariableDto GetVariableByKey([JetBrains.Annotations.NotNull] string key)
        {
            return VariableDtos[key];
        }
        /*
        private CalcVariableRepository _repository;

        [JetBrains.Annotations.NotNull]
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
        [JetBrains.Annotations.NotNull]
        public CalcVariableDto RegisterVariableIfNotRegistered([JetBrains.Annotations.NotNull] Variable variable, [JetBrains.Annotations.NotNull] string locationName,
                                                               StrGuid locationGuid,
                                                               [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            string key = MakeKey(variable, locationName, householdKey);
            if (!IsKeyRegistered(key))
            {
                return RegisterVariable(variable, locationName, locationGuid, householdKey);
            }

            return GetVariableByKey(key);
        }
        [JetBrains.Annotations.NotNull]
        public CalcVariableDto RegisterVariableIfNotRegistered([JetBrains.Annotations.NotNull] Variable variable, [JetBrains.Annotations.NotNull] Location location,
                                                               [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
            [JetBrains.Annotations.NotNull] LocationDtoDict locationDict)
        {
            StrGuid locGuid = locationDict.LocationDict[location].Guid;
            return RegisterVariableIfNotRegistered(variable, location.PrettyName, locGuid,
                householdKey);
        }

        [JetBrains.Annotations.NotNull]
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
