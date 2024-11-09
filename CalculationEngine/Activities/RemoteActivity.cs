using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;
using Common;


namespace CalculationEngine.Activities
{

    public class RemoteActivity(string dataSource, string personName, PointOfInterestId? destination, CalcAffordanceRemote affordance)
        : DynamicActivity(dataSource, personName, destination)
    {
        public override bool IsTravel => false;

        public override CalcAffordanceRemote Affordance => affordance;
    }
}
