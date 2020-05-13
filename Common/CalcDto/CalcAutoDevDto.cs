using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcAutoDevDto : CalcDeviceDto
    {
        [NotNull]
        public CalcProfileDto CalcProfile { get; }
        [NotNull]
        public string LoadTypeName { get; }
        [NotNull]
        public StrGuid LoadtypeGuid { get; }
        public double TimeStandardDeviation { get; }
        public double Multiplier { get; }
        [NotNull]
        public string CalclocationName { get; }
        [NotNull]
        public StrGuid CalcLocationGuid { get; }
        /*[CanBeNull]
        public string VariableName { get; }
        public string VariableGuid { get; }
        public double VariableValue { get; }
        public VariableCondition VariableCondition { get; }*/
        [NotNull]
        public string DeviceCategoryFullPath { get; }
        [NotNull]
        public AvailabilityDataReferenceDto BusyArr { get; }
        [ItemNotNull]
        [NotNull]
        public List<VariableRequirementDto> Requirements { get; }
        public CalcAutoDevDto([NotNull]string name, [NotNull]CalcProfileDto calcProfile, [NotNull] string loadTypeName, [NotNull]StrGuid loadtypeGuid,
                              [ItemNotNull] [NotNull] List<CalcDeviceLoadDto> loads, double timeStandardDeviation,
                              [NotNull] StrGuid deviceCategoryGuid,
                              [NotNull] HouseholdKey householdKey, double multiplier,
                              [NotNull] string calclocationName, [NotNull] StrGuid calcLocationGuid,
                             //[CanBeNull] string variableName, double variableValue, VariableCondition variableCondition,
                              [NotNull] string deviceCategoryFullPath, [NotNull] StrGuid guid, [NotNull]AvailabilityDataReferenceDto busyArr,
                              //string variableGuid,
                              [ItemNotNull] [NotNull]List<VariableRequirementDto> requirements,
                             [NotNull] string deviceCategoryName):
            base(name,deviceCategoryGuid,householdKey,OefcDeviceType.AutonomousDevice,
                deviceCategoryName,"",guid,calcLocationGuid,calclocationName)
        {
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