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
                Print("Desires", sim.Desires.Items.Count);
                Print("Persons", sim.Persons.Items.Count);
                Print("Devices", sim.RealDevices.Items.Count);
                Print("Device Categories", sim.DeviceCategories.Items.Count);
                Print("Device Actions", sim.DeviceActions.Items.Count);
                Print("Device Action Groups", sim.DeviceActionGroups.Items.Count);
                Print("Time Profiles (für Geräte)", sim.Timeprofiles.Items.Count);
                Print("Time Limits", sim.TimeLimits.Items.Count);
                Print("Subaffordances", sim.SubAffordances.Items.Count);
                Print("Affordances", sim.Affordances.Items.Count);
                Print("Locations", sim.Locations.Items.Count);
                Print("Trait Tags", sim.TraitTags.Items.Count);
                Print("Household Traits", sim.HouseholdTraits.Items.Count);
                int hhcount = sim.ModularHouseholds.Items.Count(x => x.GeneratorID == null);
                Print("Modular Households", hhcount);
                Print("House Types", sim.HouseTypes.Items.Count);

                int housecount = sim.Houses.Items.Count(x => x.Source != null && !x.Source.ToLower(CultureInfo.CurrentCulture)
                                                              .Contains("generated"));
                Print("Houses", housecount);
            }
        }

        private static void Print([NotNull] string name, int count) => Logger.Info(name + " & " + count + " \\\\");
    }
}