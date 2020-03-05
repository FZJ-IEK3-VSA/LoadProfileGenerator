using System.Collections.Generic;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public interface ICalcAbleObjectDto {
        [NotNull]
        HouseholdKey HouseholdKey { get; }

        [NotNull]
        string Name { get; }

        [NotNull]
        Dictionary<int, CalcProfileDto> CollectAllProfiles();

        [ItemNotNull]
        [NotNull]
        List<CalcAutoDevDto> CollectAutoDevs();

        [ItemNotNull]
        [NotNull]
        List<CalcDeviceDto> CollectDevices();

        [ItemNotNull]
        [NotNull]
        List<CalcLocationDto> CollectLocations();

        [ItemNotNull]
        [NotNull]
        List<CalcPersonDto> CollectPersons();
    }
}