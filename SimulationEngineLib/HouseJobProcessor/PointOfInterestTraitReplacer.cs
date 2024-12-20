#nullable enable

using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using PowerArgs;

namespace SimulationEngineLib.HouseJobProcessor
{
    /// <summary>
    /// Stores information on a location replacement that needs to be carried out within traits. This replacement is for a single
    /// point of interest. Each point of interest is specified via a reference location. If applied, occurrences of the reference location
    /// within the traits of a household must be replaced with the new location. There can be multiple replacements with the same reference
    /// location. Which of those are applied depends on the POI preferences of the person.
    /// </summary>
    /// <param name="PointOfInterestId">the ID of the point of interest that this replacement was created for</param>
    /// <param name="ReferenceLocation">the original location that needs to be replaced</param>
    /// <param name="NewLocation">the new location to replace the reference location in traits</param>
    /// <param name="NewSite">a dedicated new site for the new location</param>
    /// <param name="Timelimit">an optional timelimit for all affordances at the new location</param>
    record PoiLocationReplacement(string PointOfInterestId, Location ReferenceLocation, Location NewLocation, Site NewSite, TimeLimit? Timelimit = null);

    /// <summary>
    /// A PoiLocationReplacement, but with an additional weight from the POI preferences of a person. This is relevant if multiple POIs of the
    /// same type (same reference location) are used by the same person.
    /// </summary>
    /// <param name="Replacement">the normal POI replacement</param>
    /// <param name="Weight">the weight for selection of the POI of this replacement</param>
    record WheightedPoiLocationReplacement(PoiLocationReplacement Replacement, double Weight);

    /// <summary>
    /// Handles integration of points of interest for the city simulation.
    /// Creates a new location and site for each POI, and replaces the specified reference locations with them in
    /// all affected traits.
    /// </summary>
    internal class PointOfInterestTraitReplacer
    {
        /// <summary>
        /// Database access object
        /// </summary>
        private readonly Simulator sim;

        /// <summary>
        /// Stores which location must be replaced with which new one, for each point of interest
        /// separately. Uses the POI-ID as key.
        /// </summary>
        public IReadOnlyDictionary<string, PoiLocationReplacement> LocationReplacements { get; }

        /// <summary>
        /// Creates a new PointOfInterestTraitReplacer
        /// </summary>
        /// <param name="sim">database access object</param>
        /// <param name="cityData">city data from the house job</param>
        internal PointOfInterestTraitReplacer(Simulator sim, CityData cityData)
        {
            this.sim = sim;
            LocationReplacements = CreateRemoteLocations(cityData);
        }

        /// <summary>
        /// Creates a dedicated new location and a new site for each point of interested specified in the CityData object.
        /// Each new site only contains the newly created location.
        /// </summary>
        /// <param name="cityData">city data from the house job which defines the required points of interests</param>
        /// <returns>a dictionary mapping each POI-ID to the corresponding location replacement</returns>
        private Dictionary<string, PoiLocationReplacement> CreateRemoteLocations(CityData cityData)
        {
            Dictionary<string, PoiLocationReplacement> locationReplacements = [];
            // get the specified location and timelimit objects for each entry
            foreach (var entry in cityData.PointsOfInterest)
            {
                var poiId = entry.Key;
                var referenceLocation = sim.Locations.FindWithException(entry.Value.LocationType);
                var timelimit = sim.TimeLimits.FindWithException(entry.Value.TimeLimit, true);

                // create and add the new location
                var location = sim.Locations.CreateNewItem(sim.ConnectionString);
                location.Name = $"{referenceLocation.Name} ({poiId})";
                location.SaveToDB();

                // create a new site that only contains the new location
                var site = sim.Sites.CreateNewItem(sim.ConnectionString);
                site.Name = poiId;
                site.Description = "Generated site for a single point of interest";
                site.AddLocation(location);
                site.SaveToDB();

                locationReplacements[poiId] = new PoiLocationReplacement(entry.Key, referenceLocation, location, site, timelimit);
            }
            return locationReplacements;
        }

        /// <summary>
        /// Replaces all traits in a household that use a location for which a specific new POI location should now be used instead.
        /// </summary>
        /// <param name="household">the household whose traits should be replaced</param>
        /// <param name="travelPreferences">the POI preferences of all persons in the household</param>
        /// <exception cref="LPGException">if travel preferences for a person are missing</exception>
        internal void ReplaceTraitsInHousehold(ModularHousehold household, Dictionary<string, PersonPoiPreferences> travelPreferences)
        {
            // create new adapted traits
            foreach (var person in household.Persons)
            {
                if (!travelPreferences.TryGetValue(person.PrettyName, out var personTravelPreferences))
                    throw new LPGException($"Missing travel preferences for {person.PrettyName} in household {household.Name}");

                // get a lookup object mapping each location that needs to be replaced in this person's traits to all new locations that will replace it
                var locationsToReplace = personTravelPreferences.PoiWeights.Select(poi => new WheightedPoiLocationReplacement(LocationReplacements[poi.Key], poi.Value))
                    .ToLookup(x => x.Replacement.ReferenceLocation);

                // replace all affected traits
                HashSet<HouseholdTrait> newTraits = [];
                HashSet<ModularHouseholdTrait> traitEntriesToDelete = [];
                foreach (var traitEntry in household.Traits)
                {
                    // check if this trait entry is relevant for the current person
                    if (traitEntry.DstPerson != person.Person)
                        continue;

                    var originalTrait = traitEntry.HouseholdTrait;
                    // replace all traits that contain at least one of the locations to replace
                    if (originalTrait.Locations.Any(x => locationsToReplace.Contains(x.Location)))
                    {
                        var newTrait = CreateReplacementTrait(originalTrait, locationsToReplace);
                        newTraits.Add(newTrait);
                        traitEntriesToDelete.Add(traitEntry);
                    }
                }
                // remove the replaced trait entries
                foreach (var entryToDelete in traitEntriesToDelete)
                {
                    household.DeleteTraitFromDB(entryToDelete);
                }
                // add the new traits to the household for the current person
                foreach (var newTrait in newTraits)
                {
                    household.AddTrait(newTrait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, person.Person);
                }
            }
            CheckTraitLocations(sim, household);
        }

        /// <summary>
        /// Creates a new trait as replacement that uses new POI locations instead of the existing generic locations.
        /// If a person visits multiple POIs for the same location type, the new trait will contain all of them with the
        /// respective weights.
        /// </summary>
        /// <param name="originalTrait">the original trait that will be replaced</param>
        /// <param name="locationsToReplace">a lookup object providing all weighted replacements for each location</param>
        /// <returns>a new trait that uses the new POI locations</returns>
        private HouseholdTrait CreateReplacementTrait(HouseholdTrait originalTrait, ILookup<Location, WheightedPoiLocationReplacement> locationsToReplace)
        {
            // create the new trait
            var newTrait = originalTrait.MakeCopy(sim);
            newTrait.Name = originalTrait.Name + " - adapted for CitySimulation";
            newTrait.Description = $"Automatically generated as a copy of trait {originalTrait.Name}, but with new POI locations";
            // this trait should not be used when generating other households from templates
            newTrait.CanBeUsedForNewHouseholds = false;

            // replace all location entries of the trait as required
            var copyForIterating = newTrait.Locations.ToList();
            foreach (var locationEntry in copyForIterating)
            {
                var locationReplacements = locationsToReplace[locationEntry.Location];
                if (locationReplacements.IsNullOrEmpty())
                {
                    // this location does not need to be replaced
                    continue;
                }

                // add all new POI locations instead
                foreach (var weightedRepl in locationReplacements)
                {
                    var newLocationEntry = newTrait.AddLocation(weightedRepl.Replacement.NewLocation);
                    // add all affordances of the removed location for each new location
                    foreach (var affordance in locationEntry.AffordanceLocations)
                    {
                        newTrait.AddAffordanceToLocation(newLocationEntry, affordance.Affordance, weightedRepl.Replacement.Timelimit, weightedRepl.Weight, 0, 0, 0, 0);
                    }
                }
                // remove the original location entry
                newTrait.DeleteHHTLocationFromDB(locationEntry);
            }
            newTrait.SaveToDB();
            return newTrait;
        }

        /// <summary>
        /// Checks if all locations that are used in the traits of a household either belong to the site "Home" or have 
        /// been replaced with new ones for the city simulation.
        /// </summary>
        /// <param name="sim">database access object</param>
        /// <param name="household">the household whose locations to check</param>
        /// <exception cref="LPGPBadParameterException">if the household contains invalid locations for a city simulation</exception>
        public void CheckTraitLocations(Simulator sim, ModularHousehold household)
        {
            var homeSite = TravelRouteSetBuilderFromPersonData.GetHomeSite(sim);
            var homeLocations = homeSite.Locations.Select(loc => loc.Location).ToHashSet();
            var newCityLocations = LocationReplacements.Select(kvp => kvp.Value.NewLocation).ToHashSet();
            foreach (var trait in household.Traits)
            {
                foreach (var locationEntry in trait.HouseholdTrait.Locations)
                {
                    var location = locationEntry.Location;
                    if (!newCityLocations.Contains(location) && !homeLocations.Contains(location))
                    {
                        throw new LPGPBadParameterException($"Invalid PointOfInterestPreferences: no point of interest for reference location {location} " +
                            $"was specified, although this location is used in household {household.Name}");
                    }
                }
            }
        }
    }
}
