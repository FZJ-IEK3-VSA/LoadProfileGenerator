using System.Collections.Generic;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.CalcDto {
    public sealed class CalcHouseDto {
        public CalcHouseDto([NotNull] string houseName, [ItemNotNull] [NotNull] List<CalcAutoDevDto> autoDevs, [CanBeNull] CalcAirConditioningDto calcAirConditioning,
                            [CanBeNull] CalcSpaceHeatingDto calcSpaceHeating, [NotNull] [ItemNotNull] List<CalcEnergyStorageDto> energyStorages,
                            [NotNull] [ItemNotNull] List<CalcGeneratorDto> generators, [NotNull] [ItemNotNull] List<CalcTransformationDeviceDto> transformationDevices,
                            [NotNull] [ItemNotNull] List<CalcHouseholdDto> households,
                            //[JetBrains.Annotations.NotNull]                            [ItemNotNull] List<CalcVariableDto> calcVariables,
                            [NotNull] HouseholdKey houseKey, [NotNull] [ItemNotNull] List<CalcLocationDto> houseLocations, [CanBeNull] string description)
        {
            HouseName = houseName;
            AutoDevs = autoDevs;
            AirConditioning = calcAirConditioning;
            SpaceHeating = calcSpaceHeating;
            EnergyStorages = energyStorages;
            Generators = generators;
            TransformationDevices = transformationDevices;
            Households = households;
            //CalcVariables = calcVariables;
            HouseKey = houseKey;
            HouseLocations = houseLocations;
            Description = description;
        }

        [CanBeNull]
        public CalcAirConditioningDto AirConditioning { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcAutoDevDto> AutoDevs { get; }

        [CanBeNull]
        [UsedImplicitly]
        [JsonProperty]
        public string Description { get; set; }

        [NotNull]
        [ItemNotNull]
        public List<CalcEnergyStorageDto> EnergyStorages { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcGeneratorDto> Generators { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcHouseholdDto> Households { get; }

        /*[JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcVariableDto> CalcVariables { get; }*/
        [NotNull]
        public HouseholdKey HouseKey { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcLocationDto> HouseLocations { get; }

        [NotNull]
        public string HouseName { get; }

        [CanBeNull]
        public CalcSpaceHeatingDto SpaceHeating { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcTransformationDeviceDto> TransformationDevices { get; }

        [ItemNotNull]
        [NotNull]
        public List<HouseholdKeyEntry> GetHouseholdKeyEntries()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var households = new List<HouseholdKeyEntry>();
            households.Add(new HouseholdKeyEntry(Constants.HouseKey, "House Infrastructure", HouseholdKeyType.House, Description ?? "", HouseName, Description));
            households.Add(new HouseholdKeyEntry(Constants.GeneralHouseholdKey, "General Information", HouseholdKeyType.General, Description ?? "", HouseName, Description));
            foreach (var calcAbleObject in Households) {
                households.Add(new HouseholdKeyEntry(calcAbleObject.HouseholdKey, calcAbleObject.Name, HouseholdKeyType.Household, calcAbleObject.Description, HouseName, Description));
            }
            return households;
        }
    }
}