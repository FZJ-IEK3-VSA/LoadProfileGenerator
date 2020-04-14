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

using System.Collections.ObjectModel;
using Automation;
using CalculationController.DtoFactories;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicElements;
using NUnit.Framework;

namespace Database.Tests.Tables {
    [TestFixture]
    public class GeographicLocationTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitLoadCreationAndSaveTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());

            db.ClearTable(GeographicLocation.TableName);
            db.ClearTable(GeographicLocHoliday.TableName);
            var holidays = db.LoadHolidays();
            var geolocs = new ObservableCollection<GeographicLocation>();
            var dbp = db.LoadDateBasedProfiles();
            var timeLimits = db.LoadTimeLimits(dbp);
            GeographicLocation.LoadFromDatabase(geolocs, db.ConnectionString, holidays, timeLimits, false);
            Assert.AreEqual(0, geolocs.Count);
            var geoloc = new GeographicLocation("bla", 1, 2, 3, 4, 5, 6, "North", "West",
                db.ConnectionString, timeLimits[0], System.Guid.NewGuid().ToString());
            geoloc.SaveToDB();
            geoloc.AddHoliday(holidays[0]);
            GeographicLocation.LoadFromDatabase(geolocs, db.ConnectionString, holidays, timeLimits, false);
            Assert.AreEqual(1, geolocs.Count);
            Assert.AreEqual(1, geolocs[0].Holidays.Count);
            var gl = geolocs[0];
            Assert.AreEqual(gl.Name, "bla");
            gl.DeleteFromDB();
            geolocs.Clear();
            GeographicLocation.LoadFromDatabase(geolocs, db.ConnectionString, holidays, timeLimits, false);
            Assert.AreEqual(0, geolocs.Count);

            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void GeographicLocationTypoTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            var pars = CalcParametersFactory.MakeGoodDefaults();
            foreach (GeographicLocation location in sim.GeographicLocations.It) {
                Logger.Info("Calculating " + location.PrettyName);
                SunriseTimes st = new SunriseTimes(location);
                st.MakeArray(pars.InternalTimesteps, pars.InternalStartTime, pars.InternalEndTime,
                    pars.InternalStepsize);
            }
            db.Cleanup();
        }
    }
}