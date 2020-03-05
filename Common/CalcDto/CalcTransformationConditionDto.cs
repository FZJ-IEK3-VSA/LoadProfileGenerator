using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTransformationConditionDto {
        [CanBeNull]
        public CalcLoadTypeDto DstLoadType { get; }

        public double MaxValue { get; }
        [NotNull]
        public string Guid { get; }
        public double MinValue { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        public CalcTransformationConditionType Type { get; }
        [CanBeNull]
        public string EnergyStorageName { get; }
        [CanBeNull]
        public string EnergyStorageGuid { get; }
        //todo: split in two conditiontypes with the same interface to avoid the can be null mess
        public CalcTransformationConditionDto([NotNull]string name, int id, CalcTransformationConditionType type,
                                              [CanBeNull]CalcLoadTypeDto loadType, double minValue, double maxValue, [NotNull] string guid,
                                              [CanBeNull] string energyStorageName, [CanBeNull] string energyStorageGuid)
        {
            Name = name;
            ID = id;
            Type = type;
            DstLoadType = loadType;
            MinValue = minValue;
            MaxValue = maxValue;
            Guid = guid;
            EnergyStorageName = energyStorageName;
            EnergyStorageGuid = energyStorageGuid;
        }
    }
}