using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcAutoDevDto : ICalcDeviceDto, IHouseholdKey
    {
        [NotNull]
        public string Name { get; }
        [NotNull]
        public CalcProfileDto CalcProfile { get; }
        [NotNull]
        public string LoadTypeName { get; }
        [NotNull]
        public string LoadtypeGuid { get; }
        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceLoadDto> Loads { get; }
        public double TimeStandardDeviation { get; }
        [NotNull]
        public string DeviceCategoryGuid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        public double Multiplier { get; }
        [NotNull]
        public string CalclocationName { get; }
        [NotNull]
        public string CalcLocationGuid { get; }
        /*[CanBeNull]
        public string VariableName { get; }
        public string VariableGuid { get; }
        public double VariableValue { get; }
        public VariableCondition VariableCondition { get; }*/
        [NotNull]
        public string DeviceCategoryFullPath { get; }
        [NotNull]
        public string Guid { get; }
        [NotNull]
        public AvailabilityDataReferenceDto BusyArr { get; }
        [ItemNotNull]
        [NotNull]
        public List<VariableRequirementDto> Requirements { get; }
        public CalcAutoDevDto([NotNull]string name, [NotNull]CalcProfileDto calcProfile, [NotNull] string loadTypeName, [NotNull]string loadtypeGuid,
                              [ItemNotNull] [NotNull] List<CalcDeviceLoadDto> loads, double timeStandardDeviation, [NotNull] string deviceCategoryGuid,
                              [NotNull] HouseholdKey householdKey, double multiplier,
                              [NotNull] string calclocationName, [NotNull] string calcLocationGuid,
                             //[CanBeNull] string variableName, double variableValue, VariableCondition variableCondition,
                              [NotNull] string deviceCategoryFullPath, [NotNull] string guid, [NotNull]AvailabilityDataReferenceDto busyArr,
                              //string variableGuid,
                              [ItemNotNull] [NotNull]List<VariableRequirementDto> requirements)
        {
            Name = name;
            CalcProfile = calcProfile;
            LoadTypeName = loadTypeName;
            LoadtypeGuid = loadtypeGuid;
            Loads = loads;
            TimeStandardDeviation = timeStandardDeviation;
            DeviceCategoryGuid = deviceCategoryGuid;
            HouseholdKey = householdKey;
            Multiplier = multiplier;
            CalclocationName = calclocationName;
            CalcLocationGuid = calcLocationGuid;
            //VariableName = variableName;
            //VariableValue = variableValue;
            //VariableCondition = variableCondition;
            DeviceCategoryFullPath = deviceCategoryFullPath;
            Guid = guid;
            BusyArr = busyArr; //busyArr;
            //VariableGuid = variableGuid;
            Requirements = requirements;
        }
    }
}