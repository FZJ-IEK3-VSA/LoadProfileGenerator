using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.SQLResultLogging.Loggers;

namespace CalculationEngine.Transportation
{
    internal class AffordanceBaseTransportDecoratorDynamic : AffordanceBaseTransportDecorator
    {
        public AffordanceBaseTransportDecoratorDynamic(ICalcAffordanceBase sourceAffordance, TransportationHandler transportationHandler, string name, HouseholdKey householdkey, StrGuid guid, CalcRepo calcRepo)
            : base(sourceAffordance, transportationHandler, name, householdkey, guid, calcRepo)
        {
        }

        /// <summary>
        /// Creates a copy of the specified transport decorator, but replaces the source affordance with a new remote affordance.
        /// </summary>
        /// <param name="original">the original affordance transport decorator</param>
        /// <param name="remoteAffordance">the remote affordance that will be used as source affordance</param>
        public AffordanceBaseTransportDecoratorDynamic(AffordanceBaseTransportDecoratorDynamic original, CalcAffordanceRemote remoteAffordance)
            : base(remoteAffordance, original._transportationHandler, original.Name, original._householdkey, StrGuid.New(), original._calcRepo)
        {
            var message = "Copying affordance base transport decorator for remote affordance " + remoteAffordance;
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(new TimeStep(0, 0, false), _householdkey, message));
        }

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation,
            out IAffordanceActivation activationInfo)
        {
            if (_myLastTimeEntry.TimeOfLastEvalulation != startTime)
            {
                throw new LPGException("trying to activate without first checking if the affordance is busy is a bug. Please report.");
            }

            // TODO: this condition is not sufficient with the current CalcSites and Locations in the LPG (multiple locations all in the same "Event Location" site)
            if (personSourceLocation.CalcSite == SourceAffordance.Site)
            {
                // no transport is necessary - simply activate the source affordance
                SourceAffordance.Activate(startTime, activatorName, personSourceLocation, out activationInfo);
                return;
            }


            // get the route which was already determined in IsBusy and activate it
            CalcTravelRoute route = _myLastTimeEntry.PreviouslySelectedRoutes[personSourceLocation];
            int routeduration = route.Activate(startTime, activatorName, out var usedDeviceEvents, _transportationHandler.DeviceOwnerships);
            // TODO: probably with full transport simulation, the route will not be activated here, but step by step in CalcPerson

            // log transportation info
            string status;
            if (routeduration == 0)
            {
                status = $"\tActivating {Name} at {startTime} with no transportation and moving from {personSourceLocation} to "
                    + $"{SourceAffordance.ParentLocation.Name} for affordance {SourceAffordance.Name}";
            }
            else
            {
                status = $"\tActivating {Name} at {startTime} with a transportation duration of {routeduration} for moving from "
                    + $"{personSourceLocation} to {SourceAffordance.ParentLocation.Name}";
            }
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(startTime, _householdkey, status));


            // person has to travel to the target site with an unknown duration - cannot activate the source affordance yet
            var activationName = "Dynamic Travel Profile for Route " + route.Name + " to affordance " + SourceAffordance.Name;
            // determine the travel target: the POI of the affordance if it is remote, else null (for home)
            var destination = SourceAffordance is CalcAffordanceRemote remoteAff ? remoteAff.Site.PointOfInterest : null;
            activationInfo = new RemoteAffordanceActivation(activationName, SourceAffordance.Name, startTime, destination, route, personSourceLocation.CalcSite, this);
        }
    }
}
