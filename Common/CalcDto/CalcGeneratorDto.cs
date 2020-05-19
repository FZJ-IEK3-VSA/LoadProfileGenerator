using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcGeneratorDto {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public CalcLoadTypeDto LoadType { get; }
        [NotNull]
        public List<double> Values { get; }
        public StrGuid Guid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }

        public CalcGeneratorDto([NotNull]string name, int id, [NotNull]CalcLoadTypeDto loadType,
                                [NotNull]  List<double> values, [NotNull] HouseholdKey householdKey, StrGuid guid)
        {
            Name = name;
            ID = id;
            LoadType = loadType;
            Values = values;
            Guid = guid;
            HouseholdKey = householdKey;
        }
    }
}