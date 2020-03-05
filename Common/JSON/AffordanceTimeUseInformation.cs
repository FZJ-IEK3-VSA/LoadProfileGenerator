using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.JSON {
        public class AffordanceInformation {
            //TODO: remove  this
            public AffordanceInformation([NotNull] string affordanceName, double timeUsedInMinutes)
            {
                AffordanceName = affordanceName;
                TimeUsedInMinutes = timeUsedInMinutes;
            }

            [NotNull]
            [UsedImplicitly]
            public string AffordanceName { get; set; }
            [NotNull]
            public Dictionary<string, string> AffordanceTags { get; } = new Dictionary<string, string>();
            [UsedImplicitly]
            public double TimeUsedInMinutes { get; set; }
        }

        public class PersonAffordanceInformation : IHouseholdKey {
            //TODO: remove  this
            public PersonAffordanceInformation([NotNull] string personName, [NotNull] HouseholdKey householdKey)
            {
                PersonName = personName;
                HouseholdKey = householdKey;
            }

            [NotNull]
            [ItemNotNull]
            public List<AffordanceInformation> Affordances { get; } = new List<AffordanceInformation>();

            [UsedImplicitly]
            [NotNull]
            public string PersonName { get; set; }

            public HouseholdKey HouseholdKey { get; set; }
        }
}