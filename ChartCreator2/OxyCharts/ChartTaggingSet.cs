using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ChartCreator2.OxyCharts {
    public class ChartTaggingSet {
        [NotNull] [ItemNotNull] private List<string> _categories = new List<string>();

        public ChartTaggingSet([NotNull] string name) => Name = name;

        [NotNull]
        public Dictionary<string, string> AffordanceToCategories { get; } = new Dictionary<string, string>();

        [ItemNotNull]
        [NotNull]
        public List<string> Categories {
            get {
                _categories = AffordanceToCategories.Values.Distinct().ToList();
                return _categories;
            }
        }

        [NotNull]
        public string Name { get; }

        public int GetCategoryIndexOfCategory([NotNull] string category) => _categories.IndexOf(category);
        /*
        public int GetCategoryIndexOfItem(string item)
        {
            var category = AffordanceToCategories[item];
            return _categories.IndexOf(category);
        }*/
    }
}