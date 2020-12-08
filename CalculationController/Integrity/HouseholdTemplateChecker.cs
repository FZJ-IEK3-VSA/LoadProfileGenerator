using System.Collections.Generic;
using System.Linq;
using Common;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;

namespace CalculationController.Integrity {
    internal class HouseholdTemplateChecker : BasicChecker {
        public HouseholdTemplateChecker(bool performCleanupChecks) : base("Household Templates", performCleanupChecks) {
        }

        private static void CheckClassifications([JetBrains.Annotations.NotNull] Simulator sim) {
            var generalTagClassifications =
                sim.HouseholdTags.Items.Where(x => !string.IsNullOrWhiteSpace(x.Classification))
                    .Select(x => x.Classification).Distinct().ToList();
            foreach (var householdTemplate in sim.HouseholdTemplates.Items) {
                var tagclasses = new List<string>();
                foreach (var hhTemplateTag in householdTemplate.TemplateTags) {
                    if (tagclasses.Contains(hhTemplateTag.Tag.Classification)) {
                        throw new DataIntegrityException("There are two tags with the classification " +
                                                         hhTemplateTag.Tag.Classification);
                    }
                    tagclasses.Add(hhTemplateTag.Tag.Classification);
                }
                foreach (var s in generalTagClassifications) {
                    if (!tagclasses.Contains(s)) {
                        throw new DataIntegrityException(
                            "The household template " + householdTemplate.PrettyName + " is missing a tag of the " +
                            "classification " + s, householdTemplate);
                    }
                }
            }
        }

        private void CheckGeneralTemplates([JetBrains.Annotations.NotNull] Simulator sim) {
            foreach (var hhg in sim.HouseholdTemplates.Items) {
                var ages = hhg.Persons.Select(x => x.Person.Age).ToList();
                if (ages.Count == 0) {
                    throw new DataIntegrityException("No persons were defined. This is not going to work.");
                }
                var minAge = ages.Min();
                var maxAge = ages.Max();
                foreach (var templateVacation in hhg.Vacations) {
                    if (templateVacation.Vacation.MinimumAge >= minAge ||
                        templateVacation.Vacation.MaximumAge <= maxAge) {
                        throw new DataIntegrityException(
                            "The vacation " + templateVacation.Vacation.PrettyName +
                            " cannot be used in the household template " + hhg.PrettyName +
                            " due to min/max age of the people." + " The persons have a minimum age of " + minAge +
                            " and a maximum age of " + maxAge, hhg);
                    }
                }
                if (hhg.Persons.Count == 0) {
                    throw new DataIntegrityException(
                        "The household template " + hhg.PrettyName + " has no persons. Please add at least one.", hhg);
                }
                if (hhg.Vacations.Count == 0 && hhg.TemplateVacationType == TemplateVacationType.FromList) {
                    throw new DataIntegrityException(
                        "The household template " + hhg.PrettyName + " has no vacations. Please add at least one.",
                        hhg);
                }
                var personsInHousehold = hhg.Persons.Select(x => x.Person).ToList();
                var personsInGenerator = new List<Person>();
                foreach (var entry in hhg.Entries) {
                    personsInGenerator.AddRange(entry.Persons.Select(x => x.Person));
                }
                personsInGenerator = personsInGenerator.Distinct().ToList();
                foreach (var person in personsInGenerator) {
                    if (!personsInHousehold.Contains(person)) {
                        throw new DataIntegrityException(
                            "The Person " + person.PrettyName + " is not in the list of persons " +
                            " in the householdgenerator " + hhg.Name + ". Please fix.", hhg);
                    }
                }
                if(hhg.TemplateVacationType == TemplateVacationType.RandomlyGenerated && hhg.TimeProfileForVacations == null) {
                    throw new DataIntegrityException("The template " + hhg.PrettyName + " has no vacation time profile set even though it is required.",hhg);
                }
                if (hhg.Persons.Any(x => x.LivingPatternTag == null)) {
                    throw new DataIntegrityException("No living pattern set for the persons in " + hhg,hhg);
                }
                //check for food / unhungry
                if (PerformCleanupChecks && !hhg.Name.StartsWith("O")) {
                    foreach (HHTemplatePerson hhTemplatePerson in hhg.Persons) {
                        //find all the traits for this person
                        var traits = hhg.Entries.Where(x => x.Persons.Any(y => y.Person == hhTemplatePerson.Person))
                            .ToList();
                        bool foundFood = traits.Any(x => x.TraitTag.Name.StartsWith("Food"));
                        if (!foundFood) {
                            throw new DataIntegrityException("The person " + hhTemplatePerson.Person.PrettyName +
                                                             " in the template "
                                                             + hhg.Name +
                                                             " does not seem to have any food traits. This seems wrong. Please fix",
                                hhg);
                        }
                    }
                }
            }
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            CheckClassifications(sim);
            CheckGeneralTemplates(sim);
        }
    }
}