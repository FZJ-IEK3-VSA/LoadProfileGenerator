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
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class SubAffordance : DBBaseElement {
        public const string TableName = "tblSubAffordances";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<SubAffordanceDesire> _subAffordanceDesires;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<SubAffordanceVariableOp> _subAffordanceVariableOps;
        private bool _isInterruptable;
        private bool _isInterrupting;
        private int _maximumAge;
        private int _minimumAge;
        private PermittedGender _permittedGender;

        public SubAffordance([NotNull] string pName, [CanBeNull] int? id, PermittedGender permittedGender, [NotNull] string connectionString,
            bool isInterruptable, bool isInterrupting, int maximumAge, int minimumAge, StrGuid guid) : base(pName, TableName,
            connectionString, guid)
        {
            ID = id;
            _subAffordanceDesires = new ObservableCollection<SubAffordanceDesire>();
            _subAffordanceVariableOps = new ObservableCollection<SubAffordanceVariableOp>();
            TypeDescription = "SubAffordance";
            _permittedGender = permittedGender;
            _isInterruptable = isInterruptable;
            _isInterrupting = isInterrupting;
            _maximumAge = maximumAge;
            _minimumAge = minimumAge;
        }

        [NotNull]
        public string DesireList {
            get {
                var builder = new StringBuilder();
                foreach (var affordanceDesire in _subAffordanceDesires) {
                    builder.Append(affordanceDesire).Append(", ");
                }
                var s = builder.ToString();
                if (s.Length > 0) {
                    s = s.Substring(0, s.Length - 2);
                }
                return s;
            }
        }

        [UsedImplicitly]
        public bool IsInterruptable {
            get => _isInterruptable;
            set => SetValueWithNotify(value, ref _isInterruptable, nameof(IsInterruptable));
        }

        [UsedImplicitly]
        public bool IsInterrupting {
            get => _isInterrupting;
            set => SetValueWithNotify(value, ref _isInterrupting, nameof(IsInterrupting));
        }

        [UsedImplicitly]
        public int MaximumAge {
            get => _maximumAge;

            set => SetValueWithNotify(value, ref _maximumAge, nameof(MaximumAge));
        }

        [UsedImplicitly]
        public int MinimumAge {
            get => _minimumAge;

            set => SetValueWithNotify(value, ref _minimumAge, nameof(MinimumAge));
        }

        public PermittedGender PermittedGender {
            get => _permittedGender;

            [UsedImplicitly] set => SetValueWithNotify(value, ref _permittedGender, nameof(PermittedGender));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<SubAffordanceDesire> SubAffordanceDesires => _subAffordanceDesires;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<SubAffordanceVariableOp> SubAffordanceVariableOps => _subAffordanceVariableOps;

        public void AddDesire([NotNull] Desire desire, decimal satisfactionvalue, [ItemNotNull] [NotNull] ObservableCollection<Desire> simdesires)
        {
            for (var i = 0; i < _subAffordanceDesires.Count; i++) {
                if (_subAffordanceDesires[i].Desire == desire) {
                    _subAffordanceDesires[i].DeleteFromDB();
                    _subAffordanceDesires.RemoveAt(i);
                    i = 0;
                }
            }
            if (ID == null) {
                throw new LPGException("Desire added to sub affordance that wasn't saved!");
            }
            var affd = new SubAffordanceDesire(null, desire, ID, satisfactionvalue, simdesires,
                DeleteAffordanceDesireFromDB, ConnectionString, desire.Name, System.Guid.NewGuid().ToStrGuid());
            _subAffordanceDesires.Add(affd);
            affd.SaveToDB();
        }

        public void AddVariableOperation(double value, VariableLocationMode clm, [CanBeNull] Location loc,
            VariableAction action, [NotNull] Variable variable, VariableExecutionTime variableExecutionTime)
        {
            var itemstodelete = _subAffordanceVariableOps
                .Where(x => x.Variable == variable && x.LocationMode == clm && x.Location == loc)
                .ToList();
            foreach (var variableOp in itemstodelete) {
                variableOp.DeleteFromDB();
                _subAffordanceVariableOps.Remove(variableOp);
            }
            var at = new SubAffordanceVariableOp(value, null, IntID, ConnectionString, clm, loc,
                action, variable, variableExecutionTime, variable.Name, System.Guid.NewGuid().ToStrGuid());

            _subAffordanceVariableOps.Add(at);
            at.SaveToDB();
            _subAffordanceVariableOps.Sort();
        }

        [NotNull]
        private static SubAffordance AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name =dr.GetString("Name","(no name)");

            var permittedGender = PermittedGender.All;
            if (dr["PermittedGender"] != DBNull.Value) {
                permittedGender = (PermittedGender) dr.GetInt("PermittedGender");
            }
            var isInterruptable = dr.GetBool("IsInterruptable", false, false, ignoreMissingFields);
            var isInterrupting = dr.GetBool("isInterrupting", false, false, ignoreMissingFields);
            var minimumAge =  dr.GetInt("MinimumAge");
            var maximumAge = dr.GetInt("MaximumAge");
            var guid = GetGuid(dr, ignoreMissingFields);
            var aff = new SubAffordance(name, id, permittedGender, connectionString, isInterruptable,
                isInterrupting, maximumAge, minimumAge, guid);
            return aff;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var subAffordance = new SubAffordance(FindNewName(isNameTaken, "New Sub-Affordance "), null,
                PermittedGender.All, connectionString, false, false, 0, 99,
                System.Guid.NewGuid().ToStrGuid());
            return subAffordance;
        }

        public bool DeleteAffordanceDesireFromDB([NotNull] SubAffordanceDesire affd)
        {
            affd.DeleteFromDB();
            _subAffordanceDesires.Remove(affd);
            return true;
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            foreach (var affordanceDesire in _subAffordanceDesires) {
                affordanceDesire.DeleteFromDB();
            }
            foreach (var affordanceVariableOp in _subAffordanceVariableOps) {
                affordanceVariableOp.DeleteFromDB();
            }
        }

        public void DeleteVariableOpFromDB([NotNull] SubAffordanceVariableOp affd)
        {
            affd.DeleteFromDB();
            _subAffordanceVariableOps.Remove(affd);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport,  Simulator dstSim)
            => ImportFromItem((SubAffordance)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var usedins = new List<UsedIn>();
            foreach (var aff in sim.Affordances.Items) {
                foreach (var subaff in aff.SubAffordances) {
                    if (subaff.SubAffordance == this) {
                        usedins.Add(new UsedIn(aff, "Subaffordance"));
                        break;
                    }
                }
            }
            return usedins;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] SubAffordance item,[NotNull] Simulator dstSim)
        {
            var subaff = new SubAffordance(item.Name, null, item.PermittedGender,dstSim.ConnectionString,
                item.IsInterruptable, item.IsInterrupting, item.MaximumAge, item.MinimumAge,item.Guid);
            subaff.SaveToDB();
            foreach (var affordanceDesire in item.SubAffordanceDesires) {
                var d = GetItemFromListByName(dstSim.Desires.Items, affordanceDesire.Desire?.Name);
                if (d == null) {
                    Logger.Error("Could not find a desire while importing. Skipping.");
                    continue;
                }
                subaff.AddDesire(d, affordanceDesire.SatisfactionValue, dstSim.Desires.Items);
            }
            foreach (var op in item.SubAffordanceVariableOps) {
                Location loc = null;
                if (op.Location != null) {
                    loc = GetItemFromListByName(dstSim.Locations.Items, op.Location.Name);
                }
                Variable va = null;
                if (op.Variable != null) {
                    va = GetItemFromListByName(dstSim.Variables.Items, op.Variable.Name);
                }

                if (va == null) {
                    Logger.Error("Could not find a variable while importing. Skipping.");
                    continue;
                }
                subaff.AddVariableOperation(op.Value, op.LocationMode, loc, op.VariableAction, va, op.ExecutionTime);
            }
            return subaff;
        }

        private static bool IsCorrectParentDesire([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var affdes = (SubAffordanceDesire) child;
            if (parent.ID == affdes.SubaffordanceID) {
                var subaff = (SubAffordance) parent;
                subaff.SubAffordanceDesires.Add(affdes);
                affdes.SetDeleteFuncFromParent(subaff.DeleteAffordanceDesireFromDB);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentVariableOp([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var affdes = (SubAffordanceVariableOp) child;
            if (parent.ID == affdes.AffordanceID) {
                var subaff = (SubAffordance) parent;
                subaff.SubAffordanceVariableOps.Add(affdes);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<SubAffordance> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Desire> desires, bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<Location> locations,
            [ItemNotNull] [NotNull] ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(desires: desires);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var subAffordanceDesires =
                new ObservableCollection<SubAffordanceDesire>();
            SubAffordanceDesire.LoadFromDatabase(subAffordanceDesires, connectionString, desires, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(subAffordanceDesires), IsCorrectParentDesire,
                ignoreMissingTables);
            var subafftrigops =
                new ObservableCollection<SubAffordanceVariableOp>();
            SubAffordanceVariableOp.LoadFromDatabase(subafftrigops, connectionString, ignoreMissingTables, locations,
                variables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(subafftrigops), IsCorrectParentVariableOp,
                ignoreMissingTables);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var subaffordanceDesire in _subAffordanceDesires) {
                subaffordanceDesire.SaveToDB();
            }
            foreach (var op in _subAffordanceVariableOps) {
                op.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("MinimumAge", "@MinimumAge", _minimumAge);
            cmd.AddParameter("MaximumAge", "@MaximumAge", _maximumAge);
            cmd.AddParameter("PermittedGender", "@PermittedGender", _permittedGender);
            cmd.AddParameter("IsInterruptable", _isInterruptable);
            cmd.AddParameter("IsInterrupting", _isInterrupting);
        }

        public override string ToString() => Name;
    }
}