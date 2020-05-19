using Common;
using Database;
using Database.Tables.BasicElements;

namespace CalculationController.Integrity {
    internal class TimeProfileChecker : BasicChecker {
        public TimeProfileChecker(bool performCleanupChecks) : base("Time Profile Checker", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var profile in sim.Timeprofiles.It) {
                if (profile.DataSource.Length == 0) {
                    throw new DataIntegrityException(
                        "The time profile " + profile.Name + " has no data source entered.", profile);
                }
                if (profile.ObservableDatapoints.Count < 2) {
                    throw new DataIntegrityException(
                        "The time profile " + profile.Name +
                        " seems to be broken, because it has less than 2 data points.", profile);
                }
                var tp = profile.ObservableDatapoints[0];
                // distance between data points
                foreach (var tp2 in profile.ObservableDatapoints) {
                    var timedistance = tp2.Time - tp.Time;
                    var days = timedistance.TotalDays;
                    if (days > 366) {
                        throw new DataIntegrityException(
                            "The time profile " + profile.Name +
                            " is longer than 366 days, which is the maximum that's allowed.", profile);
                    }
                }
                // absolute data point value
                foreach (var tp2 in profile.ObservableDatapoints) {
                    if (tp2.Time.TotalDays > 366) {
                        throw new DataIntegrityException(
                            "The time profile " + profile.Name +
                            " is longer than 366 days, which is the maximum that's allowed.", profile);
                    }
                }
                if (profile.TimeProfileType == TimeProfileType.Relative) {
                    foreach (var dataPoint in profile.ObservableDatapoints) {
                        if (dataPoint.Value > 100) {
                            throw new DataIntegrityException(
                                "The time based profile " + profile.Name +
                                " has some values that are above 100%. This is not non-sensical for" +
                                " a relative profile. The value was " + dataPoint.Value + ". Please fix.", profile);
                        }
                    }
                }
            }
        }
    }
}