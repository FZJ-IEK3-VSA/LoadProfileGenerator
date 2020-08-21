using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class ModularHouseholdChecker : BasicChecker {
        public ModularHouseholdChecker(bool performCleanupChecks) : base("Modular Households", performCleanupChecks)
        {
        }

        private void CheckCleanupNames([NotNull] ModularHousehold chh)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            var name = chh.Name;
            if (!name.StartsWith("x ", StringComparison.Ordinal) && name.Contains(" ")) {
                name = name.Substring(0, name.IndexOf(" ", StringComparison.Ordinal));
                foreach (var person in chh.Persons) {
                    if (!person.Person.Name.StartsWith(name, StringComparison.CurrentCulture)) {
                        throw new DataIntegrityException(
                            "In the modular household " + chh.Name + " the Person with the name " +
                            person.Person.PrettyName +
                            " doesn't fit. The CHxxx is not equal to the household. Please fix.", chh);
                    }
                }
            }
        }

        private void CheckLivingPattern([NotNull] ModularHousehold mhh)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var mhhPerson in mhh.Persons) {
                if (mhhPerson.LivingPatternTag == null) {
                    continue;
                }
                var traits = mhh.Traits.Where(x => x.DstPerson == mhhPerson.Person).ToList();

                if (mhhPerson.LivingPatternTag.Name.ToLower(CultureInfo.InvariantCulture).Contains("school")) {
                    //this is a school child
                    if (!traits.Any(x => x.PrettyName.ToLower(CultureInfo.InvariantCulture).Contains("school"))) {
                        throw new DataIntegrityException(
                            "The " + mhhPerson.Name +
                            " does not seem to have any school, even though the living pattern says it is a school child",
                            mhh);
                    }
                }
                if (mhhPerson.LivingPatternTag.Name.ToLower(CultureInfo.InvariantCulture).Contains("worker")) {
                    //this is an office worker
                    if (!traits.Any(x => x.PrettyName.ToLower(CultureInfo.InvariantCulture).Contains("work"))) {
                        throw new DataIntegrityException(
                            "The " + mhhPerson.Name +
                            " does not seem to have any work, even though the living pattern says it is an worker",
                            mhh);
                    }
                }
            }
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        private void CheckLivingPatternTraits([NotNull] ModularHousehold mhh, [NotNull] LivingPatternTag tagForAll)
        {
            if (!PerformCleanupChecks) {
                return;
            }

            foreach (var mhhPerson in mhh.Persons) {
                if (mhhPerson.LivingPatternTag == null) {
                    throw new DataIntegrityException(
                        "The person " + mhhPerson.Person.PrettyName + " has no living pattern tag set.", mhh);
                }
                var traitsWithMissingTags = new List<HouseholdTrait>();
                var personTraits = mhh.Traits.Where(x => x.DstPerson == mhhPerson.Person).ToList();
                foreach (var personTrait in personTraits) {
                    if (personTrait.HouseholdTrait == null) {
                        throw new DataIntegrityException("HouseholdTrait was null");
                    }
                    if (personTrait.HouseholdTrait.LivingPatternTags.Any(x => x.Tag == tagForAll)) {
                        //all is good since this trait was for everything
                        continue;
                    }
                    if (personTrait.HouseholdTrait.LivingPatternTags.Any(x => mhhPerson.LivingPatternTag.Name.StartsWith(x.Name))) {
                        //the person will have the most specific tag, the traits can be more generic
                        continue;
                    }
                    traitsWithMissingTags.Add(personTrait.HouseholdTrait);
                }
                if (traitsWithMissingTags.Count > 0) {
                    var elementsToOpen = new List<BasicElement>
                    {
                        mhh
                    };
                    elementsToOpen.AddRange(traitsWithMissingTags);
                    throw new DataIntegrityException(
                        "The opened traits have no matching trait tag for " + mhhPerson.LivingPatternTag, elementsToOpen);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void CheckMinimumPersonRequirements([NotNull] ModularHousehold modularHousehold,
            [NotNull] ModularHouseholdPerson modularHouseholdPerson)
        {
            if (!PerformCleanupChecks) {
                return;
            }

            if (modularHouseholdPerson.Person.Name.ToUpperInvariant().Contains("MAID") ||
                modularHouseholdPerson.Person.Description.ToUpperInvariant().Contains("MAID")) {
                return;
            }
            var traitsForPerson =
                modularHousehold.Traits.Where(x => x.DstPerson == modularHouseholdPerson.Person).ToList();
            var traitNames =
                traitsForPerson.Select(x => x.HouseholdTrait.PrettyName.ToUpperInvariant()).ToList();
            var shower = traitNames.Any(x => x.Contains("SHOWER"));
            if (!shower && modularHouseholdPerson.Person.Age > 10) {
                throw new DataIntegrityException(
                    "No shower in the household " + modularHousehold.PrettyName + " for " +
                    modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            var sleep = traitNames.Any(x => x.Contains("SLEEP"));
            if (!sleep) {
                throw new DataIntegrityException(
                    "No sleep in the household " + modularHousehold.PrettyName + " for " +
                    modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            var unhungry = traitNames.Any(x => x.Contains("UNHUNGRY"));
            if (!unhungry) {
                //var hungryTrait = sim.HouseholdTraits.It.First(x => x.Name.Contains("Desire for food, join only"));
                //modularHousehold.AddTrait(hungryTrait,ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, modularHouseholdPerson.Person);
                throw new DataIntegrityException("Unhungry not set in the household " + modularHousehold.PrettyName + " for " +modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            var sickness = traitNames.Any(x => x.Contains("SICKNESS"));
            if (!sickness) {
                throw new DataIntegrityException(
                    "No sickness activities in the household " + modularHousehold.PrettyName + " for " +
                    modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            var toilet = traitNames.Any(x => x.Contains("TOILET"));
            if (!toilet && modularHouseholdPerson.Person.Age > 3) {
                throw new DataIntegrityException(
                    "No toilet activities in the household " + modularHousehold.PrettyName + " for " +
                    modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            var getready = traitNames.Any(x => x.Contains("READY"));
            if (!getready && modularHouseholdPerson.Person.Age > 5) {
                throw new DataIntegrityException(
                    "No get ready activities in the household " + modularHousehold.PrettyName + " for " +
                    modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            var work = traitNames.Any(x => x.Contains("WORK"));
            var school = traitsForPerson.Any(y => y.HouseholdTrait.Tags.Any(x => x.Name.ToUpperInvariant() == "CHILD / SCHOOL"));
            var outsideafternoonTraits =
                traitsForPerson.Where(
                    x =>
                        x.HouseholdTrait.Tags.Any(
                            y => y.Name.ToUpperInvariant().Contains("OUTSIDE AFTERNOON ENTERTAINMENT"))).ToList();
            if (outsideafternoonTraits.Count == 0 && !work && !school && modularHouseholdPerson.Person.Age > 10) {
                throw new DataIntegrityException(
                    "Not a single outside afternoon entertainment in the household " + modularHousehold.PrettyName +
                    " for " + modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
            if (school) {
                var homework = traitsForPerson.Any(x => x.HouseholdTrait.Name.ToLower(CultureInfo.InvariantCulture).Contains("homework"));
                if(!homework) {
                    throw new DataIntegrityException("No homework in the household " + modularHousehold.PrettyName
                        + " for the person " + modularHouseholdPerson.Person.PrettyName, modularHousehold);
                }
            }

            var outsideEveningTraits =
                traitsForPerson.Where(
                    x =>
                        x.HouseholdTrait.Tags.Any(
                            y => y.Name.ToUpperInvariant().Contains("OUTSIDE EVENING ENTERTAINMENT"))).ToList();

            if (outsideEveningTraits.Count == 0 && modularHouseholdPerson.Person.Age > 15) {
                throw new DataIntegrityException(
                    "Not a single outside evening entertainment in the household " + modularHousehold.PrettyName +
                    " for " + modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }

            var hobby =
                traitsForPerson.Where(x => x.HouseholdTrait.Tags.Any(y => y.Name.ToUpperInvariant().Contains("HOBBY")))
                    .ToList();

            if (hobby.Count == 0 && modularHouseholdPerson.Person.Age > 15) {
                throw new DataIntegrityException(
                    "Not a single hobby in the household " + modularHousehold.PrettyName + " for " +
                    modularHouseholdPerson.Person.PrettyName, modularHousehold);
            }
        }

        private void CheckModularHouseholdsMinimum([NotNull] ModularHousehold modularHousehold)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            if (modularHousehold.Name.StartsWith("O", StringComparison.Ordinal) ||
                modularHousehold.Name.StartsWith("x O", StringComparison.Ordinal)) {
                return;
            }
            var laundry = modularHousehold.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("LAUNDRY"));
            if (!laundry) {
                throw new DataIntegrityException("No laundry in the household " + modularHousehold.PrettyName,
                    modularHousehold);
            }
            var dishwashing = modularHousehold.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("DISHWASH"));
            if (!dishwashing) {
                throw new DataIntegrityException("No dishwashing in the household " + modularHousehold.PrettyName,
                    modularHousehold);
            }
            var cleanBathroom =
                modularHousehold.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("CLEAN BATHROOM"));
            if (!cleanBathroom) {
                throw new DataIntegrityException(
                    "No bathroom cleaning in the household " + modularHousehold.PrettyName, modularHousehold);
            }
            var foodshopping =
                modularHousehold.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("FOOD SHOPPING"));
            if (!foodshopping) {
                throw new DataIntegrityException("No food shopping in the household " + modularHousehold.PrettyName,
                    modularHousehold);
            }
            var vacuum = modularHousehold.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("VACUUM"));
            if (!vacuum) {
                throw new DataIntegrityException("No vacuuming in the household " + modularHousehold.PrettyName,
                    modularHousehold);
            }
            var drying = modularHousehold.Traits.Any(x => x.PrettyName.ToUpperInvariant().Contains("DRY LAUNDRY"));
            if (!drying) {
                throw new DataIntegrityException("No laundry drying in the household " + modularHousehold.PrettyName,
                    modularHousehold);
            }

            foreach (var modularHouseholdPerson in modularHousehold.Persons) {
                CheckMinimumPersonRequirements(modularHousehold, modularHouseholdPerson);
            }
        }

        private void CheckPersonDefinitions([NotNull] ModularHousehold chh)
        {
            if (!PerformCleanupChecks) {
                return;
            }

            foreach (var person in chh.Persons) {
                var classifications = new Dictionary<string,HouseholdTrait>();
                foreach (var trait in chh.Traits) {
                    if (trait.DstPerson == person.Person) {
                        if (classifications.ContainsKey(trait.HouseholdTrait.Classification)&&!trait.HouseholdTrait.Classification.ToLower().Contains("resting")) {
                            string traits = Environment.NewLine +   classifications[trait.HouseholdTrait.Classification].PrettyName;
                            traits += Environment.NewLine + trait.HouseholdTrait.PrettyName;
                            throw new DataIntegrityException(
                                "The Person " + person.Person?.Name + " in the household " + chh.PrettyName +
                                " has more than one trait of the classification " +
                                trait.HouseholdTrait.Classification +
                                ". Please fix:" + traits, chh);
                        }

                        if (!trait.HouseholdTrait.Classification.ToLower().Contains("resting")) {
                            classifications.Add(trait.HouseholdTrait.Classification, trait.HouseholdTrait);
                        }
                    }
                }
            }
        }

        private void CheckTags([NotNull][ItemNotNull] ObservableCollection<ModularHousehold> modularHouseholds,
            [NotNull][ItemNotNull] ObservableCollection<HouseholdTag> householdTags)
        {
            if (!PerformCleanupChecks) {
                return;
            }

            var generalTagClassifications =
                householdTags.Where(x => !string.IsNullOrWhiteSpace(x.Classification))
                    .Select(x => x.Classification)
                    .ToList();
            foreach (var modularHousehold in modularHouseholds) {
                var tagclasses = new List<string>();
                foreach (var chhTag in modularHousehold.ModularHouseholdTags) {
                    if (tagclasses.Contains(chhTag.Tag.Classification)) {
                        throw new DataIntegrityException("There are two tags with the classification " +
                                                         chhTag.Tag.Classification);
                    }
                    tagclasses.Add(chhTag.Tag.Classification);
                }
                if (PerformCleanupChecks) {
                    foreach (var s in generalTagClassifications) {
                        if (!tagclasses.Contains(s)) {
                            throw new DataIntegrityException(
                                "The modular household " + modularHousehold.PrettyName + " is missing a tag of the " +
                                "classification " + s, modularHousehold);
                        }
                    }
                }
            }
        }

        private void CheckTraitsInHH([NotNull] ModularHousehold chh)
        {
            var persons = chh.Persons.Select(x => x.Person).ToList();
            foreach (var trait in chh.Traits) {
                if (trait.AssignType == ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name &&
                    trait.HouseholdTrait.CollectAffordances(true).Count > 0)
                    // no need to check traits without any affordances.
                {
                    var person = trait.DstPerson;
                    if (person == null) {
                        throw new DataIntegrityException("No person was set in the trait " + trait.HouseholdTrait.Name, trait.HouseholdTrait);
                    }
                    if (!persons.Contains(person)) {
                        throw new DataIntegrityException(
                            "The trait " + trait + " is assigned to " + person.PrettyName +
                            " but that person is not in the household.", chh);
                    }

                    var foundone = false;
                    if (!trait.IsValidPerson(person)) {
                        throw new DataIntegrityException(
                            "The trait " + trait.Name + " in the household " + chh.Name + " is assigned to " +
                            trait.DstPerson + ", which seems to be invalid. This is probably not intended. " +
                            "Please fix the desire age/gender limits or change the Person", chh);
                    }
                    if (!trait.HouseholdTrait.IsValidForPerson(person) && PerformCleanupChecks) {
                        throw new DataIntegrityException(
                            "The trait " + trait.Name + " in the household " + chh.Name + " is assigned to " +
                            trait.DstPerson +
                            ", which seems to be invalid. Not a single affordance in the trait is executeable. This is probably not intended. " +
                            "Please fix the desire age/gender limits or change the Person", chh);
                    }
                    var affordances = trait.HouseholdTrait.CollectAffordances(true);
                    foreach (var aff in affordances) {
                        if (aff.IsValidPerson(person)) {
                            foundone = true;
                        }
                    }
                    if (!foundone && PerformCleanupChecks) {
                        throw new DataIntegrityException(
                            "The trait " + trait.Name + " in the household " + chh.Name + " is assigned to " +
                            trait.DstPerson +
                            ", which doesn't fit any of the desires. This is probably not intended. " +
                            "Please fix the desire age/gender limits or change the Person", chh);
                    }
                }
            }
        }

        private static void GeneralChecks([NotNull] ModularHousehold chh)
        {
            if (chh.Vacation == null) {
                throw new DataIntegrityException(
                    "The modular household " + chh.Name + " has no vacation set. Please choose one.", chh);
            }
            var allPersons = chh.AllPersons;
            if (allPersons.Count == 0) {
                throw new DataException("The household " + chh.Name + " has no persons!");
            }
            var minAge = allPersons.Min(x => x.Age);
            var maxAge = allPersons.Max(x => x.Age);
            if (chh.Vacation.MinimumAge > minAge || chh.Vacation.MaximumAge < maxAge) {
                throw new DataIntegrityException(
                    "The vacation " + chh.Vacation.PrettyName + " cannot be used in the household template " +
                    chh.PrettyName + " due to min/max age of the people. The persons have a minimum age of " +
                    minAge + " and a maximum age of " + maxAge, chh);
            }
            if (chh.Persons.Count == 0) {
                throw new DataIntegrityException("The modular household " + chh.Name + " has no people!", chh);
            }
            var traitCounts = new Dictionary<HouseholdTrait, int>();
            foreach (var ctrait in chh.Traits) {
                if (!traitCounts.ContainsKey(ctrait.HouseholdTrait)) {
                    traitCounts.Add(ctrait.HouseholdTrait, 0);
                }
                traitCounts[ctrait.HouseholdTrait]++;
            }
            var personCount = chh.Persons.Count;
            foreach (var ctrait in chh.Traits) {
                var count = traitCounts[ctrait.HouseholdTrait];
                if (count > ctrait.HouseholdTrait.MaximumNumberInCHH) {
                    throw new DataIntegrityException(
                        "The trait " + ctrait.HouseholdTrait.Name + " is only allowed " +
                        ctrait.HouseholdTrait.MaximumNumberInCHH + " in a household, but " + chh.Name + " has " +
                        count +
                        ". Please fix.", chh);
                }
                if (personCount < ctrait.HouseholdTrait.MinimumPersonsInCHH ||
                    personCount > ctrait.HouseholdTrait.MaximumPersonsInCHH) {
                    throw new DataIntegrityException(
                        "The trait " + ctrait.HouseholdTrait.Name + " requires more or equal to " +
                        ctrait.HouseholdTrait.MinimumPersonsInCHH + " persons and less or equal to " +
                        ctrait.HouseholdTrait.MaximumPersonsInCHH + " persons in the household " + chh.PrettyName +
                        ". Please fix.", chh);
                }
            }
        }

        private static void CheckTagPersonAssignments([NotNull] ModularHousehold chh)
        {
            foreach (ModularHouseholdTrait modularHouseholdTrait in chh.Traits) {
                Person p = modularHouseholdTrait.DstPerson;
                if (p!= null &&  !modularHouseholdTrait.IsValidPerson(p)) {
                    throw new DataIntegrityException("Person is not valid in the trait " + modularHouseholdTrait.HouseholdTrait.Name + " in the house "+ chh.Name + ": Not a single affordance in the trait could be executed by the person, either due to age or gender restrictions on the affordances.",chh);
                }
            }
        }

        /*
        private void CheckTimeAllocations(ModularHousehold mhh)
        {
            if (!PerformCleanupChecks)
                return;
            var persons = mhh.PurePersons.ToList();
            if(persons.Count ==0)
                throw new DataIntegrityException("No persons in " + mhh.PrettyName);
            double minTime = 365 * 24 * 0.8;
            double maxTime = 365 * 24 * 1.2;
            foreach (Person person in persons)
            {
                var traitsForPerson = mhh.Traits.Where(x => x.DstPerson == person);
                double timesum = traitsForPerson.Sum(x => x.HouseholdTrait.EstimatedTimePerYearInH);
                if(timesum < minTime)
                    throw  new DataIntegrityException("Seems like the person " + person.Name + " in the house " + mhh.PrettyName + " has not enough activities allocated: " +timesum);
                if (timesum > maxTime)
                    throw new DataIntegrityException("Seems like the person " + person.Name + " in the house " + mhh.PrettyName + " has too many activities allocated: " + timesum);

            }
            

        }
        */
        protected override void Run(Simulator sim)
        {
            CheckTags(sim.ModularHouseholds.Items, sim.HouseholdTags.Items);
            var tagForAll = sim.LivingPatternTags.Items.FirstOrDefault(x => x.Name == "Living Pattern / All");
            if (tagForAll == null) {
                throw new LPGException("The trait tag Living Pattern / All was not found.");
            }
            foreach (var modularHousehold in sim.ModularHouseholds.Items) {
                GeneralChecks(modularHousehold);
                CheckModularHouseholdsMinimum(modularHousehold);
                CheckCleanupNames(modularHousehold);
                CheckPersonDefinitions(modularHousehold);
                CheckTraitsInHH(modularHousehold);
                CheckLivingPatternTraits(modularHousehold, tagForAll);
                CheckLivingPattern(modularHousehold);
                CheckTagPersonAssignments(modularHousehold);
                //  CheckTimeAllocations(modularHousehold);
            }
        }
    }
}