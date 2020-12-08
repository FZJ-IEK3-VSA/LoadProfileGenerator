using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;

namespace Common.CalcDto {
    public class CalcGeneratorDto {
        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public int ID { get; }
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto LoadType { get; }
        [JetBrains.Annotations.NotNull]
        public List<double> Values { get; }
        public StrGuid Guid { get; }
        [JetBrains.Annotations.NotNull]
        public HouseholdKey HouseholdKey { get; }

        public CalcGeneratorDto([JetBrains.Annotations.NotNull]string name, int id, [JetBrains.Annotations.NotNull]CalcLoadTypeDto loadType,
                                [JetBrains.Annotations.NotNull]  List<double> values, [JetBrains.Annotations.NotNull] HouseholdKey householdKey, StrGuid guid)
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