using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common;
using Common.Enums;
using Database;
using Database.Tables.BasicHouseholds;

namespace CalculationController.Integrity {
    internal class PersonChecker : BasicChecker {
        public PersonChecker(bool performCleanupChecks) : base("Persons", performCleanupChecks) {
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override void Run(Simulator sim, CheckingOptions options) {
            var allnames = new Dictionary<string, string>();
            var errrorPersons = new List<Person>();
            foreach (var person in sim.Persons.Items) {
                if (PerformCleanupChecks) {
                    var usedIns = person.CalculateUsedIns(sim);
                    if(usedIns.Count == 0) {
                        throw new DataIntegrityException("The Person " + person.PrettyName + " is never used in any household. Please fix or delete.",person);
                    }

                    var namearr = person.Name.Split(' ');
                    if (namearr.Length != 2 && !person.Name.Contains(" - ")) {
                        throw new DataIntegrityException(
                            "The person " + person.PrettyName + " has a space in the name. Please fix.", person);
                    }
                }

                if (person.Description.StartsWith("female", StringComparison.Ordinal) &&
                    person.Gender != PermittedGender.Female) {
                    person.Gender = PermittedGender.Female;
                    errrorPersons.Add(person);
                }
                var name = person.Name;
                if (name.StartsWith("H", StringComparison.Ordinal) && name.Length > 6) {
                    name = name.Substring(4);
                }
                name = name.Trim();
                if (allnames.ContainsKey(name)) {
                    var name1 = person.Name;
                    var name2 = allnames[name];
                    throw new DataIntegrityException("The Person name \"" + name + "\" exists twice. Once as " + name1 +
                                                     " and again as " + name2);
                }
                allnames.Add(name, person.Name);
            }
            if (errrorPersons.Count > 0) {
                throw new DataIntegrityException("The gender in the opened tabs seems to be wrong.",
                    errrorPersons.Take(20).Cast<BasicElement>().ToList());
            }
            if (PerformCleanupChecks) {
                var usedPersons = new List<Person>();
                foreach (var household in sim.ModularHouseholds.Items) {
                    foreach (var person in household.Persons) {
                        usedPersons.Add(person.Person);
                    }
                }
                foreach (var person in sim.Persons.Items) {
                    if (!usedPersons.Contains(person)) {
                        throw new DataIntegrityException(
                            "The Person " + person.Name + " is not in any household. Please fix.", person);
                    }
                }
            }
        }
    }
}