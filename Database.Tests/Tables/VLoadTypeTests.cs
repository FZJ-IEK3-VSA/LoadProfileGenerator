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
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables {
    [TestFixture]
    public class VLoadTypeTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ConvertPowerValueWithTimeTest() {
            var vlt = new VLoadType("bla", string.Empty, "Watt", "kWh", 1000, 1, new TimeSpan(1, 0, 0), 1,
                string.Empty, LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());

            Assert.AreEqual(10, vlt.ConvertPowerValueWithTime(10000, new TimeSpan(1, 0, 0)));
            Assert.AreEqual(5, vlt.ConvertPowerValueWithTime(10000, new TimeSpan(0, 30, 0)));
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void VLoadTypeTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());

            db.ClearTable(VLoadType.TableName);
            var loadts = new ObservableCollection<VLoadType>();
            VLoadType.LoadFromDatabase(loadts, db.ConnectionString, false);
            Assert.AreEqual(0, loadts.Count);
            var loadType = new VLoadType("loadtype1", "blub", "bla", "blu", 1000, 1, new TimeSpan(1, 0, 0), 1,
                db.ConnectionString, LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            loadType.SaveToDB();
            VLoadType.LoadFromDatabase(loadts, db.ConnectionString, false);
            Assert.AreEqual(1, loadts.Count);
            var lt = loadts[0];
            Assert.That(lt.Name, Is.EqualTo("loadtype1"));
            Assert.AreEqual("blub", lt.Description );
            lt.DeleteFromDB();
            loadts.Clear();
            VLoadType.LoadFromDatabase(loadts, db.ConnectionString, false);
            Assert.AreEqual(0, loadts.Count);
            db.Cleanup();
        }
    }
}