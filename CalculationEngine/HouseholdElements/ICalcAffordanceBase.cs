using System.Collections.Generic;
using Automation;
using CalculationEngine.Activities;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    public enum BusynessType
    {
        NotBusy,
        Occupied,
        NoTransportation,
        VariableRequirementsNotMet,
        BeyondTimeLimit,
        NoRoute
    }
    public interface ICalcAffordanceBase
    {
        string Name { get; }
        string AffCategory { get; }
        ActionAfterInterruption AfterInterruption { get; }
        CalcAffordanceType CalcAffordanceType { get; }
        bool IsInterruptable { get; }
        bool IsInterrupting { get; }
        int MaximumAge { get; }
        int MiniumAge { get; }
        string PrettyNameForDumping { get; }
        bool NeedsLight { get; }
        CalcLocation ParentLocation { get; }
        PermittedGender PermittedGender { get; }
        bool RandomEffect { get; }
        bool RequireAllAffordances { get; }
        List<CalcDesire> Satisfactionvalues { get; }
        int Weight { get; }
        StrGuid Guid { get; }

        IEnumerable<IActivity> PlanActivation(TimeStep startTime, CalcPersonDto activator, ICalcSite? personSourceSite);

        void StartActivation(TimeStep startTime, string activatorName, ICalcSite? personSourceSite);

        void FinishActivation(TimeStep endTime, string activatorName);

        // TODO: delete this method, or reuse it for something?
        void Activate(TimeStep startTime, string activatorName, ICalcSite? personSourceSite, out IActivity personTimeProfile);

        BusynessType IsBusy(TimeStep time, ICalcSite? srcSite, CalcPersonDto calcPerson, bool clearDictionaries = true);

        IEnumerable<ICalcAffordanceBase> CollectSubAffordances(TimeStep time, bool onlyInterrupting, ICalcSite? srcSite);

        CalcSubAffordance GetAsSubAffordance();

        List<ICalcAffordanceBase> SubAffordances { get; }

        List<DeviceEnergyProfileTuple> Energyprofiles { get; }

        ColorRGB AffordanceColor { get; }
        
        string SourceTrait { get; }
        
        string? TimeLimitName { get; }
        
        bool AreThereDuplicateEnergyProfiles();
        
        string? AreDeviceProfilesEmpty();

        CalcSite? Site { get; }

        BodilyActivityLevel BodilyActivityLevel { get; }
    }
}