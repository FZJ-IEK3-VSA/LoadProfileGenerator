using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Contains information on travel times and distances between two sites (POIs or houses) in the city.
    /// The information is available for multiple modes and for multiple household characteristics (e.g., with
    /// and without a car).
    /// Origin and destination IDs can also be cluster IDs. In that case, they need to be mapped using a separately
    /// provided mapping.
    /// </summary>
    /// <param name="origin_id">building ID of the origin site, or origin cluster</param>
    /// <param name="destination_id">building ID of the destination site, or destination cluster</param>
    /// <param name="mode_times">travel times per mode in minutes</param>
    /// <param name="mode_distances">travel distances per mode in km</param>
    /// <param name="prob_all">general mode choice probabilities</param>
    /// <param name="prob_with_car_hh">general mode choice probabilities if the household has a car</param>
    /// <param name="prob_with_car_hh_pt_abo">mode choice probabilities if the household has a car and public transport abo</param>
    /// <param name="prob_with_car_hh_no_pt_abo">mode choice probabilities if the household has a car, but no public transport abo</param>
    /// <param name="prob_no_car_hh">general mode choice probabilities if the household has no car</param>
    /// <param name="prob_no_car_hh_pt_abo">mode choice probabilities if the household has no car, but a public transport abo</param>
    /// <param name="prob_no_car_hh_no_pt_abo">mode choice probabilities if the household has no car and no public transport abo</param>
    /// <param name="index">an optional index to identify this RouteData object</param>
    public record RouteData(string origin_id, string destination_id, Dictionary<string, double?> mode_times,
        Dictionary<string, double?> mode_distances, Dictionary<string, double> prob_all,
        Dictionary<string, double> prob_with_car_hh, Dictionary<string, double> prob_with_car_hh_pt_abo,
        Dictionary<string, double> prob_with_car_hh_no_pt_abo, Dictionary<string, double> prob_no_car_hh,
        Dictionary<string, double> prob_no_car_hh_pt_abo, Dictionary<string, double> prob_no_car_hh_no_pt_abo,
        int index = -1);
}
