using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace LoadProfileGenerator {
    public static class TemplatePersonCreator {
        public static void CreateTemplatePersons([NotNull] Simulator sim)
        {
            Logger.Info("Starting the creation of the templated persons");
            var tps = sim.TemplatePersons.Items.ToList();
            foreach (var tpperson in tps) {
                sim.TemplatePersons.DeleteItem(tpperson);
            }

            var tp = 1;
            var allPersons = new List<PersonEntry>();
            foreach (var simPerson in sim.Persons.Items) {
                allPersons.Add(new PersonEntry(simPerson));
            }

            allPersons.Sort();
            foreach (var personEntry in allPersons) {
                var person = personEntry.Person;
                TemplatePerson tpPerson = null;
                Logger.Get().SafeExecuteWithWait(() => tpPerson =
                    sim.TemplatePersons.CreateNewItem(sim.ConnectionString));

                var tpName = "TP" + tp.ToString("000", CultureInfo.CurrentCulture) + " " + person.Description;
                Logger.Info("Creating " + tpName);
                tpPerson.Name = tpName;
                tpPerson.Gender = person.Gender;
                tpPerson.Age = person.Age;
                tpPerson.SickDays = person.SickDays;
                var mhh = sim.ModularHouseholds.Items.First(x => x.Persons.Any(y => y.Person == person));
                tpPerson.BaseHousehold = mhh;
                tpPerson.BasePerson = personEntry.Person;
                tpPerson.SaveToDB();
                var chh =
                    sim.ModularHouseholds.Items.First(x => x.Persons.Select(y => y.Person).Any(z => z == person));

                Logger.Info("Found " + chh.PrettyName);
                var traits = chh.Traits.Where(x => x.DstPerson == person).ToList();
                traits.ForEach(x => tpPerson.AddTrait(x.HouseholdTrait));
                Logger.Info("Added " + traits.Count + " traits");
                tp++;
            }

            Logger.Info("finished the template persons");
        }

        [ItemNotNull]
        [NotNull]
        public static List<ModularHousehold> RunCalculationTests([NotNull] Simulator sim)
        {
            var allhh = new List<ModularHousehold>();
            Logger.Info("Starting sync");
            foreach (var templatePerson in sim.TemplatePersons.Items) {
                var personCode = templatePerson.Name.Substring(0, 4);
                if (!personCode.StartsWith("TP", StringComparison.Ordinal)) {
                    Logger.Warning("Ignoring " + templatePerson.Name + " because name doesn't start with TP");
                    continue;
                }

                // Person
                var p = sim.Persons.Items.FirstOrDefault(x => x.Name == templatePerson.Name);
                if (p == null) {
                    Logger.Info("Creating person " + templatePerson.Name);
                    p = sim.Persons.CreateNewItem(sim.ConnectionString);
                }
                else {
                    Logger.Warning("Person " + templatePerson.Name + " already existed.");
                }

                p.Name = templatePerson.Name;
                p.Gender = templatePerson.Gender;
                p.Age = templatePerson.Age;
                p.SickDays = templatePerson.SickDays;
                p.Description = "Created to test the template person " + templatePerson.Name;
                p.SaveToDB();
                // modular Household
                var chh = sim.ModularHouseholds.Items.FirstOrDefault(x => x.Name == templatePerson.Name);
                if (chh == null) {
                    Logger.Info("Creating modular household  " + templatePerson.Name);
                    chh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
                }
                else {
                    Logger.Warning("Modular Household " + templatePerson.Name + " already existed.");
                }

                chh.Name = templatePerson.Name;
                chh.Vacation = sim.Vacations[1];
                chh.SaveToDB();
                chh.Description = "Created to test the template person " + templatePerson.Name;
                if (templatePerson.BaseHousehold == null)
                {
                    throw new LPGException("base household was null");
                }
                //TODO: add traittag to template person
                ModularHouseholdPerson mhhperson =
                    templatePerson.BaseHousehold.Persons.FirstOrDefault(x => x.Person == templatePerson.BasePerson);
                LivingPatternTag tt = null;
                if (mhhperson != null) {
                    tt = mhhperson.LivingPatternTag;
                }
                else {
                    Logger.Error("no tag");
                }

                chh.AddPerson(p, tt);
                allhh.Add(chh);
                var i = 0;
                foreach (var personTrait in templatePerson.Traits) {
                    if (personTrait.Trait.Name.Contains("boardgame")) {
                        continue;
                    }

                    chh.AddTrait(personTrait.Trait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, p);
                    i++;
                }

                Logger.Info("Added " + i + " traits.");
                var traits = templatePerson.Traits.Select(x => x.Trait).ToList();
                var traitsToDelete = new List<ModularHouseholdTrait>();
                i = 0;
                foreach (var chhtrait in chh.Traits) {
                    if (!traits.Contains(chhtrait.HouseholdTrait)) {
                        traitsToDelete.Add(chhtrait);
                        i++;
                    }
                }

                foreach (var trait in traitsToDelete) {
                    chh.DeleteTraitFromDB(trait);
                }

                Logger.Info("Deleted " + i + " traits.");
            }

            return allhh;
        }

        private class PersonEntry : IComparable<PersonEntry> {
            public PersonEntry([NotNull] Person person)
            {
                Person = person;
                Age = person.Age;
                Gender = person.Gender;
            }

            [UsedImplicitly]
            public int Age { get; }

            [UsedImplicitly]
            public PermittedGender Gender { get; }

            [NotNull]
            public Person Person { get; }

            public int CompareTo([CanBeNull] PersonEntry other)
            {
                if (other == null) {
                    return 0;
                }

                if (other.Gender != Gender) {
                    return other.Gender.CompareTo(Gender);
                }

                if (other.Age != Age) {
                    return Age.CompareTo(other.Age);
                }

                return string.Compare(Person.PrettyName, other.Person.PrettyName, StringComparison.Ordinal);
            }
        }
    }
}