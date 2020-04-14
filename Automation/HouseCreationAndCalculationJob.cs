using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Automation {
    public class HouseCreationAndCalculationJob
    {
        // ReSharper disable once NotNullMemberIsNotInitialized
        public HouseCreationAndCalculationJob( string scenario, string year, string districtname)
        {
            Scenario = scenario;
            Year = year;
            DistrictName = districtname;
        }

        //for json loading
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseCreationAndCalculationJob()
        {
        }

        [CanBeNull]
        public HouseData House { get; set; }
        public JsonCalcSpecification CalcSpec { get; set; }

        public string Scenario { get; set; }
        public string Year { get; set; }
        public string DistrictName { get; set; }


        [Comment(
            "Path to the database file to use. Defaults to profilegenerator.db3 in the current directry if not set.")]
        [CanBeNull]
        public string PathToDatabase { get; set; } = "profilegenerator.db3";
        [NotNull]
        public static HouseCreationAndCalculationJob LoadFromFile([NotNull] string inputFile)
        {
            string s = File.ReadAllText(inputFile);
            var jcs = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(s);
            return jcs;
        }

    }
}