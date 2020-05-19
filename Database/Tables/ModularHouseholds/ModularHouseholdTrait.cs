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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class ModularHouseholdTrait : DBBase, IComparable<ModularHouseholdTrait>, IJSonSubElement<ModularHouseholdTrait.JsonModularHouseholdTrait> {
        public enum ModularHouseholdTraitAssignType {
            Age = 0,
            Name
        }

        public class JsonModularHouseholdTrait : IGuidObject
        {
            /// <summary>
            /// for json
            /// </summary>
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            public JsonModularHouseholdTrait()
            {
            }

            public JsonModularHouseholdTrait(StrGuid guid) => Guid = guid;

            public ModularHouseholdTraitAssignType AssignType { get; set; }

            [CanBeNull]
            public JsonReference DstPerson { get; set; }

            [CanBeNull]
            public JsonReference HouseholdTrait { get; set; }
            public StrGuid Guid { get; set; }


        }

        public void SynchronizeDataFromJson(JsonModularHouseholdTrait json, Simulator sim)
        {
            AssignType = json.AssignType;
            if (json.DstPerson != null &&  DstPerson?.Guid != json.DstPerson.Guid) {
                DstPerson = sim.Persons.FindByGuid(json.DstPerson?.Guid);
                if (DstPerson == null)
                {
                    throw new LPGException("Person "+ json.DstPerson+ " could not be found in the database.");
                }
            }

            if (HouseholdTrait.Guid != json.HouseholdTrait?.Guid) {
                var foundtrait = sim.HouseholdTraits.FindByGuid(json.HouseholdTrait?.Guid);
                if (foundtrait == null) {
                    throw new LPGException("Trait was not found: " + json.HouseholdTrait);
                }

                HouseholdTrait = foundtrait;
            }
        }

        public JsonModularHouseholdTrait GetJson()
        {
            var p = new JsonModularHouseholdTrait(Guid) {DstPerson = DstPerson?.GetJsonReference(), HouseholdTrait = HouseholdTrait.GetJsonReference(), AssignType = AssignType};
            return p;
        }


        public const string ParentIDField = "ModularHouseholdID";

        public const string TableName = "tblCHHTraits";

        [CanBeNull] private HouseholdTrait _householdTrait;

        [CanBeNull]
        private readonly int? _modularHouseholdID;

        [CanBeNull] private Person _dstPerson;

        public ModularHouseholdTrait([CanBeNull]int? pID, [CanBeNull]int? modularHouseholdID, [NotNull] string name, [NotNull] string connectionString,
            [CanBeNull] HouseholdTrait ht, [CanBeNull] Person dstPerson, ModularHouseholdTraitAssignType assignType,
                                     StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            AssignType = assignType;
            _dstPerson = dstPerson;
            _householdTrait = ht;
            ID = pID;
            _modularHouseholdID = modularHouseholdID;
            TypeDescription = "Modular Household Trait";
        }

        public ModularHouseholdTraitAssignType AssignType { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string AssignTypeString => AssignType.ToString();

        [CanBeNull]
        public Person DstPerson {
            get => _dstPerson;
            set => SetValueWithNotify(value, ref _dstPerson ,true);
        }

        [UsedImplicitly]
        [NotNull]
        public string ExecutableAffordances {
            get {
                if (_householdTrait == null) {
#pragma warning disable S2372 // Exceptions should not be thrown from property getters {
                    throw new LPGException("Household trait was null");
                }
#pragma warning restore S2372 // Exceptions should not be thrown from property getters
                var affs = _householdTrait.CollectAffordances(true).Count;
                if (DstPerson != null) {
                    return _householdTrait.GetExecuteableAffordanes(DstPerson) + "/" + affs;
                }
                return affs + "/" + affs;
            }
        }
        [NotNull]
        public HouseholdTrait HouseholdTrait {
            get => _householdTrait ?? throw new InvalidOperationException();
            set => SetValueWithNotify(value, ref _householdTrait);
        }

        [CanBeNull]
        public int? ModularHouseholdID => _modularHouseholdID;

        [UsedImplicitly]
        [NotNull]
        public new string Name => ToString();

        public int CompareTo([CanBeNull] ModularHouseholdTrait other) {
            if (other == null) {
                return 0;
            }
            var assigntypecom = AssignType.CompareTo(other.AssignType);
            if (assigntypecom != 0) {
                return assigntypecom;
            }
            if (AssignType == ModularHouseholdTraitAssignType.Name) {
                if (DstPerson != other.DstPerson && DstPerson != null && other.DstPerson != null) {
                    return string.Compare(DstPerson.Name, other.DstPerson.Name, StringComparison.Ordinal);
                }
            }
//            if (other.HouseholdTrait == null || HouseholdTrait == null) {
  //              return 0;
    //        }
            return string.Compare(HouseholdTrait.PrettyName, other.HouseholdTrait.PrettyName, StringComparison.Ordinal);
        }

        [NotNull]
        private static ModularHouseholdTrait AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            int modularHouseholdID;
            if (!ignoreMissingFields) {
                modularHouseholdID = dr.GetIntFromLong("ModularHouseholdID");
            }
            else {
                if (dr.CheckForField("ModularHouseholdID")) {
                    modularHouseholdID = dr.GetIntFromLong("ModularHouseholdID");
                }
                else {
                    modularHouseholdID = dr.GetIntFromLong("CombinedHouseholdID");
                }
            }
            var modularHouseholdTraitAssignType =
                (ModularHouseholdTraitAssignType) dr.GetIntFromLong("AssignType", false, ignoreMissingFields);
            var personID = dr.GetIntFromLong("PersonID", false, ignoreMissingFields, -1);
            var p = aic.Persons.FirstOrDefault(myPerson => myPerson.ID == personID);
            var householdTraitid = dr.GetIntFromLong("HouseholdTraitID");
            var ht = aic.HouseholdTraits.FirstOrDefault(mytrait => mytrait.ID == householdTraitid);
            var name = "(no name)";
            if (ht != null) {
                name = ht.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var chht = new ModularHouseholdTrait(id, modularHouseholdID,
                name, connectionString, ht, p,
                modularHouseholdTraitAssignType, guid);
            return chht;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_householdTrait == null) {
                message = "Household trait is missing";
                return false;
            }
            message = "";
            return true;
        }

        public bool IsValidPerson([CanBeNull] Person person)
        {
            if (person == null) {
                return false;
            }

            switch (AssignType) {
                case ModularHouseholdTraitAssignType.Name:
                    if (person == DstPerson) {
                        return true;
                    }
                    return false;
                case ModularHouseholdTraitAssignType.Age:
                    if (_householdTrait == null) {
                        throw new LPGException("_householdtrait was null");
                    }

                    foreach (var desire in _householdTrait.Desires) {
                        if (person.Age >= desire.MinAge && person.Age <= desire.MaxAge &&
                            (desire.Gender == PermittedGender.All || desire.Gender == person.Gender)) {
                            return true;
                        }
                    }
                    return false;
                default:
                    throw new LPGException("Unknown AssignType. Please report!");
            }
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<ModularHouseholdTrait> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> householdTraits, bool ignoreMissingTables,
            [ItemNotNull] [NotNull] ObservableCollection<Person> persons) {
            var aic = new AllItemCollections(householdTraits: householdTraits, persons: persons);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var items2Delete = new ObservableCollection<ModularHouseholdTrait>();
            foreach (var chht in result) {
                if (chht.ModularHouseholdID == null) {
                    items2Delete.Add(chht);
                }
            }
            foreach (var chht in items2Delete) {
                chht.DeleteFromDB();
                result.Remove(chht);
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_modularHouseholdID != null) {
                cmd.AddParameter("ModularHouseholdID", _modularHouseholdID);
            }
            if (_householdTrait != null) {
                cmd.AddParameter("HouseholdTraitID", _householdTrait.IntID);
            }
            if (DstPerson != null) {
                cmd.AddParameter("PersonID", DstPerson.IntID);
            }
            cmd.AddParameter("AssignType", AssignType);
        }

        public override string ToString() {
            if (_householdTrait != null) {
                var trait = _householdTrait.Name;
                if (DstPerson != null) {
                    trait = DstPerson + " - " + trait;
                }
                return trait;
            }
            return "(no name)";
        }

        public StrGuid RelevantGuid => Guid;
    }
}