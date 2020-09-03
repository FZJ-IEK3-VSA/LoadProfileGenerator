using System.Collections.Generic;
using Common;
using Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class DesireChecker : BasicChecker {
        public DesireChecker(bool performCleanupChecks) : base("Desire", performCleanupChecks) {
        }

        private void CheckDesireNamesForSlash([NotNull] Simulator sim) {
            if (!PerformCleanupChecks) {
                return;
            }
            {
                var count = 0;
                var elements = new List<BasicElement>();
                foreach (var desire in sim.Desires.Items) {
                    if (!desire.Name.Contains(" / ")) {
                        elements.Add(desire);
                        count++;
                    }
                    if (count > 20) {
                        throw new DataIntegrityException("The opened desires don't have a ' / '. Please fix.",
                            elements);
                    }
                }
                if (elements.Count > 0) {
                    throw new DataIntegrityException("The opened desires don't have a ' / '. Please fix.", elements);
                }
            }
        }

        private void CheckDesireUsage([NotNull] Simulator sim) {
            if (!PerformCleanupChecks) {
                return;
            }
            var useddesires = new List<Desire>();
            foreach (var affordance in sim.Affordances.Items) {
                foreach (var desire in affordance.AffordanceDesires) {
                    useddesires.Add(desire.Desire);
                }
            }
            foreach (var subaff in sim.SubAffordances.Items) {
                foreach (var desire in subaff.SubAffordanceDesires) {
                    useddesires.Add(desire.Desire);
                }
            }

            for (var i = 0; i < sim.Desires.Items.Count; i++) {
                var desire = sim.Desires[i];
                if (!useddesires.Contains(desire)) {
                    var mbr =
                        MessageWindowHandler.Mw.ShowYesNoMessage(
                            "The desire " + desire.PrettyName + " is not used anywhere. Delete?", "Delete?");
                    if (mbr == LPGMsgBoxResult.Yes) {
                        sim.Desires.DeleteItem(desire);
                        i = 0;
                    }
                    else {
                        throw new DataIntegrityException(
                            "It seems the desire " + desire + " is not used in a single affordance. Please fix.",
                            desire);
                    }
                }
            }
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            CheckDesireNamesForSlash(sim);
            CheckDesireUsage(sim);
        }
    }
}