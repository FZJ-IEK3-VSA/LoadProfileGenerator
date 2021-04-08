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
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class HouseType : DBBaseElement {
        public const string TableName = "tblHouseTypes";

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<HouseTypeTransformationDevice> _houseTransformationDevices =
            new ObservableCollection<HouseTypeTransformationDevice>();

        private bool _adjustYearlyCooling;
        private bool _adjustYearlyEnergy;

        [CanBeNull] private VLoadType _coolingLoadType;

        private double _coolingTemperature;
        private double _coolingYearlyTotal;
        [JetBrains.Annotations.NotNull] private string _description;

        [CanBeNull] private VLoadType _heatingLoadType;

        private double _heatingTemperature;
        private double _heatingYearlyTotal;
        private int _maximumHouseholdCount;

        private int _minimumHouseholdCount;
        private double _referenceCoolingHours;
        private double _referenceDegreeDays;
        private double _roomTemperature;

        public HouseType([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] string description, double heatingYearlyTotal, double heatingTemperature,
            double roomTemperature, [CanBeNull] VLoadType heatingLoadType, [JetBrains.Annotations.NotNull] string connectionString,
            double coolingTemperature,
            double coolingYearlyTotal, [CanBeNull] VLoadType coolingLoadType, bool adjustYearlyEnergy,
            double referenceDegreeDays,
            bool adjustYearlyCooling, double referenceCoolingHours, int minimumHouseholdCount,
            int maximumHouseholdCount,[NotNull] StrGuid guid,
            [CanBeNull]int? pID = null) : base(pName, TableName, connectionString, guid)
        {
            ID = pID;
            TypeDescription = "House type";
            _description = description;
            _heatingLoadType = heatingLoadType;
            _coolingTemperature = coolingTemperature;
            _coolingYearlyTotal = coolingYearlyTotal;
            _heatingTemperature = heatingTemperature;
            _roomTemperature = roomTemperature;
            _heatingYearlyTotal = heatingYearlyTotal;
            _coolingLoadType = coolingLoadType;
            _adjustYearlyEnergy = adjustYearlyEnergy;
            _referenceDegreeDays = referenceDegreeDays;
            _adjustYearlyCooling = adjustYearlyCooling;
            _referenceCoolingHours = referenceCoolingHours;
            _minimumHouseholdCount = minimumHouseholdCount;
            _maximumHouseholdCount = maximumHouseholdCount;
        }

        [UsedImplicitly]
        public bool AdjustYearlyCooling {
            get => _adjustYearlyCooling;
            set => SetValueWithNotify(value, ref _adjustYearlyCooling, nameof(AdjustYearlyCooling));
        }
        [JetBrains.Annotations.NotNull]
        public string HouseTypeCode
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return "";
                }

                if (!Name.Contains(" "))
                {
                    return Name;
                }

                return Name.Substring(0, Name.IndexOf(" ",StringComparison.InvariantCulture));
            }
        }

        [UsedImplicitly]
        public bool AdjustYearlyEnergy {
            get => _adjustYearlyEnergy;
            set => SetValueWithNotify(value, ref _adjustYearlyEnergy, nameof(AdjustYearlyEnergy));
        }

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType CoolingLoadType {
            get => _coolingLoadType;
            set => SetValueWithNotify(value, ref _coolingLoadType,false, nameof(CoolingLoadType));
        }

        [UsedImplicitly]
        public double CoolingTemperature {
            get => _coolingTemperature;
            set => SetValueWithNotify(value, ref _coolingTemperature, nameof(CoolingTemperature));
        }

        [UsedImplicitly]
        public double CoolingYearlyTotal {
            get => _coolingYearlyTotal;
            set => SetValueWithNotify(value, ref _coolingYearlyTotal, nameof(CoolingYearlyTotal));
        }

        [JetBrains.Annotations.NotNull]
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

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType HeatingLoadType {
            get => _heatingLoadType;
            set => SetValueWithNotify(value, ref _heatingLoadType,false, nameof(HeatingLoadType));
        }

        [UsedImplicitly]
        public double HeatingTemperature {
            get => _heatingTemperature;
            set => SetValueWithNotify(value, ref _heatingTemperature, nameof(HeatingTemperature));
        }

        [UsedImplicitly]
        public double HeatingYearlyTotal {
            get => _heatingYearlyTotal;
            set => SetValueWithNotify(value, ref _heatingYearlyTotal, nameof(HeatingYearlyTotal));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseTypeDevice> HouseDevices { get; } =
            new ObservableCollection<HouseTypeDevice>();

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseTypeEnergyStorage> HouseEnergyStorages { get; } =
            new ObservableCollection<HouseTypeEnergyStorage>();

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseTypeGenerator> HouseGenerators { get; } =
            new ObservableCollection<HouseTypeGenerator>();

        [ItemNotNull]
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HouseTypeTransformationDevice> HouseTransformationDevices
            => _houseTransformationDevices;

        [UsedImplicitly]
        public int MaximumHouseholdCount {
            get => _maximumHouseholdCount;
            set => SetValueWithNotify(value, ref _maximumHouseholdCount, nameof(MaximumHouseholdCount));
        }

        [UsedImplicitly]
        public int MinimumHouseholdCount {
            get => _minimumHouseholdCount;
            set => SetValueWithNotify(value, ref _minimumHouseholdCount, nameof(MinimumHouseholdCount));
        }

        public override string PrettyName => Name + " (" + _minimumHouseholdCount + "-" + _maximumHouseholdCount + ")";

        [UsedImplicitly]
        public double ReferenceCoolingHours {
            get => _referenceCoolingHours;
            set => SetValueWithNotify(value, ref _referenceCoolingHours, nameof(ReferenceCoolingHours));
        }

        [UsedImplicitly]
        public double ReferenceDegreeDays {
            get => _referenceDegreeDays;
            set => SetValueWithNotify(value, ref _referenceDegreeDays, nameof(ReferenceDegreeDays));
        }

        [UsedImplicitly]
        public double RoomTemperature {
            get => _roomTemperature;
            set => SetValueWithNotify(value, ref _roomTemperature, nameof(RoomTemperature));
        }

        public void AddEnergyStorage([CanBeNull] EnergyStorage es)
        {
            foreach (var energyStorage in HouseEnergyStorages) {
                if (energyStorage.EnergyStorage == es) {
                    Logger.Error("You can only add each energy storage once to each house.");
                    return;
                }
            }
            if (es== null)
            {
                throw new LPGException("Energy storage was null");
            }
            var hes = new HouseTypeEnergyStorage(null, es.Name, IntID, es, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            HouseEnergyStorages.Add(hes);
            hes.SetDeleteFunction(DeleteHouseEnergyStorage);
            hes.SaveToDB();
        }

        public void AddGenerator([JetBrains.Annotations.NotNull] Generator gen)
        {
            var hgen = new HouseTypeGenerator(null, IntID, gen, ConnectionString, gen.Name, System.Guid.NewGuid().ToStrGuid());
            HouseGenerators.Add(hgen);
            hgen.SaveToDB();
        }

        public void AddHouseTypeDevice([JetBrains.Annotations.NotNull] IAssignableDevice adev, [JetBrains.Annotations.NotNull] TimeLimit devTimeLimit, [CanBeNull] TimeBasedProfile profile,
            double standardDeviation,[CanBeNull] VLoadType vLoadType,[CanBeNull] Location loc, double variableValue,
            VariableCondition variableCondition,[CanBeNull] Variable variable)
        {
            var hd = new HouseTypeDevice(null, adev, profile, IntID, devTimeLimit, standardDeviation,
                vLoadType, ConnectionString, adev.Name + " [" + devTimeLimit + "]", loc, variableValue,
                variableCondition, variable, System.Guid.NewGuid().ToStrGuid());
            HouseDevices.Add(hd);
            hd.SaveToDB();
        }

        public void AddTransformationDevice([JetBrains.Annotations.NotNull] TransformationDevice td)
        {
            var htd = new HouseTypeTransformationDevice(null, IntID, td, ConnectionString,
                td.Name, System.Guid.NewGuid().ToStrGuid());
            HouseTransformationDevices.Add(htd);
            htd.SaveToDB();
        }

        [JetBrains.Annotations.NotNull]
        private static HouseType AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name =  dr.GetString("Name");
            var id = dr.GetIntFromLong("ID");
            var description =  dr.GetString("Description");
            var heatingYearlyTotal = dr.GetDouble("HeatingYearlyTotal", false, 0, ignoreMissingFields);
            var heatingTemperature = dr.GetDouble("HeatingTemperature", false, 0, ignoreMissingFields);
            var roomTemperature = dr.GetDouble("RoomTemperature", false, 0, ignoreMissingFields);
            var coolingTemperature = dr.GetDouble("CoolingTemperature", false, 0, ignoreMissingFields);
            var coolingYearlyTotal = dr.GetDouble("CoolingYearlyTotal", false, 0, ignoreMissingFields);
            var heatingLoadTypeID = dr.GetNullableIntFromLong("HeatingLoadTypeID", false, ignoreMissingFields);
            VLoadType heatingLoadType = null;
            if (heatingLoadTypeID != null) {
                heatingLoadType = aic.LoadTypes.FirstOrDefault(lt1 => lt1.ID == heatingLoadTypeID);
            }
            var coolingLoadTypeID = dr.GetNullableIntFromLong("CoolingLoadTypeID", false, ignoreMissingFields);
            VLoadType coolingLoadType = null;
            if (coolingLoadTypeID != null) {
                coolingLoadType = aic.LoadTypes.FirstOrDefault(clt1 => clt1.ID == coolingLoadTypeID);
            }

            var adjustYearlyEnergyConsumption = dr.GetBool("AdjustYearlyEnergy", false, false, ignoreMissingFields);
            var referenceDegreeDays = dr.GetDouble("ReferenceDegreeDays", false, 0, ignoreMissingFields);

            var adjustYearlyCoolingHours = dr.GetBool("AdjustYearlyCoolingHours", false, false, ignoreMissingFields);
            var referenceCoolingHours = dr.GetDouble("ReferenceCoolingHours", false, 0, ignoreMissingFields);
            var minimumHouseholdCount = dr.GetIntFromLong("MinimumHouseholdCount", false, ignoreMissingFields);
            var maximumHouseholdCount = dr.GetIntFromLong("MaximumHouseholdCount", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseType(name, description, heatingYearlyTotal, heatingTemperature, roomTemperature,
                heatingLoadType, connectionString, coolingTemperature, coolingYearlyTotal, coolingLoadType,
                adjustYearlyEnergyConsumption, referenceDegreeDays, adjustYearlyCoolingHours, referenceCoolingHours,
                minimumHouseholdCount, maximumHouseholdCount,guid, id);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString)
        {
            var housetype = new HouseType(FindNewName(isNameTaken, "New House Type "),
                "New house type description", 5000, 15, 20, null, connectionString, 19, 2000, null, false, 400, false,
                200, 1, 10, System.Guid.NewGuid().ToStrGuid());
            return housetype;
        }

        public void DeleteDeviceFromDB([CanBeNull] IAssignableDevice db)
        {
            var deleteDevice = new List<HouseTypeDevice>();
            foreach (var houseDevice in HouseDevices) {
                if (houseDevice.Device == db) {
                    deleteDevice.Add(houseDevice);
                }
            }
            foreach (var houseDevice in deleteDevice) {
                houseDevice.DeleteFromDB();
                HouseDevices.Remove(houseDevice);
            }
        }

        public void DeleteHouseDeviceFromDB([JetBrains.Annotations.NotNull] HouseTypeDevice hd)
        {
            HouseDevices.Remove(hd);
            hd.DeleteFromDB();
        }

        public void DeleteHouseEnergyStorage([JetBrains.Annotations.NotNull] HouseTypeEnergyStorage houseEnergyStorage)
        {
            HouseEnergyStorages.Remove(houseEnergyStorage);
            houseEnergyStorage.DeleteFromDB();
        }

        public void DeleteHouseGenerator([JetBrains.Annotations.NotNull] HouseTypeGenerator houseGen)
        {
            HouseGenerators.Remove(houseGen);
            houseGen.DeleteFromDB();
        }

        public void DeleteHouseTransformationDeviceFromDB([JetBrains.Annotations.NotNull] HouseTypeTransformationDevice htd)
        {
            _houseTransformationDevices.Remove(htd);
            htd.DeleteFromDB();
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((HouseType)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim)
        {
            var usedIns = new List<UsedIn>();
            foreach (var house in sim.Houses.Items) {
                if(house.HouseType == this) {
                    usedIns.Add(new UsedIn(house, "House"));
                }
            }
            return usedIns;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([JetBrains.Annotations.NotNull] HouseType item,  [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            VLoadType heatingvlt = null;
            if (item.HeatingLoadType != null) {
                heatingvlt = GetItemFromListByName(dstSim.LoadTypes.Items, item.HeatingLoadType.Name);
            }
            VLoadType coolingVlt = null;
            if (item.CoolingLoadType != null) {
                coolingVlt = GetItemFromListByName(dstSim.LoadTypes.Items, item.CoolingLoadType.Name);
            }
            var houseType = new HouseType(item.Name, item.Description, item.HeatingYearlyTotal,
                item.HeatingTemperature, item.RoomTemperature, heatingvlt,dstSim.ConnectionString, item.CoolingTemperature,
                item.CoolingYearlyTotal, coolingVlt, item.AdjustYearlyEnergy, item.ReferenceDegreeDays,
                item.AdjustYearlyCooling, item.ReferenceCoolingHours, item.MinimumHouseholdCount,
                item.MaximumHouseholdCount, item.Guid);
            houseType.SaveToDB();
            foreach (var houseDevice in item.HouseDevices) {
                var iad = GetAssignableDeviceFromListByName(dstSim.RealDevices.Items,
                    dstSim.DeviceCategories.Items, dstSim.DeviceActions.Items, dstSim.DeviceActionGroups.Items,
                    houseDevice.Device);

                TimeBasedProfile tbp = null;
                if (houseDevice.Profile != null) {
                    tbp = GetItemFromListByName(dstSim.Timeprofiles.Items, houseDevice.Profile.Name);
                }
                VLoadType vlt = null;
                if (houseDevice.LoadType != null) {
                    vlt = GetItemFromListByName(dstSim.LoadTypes.Items, houseDevice.LoadType.Name);
                }

                TimeLimit dt = null;
                if (houseDevice.TimeLimit != null) {
                    dt = GetItemFromListByName(dstSim.TimeLimits.Items, houseDevice.TimeLimit.Name);
                }

                Location loc = null;
                if (houseDevice.Location != null) {
                    loc = GetItemFromListByName(dstSim.Locations.Items, houseDevice.Location.Name);
                }
                Variable variable = null;
                if (houseDevice.Variable != null) {
                    variable = GetItemFromListByName(dstSim.Variables.Items, houseDevice.Variable.Name);
                }
                houseType.AddHouseTypeDevice(iad, dt, tbp, (double) houseDevice.TimeStandardDeviation, vlt, loc,
                    houseDevice.VariableValue, houseDevice.VariableCondition, variable);
            }
            foreach (var houseEnergyStorage in item.HouseEnergyStorages) {
                var es = GetItemFromListByName(dstSim.EnergyStorages.Items,
                    houseEnergyStorage.EnergyStorage.Name);
                houseType.AddEnergyStorage(es);
            }
            foreach (var generator in item.HouseGenerators) {
                var gen = GetItemFromListByName(dstSim.Generators.Items, generator.Generator.Name);
                houseType.AddGenerator(gen);
            }
            foreach (var houseTransformationDevice in item.HouseTransformationDevices) {
                var trafo = GetItemFromListByName(dstSim.TransformationDevices.Items,
                    houseTransformationDevice.TransformationDevice.Name);
                houseType.AddTransformationDevice(trafo);
            }

            return houseType;
        }

        private static bool IsCorrectParentHouseDevice([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (HouseTypeDevice) child;
            if (parent.ID == hd.HouseID) {
                var houseType = (HouseType) parent;
                houseType.HouseDevices.Add(hd);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentHouseEnergyStorage([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (HouseTypeEnergyStorage) child;
            if (parent.ID == hd.HouseID) {
                var houseType = (HouseType) parent;
                houseType.HouseEnergyStorages.Add(hd);
                hd.SetDeleteFunction(houseType.DeleteHouseEnergyStorage);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentHouseGenerator([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var gen = (HouseTypeGenerator) child;
            if (parent.ID == gen.HouseID) {
                var houseType = (HouseType) parent;
                houseType.HouseGenerators.Add(gen);
                return true;
            }
            return false;
        }

        private static bool IsCorrectParentHouseTransformationDevice([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (HouseTypeTransformationDevice) child;
            if (parent.ID == hd.HouseID) {
                var houseType = (HouseType) parent;
                houseType.HouseTransformationDevices.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HouseType> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<RealDevice> pallDevices, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceCategory> pallDeviceCategories,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeBasedProfile> pallTimeBasedProfiles, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TimeLimit> timeLimits,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> vLoadTypes,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransformationDevice> transformationDevices,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<EnergyStorage> energyStorages, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Generator> generators,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Location> allLocations,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(loadTypes: vLoadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var hds = new ObservableCollection<HouseTypeDevice>();
            HouseTypeDevice.LoadFromDatabase(hds, connectionString, pallDevices, pallDeviceCategories,
                pallTimeBasedProfiles, timeLimits, vLoadTypes, ignoreMissingTables, allLocations, deviceActions,
                deviceActionGroups, variables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hds), IsCorrectParentHouseDevice,
                ignoreMissingTables);
            var htd =
                new ObservableCollection<HouseTypeTransformationDevice>();
            HouseTypeTransformationDevice.LoadFromDatabase(htd, connectionString, transformationDevices,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(htd), IsCorrectParentHouseTransformationDevice,
                ignoreMissingTables);

            var hes = new ObservableCollection<HouseTypeEnergyStorage>();
            HouseTypeEnergyStorage.LoadFromDatabase(hes, connectionString, energyStorages, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hes), IsCorrectParentHouseEnergyStorage,
                ignoreMissingTables);

            var hgens = new ObservableCollection<HouseTypeGenerator>();
            HouseTypeGenerator.LoadFromDatabase(hgens, connectionString, generators, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(hgens), IsCorrectParentHouseGenerator,
                ignoreMissingTables);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var houseDevice in HouseDevices) {
                houseDevice.SaveToDB();
            }
            foreach (var houseTransformationDevice in HouseTransformationDevices) {
                houseTransformationDevice.SaveToDB();
            }
            foreach (var houseEnergyStorage in HouseEnergyStorages) {
                houseEnergyStorage.SaveToDB();
            }
            foreach (var generator in HouseGenerators) {
                generator.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            cmd.AddParameter("Description", "@description", _description);
            cmd.AddParameter("HeatingYearlyTotal", _heatingYearlyTotal);
            cmd.AddParameter("HeatingTemperature", _heatingTemperature);
            cmd.AddParameter("RoomTemperature", _roomTemperature);
            cmd.AddParameter("CoolingTemperature", _coolingTemperature);
            cmd.AddParameter("CoolingYearlyTotal", _coolingYearlyTotal);
            cmd.AddParameter("AdjustYearlyEnergy", _adjustYearlyEnergy);
            cmd.AddParameter("ReferenceDegreeDays", _referenceDegreeDays);
            cmd.AddParameter("AdjustYearlyCoolingHours", _adjustYearlyCooling);
            cmd.AddParameter("ReferenceCoolingHours", _referenceCoolingHours);
            cmd.AddParameter("MinimumHouseholdCount", _minimumHouseholdCount);
            cmd.AddParameter("MaximumHouseholdCount", _maximumHouseholdCount);
            if (_heatingLoadType != null) {
                cmd.AddParameter("HeatingLoadTypeID", _heatingLoadType.IntID);
            }
            if (_coolingLoadType != null) {
                cmd.AddParameter("CoolingLoadTypeID", _coolingLoadType.IntID);
            }
        }

        public override string ToString() => Name;
    }
}