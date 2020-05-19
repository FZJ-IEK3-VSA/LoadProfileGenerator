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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public class Variable : DBBaseElement {
        public const string TableName = "tblVariables";

        [NotNull] private string _description;
        [NotNull] private string _unit;

        public Variable([NotNull] string name, [NotNull] string description, [NotNull] string unit, [NotNull] string connectionString,
                        StrGuid guid,[CanBeNull]int? pID = null)
            : base(name, TableName, connectionString, guid) {
            _description = description;
            _unit = unit;
            ID = pID;
            TypeDescription = "Variable";
            _description = description;
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [NotNull]
        [UsedImplicitly]
        public string Unit {
            get => _unit;
            set => SetValueWithNotify(value, ref _unit, nameof(Unit));
        }

        [NotNull]
        private static Variable AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var name =dr.GetString("Name","(no name)");
            var description =  dr.GetString("Description");
            var unit = dr.GetString("Unit", false, string.Empty, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new Variable(name, description, unit, connectionString, guid,id);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new Variable(
            FindNewName(isNameTaken, "New Variable "), "(no description)", "(no unit)",
            connectionString, System.Guid.NewGuid().ToStrGuid());

        public override DBBase ImportFromGenericItem(DBBase toImport,  Simulator dstSim)
            => ImportFromItem((Variable)toImport,  dstSim);

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override List<UsedIn> CalculateUsedIns(Simulator sim) {
            var usedIns = new List<UsedIn>();
            foreach (var affordance in sim.Affordances.It) {
                foreach (var operation in affordance.ExecutedVariables) {
                    if (operation.Variable == this) {
                        var ui = new UsedIn(affordance, "Affordance Operation", operation.ToString());
                        usedIns.Add(ui);
                    }
                }
                foreach (var req in affordance.RequiredVariables) {
                    if (req.Variable == this) {
                        var ui = new UsedIn(affordance, "Affordance Requirement", req.ToString());
                        usedIns.Add(ui);
                    }
                }
            }
            foreach (var subAffordance in sim.SubAffordances.It) {
                foreach (var variableOp in subAffordance.SubAffordanceVariableOps) {
                    if (variableOp.Variable == this) {
                        var ui = new UsedIn(subAffordance, "Subaffordance Operation", variableOp.ToString());
                        usedIns.Add(ui);
                    }
                }
            }
            foreach (var trait in sim.HouseholdTraits.It) {
                foreach (var autodev in trait.Autodevs) {
                    if (autodev.Variable == this) {
                        usedIns.Add(new UsedIn(trait, "Household Trait Autonomous Device", autodev.Device?.Name ?? "no name"));
                    }
                }
            }
            foreach (var ht in sim.HouseTypes.It) {
                foreach (var autodev in ht.HouseDevices) {
                    if (autodev.Variable == this) {
                        string name = autodev.Device?.Name;
                        if (name == null) {
                            name = "no name";
                        }
                        usedIns.Add(new UsedIn(ht, "Housetype Autonomous Device", name));
                    }
                }
            }
            return usedIns;
        }

        [NotNull]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "sim")]
        [UsedImplicitly]
#pragma warning disable RCS1163 // Unused parameter.
        public static Variable ImportFromItem([NotNull] Variable toImport, [NotNull] Simulator sim) {
#pragma warning restore RCS1163 // Unused parameter.
            var hd = new Variable(toImport.Name, toImport.Description,
                toImport.Unit,sim.ConnectionString, toImport.Guid);
            hd.SaveToDB();
            return hd;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<Variable> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", Description);
            cmd.AddParameter("Unit", _unit);
        }

        public override string ToString() => Name + " [" + _unit + "]";
    }
}