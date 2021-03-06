using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcEnergyStorageDto {
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        public StrGuid Guid { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public CalcLoadTypeDto InputLoadType { get; }
        public double MaximumStorageRate { get; }
        public double MaximumWithdrawRate { get; }
        public double MinimumStorageRate { get; }
        public double MinimumWithdrawRate { get; }
        public double InitialFill { get; }
        [NotNull][ItemNotNull]
        public List<CalcEnergyStorageSignalDto> Signals { get; }
        public double StorageCapacity { get; }
        public double CurrentFillLevel { get; }
        [NotNull]
        public string Capacity { get; }

        public CalcEnergyStorageDto([NotNull]string name, int id,
                                    [NotNull]CalcLoadTypeDto loadType,
                                    double maximumStorageRate, double maximumWithdrawRate, double minimumStorageRate,
                                    double minimumWithdrawRate,
                                    double initialFill, double storageCapacity,
                                    [NotNull]HouseholdKey householdKey, StrGuid guid, [ItemNotNull] [NotNull]List<CalcEnergyStorageSignalDto> signals)
        {
            Name = name;
            ID = id;
            InputLoadType = loadType;
            HouseholdKey = householdKey;
            Guid = guid;
            Signals = signals;
            MaximumStorageRate = maximumStorageRate;
            MaximumWithdrawRate = maximumWithdrawRate;
            MinimumStorageRate = minimumStorageRate;
            MinimumWithdrawRate = minimumWithdrawRate;
            InitialFill = initialFill;
            StorageCapacity = storageCapacity ;
            CurrentFillLevel = initialFill;
            Capacity = storageCapacity + " " + loadType.UnitOfSum + "; initial fill " + initialFill + " " + loadType.UnitOfSum;
        }
    }
}