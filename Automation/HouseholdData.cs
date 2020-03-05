using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Automation {
    public class TransportationDistanceModifier
    {
        public TransportationDistanceModifier([NotNull] string routeKey, [NotNull] string stepKey, double newDistanceInMeters)
        {
            RouteKey = routeKey;
            StepKey = stepKey;
            NewDistanceInMeters = newDistanceInMeters;
        }

        [Obsolete("For Json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public TransportationDistanceModifier()
        {
        }

        [NotNull]
        public string RouteKey { get; set; }
        [NotNull]
        public string StepKey { get; set; }
        public double NewDistanceInMeters { get; set; }
    }

    public enum HouseholdDataSpecifictionType {
        ByPersons,
        ByTemplateName,
        ByHouseholdName
    }

    public class HouseholdDataPersonSpecification {
        public HouseholdDataPersonSpecification([NotNull] List<PersonData> persons)
        {
            Persons = persons;
        }

        [Obsolete("Only for json")]
        public HouseholdDataPersonSpecification()
        {
        }

        [NotNull]
        [ItemNotNull]
        public List<PersonData> Persons { get; set; } = new List<PersonData>();
    }

    public class HouseholdTemplateSpecification
    {
        public HouseholdTemplateSpecification([NotNull] string householdTemplateName) => HouseholdTemplateName = householdTemplateName;

        [Obsolete("Only for Json")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseholdTemplateSpecification()
        {
        }

        [NotNull]
        public string HouseholdTemplateName { get; set; }
    }

    public class HouseholdNameSpecification {
        public HouseholdNameSpecification([NotNull] string householdName) => HouseholdName = householdName;

        [Obsolete("only for json")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseholdNameSpecification()
        {
        }

        [NotNull]
        public string HouseholdName { get; set; }
    }
    public class HouseholdData {
        public HouseholdData([NotNull] string householdGuid, double targetEnergyUse,
                             ElectricCarUse useElectricCar, [NotNull] string name, [CanBeNull] JsonReference chargingStationSet,
                             [CanBeNull] JsonReference transportationDeviceSet, [CanBeNull] JsonReference travelRouteSet,
                             [ItemNotNull][CanBeNull] List<TransportationDistanceModifier> transportationDistanceModifiers,
                             HouseholdDataSpecifictionType householdDataSpecifictionType)
        {
            HouseholdGuid = householdGuid;
            TargetEnergyUse = targetEnergyUse;
            UseElectricCar = useElectricCar;
            Name = name;
            ChargingStationSet = chargingStationSet;
            TransportationDeviceSet = transportationDeviceSet;
            TravelRouteSet = travelRouteSet;
            TransportationDistanceModifiers = transportationDistanceModifiers;
            HouseholdDataSpecifictionType = householdDataSpecifictionType;
        }

        [Obsolete("For json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseholdData()
        {
        }

        [CanBeNull]
    public  HouseholdDataPersonSpecification HouseholdDataPersonSpecification { get; set; }
    [CanBeNull]
        public HouseholdTemplateSpecification HouseholdTemplateSpecification { get; set; }
        [CanBeNull]
        public HouseholdNameSpecification HouseholdNameSpecification { get; set; }

                [NotNull]
        public string HouseholdGuid { get; set; }

        [NotNull]
        public string Name { get; set; }


        public double TargetEnergyUse { get; set; }
        public ElectricCarUse UseElectricCar { get; set; }
        [CanBeNull]
        public JsonReference ChargingStationSet { get; set; }
        [CanBeNull]
        public JsonReference TransportationDeviceSet { get; set; }
        [CanBeNull]
        public JsonReference TravelRouteSet { get; set; }

        [CanBeNull]
        [ItemNotNull]
        public List<TransportationDistanceModifier> TransportationDistanceModifiers { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public HouseholdDataSpecifictionType HouseholdDataSpecifictionType { get; set; }

        public bool IsHouseholdProfileCalculated { get; set; }
        public bool IsCarProfileCalculated { get; set; }
    }
}