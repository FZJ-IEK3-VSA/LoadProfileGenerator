using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.BasicHouseholds
{
    [TestFixture]
    public class VacationTests : UnitTestBaseClass
    {
        [Test]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [Category("BasicTest")]
        public void VacationTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            db.ClearTable(Vacation.TableName);
            db.ClearTable(VacationTime.TableName);
            Vacation vac = new Vacation("name", null, db.ConnectionString, 1, 99,CreationType.ManuallyCreated, Guid.NewGuid().ToString());
            DateTime dt = new DateTime(2010, 1, 1);
            vac.SaveToDB();
            vac.AddVacationTime(dt, dt,VacationType.GoAway);
            ObservableCollection<Vacation> vacations = new ObservableCollection<Vacation>();
            Vacation.LoadFromDatabase(vacations, db.ConnectionString, false);
            Assert.AreEqual(1, vacations.Count);
            Vacation v1 = vacations[0];
            Assert.AreEqual("name", v1.Name);
            Assert.AreEqual(1, v1.VacationTimes.Count);
            VacationTime vt = v1.VacationTimes[0];
            Assert.AreEqual(dt, vt.Start);
            Assert.AreEqual(dt, vt.End);
            v1.DeleteVacationTime(vt);
#pragma warning disable S1854 // Dead stores should be removed
#pragma warning disable IDE0059 // Value assigned to symbol is never used
            v1 = null; // to enable garbage collection immidiately
#pragma warning restore IDE0059 // Value assigned to symbol is never used
#pragma warning restore S1854 // Dead stores should be removed
            ObservableCollection<Vacation> vacations1 = new ObservableCollection<Vacation>();
            Vacation.LoadFromDatabase(vacations1, db.ConnectionString, false);
            Assert.AreEqual(1, vacations.Count);
            Vacation v2 = vacations[0];
            Assert.AreEqual("name", v2.Name);
            Assert.AreEqual(0, v2.VacationTimes.Count);

            db.Cleanup();
        }
    }
}