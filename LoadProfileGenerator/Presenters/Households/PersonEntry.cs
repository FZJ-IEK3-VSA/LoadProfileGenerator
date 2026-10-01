using Database.Tables.BasicHouseholds;
using LoadProfileGenerator.Controls;

namespace LoadProfileGenerator.Presenters.Households {
    public class PersonEntry : Notifier {
        private bool _isChecked;

        public PersonEntry([JetBrains.Annotations.NotNull] Person person, bool isChecked) {
            Person = person;
            _isChecked = isChecked;
        }

        public bool IsChecked {
            get => _isChecked;
            set {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        [JetBrains.Annotations.NotNull]
        public Person Person { get; }
    }
}