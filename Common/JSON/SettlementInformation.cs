using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Common.Enums;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SettlementInformation {
        public SettlementInformation([JetBrains.Annotations.NotNull] string csvCharacter,
                                     [JetBrains.Annotations.NotNull] string name,
                                     EnergyIntensityType energyIntensity,
                                     [JetBrains.Annotations.NotNull] string lpgVersion)
        {
            CSVCharacter = csvCharacter;
            Name = name;
            EnergyIntensity = energyIntensity;
            LPGVersion = lpgVersion;
        }
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string CSVCharacter { get;}
        public EnergyIntensityType EnergyIntensity { get; }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<HouseInformation> HouseInformations { get; set; } =
            new List<HouseInformation>();

        public int ID { get; set; }
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string LPGVersion { get;  }
        [JetBrains.Annotations.NotNull]
        public string Name { get; }

        [JetBrains.Annotations.NotNull]
        public static SettlementInformation Read([JetBrains.Annotations.NotNull] string path)
        {
            var dstPath = Path.Combine(path, Constants.SettlementJsonName);
            string json;
            using (var sw = new StreamReader(dstPath)) {
                json = sw.ReadToEnd();
            }
            var o = JsonConvert.DeserializeObject<SettlementInformation>(json);
            return o;
        }

        public void WriteResultEntries([JetBrains.Annotations.NotNull] string path)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
            var dstPath = Path.Combine(path, Constants.SettlementJsonName);
            using (var sw = new StreamWriter(dstPath)) {
                sw.WriteLine(json);
            }
        }

        public class HouseInformation {
            public HouseInformation([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string directory,
[JetBrains.Annotations.NotNull] string houseType)
            {
                Directory = directory;
                Name = name;
                HouseType = houseType;
            }
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public string Directory { get;  }
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public string Name { get;  }
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<HouseholdInformation> HouseholdInformations { get; set; } = new List<HouseholdInformation>();
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public string HouseType { get;  }
        }

        public class SettlementPersonInformation {
            public SettlementPersonInformation([JetBrains.Annotations.NotNull] string name, int age, PermittedGender gender, [JetBrains.Annotations.NotNull] string personTag)
            {
                Name = name;
                Age = age;
                Gender = gender;
                PersonTag = personTag;
            }

            [UsedImplicitly]
            public int Age { get; set; }
            [UsedImplicitly]
            public PermittedGender Gender { get; set; }
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public string Name { get; set; }

            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public string PersonTag { get; set; }
        }

        public class HouseholdInformation {
            public HouseholdInformation([JetBrains.Annotations.NotNull] string name) => Name = name;

            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            public string Name { get; set; }
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<SettlementPersonInformation> Persons { get; set; } = new List<SettlementPersonInformation>();
            [UsedImplicitly]
            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<string> Tags { get; set; } = new List<string>();
        }
    }
}