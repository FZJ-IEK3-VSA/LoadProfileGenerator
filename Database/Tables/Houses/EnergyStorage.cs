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
using System.Linq;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class EnergyStorage : DBBaseElement {
        public const string TableName = "tblEnergyStorages";

        [NotNull] [ItemNotNull] private readonly ObservableCollection<EnergyStorageSignal> _signals =
            new ObservableCollection<EnergyStorageSignal>();

        [CanBeNull] private string _description;
        private double _initialFill;

        [CanBeNull] private VLoadType _loadType;

        private double _maximumStorageRate;
        private double _maximumWithdrawRate;
        private double _minimumStorageRate;
        private double _minimumWithdrawRate;
        private double _storageCapacity;

        public EnergyStorage([NotNull] string pName, [CanBeNull] string description,
                             [CanBeNull] VLoadType loadType, double storageCapacity,
            double initialFill, double minimumStorageRate, double maximumStorageRate,
                             double minimumWithdrawRate,
            double maximumWithdrawRate, [NotNull] string connectionString,[NotNull] string guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            TypeDescription = "Energy Storage Device";
            _description = description;
            _loadType = loadType;
            _storageCapacity = storageCapacity;
            _initialFill = initialFill;
            _minimumStorageRate = minimumStorageRate;
            _maximumStorageRate = maximumStorageRate;
            _minimumWithdrawRate = minimumWithdrawRate;
            _maximumWithdrawRate = maximumWithdrawRate;
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set {
                if (_description == value) {
                    return;
                }
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [UsedImplicitly]
        public double InitialFill {
            get => _initialFill;
            set => SetValueWithNotify(value, ref _initialFill, nameof(InitialFill));
        }

        [CanBeNull]
        public VLoadType LoadType {
            get => _loadType;
            set => SetValueWithNotify(value, ref _loadType, false, nameof(LoadType));
        }

        [UsedImplicitly]
        public double MaximumStorageRate {
            get => _maximumStorageRate;
            set => SetValueWithNotify(value, ref _maximumStorageRate, nameof(MaximumStorageRate));
        }

        [UsedImplicitly]
        public double MaximumWithdrawRate {
            get => _maximumWithdrawRate;
            set => SetValueWithNotify(value, ref _maximumWithdrawRate, nameof(MaximumWithdrawRate));
        }

        [UsedImplicitly]
        public double MinimumStorageRate {
            get => _minimumStorageRate;
            set => SetValueWithNotify(value, ref _minimumStorageRate, nameof(MinimumStorageRate));
        }

        [UsedImplicitly]
        public double MinimumWithdrawRate {
            get => _minimumWithdrawRate;
            set => SetValueWithNotify(value, ref _minimumWithdrawRate, nameof(MinimumWithdrawRate));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<EnergyStorageSignal> Signals => _signals;

        [UsedImplicitly]
        public double StorageCapacity {
            get => _storageCapacity;
            set => SetValueWithNotify(value, ref _storageCapacity, nameof(StorageCapacity));
        }

        public void AddSignal([NotNull] VLoadType lt, double triggerOn, double triggerOff, double value)
        {
            var ess = new EnergyStorageSignal(null, IntID, lt, triggerOn, triggerOff, value,
                ConnectionString, lt.Name, System.Guid.NewGuid().ToString());
            _signals.Add(ess);
            ess.SaveToDB();
        }

        [NotNull]
        private static EnergyStorage AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name =  dr.GetString("Name","no name");
            var id = dr.GetIntFromLong("ID");
            var description =  dr.GetString("Description");
            var vloadtypeID = dr.GetNullableIntFromLong("LoadtypeID", false);
            var storageCapacity = dr.GetDouble("StorageCapacity");
            var initialFill = dr.GetDouble("InitialFill");
            var minimumStorageRate = dr.GetDouble("MinimumStorageRate");
            var maximumStorageRate = dr.GetDouble("MaximumStorageRate");
            var minimumWithdrawRate = dr.GetDouble("MinimumWithdrawRate");
            var maximumWithdrawRate = dr.GetDouble("MaximumWithdrawRate");
            VLoadType vlt = null;
            if (vloadtypeID != null) {
                vlt = aic.LoadTypes.FirstOrDefault(vlt1 => vlt1.ID == vloadtypeID);
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            return new EnergyStorage(name, description, vlt, storageCapacity, initialFill, minimumStorageRate,
                maximumStorageRate, minimumWithdrawRate, maximumWithdrawRate, connectionString,guid, id);
        }

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((EnergyStorage)toImport, dstSim);

        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            var usedIns = new List<UsedIn>();
            foreach (var type in sim.HouseTypes.It) {
                if (type.HouseEnergyStorages.Any(x => x.EnergyStorage == this)) {
                    var ui = new UsedIn(type, type.TypeDescription, "");
                    usedIns.Add(ui);
                }
            }
            return usedIns;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var house = new EnergyStorage(FindNewName(isNameTaken, "New Energy Storage Device "),
                "New energy storage device description", null, 5, 5,
                0, 10, 0, 10, connectionString,
                System.Guid.NewGuid().ToString());
            return house;
        }

        public override void DeleteFromDB()
        {
            base.DeleteFromDB();
            foreach (var signal in _signals) {
                signal.DeleteFromDB();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public static EnergyStorage ImportFromItem([NotNull] EnergyStorage toImport, [NotNull] Simulator dstSim)
        {
            if(toImport.LoadType == null) {
                throw new LPGException("Load type was null");
            }

            var vlt = GetItemFromListByName(dstSim.LoadTypes.MyItems, toImport.LoadType.Name);
            var es = new EnergyStorage(toImport.Name, toImport.Description, vlt, toImport.StorageCapacity,
                toImport.InitialFill, toImport.MinimumStorageRate, toImport.MaximumStorageRate,
                toImport.MinimumWithdrawRate, toImport.MaximumWithdrawRate,
                dstSim.ConnectionString, toImport.Guid);
            es.SaveToDB();
            foreach (var signal in toImport.Signals) {
                if(signal.LoadType ==null) {
                    continue;
                }
                var vlt2 = GetItemFromListByName(dstSim.LoadTypes.MyItems, signal.LoadType.Name);
                if (vlt2 != null) {
                    es.AddSignal(vlt2, signal.TriggerLevelOn, signal.TriggerLevelOff, signal.Value);
                }
            }
            return es;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull]DBBase child)
        {
            var hd = (EnergyStorageSignal) child;
            if (parent.ID == hd.EnergyStorageID) {
                var energyStorage = (EnergyStorage) parent;
                energyStorage._signals.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<EnergyStorage> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var esss = new ObservableCollection<EnergyStorageSignal>();
            EnergyStorageSignal.LoadFromDatabase(esss, connectionString, loadTypes, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(esss), IsCorrectParent, ignoreMissingTables);
        }

        public void RemoveSignal([NotNull] EnergyStorageSignal ess)
        {
            _signals.Remove(ess);
            ess.DeleteFromDB();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var energyStorageSignal in _signals) {
                energyStorageSignal.SaveToDB();
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            if(_description !=null) {
                cmd.AddParameter("Description", "@description", _description);
            }

            if (LoadType != null) {
                cmd.AddParameter("LoadTypeID", LoadType.IntID);
            }
            cmd.AddParameter("StorageCapacity", StorageCapacity);
            cmd.AddParameter("InitialFill", InitialFill);
            cmd.AddParameter("MinimumStorageRate", MinimumStorageRate);
            cmd.AddParameter("MaximumStorageRate", MaximumStorageRate);
            cmd.AddParameter("MinimumWithdrawRate", MinimumWithdrawRate);
            cmd.AddParameter("MaximumWithdrawRate", MaximumWithdrawRate);
        }

        public override string ToString() => Name;
    }
}