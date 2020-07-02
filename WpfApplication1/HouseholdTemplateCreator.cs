using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Automation;
using CalculationController.Integrity;
using Common;
using Common.Enums;
using Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace LoadProfileGenerator {
    public class HouseholdTemplateCreator {
        [NotNull] private readonly Simulator _sim;

        public HouseholdTemplateCreator([NotNull] Simulator sim) => _sim = sim;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Run(bool multithread, [NotNull] Simulator sim)
        {
            var basicHouseholds =
                _sim.ModularHouseholds.Items.Where(x => !x.Name.StartsWith("x ", StringComparison.Ordinal)).ToList();
            var pbw = new ProgressbarWindow("Creating templates", "Template Creation",
                basicHouseholds.Count);
            pbw.Show();
            Random rnd = new Random();
            void CreateHouseholds()
            {
                try
                {
                    var toDelete = _sim.HouseholdTemplates.Items.Where(x => x.Persons.Any(y => y.LivingPattern == null))
                        .ToList();
                    var toDelete2 = _sim.HouseholdTemplates.Items
                        .Where(x => x.Entries.Any(y =>
                            y.TraitTag.Name.StartsWith("Living Pattern", StringComparison.CurrentCulture))).ToList();
                    var toDelete3 = _sim.HouseholdTemplates.Items
                        .Where(x => x.Entries.Any(y =>
                            y.TraitTag.Name.StartsWith("Web / ", StringComparison.CurrentCulture))).ToList();
                    toDelete.AddRange(toDelete2);
                    toDelete.AddRange(toDelete3);
                    Logger.Info("Found " + toDelete.Count + " templates to delete.");
                    foreach (HouseholdTemplate template in toDelete)
                    {
                        Logger.Info("Deleting household template " + template.PrettyName);
                        sim.HouseholdTemplates.DeleteItem(template);
                        //    if (count > 2)
                        //      return;
                    }

                    int j = 0;
                    for (var i = 0; i < basicHouseholds.Count; i++)
                    {
                        if (!Config.IsInUnitTesting)
                        {
                            var i1 = i;
                            Logger.Get().SafeExecuteWithWait(() => pbw.UpdateValue(i1));
                        }

                        var modularHousehold = basicHouseholds[i];
                        if (modularHousehold.CreationType != CreationType.ManuallyCreated)
                        {
                            //skip previously templated households
                            continue;
                        }

                        var template = _sim.HouseholdTemplates.FindFirstByName(modularHousehold.Name);
                        if (template == null)
                        {
                            Logger.ImportantInfo("Creating household template for " + modularHousehold.PrettyName);
                            Logger.Get()
                                .SafeExecuteWithWait(
                                    () => template = _sim.HouseholdTemplates.CreateNewItem(_sim.ConnectionString));
                            template.Count = 10;
                            template.Name = modularHousehold.Name;
                            template.Description = "Automatically created";
                            template.EnergyIntensityType = EnergyIntensityType.Random;
                            template.NewHHName = "x " + modularHousehold.Name;

                            foreach (var person in modularHousehold.Persons)
                            {
                                template.AddPerson(person.Person, person.TraitTag);
                            }

                            foreach (var modularHouseholdTag in modularHousehold.ModularHouseholdTags)
                            {
                                template.AddTemplateTag(modularHouseholdTag.Tag);
                            }

                            SetVacation(sim, template, rnd);
                            template.ImportExistingModularHouseholds(modularHousehold);

                            template.SaveToDB();
                            j++;
                        }
                    }

                    SimIntegrityChecker.Run(_sim);
                    if (!Config.IsInUnitTesting)
                    {
                        MessageWindowHandler.Mw.ShowInfoMessage(
                            "Finished creating household templates. Created " + j + " templates.", "LPG");
                    }

                    Logger.Get().SafeExecuteWithWait(() => pbw.Close());
                }
                catch (Exception ex)
                {
                    MessageWindowHandler.Mw.ShowDebugMessage(ex);
                    Logger.Exception(ex);
                }
            }
            if (multithread) {
                var t = new Thread(() => CreateHouseholds());
                t.Start();
                return;
            }

            CreateHouseholds();
        }

        private static void SetVacation([NotNull] Simulator sim, [NotNull] HouseholdTemplate template, [NotNull] Random rnd)
        {
            template.TemplateVacationType = TemplateVacationType.RandomlyGenerated;
            var forFamiliesWithChildren =
                sim.DateBasedProfiles.Items.First(x => x.Name == "School Holidays Saxony, Germany, 2015, 1 = vacation");
            var noChildren =
                sim.DateBasedProfiles.Items.First(x => x.Name == "School Holidays Saxony, Germany, 2015, 1 = no vacation");

            template.TemplateVacationType = TemplateVacationType.RandomlyGenerated;
            template.MinNumberOfVacations = rnd.Next(2) + 1;
            template.MaxNumberOfVacations = template.MinNumberOfVacations + rnd.Next(2);
            template.MinTotalVacationDays = 5 + rnd.Next(5);
            template.MaxTotalVacationDays = 11 + rnd.Next(10);
            template.AverageVacationDuration = 5 + rnd.Next(3);
            var minAge = template.Persons.Select(x => x.Person.Age).Min();

            if (minAge > 18) {
                template.TimeProfileForVacations = noChildren;
            }
            else {
                template.TimeProfileForVacations = forFamiliesWithChildren;
            }

            template.SaveToDB();
            Logger.Info("Updated " + template.PrettyName);
        }
    }
}