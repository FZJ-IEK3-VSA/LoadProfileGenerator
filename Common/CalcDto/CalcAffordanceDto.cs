using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcAffordanceDto:IHouseholdKey {
        [NotNull]
        public AvailabilityDataReferenceDto IsBusyArray { get;  }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public CalcProfileDto PersonProfile { get; }
        [NotNull]
        public string CalcLocationName { get; }
        [NotNull]
        public StrGuid CalcLocationGuid { get; }

        public bool RandomEffect { get; }
        [NotNull]
        [ItemNotNull]
        public List<CalcDesireDto> Satisfactionvalues { get; }

        public int MiniumAge { get; }
        public int MaximumAge { get; }
        public PermittedGender PermittedGender { get; }
        public bool NeedsLight { get; }
        public double TimeStandardDeviation { get; }
        public byte ColorR { get; }
        public byte ColorG { get; }
        public byte ColorB { get; }
        [NotNull]
        public string AffCategory { get; }
        public bool IsInterruptable { get; }
        public bool IsInterrupting { get; }
        [NotNull][ItemNotNull]
        public List<CalcAffordanceVariableOpDto> VariableOps { get; }
        [NotNull]
        [ItemNotNull]
        public List<VariableRequirementDto> VariableRequirements { get; }

        public ActionAfterInterruption ActionAfterInterruption { get; }
        [NotNull]
        public string TimeLimitName { get; }
        public int Weight { get; }
        public bool RequireAllDesires { get; }
        [NotNull]
        public string SrcTrait { get; }
        [NotNull]
        public StrGuid Guid { get; }
        [ItemNotNull][NotNull]
        public List<CalcSubAffordanceDto> SubAffordance { get; } = new List<CalcSubAffordanceDto>();

        public CalcAffordanceDto([NotNull]string name, int id, [NotNull]CalcProfileDto personProfile, [NotNull]string  calcLocationName,
                                 [NotNull] StrGuid calcLocationGuid, bool randomEffect,
                                 [ItemNotNull] [NotNull]List<CalcDesireDto> satisfactionvalues, int miniumAge, int maximumAge,
                                 PermittedGender permittedGender,
                                 bool needsLight, double timeStandardDeviation, byte colorR, byte colorG, byte colorB, [NotNull] string affCategory,
                                 bool isInterruptable, bool isInterrupting, [ItemNotNull][NotNull]List<CalcAffordanceVariableOpDto> variableOps,
                                 [ItemNotNull] [NotNull]List<VariableRequirementDto> variableRequirements,
                                 ActionAfterInterruption actionAfterInterruption, [NotNull] string timeLimitName, int weight,
                                 bool requireAllDesires, [NotNull] string srcTrait,
                                 [NotNull] StrGuid guid, [NotNull] AvailabilityDataReferenceDto isBusyArray,
                                 [NotNull] HouseholdKey householdKey, BodilyActivityLevel bodilyActivityLevel)
        {
            Name = name;
            ID = id;
            PersonProfile = personProfile;
            CalcLocationName = calcLocationName;
            CalcLocationGuid = calcLocationGuid;
            RandomEffect = randomEffect;
            Satisfactionvalues = satisfactionvalues;
            MiniumAge = miniumAge;
            MaximumAge = maximumAge;
            PermittedGender = permittedGender;
            NeedsLight = needsLight;
            TimeStandardDeviation = timeStandardDeviation;
            ColorR = colorR;
            ColorG = colorG;
            ColorB = colorB;
            AffCategory = affCategory;
            IsInterruptable = isInterruptable;
            IsInterrupting = isInterrupting;
            VariableOps = variableOps;
            VariableRequirements = variableRequirements;
            ActionAfterInterruption = actionAfterInterruption;
            TimeLimitName = timeLimitName;
            Weight = weight;
            RequireAllDesires = requireAllDesires;
            SrcTrait = srcTrait;
            Guid = guid;
            IsBusyArray = isBusyArray;
            HouseholdKey = householdKey;
            BodilyActivityLevel = bodilyActivityLevel;
        }
        [ItemNotNull]
        [NotNull]
        public List<DeviceEnergyProfileTupleDto> Energyprofiles { get; } = new List<DeviceEnergyProfileTupleDto>();
        public void AddDeviceTuple([NotNull]string calcDeviceName, [NotNull] StrGuid calcDeviceGuid, [NotNull]
                                   CalcProfileDto newprof, [NotNull]string calcLoadTypeName, [NotNull] StrGuid calcLoadtypeGuid, decimal timeoffset,
                                   TimeSpan internalstepsize, double multiplier, double probability)
        {
            var calctup = new DeviceEnergyProfileTupleDto(calcDeviceName, calcDeviceGuid, newprof, calcLoadTypeName, calcLoadtypeGuid, timeoffset,
                internalstepsize, multiplier, probability);
            Energyprofiles.Add(calctup);
        }

        public HouseholdKey HouseholdKey { get; }
        public BodilyActivityLevel BodilyActivityLevel { get; }
    }
}