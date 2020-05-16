using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicHouseholds
{

    public class VacationTests : UnitTestBaseClass
    {
        [Fact]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void VacationTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Vacation.TableName);
                db.ClearTable(VacationTime.TableName);
                Vacation vac = new Vacation("name", null, db.ConnectionString, 1, 99, CreationType.ManuallyCreated, Guid.NewGuid().ToStrGuid());
                DateTime dt = new DateTime(2010, 1, 1);
                vac.SaveToDB();
                vac.AddVacationTime(dt, dt, VacationType.GoAway);
                ObservableCollection<Vacation> vacations = new ObservableCollection<Vacation>();
                Vacation.LoadFromDatabase(vacations, db.ConnectionString, false);
                (vacations.Count).Should().Be(1);
                Vacation v1 = vacations[0];
                (v1.Name).Should().Be("name");
                (v1.VacationTimes.Count).Should().Be(1);
                VacationTime vt = v1.VacationTimes[0];
                (vt.Start).Should().Be(dt);
                (vt.End).Should().Be(dt);
                v1.DeleteVacationTime(vt);
#pragma warning disable S1854 // Dead stores should be removed
#pragma warning disable IDE0059 // Value assigned to symbol is never used
                v1 = null; // to enable garbage collection immidiately
#pragma warning restore IDE0059 // Value assigned to symbol is never used
#pragma warning restore S1854 // Dead stores should be removed
                ObservableCollection<Vacation> vacations1 = new ObservableCollection<Vacation>();
                Vacation.LoadFromDatabase(vacations1, db.ConnectionString, false);
                (vacations.Count).Should().Be(1);
                Vacation v2 = vacations[0];
                (v2.Name).Should().Be("name");
                (v2.VacationTimes.Count).Should().Be(0);

                db.Cleanup();
            }
        }

        public VacationTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}