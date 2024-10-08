using System.Collections.Generic;
using Automation;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    public enum BusynessType {
        NotBusy,
        Occupied,
        NoTransportation,
        VariableRequirementsNotMet,
        BeyondTimeLimit,
        NoRoute
    }
    public interface ICalcAffordanceBase
    {
        [NotNull]
        string Name { get; }
        [NotNull]
        string AffCategory { get; }
        ActionAfterInterruption AfterInterruption { get; }
        CalcAffordanceType CalcAffordanceType { get; }
        bool IsInterruptable { get; }
        bool IsInterrupting { get; }
        int MaximumAge { get; }
        int MiniumAge { get; }
        [NotNull]
        string PrettyNameForDumping { get; }
        bool NeedsLight { get; }
        [NotNull]
        CalcLocation ParentLocation { get; }
        PermittedGender PermittedGender { get; }
        bool RandomEffect { get; }
        bool RequireAllAffordances { get; }
        [NotNull]
        [ItemNotNull]
        List<CalcDesire> Satisfactionvalues { get; }
        int Weight { get; }
        StrGuid Guid { get; }

        void Activate([NotNull] TimeStep startTime, [NotNull] string activatorName,
                      [NotNull] CalcLocation personSourceLocation, [NotNull] out IAffordanceActivation personTimeProfile);
        
        BusynessType IsBusy([NotNull] TimeStep time, [NotNull] CalcLocation srcLocation, CalcPersonDto calcPerson, bool clearDictionaries = true);

        [NotNull]
        [ItemNotNull] // TODO: this method needs to return subaffs wrapped in transport decorators, if transport is enabled
        List<CalcSubAffordance> CollectSubAffordances([NotNull] TimeStep time,  bool onlyInterrupting,
            [NotNull] CalcLocation srcLocation);

        [NotNull]
        [ItemNotNull]
        List<CalcSubAffordance> SubAffordances { get; }

        [CanBeNull]
        [ItemNotNull]
        List<DeviceEnergyProfileTuple> Energyprofiles { get; }
         ColorRGB AffordanceColor { get; }
        [NotNull]
        string SourceTrait { get; }
        string? TimeLimitName { get; }
        bool AreThereDuplicateEnergyProfiles();
        string? AreDeviceProfilesEmpty();

        CalcSite? Site { get; }

        BodilyActivityLevel BodilyActivityLevel { get; }
    }
}