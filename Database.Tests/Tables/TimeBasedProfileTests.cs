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
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


#endregion

namespace Database.Tests.Tables {

    public class TimeBasedProfileTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var tps = new ObservableCollection<TimeBasedProfile>();
                TimeBasedProfile.LoadFromDatabase(tps, db.ConnectionString, false);
                tps.Count.Should().BeGreaterThan( 1);
                db.ClearTable(TimeBasedProfile.TableName);
                db.ClearTable(TimeDataPoint.TableName);
                tps.Clear();
                var tp = new TimeBasedProfile("hey", null, db.ConnectionString, TimeProfileType.Absolute,
                    "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(1, 0, 0), 1);
                TimeBasedProfile.LoadFromDatabase(tps, db.ConnectionString, false);

                (tps.Count).Should().Be(1);
                (tps[0].TimeProfileType).Should().Be(TimeProfileType.Absolute);
                (tps[0].ObservableDatapoints.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        public TimeBasedProfileTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}