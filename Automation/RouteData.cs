using System.Collections.Generic;

namespace Automation
{
    public record RouteData(string origin_id, string destination_id, Dictionary<string, double> mode_times,
        Dictionary<string, double> mode_distances, Dictionary<string, double> prob_all,
        Dictionary<string, double> prob_with_car_hh, Dictionary<string, double> prob_with_car_hh_pt_abo,
        Dictionary<string, double> prob_with_car_hh_no_pt_abo, Dictionary<string, double> prob_no_car_hh,
        Dictionary<string, double> prob_no_car_hh_pt_abo, Dictionary<string, double> prob_no_car_hh_no_pt_abo);
}
