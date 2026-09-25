using CalculationEngine.CitySimulation;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common;

namespace CalculationEngine.Activities
{

    public class StaticTravelActivity : StaticActivity
    {
        public StaticTravelActivity(string personName, CalcProfile calcProfile, AffordanceBaseTransportDecorator affordance, TravelInformation travelInfo)
            : base(personName, calcProfile)
        {
            Affordance = affordance;
            TravelInfo = travelInfo;
        }

        public override string Name => TravelInfo.TravelName;

        public override bool IsTravel => true;

        public override AffordanceBaseTransportDecorator Affordance { get; }

        public TravelInformation TravelInfo { get; }


        public override void Start(TimeStep timestep, DayLightStatus dayLightStatus)
        {
            base.Start(timestep, dayLightStatus);
            TravelInfo.StartTravel(timestep, PersonName, Affordance);
        }

        public override int Finish(TimeStep timestep, RemoteActivityFinished? remoteActivityResult)
        {
            int duration = base.Finish(timestep, remoteActivityResult);
            TravelInfo.FinishTravel(StartTime!, PersonName, duration, Affordance);
            return duration;
        }
    }
}
