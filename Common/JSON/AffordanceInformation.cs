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
            public SingleAffordanceInfo(int affordanceID, CalcAffordanceType affordanceType, [NotNull] string affordanceName, [NotNull] string locationName, [NotNull] string timeLimitName, [NotNull] string sourceTrait, TimeSpan personProfileDuration, [NotNull] string affordanceCategory, int affordanceSerial)
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
            [NotNull]
            public string AffordanceName { get;  }
            [NotNull]
            public string LocationName { get;  }
            [NotNull]
            public string TimeLimitName { get;  }

            [NotNull]
            public string SourceTrait { get;  }
            [NotNull]
            public Dictionary<string, string> AffordanceTags { get;  } = new Dictionary<string, string>();
            public TimeSpan PersonProfileDuration { get;  }
            [NotNull]
            [ItemNotNull]
            public List<DeviceInformation> DeviceInformations { get;  } = new List<DeviceInformation>();
            public int AffordanceID { get;  }
            [NotNull]
            public string AffordanceCategory { get;  }
        }

        public class DeviceInformation {
            public DeviceInformation([NotNull] string deviceName, [NotNull] string profileName, int profileTimeOffset, [NotNull] string loadType)
            {
                DeviceName = deviceName;
                ProfileName = profileName;
                ProfileTimeOffset = profileTimeOffset;
                LoadType = loadType;
            }

            [NotNull]
            public string DeviceName { get;  }

            [NotNull]
            public string ProfileName { get;  }
            public int ProfileTimeOffset { get;  }
            [NotNull]
            public string LoadType { get;  }
        }

        [NotNull]
        [ItemNotNull]
        public List<SingleAffordanceInfo> AffordanceInfos { get;  } = new List<SingleAffordanceInfo>();

        [NotNull]
        public static AffordanceInformation Read([NotNull] string fullFilePath)
        {
            string json;
            using (var sw = new StreamReader(fullFilePath)) {
                json = sw.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<AffordanceInformation>(json);
        }

        public void WriteResultEntries([NotNull] StreamWriter sw)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
            sw.WriteLine(json);
        }
    }
}*/