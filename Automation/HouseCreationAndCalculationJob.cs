using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public enum HouseDefinitionType {
        HouseData = 0,
        HouseName = 1
    }


    public class HouseReference {
        public HouseReference(JsonReference house) => House = house;

        [Obsolete("json only")]
#pragma warning disable 8618
        public HouseReference()
#pragma warning restore 8618
        {
        }

        public JsonReference House {
            get;
            set;
        }
    }
    public class HouseCreationAndCalculationJob
    {
        // ReSharper disable once NotNullMemberIsNotInitialized
        public HouseCreationAndCalculationJob( string scenario, string year, string? districtname,
                                               HouseDefinitionType houseDefinitionType)
        {
            Scenario = scenario;
            Year = year;
            DistrictName = districtname;
            HouseDefinitionType = houseDefinitionType;
        }

        //for json loading
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseCreationAndCalculationJob()
        {
        }

        [CanBeNull]
        public HouseData? House { get; set; }
        public JsonCalcSpecification? CalcSpec { get; set; }

        public HouseDefinitionType HouseDefinitionType { get; set; }
        public HouseReference? HouseReference { get; set; }
        public string? Scenario { get; set; }
        public string? Year { get; set; }
        public string? DistrictName { get; set; }


        [Comment(
            "Path to the database file to use. Defaults to profilegenerator.db3 in the current directory if not set.")]
        public string? PathToDatabase { get; set; } = "profilegenerator.db3";

    }
}