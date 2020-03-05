using System;
using System.Globalization;
using Common;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    public abstract class BasicChecker {
        protected BasicChecker([NotNull] string name, bool performCleanupChecks) {
            Name = name;
            PerformCleanupChecks = performCleanupChecks;
        }

        [NotNull]
        private string Name { get; }
        protected bool PerformCleanupChecks { get; }
        protected abstract void Run([NotNull] Simulator sim);

        public void RunCheck([NotNull] Simulator sim, int step) {
            var start = DateTime.Now;
            Run(sim);
            var end = DateTime.Now;
            Logger.Info(
                "Database integrity check " + step + ": Checking " + Name + " took " +
                (end - start).TotalMilliseconds.ToString("0", CultureInfo.CurrentCulture) + " milliseconds");
        }
    }
}