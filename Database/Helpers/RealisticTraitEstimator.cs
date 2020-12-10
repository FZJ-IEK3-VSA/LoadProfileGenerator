using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Helpers {
    internal static class RealisticTraitEstimator {
        public static void Run([NotNull] Simulator sim) {
            var allEntries = new List<TimeConsumptionEntry>();
            foreach (var outcome in sim.CalculationOutcomes.Items) {
                foreach (var afftime in outcome.AffordanceTimeUses) {
                    var tce = new TimeConsumptionEntry(afftime.AffordanceName, afftime.PersonName,
                        afftime.TimeInMinutes, afftime.Executions);
                    allEntries.Add(tce);
                }
            }
            foreach (var trait in sim.HouseholdTraits.Items) {
                trait.EstimateType = EstimateType.Theoretical;
                trait.EstimatedTimePerYearInH = 0;
                trait.EstimatedTimeCount2 = 0;
                trait.EstimatedTimes2 = 0;
                trait.EstimatedDuration2InMinutes = 0;
                trait.EstimatedTimeType2 = TimeType.Day;
                var persons = new List<Person>();
                foreach (var household in sim.ModularHouseholds.Items) {
                    foreach (var householdTrait in household.Traits) {
                        if (householdTrait.HouseholdTrait == trait) {
                            persons.Add(householdTrait.DstPerson);
                        }
                    }
                }
                var personNames = persons.Select(x => x.PrettyName).ToList();

                var allAffs = trait.CollectAffordances(false);
                var timeentryForAff = new List<TimeConsumptionEntry>();
                foreach (var affordance in allAffs) {
                    double timeSpent = 0;
                    var timesExecutedPerAff = 0;
                    var personCount = 0;
                    var relevantEntries =
                        allEntries.Where(x => x.AffordanceName == affordance.Name &&
                                              personNames.Contains(x.PersonName)).ToList();
                    if (relevantEntries.Count == 0) {
                        continue;
                    }
                    foreach (var e in relevantEntries) {
                        timeSpent += e.TimeSpent;
                        timesExecutedPerAff += e.Executions;
                        personCount++;
                    }
                    var timeconsumpentry = new TimeConsumptionEntry(affordance.Name, null,
                        timeSpent / personCount, timesExecutedPerAff / personCount)
                    {
                        PersonCount = personCount
                    };
                    timeentryForAff.Add(timeconsumpentry);
                }
                if (timeentryForAff.Count > 0) {
                    double timeSpentOverAllAffs = 0;
                    var allAffsExecuted = 0;

                    foreach (var timeConsumptionEntry in timeentryForAff) {
                        timeSpentOverAllAffs += timeConsumptionEntry.TimeSpent;
                        allAffsExecuted += timeConsumptionEntry.Executions;
                    }
                    trait.EstimatedTimePerYearInH = timeSpentOverAllAffs / 60;
                    trait.EstimateType = EstimateType.FromCalculations;
                    SetEstimate(trait, timeSpentOverAllAffs, allAffsExecuted);

                    if (Math.Abs(timeSpentOverAllAffs) > 0.00001 && allAffsExecuted > 0) {
                        Logger.Info("Set new estimate:" + trait.PrettyName + " (samples: " +
                                    timeentryForAff.Sum(x => x.PersonCount) + ") Previous: " +
                                    trait.TheoreticalTimeEstimateString());
                    }
                }
                trait.SaveToDB();
            }
        }

        private static void SetEstimate([NotNull] HouseholdTrait trait, double timeSpent, int timesExecuted) {
            if (Math.Abs(timeSpent) < 0.00001 || timesExecuted == 0) {
                trait.EstimatedDuration2InMinutes = -1;
                trait.EstimatedTimeCount2 = -1;
                trait.EstimatedTimeType2 = TimeType.Day;
                trait.EstimatedTimes2 = -1;
                trait.SaveToDB();
                return;
            }
            var timeTotal = TimeSpan.FromMinutes(timeSpent);
            var timePerExecution = TimeSpan.FromSeconds(timeTotal.TotalSeconds / timesExecuted); // average time
            trait.EstimatedDuration2InMinutes = timePerExecution.TotalMinutes;
            var timeBetweenActivations = TimeSpan.FromHours(365 * 24.0 / timesExecuted);
            trait.EstimatedDuration2InMinutes = timePerExecution.TotalMinutes;
            if (timeBetweenActivations.TotalHours < 24) {
                trait.EstimatedTimeType2 = TimeType.Day;
                trait.EstimatedTimeCount2 = 1;
                trait.EstimatedTimes2 = Math.Round(24 / timeBetweenActivations.TotalHours, 1);
                return;
            }
            if (timeBetweenActivations.TotalDays < 7) {
                trait.EstimatedTimeType2 = TimeType.Week;
                trait.EstimatedTimeCount2 = 1;
                trait.EstimatedTimes2 = Math.Round(7 / timeBetweenActivations.TotalDays, 1);
                return;
            }
            if (timeBetweenActivations.TotalDays < 30) {
                trait.EstimatedTimeType2 = TimeType.Month;
                trait.EstimatedTimeCount2 = 1;
                trait.EstimatedTimes2 = Math.Round(30 / timeBetweenActivations.TotalDays, 1);
                return;
            }
            trait.EstimatedTimeType2 = TimeType.Year;
            trait.EstimatedTimeCount2 = 1;
            trait.EstimatedTimes2 = Math.Round(365 / timeBetweenActivations.TotalDays, 1);
        }

        private class TimeConsumptionEntry {
            public TimeConsumptionEntry([NotNull] string affordanceName, [CanBeNull] string personName, double timeSpent,
                int executions) {
                AffordanceName = affordanceName;
                PersonName = personName;
                TimeSpent = timeSpent;
                Executions = executions;
            }

            [NotNull]
            public string AffordanceName { get; }
            public int Executions { get; }
            public int PersonCount { get; set; }
            [CanBeNull]
            public string PersonName { get; }
            public double TimeSpent { get; }

            // public double AverageTimePerExecution => TimeSpent / Executions;
        }
    }
}