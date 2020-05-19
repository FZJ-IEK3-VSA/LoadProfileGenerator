using System;
using System.Globalization;
using Common;
using Common.Enums;
using Database;
using Database.Tables.Houses;

namespace CalculationController.Integrity {
    internal class SettlementChecker : BasicChecker {
        public SettlementChecker(bool performCleanupChecks) : base("Settlements", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var settlement in sim.Settlements.It) {
                if (settlement.Name.ToLower(CultureInfo.CurrentCulture).StartsWith("diss", StringComparison.Ordinal)) {
                    if (settlement.Households.Count != 100) {
                        throw new DataIntegrityException(
                            "The diss household " + settlement.Name + " does not have exactly 100 houses. Please fix!",
                            settlement);
                    }
                    foreach (var household in settlement.Households) {
                        if(household.CalcObject == null) {
                            throw new DataIntegrityException("Calcobject was null");
                        }
                        if (household.CalcObjectType != CalcObjectType.House) {
                            throw new DataIntegrityException("Not a house:" + household.CalcObject.Name, settlement);
                        }
                        var house = (House) household.CalcObject;
                        foreach (var houseHousehold in house.Households) {
                            if (houseHousehold.CalcObject?.Name.Contains("OR")==true) {
                                throw new DataIntegrityException("Office in :" + household.CalcObject.Name, settlement);
                            }
                        }
                    }
                }

                if (PerformCleanupChecks) {
                    if (settlement.Households.Count == 0) {
                        throw new DataIntegrityException("The settlement " + settlement.Name + " is empty. Please fix.", settlement);
                    }
                }

                foreach (var settlement1 in sim.Settlements.It) {
                    if (settlement != settlement1 && settlement.Name == settlement1.Name) {
                        throw new DataIntegrityException("The settlement " + settlement.Name +
                                                         " seems to exist twice. This is not right.");
                    }
                }
                foreach (var household1 in settlement.Households) {
                    foreach (var household2 in settlement.Households) {
                        if (household1 != household2 && household1.CalcObject == household2.CalcObject) {
                            throw new DataIntegrityException("The settlement " + settlement.Name +
                                                             " has the household " +
                                                             household1.CalcObject?.Name +
                                                             " twice. This is not right. Please fix.");
                        }
                    }
                }
            }
        }
    }
}