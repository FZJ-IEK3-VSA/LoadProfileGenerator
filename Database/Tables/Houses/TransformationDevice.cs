//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Changed 2014-11-26 NP
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
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class TransformationDevice : DBBaseElement {
        public const string TableName = "tblTransformationDevices";

        [ItemNotNull] [NotNull] private readonly ObservableCollection<TransformationDeviceCondition> _conditions =
            new ObservableCollection<TransformationDeviceCondition>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<TransformationFactorDatapoint> _factorDatapoints =
            new ObservableCollection<TransformationFactorDatapoint>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<TransformationDeviceLoadType> _loadTypesOut =
            new ObservableCollection<TransformationDeviceLoadType>();

        [NotNull] private string _description;

        [CanBeNull] private VLoadType _loadTypeIn;

        private double _maximumInputPower;
        private double _maxValue;
        private double _minimumInputPower;
        private double _minValue;

        public TransformationDevice([NotNull] string pName,
                                    [NotNull] string description,
                                    [CanBeNull] VLoadType loadTypeIn,
                                    double minValue,
                                    double maxValue,
                                    [NotNull] string connectionString,
                                    double minimumInputPower,
                                    double maximumInputPower,
                                    StrGuid guid,
                                    [CanBeNull] int? pID = null) : base(pName, TableName, connectionString, guid)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            ID = pID;
            TypeDescription = "Transformation Device";
            _description = description;
            _loadTypeIn = loadTypeIn;
            _minimumInputPower = minimumInputPower;
            _maximumInputPower = maximumInputPower;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransformationDeviceCondition> Conditions => _conditions;

        [NotNull]
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

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<TransformationFactorDatapoint> FactorDatapoints => _factorDatapoints;

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType LoadTypeIn {
            get => _loadTypeIn;
            set => SetValueWithNotify(value, ref _loadTypeIn, false, nameof(LoadTypeIn));
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<TransformationDeviceLoadType> LoadTypesOut => _loadTypesOut;

        [UsedImplicitly]
        public double MaximumInputPower {
            get => _maximumInputPower;
            set => SetValueWithNotify(value, ref _maximumInputPower, nameof(MaximumInputPower));
        }

        [UsedImplicitly]
        public double MaxValue {
            get => _maxValue;
            set => SetValueWithNotify(value, ref _maxValue, nameof(MaxValue));
        }

        [UsedImplicitly]
        public double MinimumInputPower {
            get => _minimumInputPower;
            set => SetValueWithNotify(value, ref _minimumInputPower, nameof(MinimumInputPower));
        }

        [UsedImplicitly]
        public double MinValue {
            get => _minValue;
            set => SetValueWithNotify(value, ref _minValue, nameof(MinValue));
        }

        public void AddDataPoint(double refvalue, double factor, bool sort = true)
        {
            var dp = new TransformationFactorDatapoint(null, refvalue, factor, IntID, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            _factorDatapoints.Add(dp);
            dp.SaveToDB();
            if (sort) {
                _factorDatapoints.Sort();
            }
        }

        public void AddOutTransformationDeviceLoadType([NotNull] VLoadType lt, double factor, TransformationFactorType factorType)
        {
            var items2Delete = new List<TransformationDeviceLoadType>();
            foreach (var transformationDeviceLoadType in _loadTypesOut) {
                if (transformationDeviceLoadType.VLoadType == lt) {
                    items2Delete.Add(transformationDeviceLoadType);
                }
            }

            foreach (var type in items2Delete) {
                DeleteTransformationLoadtypeFromDB(type);
            }

            var tdlt = new TransformationDeviceLoadType(null,
                lt,
                factor,
                IntID,
                ConnectionString,
                lt.Name,
                factorType,
                System.Guid.NewGuid().ToStrGuid());
            _loadTypesOut.Add(tdlt);
            tdlt.SaveToDB();
        }

        public void AddTransformationDeviceCondition([NotNull] Variable variable, double minValue, double maxValue)
        {
            var tdlt = new TransformationDeviceCondition(null,
                minValue,
                maxValue,
                IntID,
                ConnectionString,
                variable.Name,
                System.Guid.NewGuid().ToStrGuid(),
                variable);
            _conditions.Add(tdlt);
            tdlt.SaveToDB();
        }

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var used = new List<UsedIn>();
            foreach (var ht in sim.HouseTypes.It) {
                foreach (var device in ht.HouseTransformationDevices) {
                    if (device.TransformationDevice == this) {
                        used.Add(new UsedIn(ht, "House Type"));
                    }
                }
            }

            return used;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            var house = new TransformationDevice(FindNewName(isNameTaken, "New Transformation Device "),
                "New transformation device description",
                null,
                0,
                1000000,
                connectionString,
                0,
                100000,
                System.Guid.NewGuid().ToStrGuid());
            return house;
        }

        public void DeleteFactorDataPoint([NotNull] TransformationFactorDatapoint factorDatapoint)
        {
            factorDatapoint.DeleteFromDB();
            _factorDatapoints.Remove(factorDatapoint);
        }

        public void DeleteTransformationDeviceCondition([NotNull] TransformationDeviceCondition condition)
        {
            condition.DeleteFromDB();
            _conditions.Remove(condition);
        }

        public void DeleteTransformationLoadtypeFromDB([NotNull] TransformationDeviceLoadType lt)
        {
            lt.DeleteFromDB();
            _loadTypesOut.Remove(lt);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim) =>
            ImportFromItem((TransformationDevice)toImport, dstSim);

        [NotNull]
        [UsedImplicitly]
        public static TransformationDevice ImportFromItem([NotNull] TransformationDevice toImport, [NotNull] Simulator dstSim)
        {
            VLoadType vlt = null;
            if (toImport.LoadTypeIn != null) {
                vlt = GetItemFromListByName(dstSim.LoadTypes.MyItems, toImport.LoadTypeIn.Name);
            }

            var td = new TransformationDevice(toImport.Name,
                toImport.Description,
                vlt,
                toImport.MinValue,
                toImport.MaxValue,
                dstSim.ConnectionString,
                toImport.MinimumInputPower,
                toImport.MaximumInputPower,
                toImport.Guid);
            td.SaveToDB();
            foreach (var deviceLoadType in toImport.LoadTypesOut) {
                var vlt2 = GetItemFromListByName(dstSim.LoadTypes.MyItems, deviceLoadType.VLoadType?.Name);
                if (vlt2 == null) {
                    Logger.Error("Could not find a load type for import. Skipping");
                    continue;
                }

                td.AddOutTransformationDeviceLoadType(vlt2, deviceLoadType.Factor, deviceLoadType.FactorType);
            }

            foreach (var srccondition in toImport.Conditions) {
                if (srccondition.Variable == null) {
                    continue;
                }

                var variable = GetItemFromListByName(dstSim.Variables.MyItems, srccondition.Variable.Name);
                if (variable == null) {
                    continue;
                }

                td.AddTransformationDeviceCondition(variable, srccondition.MinValue, srccondition.MaxValue);
            }

            foreach (var datapoint in toImport._factorDatapoints) {
                    td.AddDataPoint(datapoint.ReferenceValue, datapoint.Factor);
                }

                td.SaveToDB();
                return td;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransformationDevice> result,
                                            [NotNull] string connectionString,
                                            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes,
                                            [ItemNotNull] [NotNull] ObservableCollection<Variable> variables,
                                            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var tdlt = new ObservableCollection<TransformationDeviceLoadType>();
            TransformationDeviceLoadType.LoadFromDatabase(tdlt, connectionString, loadTypes, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(tdlt), IsCorrectParent, ignoreMissingTables);

            var conditions = new ObservableCollection<TransformationDeviceCondition>();
            TransformationDeviceCondition.LoadFromDatabase(conditions, connectionString, loadTypes, ignoreMissingTables, variables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(conditions), IsCorrectConditionParent, ignoreMissingTables);

            var datapoints = new ObservableCollection<TransformationFactorDatapoint>();
            TransformationFactorDatapoint.LoadFromDatabase(datapoints, connectionString, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(datapoints), IsCorrectDataPointParent, ignoreMissingTables);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var transformationDeviceLoadType in _loadTypesOut) {
                transformationDeviceLoadType.SaveToDB();
            }

            foreach (var condition in _conditions) {
                condition.SaveToDB();
            }

            foreach (var datapoint in _factorDatapoints) {
                datapoint.SaveToDB();
            }
        }

        public override string ToString() => Name;

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", "@description", _description);
            cmd.AddParameter("MinValue", _minValue);
            cmd.AddParameter("MaxValue", _maxValue);
            cmd.AddParameter("MinPower", _minimumInputPower);
            cmd.AddParameter("MaxPower", _maximumInputPower);
            if (LoadTypeIn != null) {
                cmd.AddParameter("VLoadTypeIDIn", LoadTypeIn.IntID);
            }
        }

        [NotNull]
        private static TransformationDevice AssignFields([NotNull] DataReader dr,
                                                         [NotNull] string connectionString,
                                                         bool ignoreMissingFields,
                                                         [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", "(no name)");
            var id = dr.GetIntFromLong("ID");
            var description = dr.GetString("Description");
            var minValue = dr.GetDouble("MinValue", false, 0, ignoreMissingFields);
            var maxValue = dr.GetDouble("MaxValue", false, 10000000, ignoreMissingFields);
            var minPower = dr.GetDouble("MinPower", false, 0, ignoreMissingFields);
            var maxPower = dr.GetDouble("MaxPower", false, 10000000, ignoreMissingFields);
            var vloadtypeID = dr.GetNullableIntFromLong("VLoadtypeIDIn", false);
            VLoadType vlt = null;
            if (vloadtypeID != null) {
                vlt = aic.LoadTypes.FirstOrDefault(vlt1 => vlt1.ID == vloadtypeID);
            }

            var guid = GetGuid(dr, ignoreMissingFields);

            return new TransformationDevice(name, description, vlt, minValue, maxValue, connectionString, minPower, maxPower, guid, id);
        }

        private static bool IsCorrectConditionParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (TransformationDeviceCondition)child;
            if (parent.ID == hd.TransformationDeviceID) {
                var transformationDevice = (TransformationDevice)parent;
                transformationDevice.Conditions.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectDataPointParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (TransformationFactorDatapoint)child;
            if (parent.ID == hd.TransformationDeviceID) {
                var transformationDevice = (TransformationDevice)parent;
                transformationDevice._factorDatapoints.Add(hd);
                return true;
            }

            return false;
        }

        private static bool IsCorrectParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (TransformationDeviceLoadType)child;
            if (parent.ID == hd.TransformationDeviceID) {
                var transformationDevice = (TransformationDevice)parent;
                transformationDevice.LoadTypesOut.Add(hd);
                return true;
            }

            return false;
        }
    }
}