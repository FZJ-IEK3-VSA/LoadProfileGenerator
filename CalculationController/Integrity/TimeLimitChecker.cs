using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class TimeLimitChecker : BasicChecker {
        public TimeLimitChecker(bool performCleanupChecks) : base("Time Limits", performCleanupChecks) {
        }

        private void CheckTimeLimits([NotNull][ItemNotNull] ObservableCollection<TimeLimit> timeLimits) {
            if (!PerformCleanupChecks) {
                return;
            }
            var mode = new List<PermissionMode>
            {
                PermissionMode.Temperature,
                PermissionMode.LightControlled,
                PermissionMode.ControlledByDateProfile,
                PermissionMode.VacationControlled,
                PermissionMode.HolidayControlled
            };
            foreach (var timeLimit in timeLimits) {
                foreach (var entry in timeLimit.TimeLimitEntries) {
                    if (entry.RandomizeTimeAmount == 0) {
                        if (!mode.Contains(entry.RepeaterType) && entry.Subentries.Count == 0) {
                            throw new DataIntegrityException(
                                "The time limit " + timeLimit.PrettyName + " has no variation.", timeLimit);
                        }
                    }
                }
            }
        }

        protected override void Run(Simulator sim) {
            CheckTimeLimits(sim.TimeLimits.Items);
            foreach (var devt in sim.TimeLimits.Items) {
                if (devt.TimeLimitEntries.Count == 0) {
                    throw new DataIntegrityException("The time limit " + devt.PrettyName +
                                                     " has not a single limit set. Please fix.");
                }
                foreach (var entry in devt.TimeLimitEntries) {
                    if (entry.Subentries.Count == 0 && entry.RepeaterType == PermissionMode.ControlledByDateProfile &&
                        entry.DateBasedProfile == null) {
                        throw new DataIntegrityException(
                            "The time limit " + devt.PrettyName + " contains a date time profile condition, but " +
                            " no such profile is set. Please fix!", devt);
                    }
                }
            }

            if (PerformCleanupChecks) {
                foreach (var devt in sim.TimeLimits.Items) {
                    var uses = devt.CalculateUsedIns(sim);
                    if (uses.Count == 0) {
                        Logger.Warning("Unused time limit: " + devt.PrettyName);
                    }
                }
            }
        }
    }
}