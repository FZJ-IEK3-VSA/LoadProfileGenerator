using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using JetBrains.Annotations;

namespace Common.JSON {
    public class AffordanceTaggingSetInformation {
        [NotNull] private readonly Dictionary<string, string> _affordanceToTagDict;

        [ItemNotNull] [NotNull] private readonly List<LoadTypeInformation> _loadTypes = new List<LoadTypeInformation>();

        [ItemNotNull] [NotNull] private readonly List<ReferenceEntry> _referenceEntries = new List<ReferenceEntry>();

        public AffordanceTaggingSetInformation([NotNull] string name, bool makeCharts)
        {
            Name = name;
            MakeCharts = makeCharts;
            _affordanceToTagDict = new Dictionary<string, string>();
        }

        [NotNull]
        public Dictionary<string, string> AffordanceToTagDict => _affordanceToTagDict;

        [NotNull]
        public Dictionary<string, ColorRGB> Colors { get; } = new Dictionary<string, ColorRGB>();

        public bool HasReferenceEntries => _referenceEntries.Count > 0;

        [NotNull]
        [ItemNotNull]
        public List<LoadTypeInformation> LoadTypes => _loadTypes;

        [UsedImplicitly]
        [NotNull]
        [ItemNotNull]
        public List<LoadTypeInformation> LoadTypesForThisSet { get; set; } = new List<LoadTypeInformation>();

        public bool MakeCharts { get; }

        [NotNull]
        public string Name { get; }

        [UsedImplicitly]
        [NotNull]
        public Dictionary<string, double> ReferenceValues { get; set; } = new Dictionary<string, double>();

        [UsedImplicitly]
        [NotNull]
        public Dictionary<string, string> TagByDeviceName { get; set; } = new Dictionary<string, string>();

        public void AddReference([NotNull] string tagName, PermittedGender gender, int minage, int maxAge,
                                 double referenceValue)
        {
            var re = new ReferenceEntry(tagName, gender, minage, maxAge, referenceValue);
            _referenceEntries.Add(re);
        }

        public void AddRefValue([NotNull] string referenceValue, double value, [NotNull] string loadtypename)
        {
            ReferenceValues.Add(MakeKey(loadtypename, referenceValue), value);
        }

        public void AddTag([NotNull] string affordanceName, [NotNull] string tagName)
        {
            _affordanceToTagDict.Add(affordanceName, tagName);
        }

        public double LookupReferenceValue([NotNull] string tagName, PermittedGender gender, int age)
        {
            var re =
                _referenceEntries.FirstOrDefault(
                    x => x.Tag == tagName && x.Gender == gender && age >= x.MinAge && age <= x.MaxAge);
            if (re != null) {
                return re.ReferenceValue;
            }

            return 0;
        }

        [NotNull]
        public static string MakeKey([NotNull] string loadType, [NotNull] string referenceValue) =>
            loadType + "#" + referenceValue;

        [NotNull]
        [ItemNotNull]
        public List<string> MakeReferenceTags()
        {
            return _referenceEntries.Select(x => x.Tag).ToList();
        }

        private class ReferenceEntry {
            public ReferenceEntry([NotNull] string tag, PermittedGender gender, int minAge, int maxAge,
                                  double referenceValue)
            {
                Tag = tag;
                Gender = gender;
                MinAge = minAge;
                MaxAge = maxAge;
                ReferenceValue = referenceValue;
            }

            public PermittedGender Gender { get; }
            public int MaxAge { get; }
            public int MinAge { get; }
            public double ReferenceValue { get; }

            [NotNull]
            public string Tag { get; }
        }
    }
}