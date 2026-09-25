using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.Helper;
using CalculationEngine.Transportation;
using Common;

namespace CalculationEngine.Activities
{
    public class DynamicTravelActivity(string dataSource, string personName, PointOfInterestId? destination, AffordanceBaseTransportDecoratorDynamic affordance,
        TravelInformation travelInfo, int? expectedDuration = null) : DynamicActivity(dataSource, personName, destination, expectedDuration)
    {
        public override string Name => TravelInfo.TravelName;

        public override bool IsDetermined => false;

        public override bool IsTravel => true;

        public override AffordanceBaseTransportDecoratorDynamic Affordance { get; } = affordance;

        public TravelInformation TravelInfo { get; } = travelInfo;


        public override void Start(TimeStep timestep, DayLightStatus dayLightStatus)
        {
            base.Start(timestep, dayLightStatus);
            TravelInfo.StartTravel(timestep, PersonName, Affordance);
        }

        public override bool IsFinished(TimeStep timestep, RemoteActivityFinished? remoteActivityResult)
        {
            if (StartTime is null)
                throw new LPGException("Activity has not been activated yet.");

            return remoteActivityResult is not null;
        }

        public override int Finish(TimeStep timestep, RemoteActivityFinished? remoteActivityResult)
        {
            int duration = base.Finish(timestep, remoteActivityResult);
            TravelInfo.FinishTravel(StartTime!, PersonName, duration, Affordance);
            return duration;
        }

        public override string GetStartThought()
        {
            return $"Starting to execute dynamic travel {Name} with unknown duration.";
        }
    }
}
