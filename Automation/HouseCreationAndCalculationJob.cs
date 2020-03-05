using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public class HouseCreationAndCalculationJob
    {
        // ReSharper disable once NotNullMemberIsNotInitialized
        public HouseCreationAndCalculationJob( string scenario, string year, string trafokreis)
        {
            Scenario = scenario;
            Year = year;
            Trafokreis = trafokreis;
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
        public string Trafokreis { get; set; }
    }
}