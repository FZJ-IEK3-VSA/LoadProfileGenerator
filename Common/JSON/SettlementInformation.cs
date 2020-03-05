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
        public SettlementInformation([NotNull] string csvCharacter,
                                     [NotNull] string name,
                                     EnergyIntensityType energyIntensity,
                                     [NotNull] string lpgVersion)
        {
            CSVCharacter = csvCharacter;
            Name = name;
            EnergyIntensity = energyIntensity;
            LPGVersion = lpgVersion;
        }
        [UsedImplicitly]
        [NotNull]
        public string CSVCharacter { get;}
        public EnergyIntensityType EnergyIntensity { get; }

        [UsedImplicitly]
        [NotNull]
        [ItemNotNull]
        public List<HouseInformation> HouseInformations { get; set; } =
            new List<HouseInformation>();

        public int ID { get; set; }
        [UsedImplicitly]
        [NotNull]
        public string LPGVersion { get;  }
        [NotNull]
        public string Name { get; }

        [NotNull]
        public static SettlementInformation Read([NotNull] string path)
        {
            var dstPath = Path.Combine(path, Constants.SettlementJsonName);
            string json;
            using (var sw = new StreamReader(dstPath)) {
                json = sw.ReadToEnd();
            }
            var o = JsonConvert.DeserializeObject<SettlementInformation>(json);
            return o;
        }

        public void WriteResultEntries([NotNull] string path)
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
            public HouseInformation([NotNull] string name, [NotNull] string directory,
[NotNull] string houseType)
            {
                Directory = directory;
                Name = name;
                HouseType = houseType;
            }
            [UsedImplicitly]
            [NotNull]
            public string Directory { get;  }
            [UsedImplicitly]
            [NotNull]
            public string Name { get;  }
            [UsedImplicitly]
            [NotNull]
            [ItemNotNull]
            public List<HouseholdInformation> HouseholdInformations { get; set; } = new List<HouseholdInformation>();
            [UsedImplicitly]
            [NotNull]
            public string HouseType { get;  }
        }

        public class SettlementPersonInformation {
            public SettlementPersonInformation([NotNull] string name, int age, PermittedGender gender, [NotNull] string personTag)
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
            [NotNull]
            public string Name { get; set; }

            [UsedImplicitly]
            [NotNull]
            public string PersonTag { get; set; }
        }

        public class HouseholdInformation {
            public HouseholdInformation([NotNull] string name) => Name = name;

            [UsedImplicitly]
            [NotNull]
            public string Name { get; set; }
            [UsedImplicitly]
            [NotNull]
            [ItemNotNull]
            public List<SettlementPersonInformation> Persons { get; set; } = new List<SettlementPersonInformation>();
            [UsedImplicitly]
            [NotNull]
            [ItemNotNull]
            public List<string> Tags { get; set; } = new List<string>();
        }
    }
}