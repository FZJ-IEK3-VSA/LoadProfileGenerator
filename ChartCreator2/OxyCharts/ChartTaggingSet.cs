using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ChartCreator2.OxyCharts {
    public class ChartTaggingSet {
        [JetBrains.Annotations.NotNull] [ItemNotNull] private List<string> _categories = new List<string>();

        public ChartTaggingSet([JetBrains.Annotations.NotNull] string name) => Name = name;

        [JetBrains.Annotations.NotNull]
        public Dictionary<string, string> AffordanceToCategories { get; } = new Dictionary<string, string>();

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<string> Categories {
            get {
                _categories = AffordanceToCategories.Values.Distinct().ToList();
                return _categories;
            }
        }

        [JetBrains.Annotations.NotNull]
        public string Name { get; }

        public int GetCategoryIndexOfCategory([JetBrains.Annotations.NotNull] string category) => _categories.IndexOf(category);
        /*
        public int GetCategoryIndexOfItem(string item)
        {
            var category = AffordanceToCategories[item];
            return _categories.IndexOf(category);
        }*/
    }
}