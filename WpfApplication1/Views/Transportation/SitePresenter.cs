using System.Collections.ObjectModel;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Transportation
{
    public class SitePresenter : PresenterBaseDBBase<SiteView>
    {
        [NotNull] private readonly Site _site;

        public SitePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] SiteView view, [NotNull] Site site) : base(view,
            "ThisSite.Name", site, applicationPresenter)
        {
            _site = site;
            RefreshUsedIn();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Location> Locations => Sim.Locations.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> LoadTypes => Sim.LoadTypes.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransportationDeviceCategory> TransportationDeviceCategories => Sim.TransportationDeviceCategories.MyItems;

        [NotNull]
        public Site ThisSite => _site;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns { get; } = new ObservableCollection<UsedIn>();

        public void Delete()
        {
            Sim.Sites.DeleteItem(_site);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            return (obj is SitePresenter presenter) && presenter.ThisSite.Equals(_site);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }

        public void RefreshUsedIn()
        {
            var u = ThisSite.CalculateUsedIns(Sim);
            UsedIns.SynchronizeWithList(u);
        }

        public void MakeCopy()
        {
            var newSite = ThisSite.MakeCopy(Sim);
            ApplicationPresenter.OpenItem(newSite);
        }
    }
}