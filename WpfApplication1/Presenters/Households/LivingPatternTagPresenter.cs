using System.Collections.ObjectModel;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class LivingPatternTagPresenter : PresenterBaseDBBase<LivingPatternTagView>
    {
        [NotNull] private readonly LivingPatternTag _thisTag;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIn;

        public LivingPatternTagPresenter(
            [NotNull] ApplicationPresenter applicationPresenter, [NotNull] LivingPatternTagView view,
            [NotNull] LivingPatternTag tag)
            : base(view, "ThisTag.HeaderString", tag, applicationPresenter)
        {
            _thisTag = tag;
            _usedIn = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
        }

        [NotNull]
        public LivingPatternTag ThisTag => _thisTag;
        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIn => _usedIn;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB)
            {
                _thisTag.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.LivingPatternTags.DeleteItem(_thisTag);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as LivingPatternTagPresenter;
            return presenter?.ThisTag.Equals(_thisTag) == true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }

        public void RefreshUsedIn()
        {
            var usedIn =
                _thisTag.CalculateUsedIns(Sim);
            _usedIn.Clear();
            foreach (var p in usedIn)
            {
                _usedIn.Add(p);
            }
        }
    }
}