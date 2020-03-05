using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public class DistrictData {

        public DistrictData([NotNull] string name)
        {
            Name = name;
        }

        //for json loading
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public DistrictData()
        {
        }

        [NotNull]
        [ItemNotNull]
        public List<HouseData> Houses { get; set; } = new List<HouseData>();
        [NotNull]
        public string Name { get; set; }

        public bool SkipExistingHouses { get; set; }
    }
}