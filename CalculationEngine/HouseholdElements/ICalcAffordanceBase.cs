using System.Collections.Generic;
using Automation;
using CalculationEngine.Transportation;
using Common;
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
        [JetBrains.Annotations.NotNull]
        string Name { get; }
        [JetBrains.Annotations.NotNull]
        string AffCategory { get; }
        ActionAfterInterruption AfterInterruption { get; }
        int CalcAffordanceSerial { get; }
        CalcAffordanceType CalcAffordanceType { get; }
        //BitArray IsBusyArray { get; set; }
        bool IsInterruptable { get; }
        bool IsInterrupting { get; }
        int MaximumAge { get; }
        int MiniumAge { get; }
        [JetBrains.Annotations.NotNull]
        string PrettyNameForDumping { get; }
        bool NeedsLight { get; }
        [JetBrains.Annotations.NotNull]
        CalcLocation ParentLocation { get; }
        PermittedGender PermittedGender { get; }
        bool RandomEffect { get; }
        bool RequireAllAffordances { get; }
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        List<CalcDesire> Satisfactionvalues { get; }
        int Weight { get; }
        StrGuid Guid { get; }

        void Activate([JetBrains.Annotations.NotNull] TimeStep startTime, [JetBrains.Annotations.NotNull] string activatorName,
             [JetBrains.Annotations.NotNull] CalcLocation personSourceLocation,
            [JetBrains.Annotations.NotNull] out ICalcProfile personTimeProfile);
        //ICalcProfile CollectPersonProfile();
        int DefaultPersonProfileLength { get; }
        BusynessType IsBusy([JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull] CalcLocation srcLocation, [JetBrains.Annotations.NotNull] string calcPersonName, bool clearDictionaries = true);

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        List<CalcSubAffordance> CollectSubAffordances([JetBrains.Annotations.NotNull] TimeStep time,  bool onlyInterrupting,
            [JetBrains.Annotations.NotNull] CalcLocation srcLocation);

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        List<CalcSubAffordance> SubAffordances { get; }

        [CanBeNull]
        [ItemNotNull]
        List<CalcAffordance.DeviceEnergyProfileTuple> Energyprofiles { get; }
         ColorRGB AffordanceColor { get; }
        [JetBrains.Annotations.NotNull]
        string SourceTrait { get; }
        string? TimeLimitName { get; }
        bool AreThereDuplicateEnergyProfiles();
        string? AreDeviceProfilesEmpty();

        CalcSite? Site { get; }

        BodilyActivityLevel BodilyActivityLevel { get; }
    }
}