using System.Threading;
using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using LoadProfileGenerator.Presenters.SpecialViews;
using NUnit.Framework;

namespace LoadProfileGenerator.Tests.Presenters.SpecialViews
{
    [TestFixture]
    public class CalculationOutcomesPresenterTests : UnitTestBaseClass
    {
        [Test]
        [Apartment(ApartmentState.STA)]
        [Category("BasicTest")]

        public void StartCalcOutcomesChart()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);

            Simulator sim = new Simulator(db.ConnectionString);
            CalculationOutcomesPresenter.MakeVersionComparisonChart(sim);
            db.Cleanup();
        }

        [Test]
        [Category("LongTest3")]
        public void StartOneCalculationTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            ModularHousehold hh = sim.ModularHouseholds.It[0];
            // Guid g = Guid.NewGuid();
            CalculationOutcomesPresenter.StartOneCalculation(hh, sim.GeographicLocations[0], sim.TemperatureProfiles[0],
                EnergyIntensityType.EnergySaving, wd.WorkingDirectory, sim,
                false,null,null,null);
            db.Cleanup();
            wd.CleanUp();
        }
    }
}