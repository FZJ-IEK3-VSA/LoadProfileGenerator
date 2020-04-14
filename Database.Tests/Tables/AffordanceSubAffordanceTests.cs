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

using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

#endregion

namespace Database.Tests.Tables {
    [TestFixture]
    public class AffordanceSubAffordanceTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var desires = new ObservableCollection<Desire>();
            Desire.LoadFromDatabase(desires, db.ConnectionString, false);
            var subAffordances = new ObservableCollection<SubAffordance>();
            var locations = new ObservableCollection<Location>();
            var variables = new ObservableCollection<Variable>();
            SubAffordance.LoadFromDatabase(subAffordances, db.ConnectionString, desires, false, locations, variables);
            var affordances = new ObservableCollection<Affordance>();
            var affordanceSubAffordances =
                new ObservableCollection<AffordanceSubAffordance>();
            var tbp = new ObservableCollection<TimeBasedProfile>();
            var dateBasedProfiles = db.LoadDateBasedProfiles();
            var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
            var aic = new AllItemCollections(timeProfiles: tbp, timeLimits: timeLimits);
            DBBase.LoadAllFromDatabase(affordances, db.ConnectionString, Affordance.TableName, Affordance.AssignFields,
                aic, false, true);
            AffordanceSubAffordance.LoadFromDatabase(affordanceSubAffordances, db.ConnectionString, affordances,
                subAffordances, false);
            Assert.Greater(affordanceSubAffordances.Count, 1);
            db.ClearTable(AffordanceSubAffordance.TableName);
            affordanceSubAffordances.Clear();
            AffordanceSubAffordance.LoadFromDatabase(affordanceSubAffordances, db.ConnectionString, affordances,
                subAffordances, false);
            Assert.AreEqual(0, affordanceSubAffordances.Count);
            db.Cleanup();
        }
    }
}