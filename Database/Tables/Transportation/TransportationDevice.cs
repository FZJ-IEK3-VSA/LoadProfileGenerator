using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum SpeedUnit {
        Kmh,
        MPerS
    }

    public class TransportationDevice : DBBaseElement {
        public const string TableName = "tblTransportationDevices";
        [CanBeNull]
        private string _description;
        private double _speedInMPerSecond;
        private SpeedUnit _speedUnit;

        [CanBeNull] private TransportationDeviceCategory _transportationDeviceCategory;

        public TransportationDevice([JetBrains.Annotations.NotNull] string name, [CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string connectionString,
            [CanBeNull] string description, double speedInMPerSecond,
            SpeedUnit speedUnit, [CanBeNull] TransportationDeviceCategory category,
                                    double totalRangeinMeters,
                                    double chargingPower,
                                    double chargingDistanceAmount,
                                    double chargingEnergyAmount,
                                    [CanBeNull] VLoadType chargingLoadType, StrGuid guid) : base(name, TableName,
            connectionString, guid)
        {
            _description = description;
            ID = pID;
            AreNumbersOkInNameForIntegrityCheck = true;
            TypeDescription = "Transportation Device";
            _speedInMPerSecond = speedInMPerSecond;
            _speedUnit = speedUnit;
            _transportationDeviceCategory = category;
            _totalRangeInMeters = totalRangeinMeters;
            _chargingPower = chargingPower;
            _chargingDistanceAmount = chargingDistanceAmount;
            _chargingEnergyAmount = chargingEnergyAmount;
            _chargingLoadType = chargingLoadType;
        }
        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var load in Loads)
            {
                load.SaveToDB();
            }
        }
        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TransportationDeviceLoad> Loads { get; } =
            new ObservableCollection<TransportationDeviceLoad>();

        [UsedImplicitly]
        public double SpeedInMPerSecond {
            get => _speedInMPerSecond;
            set => SetValueWithNotify(value, ref _speedInMPerSecond, nameof(SpeedInMPerSecond));
        }

        [UsedImplicitly]
        public SpeedUnit SpeedUnit {
            get => _speedUnit;
            set => SetValueWithNotify(value, ref _speedUnit, nameof(SpeedUnit));
        }

        [CanBeNull]
        [UsedImplicitly]
        public TransportationDeviceCategory TransportationDeviceCategory {
            get => _transportationDeviceCategory;
            set => SetValueWithNotify(value, ref _transportationDeviceCategory, false, nameof(TransportationDeviceCategory));
        }

        private double _totalRangeInMeters;
        public double TotalRangeInMeters
        {
            get => _totalRangeInMeters;
            set => SetValueWithNotify(value, ref _totalRangeInMeters,  nameof(TotalRangeInMeters));
        }
        private double _chargingPower;
        public double ChargingPower
        {
            get => _chargingPower;
            set => SetValueWithNotify(value, ref _chargingPower, nameof(ChargingPower));
        }

        private double _chargingDistanceAmount;
        private double _chargingEnergyAmount;
        [CanBeNull] private VLoadType _chargingLoadType;
        public double ChargingEnergyAmount
        {
            get => _chargingEnergyAmount;
            set => SetValueWithNotify(value, ref _chargingEnergyAmount, nameof(ChargingEnergyAmount));
        }

        public double ChargingDistanceAmount
        {
            get => _chargingDistanceAmount;
            set => SetValueWithNotify(value, ref _chargingDistanceAmount, nameof(ChargingDistanceAmount));
        }

        [CanBeNull]
        public VLoadType ChargingLoadType
        {
            get => _chargingLoadType;
            set => SetValueWithNotify(value, ref _chargingLoadType, false, nameof(ChargingLoadType));
        }

        public void AddLoad([JetBrains.Annotations.NotNull] VLoadType mylt, double maxpower)
        {
            TransportationDeviceLoad load =
                new TransportationDeviceLoad(mylt.Name, IntID, maxpower, mylt, ConnectionString,
                    System.Guid.NewGuid().ToStrGuid());
            load.SaveToDB();
            Loads.Add(load);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new
            TransportationDevice(FindNewName(isNameTaken, "New Transportation Device "), null, connectionString, "", 100,
                SpeedUnit.Kmh, null,-1,0,100,
                0,null, System.Guid.NewGuid().ToStrGuid());

        public void DeleteLoad([JetBrains.Annotations.NotNull] TransportationDeviceLoad transportationDeviceLoad)
        {
            transportationDeviceLoad.DeleteFromDB();
            Loads.Remove(transportationDeviceLoad);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static TransportationDevice ImportFromItem([JetBrains.Annotations.NotNull] TransportationDevice toImport,
            [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            //TODO finish this
            var loc = new TransportationDevice(toImport.Name, null,dstSim.ConnectionString,
                toImport.Description, toImport.SpeedInMPerSecond, toImport._speedUnit,toImport.TransportationDeviceCategory,
                toImport._totalRangeInMeters,toImport._chargingPower,
                toImport._chargingDistanceAmount,toImport.ChargingEnergyAmount,
                toImport._chargingLoadType, toImport.Guid);
            loc.SaveToDB();
            foreach (var load in toImport.Loads) {
                VLoadType dstlt =GetItemFromListByName(dstSim.LoadTypes.Items, load.LoadType?.Name);
                if (dstlt== null)
                {
                    throw new LPGException("load type was null");
                }
                loc.AddLoad(dstlt,load.MaxPower);
            }
            loc.SaveToDB();
            return loc;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransportationDevice> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransportationDeviceCategory> categories,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes)
        {
            var aic = new AllItemCollections(transportationDeviceCategories: categories, loadTypes:loadTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);

            var transportationDeviceLoads =
                new ObservableCollection<TransportationDeviceLoad>();
            TransportationDeviceLoad.LoadFromDatabase(transportationDeviceLoads, connectionString, loadTypes,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(transportationDeviceLoads), IsCorrectParent,
                ignoreMissingTables);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_description!= null) {
                cmd.AddParameter("Description", _description);
            }

            cmd.AddParameter("Speed", _speedInMPerSecond);
            cmd.AddParameter("SpeedUnit", _speedUnit);
            cmd.AddParameter("TotalRangeInMeters", _totalRangeInMeters);
            cmd.AddParameter("ChargingDistanceAmount", ChargingDistanceAmount);
            cmd.AddParameter("ChargingEnergyAmount", ChargingEnergyAmount);
            cmd.AddParameter("ChargingPower", _chargingPower);
            if(_chargingLoadType != null) {
                cmd.AddParameter("ChargingLoadTypeID", _chargingLoadType.IntID);
            }

            if (_transportationDeviceCategory != null) {
                cmd.AddParameter("TravelDeviceCategoryID", _transportationDeviceCategory.IntID);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static TransportationDevice AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var transportationDeviceCategoryID = dr.GetIntFromLong("TravelDeviceCategoryID", false, ignoreMissingFields, -1);
            var speed = dr.GetDouble("Speed", false, 0, ignoreMissingFields);
            var speedUnit = (SpeedUnit) dr.GetIntFromLong("SpeedUnit", false, ignoreMissingFields);
            var tdc =
                aic.TransportationDeviceCategories.FirstOrDefault(x => x.IntID == transportationDeviceCategoryID);

            double totalRangeInMeters = dr.GetDouble("TotalRangeInMeters", false, 0, ignoreMissingFields);
            double chargingDistanceAmount = dr.GetDouble("ChargingDistanceAmount", false, 0, ignoreMissingFields);
            double chargingEnergyAmount = dr.GetDouble("ChargingEnergyAmount", false, 0, ignoreMissingFields);
            double chargingPower = dr.GetDouble("ChargingPower", false, 0, ignoreMissingFields);
            var loadtypeID = dr.GetIntFromLong("ChargingLoadTypeID", false, ignoreMissingFields);
            var loadtype =
                aic.LoadTypes.FirstOrDefault(x => x.IntID == loadtypeID);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TransportationDevice(name, id, connectionString, description, speed, speedUnit, tdc,
                totalRangeInMeters,chargingPower,
                chargingDistanceAmount,chargingEnergyAmount,loadtype,guid );
        }

        private static bool IsCorrectParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (TransportationDeviceLoad) child;
            if (parent.ID == hd.TransportationDeviceID) {
                var transportationDevice = (TransportationDevice) parent;
                transportationDevice.Loads.Add(hd);
                return true;
            }

            return false;
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TransportationDevice)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}