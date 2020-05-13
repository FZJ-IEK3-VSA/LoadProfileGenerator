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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class Person : DBBaseElement {

        public const string TableName = "tblPersons";

        private int _age;
        private int _averageSicknessDuration;
        [NotNull] private string _description;
        private PermittedGender _gender;
        private int _sickDays;

        public Person([NotNull] string name, int age, [CanBeNull]int? pID, int sickDays, int averageSicknessDuration, PermittedGender gender,
            [NotNull] string connectionString, [NotNull] string description, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            _age = age;
            ID = pID;
            _sickDays = sickDays;
            _averageSicknessDuration = averageSicknessDuration;
            _gender = gender;
            TypeDescription = "Person";
            _description = description;
        }

        public int Age {
            get => _age;
            [UsedImplicitly] set => SetValueWithNotify(value, ref _age, nameof(Age));
        }

        [UsedImplicitly]
        public int AverageSicknessDuration {
            get => _averageSicknessDuration;
            set => SetValueWithNotify(value, ref _averageSicknessDuration, nameof(AverageSicknessDuration));
        }

        [NotNull]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [UsedImplicitly]
        public PermittedGender Gender {
            get => _gender;
            set => SetValueWithNotify(value, ref _gender, nameof(Gender));
        }

        [UsedImplicitly]
        public override string PrettyName => Name + " (" + Age + "/" + Gender + ")";

        [UsedImplicitly]
        public int SickDays {
            get => _sickDays;
            set => SetValueWithNotify(value, ref _sickDays, nameof(SickDays));
        }

        [NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static Person AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id =  dr.GetIntFromLong("ID");
            var age = dr.GetInt("Age");
            var name = dr.GetString("Name","(no name)");
            var sickdays = 10;
            if (dr["SickDays"] != DBNull.Value) {
                sickdays =  dr.GetInt("SickDays");
            }
            var averageSicknessduration = 5;
            if (dr["AverageSicknessDuration"] != DBNull.Value) {
                averageSicknessduration = dr.GetInt("AverageSicknessDuration");
            }
            var gender = (PermittedGender) dr.GetIntFromLong("Gender1", false, ignoreMissingFields, -1);
            if (ignoreMissingFields && (int) gender == -1) {
                var oldGender = dr.GetString("Gender", false, "", ignoreMissingFields);
                if (oldGender == "male") {
                    gender = PermittedGender.Male;
                }
                else {
                    gender = PermittedGender.Female;
                }
            }
            if (gender == (PermittedGender) (-1)) {
                gender = PermittedGender.Male;
            }
            var description = dr.GetString("Description", false, String.Empty, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new Person(name, age, id, sickdays, averageSicknessduration, gender, connectionString, description, guid);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new Person(
            FindNewName(isNameTaken, "New Person "), 20, null, 5, 2, PermittedGender.Male, connectionString,
            "(no description)", System.Guid.NewGuid().ToStrGuid());

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim) =>
            ImportFromItem((Person)toImport,dstSim);

        [ItemNotNull]
        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            var used = new List<UsedIn>();
            foreach (var chh in sim.ModularHouseholds.It) {
                foreach (var person in chh.Persons) {
                    if (person.Person == this) {
                        used.Add(new UsedIn(chh, "Modular Household"));
                    }
                }
            }
            return used;
        }

        [NotNull]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static Person ImportFromItem([NotNull] Person toImport, [NotNull] Simulator dstSim)
#pragma warning restore RCS1163 // Unused parameter.
        {
            var p = new Person(toImport.Name, toImport.Age, null, toImport.SickDays,
                toImport.AverageSicknessDuration, toImport.Gender,dstSim.ConnectionString,  toImport.Description,
                toImport.Guid);
            p.SaveToDB();
            return p;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<Person> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            // cleanup
            result.Sort();
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Age", "@Age", Age);
            cmd.AddParameter("Sickdays", "@Sickdays", _sickDays);
            cmd.AddParameter("Gender1", _gender);
            cmd.AddParameter("Description", _description);
            cmd.AddParameter("AverageSicknessDuration", "@AverageSicknessDuration", _averageSicknessDuration);
        }

        public override string ToString() => Name;
    }
}