using Automation;
using Automation.ResultFiles;
using CalculationEngine.Activities;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;

namespace CalculationEngine.Transportation
{
    public class AffordanceBaseTransportDecoratorDynamic : AffordanceBaseTransportDecorator
    {
        public AffordanceBaseTransportDecoratorDynamic(ICalcAffordanceBase sourceAffordance, TransportationHandler transportationHandler, HouseholdKey householdkey, StrGuid guid, CalcRepo calcRepo)
            : base(sourceAffordance, transportationHandler, householdkey, guid, calcRepo)
        {
        }

        /// <summary>
        /// Creates a copy of the specified transport decorator, but replaces the source affordance with a new remote affordance.
        /// </summary>
        /// <param name="original">the original affordance transport decorator</param>
        /// <param name="remoteAffordance">the remote affordance that will be used as source affordance</param>
        public AffordanceBaseTransportDecoratorDynamic(AffordanceBaseTransportDecoratorDynamic original, CalcAffordanceRemote remoteAffordance)
            : base(remoteAffordance, original._transportationHandler, original._householdkey, StrGuid.New(), original._calcRepo)
        {
            var message = "Copying affordance base transport decorator for remote affordance " + remoteAffordance;
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(new TimeStep(0, 0, false), _householdkey, message));
        }

        protected override DynamicTravelActivity CreateActivity(CalcPersonDto activator, ICalcSite personSourceSite, CalcTravelRoute route, int travelDuration, IActivity firstSourceActivity)
        {
            var activationName = "Dynamic Travel Profile for Route " + route.Name + " to affordance " + SourceAffordance.Name;
            var destination = firstSourceActivity.Destination;
            return new DynamicTravelActivity(SourceAffordance.Name, activator.Name, destination, this, new(route, personSourceSite));
        }
    }
}
