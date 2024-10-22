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

        void Activate(TimeStep startTime, string activatorName, ICalcSite? personSourceSite, out IAffordanceActivation personTimeProfile);
        
        BusynessType IsBusy(TimeStep time, ICalcSite? srcSite, CalcPersonDto calcPerson, bool clearDictionaries = true);

        IEnumerable<ICalcAffordanceBase> CollectSubAffordances(TimeStep time,  bool onlyInterrupting, ICalcSite? srcSite);

        CalcSubAffordance GetAsSubAffordance();

        [NotNull]
        [ItemNotNull]
        List<ICalcAffordanceBase> SubAffordances { get; }

        [CanBeNull]
        [ItemNotNull]
        List<DeviceEnergyProfileTuple> Energyprofiles { get; }
         ColorRGB AffordanceColor { get; }
        [NotNull]
        string SourceTrait { get; }
        string? TimeLimitName { get; }
        bool AreThereDuplicateEnergyProfiles();
        string? AreDeviceProfilesEmpty();

        ICalcSite? Site { get; }

        BodilyActivityLevel BodilyActivityLevel { get; }
    }
}