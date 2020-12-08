using System;
using System.Globalization;
using Common;
using Database;

namespace CalculationController.Integrity {
    public abstract class BasicChecker {
        protected BasicChecker([JetBrains.Annotations.NotNull] string name, bool performCleanupChecks) {
            Name = name;
            PerformCleanupChecks = performCleanupChecks;
        }

        [JetBrains.Annotations.NotNull]
        private string Name { get; }
        protected bool PerformCleanupChecks { get; }
        protected abstract void Run([JetBrains.Annotations.NotNull] Simulator sim, CheckingOptions options);

        public void RunCheck([JetBrains.Annotations.NotNull] Simulator sim, int step, CheckingOptions options) {
            var start = DateTime.Now;
            Run(sim,options);
            var end = DateTime.Now;
            Logger.Info(
                "Database integrity check " + step + ": Checking " + Name + " took " +
                (end - start).TotalMilliseconds.ToString("0", CultureInfo.CurrentCulture) + " milliseconds");
        }
    }
}