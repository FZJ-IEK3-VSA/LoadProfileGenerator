using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Automation {
    public class TransportationDistanceModifier
    {
        public TransportationDistanceModifier([JetBrains.Annotations.NotNull] string routeKey, [JetBrains.Annotations.NotNull] string stepKey, double newDistanceInMeters)
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

        [JetBrains.Annotations.NotNull]
        public string? RouteKey { get; set; }
        [JetBrains.Annotations.NotNull]
        public string? StepKey { get; set; }
        public double NewDistanceInMeters { get; set; }
    }

    public enum HouseholdDataSpecificationType {
        ByPersons,
        ByTemplateName,
        ByHouseholdName
    }

    public class HouseholdDataPersonSpecification {
        public HouseholdDataPersonSpecification([JetBrains.Annotations.NotNull] List<PersonData> persons)
        {
            Persons = persons;
        }

        [Obsolete("Only for json")]
        public HouseholdDataPersonSpecification()
        {
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<PersonData> Persons { get; set; } = new List<PersonData>();

        [ItemNotNull]
        public List<string>? HouseholdTags { get; set; } = new List<string>();
    }

    public class PersonLivingTag {
        public string? LivingPatternTag { get; set; }
        public string? PersonName { get; set; }
    }

    public class HouseholdTemplateSpecification
    {
        public HouseholdTemplateSpecification([JetBrains.Annotations.NotNull] string householdTemplateName) => HouseholdTemplateName = householdTemplateName;

        [Obsolete("Only for Json")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseholdTemplateSpecification()
        {
        }
        public List<PersonLivingTag>? Persons { get; set; } = new List<PersonLivingTag>();
        [JetBrains.Annotations.NotNull]
        public string? HouseholdTemplateName { get; set; }

        public List<string>? ForbiddenTraitTags { get; set; } = new List<string>();
    }

    public class HouseholdNameSpecification {
        public HouseholdNameSpecification([JetBrains.Annotations.NotNull] JsonReference householdName) => HouseholdReference = householdName;

        [Obsolete("only for json")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseholdNameSpecification()
        {
        }

        public JsonReference? HouseholdReference { get; set; }
    }
    public class HouseholdData {
        public HouseholdData([JetBrains.Annotations.NotNull] string uniqueHouseholdId,
                             [JetBrains.Annotations.NotNull] string name, [CanBeNull] JsonReference? chargingStationSet,
                             [CanBeNull] JsonReference? transportationDeviceSet, [CanBeNull] JsonReference? travelRouteSet,
                             [ItemNotNull][CanBeNull] List<TransportationDistanceModifier>? transportationDistanceModifiers,
                             HouseholdDataSpecificationType householdDataSpecifictionType)
        {
            UniqueHouseholdId = uniqueHouseholdId;
            Name = name;
            ChargingStationSet = chargingStationSet;
            TransportationDeviceSet = transportationDeviceSet;
            TravelRouteSet = travelRouteSet;
            TransportationDistanceModifiers = transportationDistanceModifiers;
            HouseholdDataSpecification = householdDataSpecifictionType;
        }

        [Obsolete("For json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
#pragma warning disable 8618
        public HouseholdData()
#pragma warning restore 8618
        {
        }

        [CanBeNull]
        public  HouseholdDataPersonSpecification? HouseholdDataPersonSpec { get; set; }
        [CanBeNull]
        public HouseholdTemplateSpecification? HouseholdTemplateSpec { get; set; }
        [CanBeNull]
        public HouseholdNameSpecification? HouseholdNameSpec { get; set; }

        [JetBrains.Annotations.NotNull]
        public string UniqueHouseholdId { get; set; }

        [JetBrains.Annotations.NotNull]
        public string? Name { get; set; }


        [CanBeNull]
        public JsonReference? ChargingStationSet { get; set; }
        [CanBeNull]
        public JsonReference? TransportationDeviceSet { get; set; }
        [CanBeNull]
        public JsonReference? TravelRouteSet { get; set; }

        [CanBeNull]
        [ItemNotNull]
        public List<TransportationDistanceModifier>? TransportationDistanceModifiers { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public HouseholdDataSpecificationType HouseholdDataSpecification { get; set; }
    }
}