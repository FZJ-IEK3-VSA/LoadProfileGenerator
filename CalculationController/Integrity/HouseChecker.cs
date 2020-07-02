using Common;
using Database;

namespace CalculationController.Integrity {
    internal class HouseChecker : BasicChecker {
        public HouseChecker(bool performCleanupChecks) : base("Houses", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {

            foreach (var house in sim.Houses.Items) {

                if (Config.AllowEmptyHouses) {
                    if (house.Households.Count == 0) {
                        throw new DataIntegrityException("The house " + house.PrettyName + " has no households. Please fix.", house);
                    }
                }

                bool anyTransport = false;
                foreach (var household in house.Households) {
                    if (household.ChargingStationSet != null || household.TravelRouteSet != null ||
                        household.TransportationDeviceSet != null) {
                        anyTransport = true;
                        break;
                    }
                }
                if (anyTransport) {
                    foreach (var household in house.Households) {
                        if (household.ChargingStationSet == null) {
                            throw new DataIntegrityException("The household " + household.Name + " in the house " + house.Name + " has no charging station set.", house) ;
                        }
                        if (household.TravelRouteSet == null)
                        {
                            throw new DataIntegrityException("The household " + household.Name + " in the house " + house.Name + " has no travel route set.", house);
                        }
                        if (household.TransportationDeviceSet == null)
                        {
                            throw new DataIntegrityException("The household " + household.Name + " in the house " + house.Name + " has no transportation device set.", house);
                        }
                    }
                }

                if (house.HouseType == null) {
                    throw new DataIntegrityException(
                        "The house " + house.PrettyName + " has no housetype. Please fix.", house);
                }
                if (house.GeographicLocation == null) {
                    throw new DataIntegrityException(
                        "The house " + house.PrettyName + " has no geographic Location. Please fix.", house);
                }
                if (house.TemperatureProfile == null) {
                    throw new DataIntegrityException(
                        "The house " + house.PrettyName + " has no temperature profile. Please fix.", house);
                }

                if (PerformCleanupChecks) {
                    foreach (var household in house.Households) {
                        if (household.ChargingStationSet == null) {
                            throw new DataIntegrityException(
                                "The household " + household.Name + " in the house " + house.Name + " has no charging station set.", house);
                        }

                        if (household.TravelRouteSet == null) {
                            throw new DataIntegrityException(
                                "The household " + household.Name + " in the house " + house.Name + " has no travel route set.", house);
                        }

                        if (household.TransportationDeviceSet == null) {
                            throw new DataIntegrityException(
                                "The household " + household.Name + " in the house " + house.Name + " has no transportation device set.", house);
                        }
                    }
                }
            }
        }
    }
}