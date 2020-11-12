using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcDeviceLoadDto
    {
        public CalcDeviceLoadDto([NotNull]string name, int id, [NotNull]string loadTypeName, StrGuid loadTypeGuid,
                                 double averageYearlyConsumption, double powerStandardDeviation, StrGuid guid,
                                 double maxPower)
        {
            Name = name;
            ID = id;
            LoadTypeGuid = loadTypeGuid;
            LoadTypeName = loadTypeName;
            AverageYearlyConsumption = averageYearlyConsumption;
            PowerStandardDeviation = powerStandardDeviation;
            Guid = guid;
            MaxPower = maxPower;
            //power of 0 is actually ok for air conditioners
            //if (Math.Abs(MaxPower) < 0.000000001 && loadTypeName.ToLower(CultureInfo.InvariantCulture) != "none") {
                //throw new LPGException("Trying to initialize a Device load with a max power of 0. Device Name was " + name + " and load type " + loadTypeName );
            //}
        }
        public double AverageYearlyConsumption { get; }

        public double PowerStandardDeviation { get; }
        public StrGuid Guid { get; }
        public double MaxPower { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        public StrGuid LoadTypeGuid { get; }
        [NotNull]
        public string LoadTypeName { get; }

        [NotNull]
        public override string ToString() => Name + " - " + LoadTypeName + " " + MaxPower + " Power";
    }
}
