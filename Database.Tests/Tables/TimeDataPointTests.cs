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
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

#endregion

namespace Database.Tests.Tables {
    [TestFixture]
    public class TimeDataPointTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest()
        {
            // tests loading
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var datapoints = new ObservableCollection<TimeDataPoint>();
                TimeDataPoint.LoadFromDatabase(datapoints, db.ConnectionString, false);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeDataPointTest()
        {
            // tests saving and loading
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeDataPoint.TableName);
                var tp = new TimeDataPoint(new DateTime(2010, 1, 1), 1, null, 1, db.ConnectionString,
                    Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp = new TimeDataPoint(new TimeSpan(2010, 1, 1), 1, null, 1,
                    db.ConnectionString, Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                var datapoints = new ObservableCollection<TimeDataPoint>();
                TimeDataPoint.LoadFromDatabase(datapoints, db.ConnectionString, false);
                Assert.AreEqual(datapoints.Count, 2);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeDataPointTestTimespan()
        {
            // tests init with time span and saving and loading to db
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeDataPoint.TableName);
                var tp = new TimeDataPoint(new TimeSpan(0, 1, 0),
                    1, null, 1, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                Assert.AreEqual(1, tp.Time.TotalMinutes);
                var datapoints = new ObservableCollection<TimeDataPoint>();
                TimeDataPoint.LoadFromDatabase(datapoints, db.ConnectionString, false);
                Assert.AreEqual(datapoints.Count, 1);
                Assert.AreEqual(1, datapoints[0].Time.TotalMinutes);
                db.Cleanup();
            }
        }

        public TimeDataPointTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}