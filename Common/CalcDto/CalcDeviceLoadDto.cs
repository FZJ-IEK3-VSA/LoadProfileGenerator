using System;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcDeviceLoadDto
    {
        public CalcDeviceLoadDto([NotNull]string name, int id, [NotNull]string loadTypeName, [NotNull] string loadTypeGuid,
                                 double averageYearlyConsumption, double powerStandardDeviation, [NotNull] string guid,
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
            if (Math.Abs(MaxPower) < 0.000000001 && loadTypeName.ToLower()!= "none") {
                throw new LPGException("Trying to initialize a Device load with a max power of 0. Device Name was " + name + " and load type " + loadTypeName );
            }
        }
        public double AverageYearlyConsumption { get; }

        public double PowerStandardDeviation { get; }
        [NotNull]
        public string Guid { get; }
        public double MaxPower { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public string LoadTypeGuid { get; }
        [NotNull]
        public string LoadTypeName { get; }
    }
}