using Automation;
using Database;
using Database.Tables.Transportation;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace SimulationEngineLib.HouseJobProcessor
{
    /// <summary>
    /// Builds a new travel route set based on the defined points of interests and
    /// the POI preferences of each person.
    /// </summary>
    internal class TravelRouteSetBuilderCity(Simulator simulator)
    {
        private readonly Simulator sim = simulator;

        /// <summary>
        /// Checks if all required data is given to create a travel route set based
        /// on the point of interest preferences of each person.
        /// </summary>
        /// <param name="householdData">the HouseholdData object</param>
        /// <returns>true if all data is available; otherwise, false</returns>
        public static bool IsRequiredDataAvailable(HouseholdData householdData)
        {
            return householdData.PointOfInterestPreferences is not null;
        }

        internal TravelRouteSet CreateTravelRouteSetFromPoiPreferences(HouseholdData householdData)
        {
            foreach (var preference in householdData.PointOfInterestPreferences)
            {
                string personName = preference.Key;
                //if (sim.Persons)
            }
            return null;
        }
    }
}
