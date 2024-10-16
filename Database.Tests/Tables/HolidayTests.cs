﻿//-----------------------------------------------------------------------

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
using Database.Tables.BasicElements;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables {

    public class HolidayTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HolidayProbabilitesCalculateTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                // test the normal loading
                var holidays = new ObservableCollection<Holiday>();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                // delete everything and check
                holidays.Clear();
                db.ClearTable(Holiday.TableName);
                db.ClearTable(HolidayDate.TableName);
                db.ClearTable(HolidayProbabilities.TableName);
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(0);
                // add one and load again
                var hd = new Holiday("my holiday", "blub", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                hd.SaveToDB();
                hd.ProbMonday.Tuesday = 100;
                hd.ProbMonday.Wednesday = 0;
                var holiday = new DateTime(2014, 2, 17);
                hd.AddNewDate(holiday);
                hd.SaveToDB();
                var r = new Random(1);
                var dts = hd.GetListOfWorkFreeDates(r, "test");
                (dts.Count).Should().Be(2);
                Assert.True(dts.ContainsKey(holiday));
                Assert.True(dts.ContainsKey(holiday.AddDays(1)));
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HolidayProbabilitesSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                // test the normal loading
                var holidays = new ObservableCollection<Holiday>();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                // delete everything and check
                holidays.Clear();
                db.ClearTable(Holiday.TableName);
                db.ClearTable(HolidayDate.TableName);
                db.ClearTable(HolidayProbabilities.TableName);
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(0);
                // add one and load again
                var hd = new Holiday("my holiday", "blub", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                hd.SaveToDB();
                hd.ProbMonday.Monday = 5;
                hd.SaveToDB();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(1);
                (holidays[0].ProbMonday.Monday).Should().Be(5);
                // delete the loaded one
                holidays[0].DeleteFromDB();
                holidays.Clear();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(0);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {

                // test the normal loading
                var holidays = new ObservableCollection<Holiday>();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                // delete everything and check
                holidays.Clear();
                db.ClearTable(Holiday.TableName);
                db.ClearTable(HolidayDate.TableName);
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(0);
                // add one and load again
                var hd = new Holiday("my holiday", "blub", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                hd.SaveToDB();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(1);
                // delete the loaded one
                holidays[0].DeleteFromDB();
                holidays.Clear();
                Holiday.LoadFromDatabase(holidays, db.ConnectionString, false);
                (holidays.Count).Should().Be(0);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CreateNewHolidays()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                sim.Holidays.CreateNewItem(sim.ConnectionString);

                db.Cleanup();
            }
        }

        public HolidayTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}