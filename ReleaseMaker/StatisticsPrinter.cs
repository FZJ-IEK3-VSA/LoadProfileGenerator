using System.Globalization;
using System.Linq;
using Common;
using Database;
using Database.Tests;
using JetBrains.Annotations;

namespace ReleaseMaker
{
    public class StatisticsPrinter
    {
        public void MakeStatistic()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                Print("Desires", sim.Desires.It.Count);
                Print("Persons", sim.Persons.It.Count);
                Print("Devices", sim.RealDevices.It.Count);
                Print("Device Categories", sim.DeviceCategories.It.Count);
                Print("Device Actions", sim.DeviceActions.It.Count);
                Print("Device Action Groups", sim.DeviceActionGroups.It.Count);
                Print("Time Profiles (für Geräte)", sim.Timeprofiles.It.Count);
                Print("Time Limits", sim.TimeLimits.It.Count);
                Print("Subaffordances", sim.SubAffordances.It.Count);
                Print("Affordances", sim.Affordances.It.Count);
                Print("Locations", sim.Locations.It.Count);
                Print("Trait Tags", sim.TraitTags.It.Count);
                Print("Household Traits", sim.HouseholdTraits.It.Count);
                int hhcount = sim.ModularHouseholds.It.Count(x => x.GeneratorID == null);
                Print("Modular Households", hhcount);
                Print("House Types", sim.HouseTypes.It.Count);

                int housecount = sim.Houses.It.Count(x => x.Source != null && !x.Source.ToLower(CultureInfo.CurrentCulture)
                                                              .Contains("generated"));
                Print("Houses", housecount);
            }
        }

        private static void Print([NotNull] string name, int count) => Logger.Info(name + " & " + count + " \\\\");
    }
}