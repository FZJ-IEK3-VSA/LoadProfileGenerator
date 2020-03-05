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

using System;
using System.Collections.ObjectModel;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using NUnit.Framework;

namespace Database.Tests.Tables {
    [TestFixture]
    public class DateBasedProfileTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void DateBasedProfileSaveAndRestore() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(DateBasedProfile.TableName);
            db.ClearTable(DateProfileDataPoint.TableName);
            var profiles = new ObservableCollection<DateBasedProfile>();
            DateBasedProfile.LoadFromDatabase(profiles, db.ConnectionString, false);
            Assert.AreEqual(0, profiles.Count);
            var dbp = new DateBasedProfile("tempprofil1", "desc1", db.ConnectionString, Guid.NewGuid().ToString());
            dbp.SaveToDB();
            dbp.AddNewDatePoint(new DateTime(2014, 1, 1), 15);
            dbp.AddNewDatePoint(new DateTime(2014, 2, 1), 15);
            dbp.SaveToDB();
            DateBasedProfile.LoadFromDatabase(profiles, db.ConnectionString, false);
            Assert.AreEqual(1, profiles.Count);
            Assert.AreEqual(2, profiles[0].Datapoints.Count);
            profiles[0].DeleteDatePoint(profiles[0].Datapoints[0]);
            Assert.AreEqual(1, profiles[0].Datapoints.Count);
            db.Cleanup();
        }
    }
}