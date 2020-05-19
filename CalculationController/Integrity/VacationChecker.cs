using System;
using Common;
using Database;

namespace CalculationController.Integrity {
    internal class VacationChecker : BasicChecker {
        public VacationChecker(bool performCleanupChecks) : base("Vacations", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var vacation in sim.Vacations.It) {
                if (vacation.MinimumAge < 0 || vacation.MaximumAge == 0) {
                    throw new DataIntegrityException(
                        "The vacation " + vacation.PrettyName +
                        " has no minimum or maximum age set. Values need to be bigger than 0. Please fix!", vacation);
                }
                var array = new int[366];
                foreach (var vacationTime in vacation.VacationTimes) {
                    DateTime starttime;
                    DateTime endtime;
                    try {
                        starttime = new DateTime(vacationTime.Start.Year, vacationTime.Start.Month, vacationTime.Start.Day);
                        endtime = new DateTime(vacationTime.End.Year, vacationTime.End.Month, vacationTime.End.Day);
                    }
                    catch (Exception ex) {
                        throw new DataIntegrityException("The dates in the vacation " + vacation.PrettyName + " seem to be messed up and could not be read. Please fix. The exact error message from Windows was " + ex.Message,
                            vacation);
                    }

                    var startidx = starttime.DayOfYear;
                    var endidx = endtime.DayOfYear;
                    for (var i = startidx; i < endidx; i++) {
                        if (array[i] != 0) {
                            throw new DataIntegrityException("Overlapping vacations in the vacation " + vacation,
                                vacation);
                        }
                        array[i] = 1;
                    }
                }
            }
        }
    }
}