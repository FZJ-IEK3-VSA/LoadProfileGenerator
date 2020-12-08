using System.Collections.ObjectModel;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Transportation {
    public class TransportationDeviceCategoryPresenter : PresenterBaseDBBase<TransportationDeviceCategoryView> {
        [JetBrains.Annotations.NotNull] private readonly TransportationDeviceCategory _transportationDeviceCategory;

        public TransportationDeviceCategoryPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] TransportationDeviceCategoryView view,
            [JetBrains.Annotations.NotNull] TransportationDeviceCategory category) : base(view, "ThisCategory.Name", category, applicationPresenter)
        {
            _transportationDeviceCategory = category;
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Location> Locations => Sim.Locations.Items;

        [JetBrains.Annotations.NotNull]
        public TransportationDeviceCategory ThisCategory => _transportationDeviceCategory;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns { get; } = new ObservableCollection<UsedIn>();

        public void Delete()
        {
            Sim.TransportationDeviceCategories.DeleteItem(_transportationDeviceCategory);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            TransportationDeviceCategoryPresenter presenter = obj as TransportationDeviceCategoryPresenter;
            return presenter?.ThisCategory.Equals(_transportationDeviceCategory) == true;
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

        //public void RefreshUsedIn()
        //{
        //TODO: Fix and implmenet
        /*
            Simulator s = Sim;
            List<UsedIn> usedIn = T.GetUsedIns(s.Households.MyItems, s.HouseholdTraits.It,
                s.Affordances.It, s.HouseTypes.It, s.DeviceActions.It);
            _usedIns.Clear();
            foreach (UsedIn dui in usedIn)
                _usedIns.Add(dui);*/
        //}
    }
}