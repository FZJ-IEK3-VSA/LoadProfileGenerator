using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.JSON;
using JetBrains.Annotations;

namespace Common.CalcDto {

    public class CalcAutoDevProfileDto {
        public CalcAutoDevProfileDto(CalcProfileDto profile, [NotNull] string loadTypeName, StrGuid loadtypeGuid, double multiplier)
        {
            Profile = profile;
            LoadTypeName = loadTypeName;
            LoadtypeGuid = loadtypeGuid;
            Multiplier = multiplier;
        }
        [NotNull]
        public string LoadTypeName { get; }
        public StrGuid LoadtypeGuid { get; }
        public double Multiplier { get; }
        public CalcProfileDto Profile { get; set; }
    }
    public record CalcAutoDevDto : CalcDeviceDto
    {
        [NotNull]
        public List<CalcAutoDevProfileDto> CalcProfiles { get; }
        public double TimeStandardDeviation { get; }
        [NotNull]
        public string CalclocationName { get; }
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

        [NotNull]
        public override string ToString() => CalclocationName + " - " + Name;

        [ItemNotNull]
        [NotNull]
        public List<VariableRequirementDto> Requirements { get; }
        public CalcAutoDevDto([NotNull]string name, [NotNull] List<CalcAutoDevProfileDto> profiles,
                              [ItemNotNull] [NotNull] List<CalcDeviceLoadDto> loads, double timeStandardDeviation,
                              StrGuid deviceCategoryGuid,
                              [NotNull] HouseholdKey householdKey,
                              [NotNull] string calclocationName, StrGuid calcLocationGuid,
                             //[CanBeNull] string variableName, double variableValue, VariableCondition variableCondition,
                              [NotNull] string deviceCategoryFullPath, StrGuid deviceClassGuid, [NotNull]AvailabilityDataReferenceDto busyArr,
                              //string variableGuid,
                              [ItemNotNull] [NotNull]List<VariableRequirementDto> requirements,
                             [NotNull] string deviceCategoryName, FlexibilityType flexibilityType, int maxTimeShiftInMinutes) :
            base(name,deviceCategoryGuid,householdKey,OefcDeviceType.AutonomousDevice,
                deviceCategoryName,"",deviceClassGuid,calcLocationGuid,calclocationName,flexibilityType, maxTimeShiftInMinutes)
        {
            CalcProfiles = profiles;
            Loads = loads;
            TimeStandardDeviation = timeStandardDeviation;
            DeviceCategoryGuid = deviceCategoryGuid;
            HouseholdKey = householdKey;
            CalclocationName = calclocationName;
            CalcLocationGuid = calcLocationGuid;
            //VariableName = variableName;
            //VariableValue = variableValue;
            //VariableCondition = variableCondition;
            DeviceCategoryFullPath = deviceCategoryFullPath;
            BusyArr = busyArr; //busyArr;
            //VariableGuid = variableGuid;
            Requirements = requirements;
        }
    }
}