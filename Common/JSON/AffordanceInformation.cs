namespace Common.JSON {
    public enum CalcAffordanceType {
        Affordance,
        Subaffordance,
        Vacation
    }
}
/*
    public class AffordanceInformation {
        public class SingleAffordanceInfo
        {
            public SingleAffordanceInfo(int affordanceID, CalcAffordanceType affordanceType, [JetBrains.Annotations.NotNull] string affordanceName, [JetBrains.Annotations.NotNull] string locationName, [JetBrains.Annotations.NotNull] string timeLimitName, [JetBrains.Annotations.NotNull] string sourceTrait, TimeSpan personProfileDuration, [JetBrains.Annotations.NotNull] string affordanceCategory, int affordanceSerial)
            {
                AffordanceID = affordanceID;
                AffordanceType = affordanceType;
                AffordanceName = affordanceName;
                LocationName = locationName;
                TimeLimitName = timeLimitName;
                SourceTrait = sourceTrait;
                PersonProfileDuration = personProfileDuration;
                AffordanceCategory = affordanceCategory;
                AffordanceSerial = affordanceSerial;
            }

            [JsonIgnore]
            public CalcAffordanceType AffordanceType { get;  }
            public int AffordanceSerial { get;  }
            [JetBrains.Annotations.NotNull]
            public string AffordanceName { get;  }
            [JetBrains.Annotations.NotNull]
            public string LocationName { get;  }
            [JetBrains.Annotations.NotNull]
            public string TimeLimitName { get;  }

            [JetBrains.Annotations.NotNull]
            public string SourceTrait { get;  }
            [JetBrains.Annotations.NotNull]
            public Dictionary<string, string> AffordanceTags { get;  } = new Dictionary<string, string>();
            public TimeSpan PersonProfileDuration { get;  }
            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<DeviceInformation> DeviceInformations { get;  } = new List<DeviceInformation>();
            public int AffordanceID { get;  }
            [JetBrains.Annotations.NotNull]
            public string AffordanceCategory { get;  }
        }

        public class DeviceInformation {
            public DeviceInformation([JetBrains.Annotations.NotNull] string deviceName, [JetBrains.Annotations.NotNull] string profileName, int profileTimeOffset, [JetBrains.Annotations.NotNull] string loadType)
            {
                DeviceName = deviceName;
                ProfileName = profileName;
                ProfileTimeOffset = profileTimeOffset;
                LoadType = loadType;
            }

            [JetBrains.Annotations.NotNull]
            public string DeviceName { get;  }

            [JetBrains.Annotations.NotNull]
            public string ProfileName { get;  }
            public int ProfileTimeOffset { get;  }
            [JetBrains.Annotations.NotNull]
            public string LoadType { get;  }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<SingleAffordanceInfo> AffordanceInfos { get;  } = new List<SingleAffordanceInfo>();

        [JetBrains.Annotations.NotNull]
        public static AffordanceInformation Read([JetBrains.Annotations.NotNull] string fullFilePath)
        {
            string json;
            using (var sw = new StreamReader(fullFilePath)) {
                json = sw.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<AffordanceInformation>(json);
        }

        public void WriteResultEntries([JetBrains.Annotations.NotNull] StreamWriter sw)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
            sw.WriteLine(json);
        }
    }
}*/