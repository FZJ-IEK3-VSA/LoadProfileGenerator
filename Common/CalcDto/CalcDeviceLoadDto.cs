using System;
using System.Globalization;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcDeviceLoadDto
    {
        public CalcDeviceLoadDto([NotNull]string name, int id, [NotNull]string loadTypeName, [NotNull] StrGuid loadTypeGuid,
                                 double averageYearlyConsumption, double powerStandardDeviation, [NotNull] StrGuid guid,
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
            if (Math.Abs(MaxPower) < 0.000000001 && loadTypeName.ToLower(CultureInfo.InvariantCulture) != "none") {
                throw new LPGException("Trying to initialize a Device load with a max power of 0. Device Name was " + name + " and load type " + loadTypeName );
            }
        }
        public double AverageYearlyConsumption { get; }

        public double PowerStandardDeviation { get; }
        [NotNull]
        public StrGuid Guid { get; }
        public double MaxPower { get; }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public StrGuid LoadTypeGuid { get; }
        [NotNull]
        public string LoadTypeName { get; }
    }
}