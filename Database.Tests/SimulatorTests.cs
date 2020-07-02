//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Globalization;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database.Tables;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


#endregion

namespace Database.Tests {

    public class SimulatorTests : UnitTestBaseClass
    {
        private static void LogStepProgress(ref DateTime lasttime, ref int step) {
            var now = DateTime.Now;
            Logger.Info(
                "Part " + step + ":" + (now - lasttime).TotalSeconds.ToString("0.000", CultureInfo.CurrentCulture) +
                " seconds");
            step++;
            lasttime = now;
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorAffordancesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Affordances.Items.Count;
                sim.Affordances.DeleteItem(sim.Affordances.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Affordances.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorCreateTest()
        {
            var step = 1;
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var dt = DateTime.Now;
                sim.Affordances.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Affordances.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Desires.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.DeviceCategories.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.TimeLimits.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Houses.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Locations.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Persons.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.RealDevices.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Settlements.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.SubAffordances.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.TemperatureProfiles.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.Timeprofiles.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.LoadTypes.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.HouseholdTraits.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                sim.HouseTypes.CreateNewItem(db.ConnectionString);
                LogStepProgress(ref dt, ref step);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorDateBasedProfilesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.DateBasedProfiles.Items.Count;
                sim.DateBasedProfiles.DeleteItem(sim.DateBasedProfiles.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.DateBasedProfiles.Items.Count.Should().Be(count-1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorDesiresDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Desires.Items.Count;
                sim.Desires.DeleteItem(sim.Desires.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Desires.Items.Count.Should().Be(count-1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorDeviceCategoriesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                // load to check if everything is ok
                var sim0 = new Simulator(db.ConnectionString);
                if (sim0.Persons.Items.Count == 0)
                {
                    throw new LPGException("0 persons!?");
                }
                // load for deleting one
                var sim = new Simulator(db.ConnectionString);
                var count = sim.DeviceCategories.Items.Count;
                sim.DeviceCategories.DeleteItem(sim.DeviceCategories.Items[sim.DeviceCategories.Items.Count - 1]);
                // load again to double check
                var sim2 = new Simulator(db.ConnectionString);
                sim2.DeviceCategories.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorEnergyStoragesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.EnergyStorages.Items.Count;
                sim.EnergyStorages.DeleteItem(sim.EnergyStorages.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.EnergyStorages.Items.Count.Should().Be(count-1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorGeneratorsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Generators.Items.Count;
                sim.Generators.DeleteItem(sim.Generators.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Generators.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorGeographicLocationsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.GeographicLocations.Items.Count;
                sim.GeographicLocations.DeleteItem(sim.GeographicLocations.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.GeographicLocations.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorHolidaysDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Holidays.Items.Count;
                sim.Holidays.DeleteItem(sim.Holidays.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Holidays.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorHouseholdsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.ModularHouseholds.Items.Count;
                sim.ModularHouseholds.DeleteItem(sim.ModularHouseholds.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.ModularHouseholds.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorHouseholdTraitsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.HouseholdTraits.Items.Count;
                sim.HouseholdTraits.DeleteItem(sim.HouseholdTraits.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.HouseholdTraits.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorHousesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Houses.Items.Count;
                sim.Houses.DeleteItem(sim.Houses.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Houses.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorLoadTypesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.LoadTypes.Items.Count;
                sim.LoadTypes.DeleteItem(sim.LoadTypes.Items[0]);
                sim.LoadTypes.DeleteItem(sim.LoadTypes.Items[1]);
                sim.LoadTypes.DeleteItem(sim.LoadTypes.Items[2]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.LoadTypes.Items.Count.Should().Be(count - 3);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void SimulatorLoadWithoutNeedsUpdateTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString); // load once to clear problems
                foreach (var plan in sim.HouseholdPlans.Items)
                {
                    plan.Refresh(null);
                }
                DBBase.NeedsUpdateAllowed = false;
                var sim2 = new Simulator(db.ConnectionString); // load again and see if it fails
                foreach (var plan in sim2.HouseholdPlans.Items)
                {
                    plan.Refresh(null);
                }
                DBBase.NeedsUpdateAllowed = true;
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorLocationsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var locationscount = sim.Locations.Items.Count;
                sim.Locations.DeleteItem(sim.Locations.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Locations.Items.Count.Should().Be(locationscount - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorPersonsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var personcount = sim.Persons.Items.Count;
                sim.Persons.DeleteItem(sim.Persons.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Persons.Items.Count.Should().Be(personcount - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorRealDevicesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.RealDevices.Items.Count;
                sim.RealDevices.DeleteItem(sim.RealDevices.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.RealDevices.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorSettlementsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Settlements.Items.Count;
                sim.Settlements.DeleteItem(sim.Settlements.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Settlements.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorTemperaturProfilesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.TemperatureProfiles.Items.Count;
                sim.TemperatureProfiles.DeleteItem(sim.TemperatureProfiles.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.TemperatureProfiles.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                sim.Should().NotBeNull();
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorTest2()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                sim.Should().NotBeNull();
                var emptycount = 0;
                foreach (var realDevice in sim.RealDevices.Items)
                {
                    if (realDevice.Loads.Count == 0)
                    {
                        emptycount++;
                        Logger.Info("no load:" + realDevice.Name);
                    }
                    if (string.IsNullOrEmpty(realDevice.Description))
                    {
                        emptycount++;
                        Logger.Info("no description:" + realDevice.Name);
                    }
                }
                Logger.Info(emptycount.ToString(CultureInfo.CurrentCulture));
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorTimeLimitsDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.TimeLimits.Items.Count;
                sim.TimeLimits.DeleteItem(sim.TimeLimits.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.TimeLimits.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorTimeProfilesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.Timeprofiles.Items.Count;
                sim.Timeprofiles.DeleteItem(sim.Timeprofiles.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.Timeprofiles.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SimulatorTransformationDevicesDeleteTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var count = sim.TransformationDevices.Items.Count;
                sim.TransformationDevices.DeleteItem(sim.TransformationDevices.Items[0]);
                var sim2 = new Simulator(db.ConnectionString);
                sim2.TransformationDevices.Items.Count.Should().Be(count - 1);
                db.Cleanup();
            }
        }

        public SimulatorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}