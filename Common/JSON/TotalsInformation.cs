/*using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON {
    public class TotalsInformation {
        [UsedImplicitly]
        [NotNull]
        [ItemNotNull]
        public List<HouseholdEntry> HouseholdEntries { get; set; } = new List<HouseholdEntry>();

        public class HouseholdEntry {
            [CanBeNull]
            public HouseholdKey HouseholdKey { get; set; }
            [UsedImplicitly]
            [NotNull]
            [ItemNotNull]
            public List<LoadTypeEntry> LoadTypeEntries { get; set; } = new List<LoadTypeEntry>();
            [CanBeNull]
            public string Name { get; set; }
            [CanBeNull]
            public string PureName { get; set; }
        }

        public class LoadTypeEntry {
            public LoadTypeEntry([NotNull] LoadTypeInformation loadTypeInformation, double total)
            {
                LoadTypeInformation = loadTypeInformation;
                Total = total;
            }

            [NotNull]
            public LoadTypeInformation LoadTypeInformation { get; set; }
            public double Total { get; set; }
        }

        [NotNull]
        public static TotalsInformation Read([CanBeNull] string path)
        {
            if(path == null) {
                throw new LPGException("Path was null");
            }

            var dstPath = Path.Combine(path, Constants.TotalsJsonName);
            string json;
            using (var sw = new StreamReader(dstPath))
            {
                json = sw.ReadToEnd();
            }
            var o = JsonConvert.DeserializeObject<TotalsInformation>(json);
            return o;
        }

        public void WriteResultEntries([NotNull] StreamWriter sw)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            });
            sw.WriteLine(json);
            //var dstPath = Path.Combine(path, Constants.TotalsJsonName);
            //using (var sw = new StreamWriter(dstPath))
            //{
              //  sw.WriteLine(json);
            //}
        }
    }
}*/