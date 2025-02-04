namespace Automation
{
    public record RouteDataTransportModel(string origin_id, string destination_id, double logsum_time, double logsum_util_all, ModeWeights mode_times,
        ModeWeights prob_all, ModeWeights prob_with_car_hh, ModeWeights prob_with_car_hh_pt_abo, ModeWeights prob_with_car_hh_no_pt_abo,
        ModeWeights prob_no_car_hh, ModeWeights prob_no_car_hh_pt_abo, ModeWeights prob_no_car_hh_no_pt_abo);

    public record ModeWeights(double car, double bicycle, double walk, double pt);
}
