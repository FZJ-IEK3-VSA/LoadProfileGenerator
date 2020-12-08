using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Enums {
    public static class CalcObjectTypeHelper {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [JetBrains.Annotations.NotNull]
        public static  Dictionary<CalcObjectType, string> CalcObjectTypeEnumDictionary { get; } =
            new Dictionary<CalcObjectType, string> {
                {CalcObjectType.ModularHousehold, "Modular Household"},
                {CalcObjectType.House, "House"},
                //{CalcObjectType.Settlement, "Settlement"}
            };

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [JetBrains.Annotations.NotNull]
        public static Dictionary<CalcObjectType, string> CalcObjectTypeHouseholdDictionary { get; } =
            new Dictionary<CalcObjectType, string> {
                {CalcObjectType.ModularHousehold, "Modular Household"}
            };

        public static CalcObjectType GetFromString([JetBrains.Annotations.NotNull] string val) {
            foreach (var keyValuePair in CalcObjectTypeEnumDictionary) {
                if (keyValuePair.Value == val) {
                    return keyValuePair.Key;
                }
            }
            return CalcObjectType.ModularHousehold;
        }
    }
}