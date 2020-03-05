using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using Common;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class VacationAffordance : CalcAffordanceBase {
        //TODO: figure out if this is still needed
        public VacationAffordance([NotNull] CalcParameters calcParameters, [NotNull] string guid,
                                  [ItemNotNull] [NotNull] BitArray isBusy)
            : base(
                Constants.TakingAVacationString, new CalcLocation("Vacation", System.Guid.NewGuid().ToString()),
                new List<CalcDesire>(), 0, 99, PermittedGender.All, false,
                false, "Vacation", false, false, ActionAfterInterruption.GoBackToOld, 0, false,
                CalcAffordanceType.Vacation, calcParameters, guid, isBusy)
        {
        }

        public override Color AffordanceColor { get; } = Colors.Green;

        public override int DefaultPersonProfileLength => 0;

        [NotNull]
        [ItemNotNull]
        public override List<CalcAffordance.DeviceEnergyProfileTuple> Energyprofiles { get; } =
            new List<CalcAffordance.DeviceEnergyProfileTuple>();

        [NotNull]
        public override string SourceTrait { get; } = "Vacation";

        [NotNull]
        [ItemNotNull]
        public override List<CalcSubAffordance> SubAffordances { get; } = new List<CalcSubAffordance>();

        [NotNull]
        public override string TimeLimitName { get; } = "None";

        //private static VacationAffordance _vacationAffordance;
        /* public VacationAffordance Get()
        {
            if (_vacationAffordance == null) {
                _vacationAffordance = new VacationAffordance(_calcParameters);
            }
            return _vacationAffordance;
        }*/

        public override void Activate([NotNull] TimeStep startTime, [NotNull] string activatorName,
                                      [NotNull] CalcLocation personSourceLocation,
                                      [NotNull] out ICalcProfile personTimeProfile) =>
            throw new LPGException("This function should never be called");

        [NotNull]
        public override string AreDeviceProfilesEmpty() => throw new NotImplementedException();

        public override bool AreThereDuplicateEnergyProfiles() => false;

        [NotNull]
        [ItemNotNull]
        public override List<CalcSubAffordance> CollectSubAffordances([NotNull] TimeStep time, [NotNull] NormalRandom nr,
                                                                      bool onlyInterrupting, [NotNull] Random r,
                                                                      [NotNull] CalcLocation srcLocation) =>
            new List<CalcSubAffordance>();

        //public override ICalcProfile CollectPersonProfile() => throw new LPGException("This function should never be called");

        public override bool IsBusy([NotNull] TimeStep time, [NotNull] NormalRandom nr, [NotNull] Random r,
                                    [NotNull] CalcLocation srcLocation, [NotNull] string calcPersonName,
                                    bool clearDictionaries = true) =>
            throw new LPGException("This function should never be called");
    }
}