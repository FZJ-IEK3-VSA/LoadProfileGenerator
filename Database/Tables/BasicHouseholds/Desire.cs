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
// changed 2014-10-18 - criticalthreshold introduced
// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    [Serializable]
    public class Desire : DBBaseElement {
        public const string TableName = "tblDesires";
        private decimal _criticalThreshold;
        private decimal _defaultDecayRate;
        private decimal _defaultThreshold;
        private decimal _defaultWeight;
        private bool _isSharedDesire;
        [JetBrains.Annotations.NotNull] private string _desireCategory;

        public Desire([JetBrains.Annotations.NotNull] string pName, decimal pDefaultDecayRate, decimal defaultThreshold, decimal defaultWeight,
            [JetBrains.Annotations.NotNull] string connectionString, decimal criticalThreshold, bool isSharedDesire, [CanBeNull] int? pID,
                      [JetBrains.Annotations.NotNull] string desireCategory, StrGuid guid)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            _defaultWeight = defaultWeight;
            _defaultDecayRate = pDefaultDecayRate;
            _defaultThreshold = defaultThreshold;
            TypeDescription = "Desire";
            _criticalThreshold = criticalThreshold;
            _isSharedDesire = isSharedDesire;
            _desireCategory = desireCategory;
        }
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string DesireCategory {
            get => _desireCategory;
            set => SetValueWithNotify(value, ref _desireCategory, nameof(DesireCategory));
        }

        public decimal CriticalThreshold
        {
            get => _criticalThreshold;
            set => SetValueWithNotify(value, ref _criticalThreshold, nameof(CriticalThreshold));
        }

        [UsedImplicitly]
        public decimal DefaultDecayRate {
            get => _defaultDecayRate;
            set => SetValueWithNotify(value, ref _defaultDecayRate, nameof(DefaultDecayRate));
        }

        [UsedImplicitly]
        public decimal DefaultThreshold {
            get => _defaultThreshold;

            set => SetValueWithNotify(value, ref _defaultThreshold, nameof(DefaultThreshold));
        }

        [UsedImplicitly]
        public decimal DefaultWeight {
            get => _defaultWeight;
            set => SetValueWithNotify(value, ref _defaultWeight, nameof(DefaultWeight));
        }

        [UsedImplicitly]
        public bool IsSharedDesire {
            get => _isSharedDesire;
            set => SetValueWithNotify(value, ref _isSharedDesire, nameof(IsSharedDesire));
        }

        [JetBrains.Annotations.NotNull]
        private static Desire AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var defaultdecay = dr.GetDecimal("DefaultDecayRate", false, 12m, ignoreMissingFields);
            var defaultThreshold = dr.GetDecimal("DefaultThreshold", false, 0.5m, ignoreMissingFields);
            var defaultWeight = dr.GetDecimal("DefaultWeight", false, 1m, ignoreMissingFields);
            var isSharedDesire = dr.GetBool("IsSharedDesire", false, false, ignoreMissingFields);
            var desireCategory = dr.GetString("DesireCategory", false, "", ignoreMissingFields);
            var name =  dr.GetString("Name","No name");
            var id =dr.GetIntFromLong("ID");
            var criticalThreshold = dr.GetDecimal("CriticalTreshold", false, -1, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var d = new Desire(name, defaultdecay, defaultThreshold, defaultWeight, connectionString,
                criticalThreshold, isSharedDesire, id,desireCategory, guid);
            return d;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new Desire(
            FindNewName(isNameTaken, "New Desire "), 12m, 0.5m, 1,
            connectionString, -1, false, null,"", System.Guid.NewGuid().ToStrGuid());

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((Desire)toImport, dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();

            foreach (var t in sim.HouseholdTraits.Items) {
                foreach (var traitDesire in t.Desires) {
                    if (traitDesire.Desire == this) {
                        used.Add(new UsedIn(t, "Household Trait", traitDesire.MakeDescription()));
                    }
                }
            }
            foreach (var affordance in sim.Affordances.Items) {
                foreach (var affdes in affordance.AffordanceDesires) {
                    if (affdes.Desire == this) {
                        used.Add(new UsedIn(affordance, "Affordance", affdes.MakeDescription()));
                    }
                }
            }
            foreach (var subaff in sim.SubAffordances.Items) {
                foreach (var affdes in subaff.SubAffordanceDesires) {
                    if (affdes.Desire == this) {
                        used.Add(new UsedIn(subaff, "Subaffordance", affdes.MakeDescription()));
                    }
                }
            }

            return used;
        }

        public void ImportFromExistingItem([JetBrains.Annotations.NotNull] Desire src)
        {
            Name = src.Name + " (copy)";
            CriticalThreshold = src.CriticalThreshold;
            DefaultDecayRate = src.DefaultDecayRate;
            DefaultThreshold = src.DefaultThreshold;
            DefaultWeight = src.DefaultWeight;
            IsSharedDesire = src.IsSharedDesire;
            SaveToDB();
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([JetBrains.Annotations.NotNull] Desire item, [JetBrains.Annotations.NotNull] Simulator dstSim) => new Desire(
            item.Name, item.DefaultDecayRate, item.DefaultThreshold, item.DefaultWeight,
            dstSim.ConnectionString,
            item.CriticalThreshold, item.IsSharedDesire, null,
            item.DesireCategory, item.Guid);

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Desire> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("DefaultDecayRate", "@DefaultDecayRate", _defaultDecayRate);
            cmd.AddParameter("DefaultThreshold", "@DefaultThreshold", _defaultThreshold);
            cmd.AddParameter("DefaultWeight", "@DefaultWeight", DefaultWeight);
            cmd.AddParameter("CriticalTreshold", CriticalThreshold);
            cmd.AddParameter("IsSharedDesire", _isSharedDesire);
            cmd.AddParameter("DesireCategory",_desireCategory);
        }
    }
}