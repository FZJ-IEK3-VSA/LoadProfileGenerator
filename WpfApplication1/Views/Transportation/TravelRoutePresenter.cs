using System;
using System.Collections.ObjectModel;
using Database.Tables.BasicElements;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Transportation
{
    public class TravelRoutePresenter : PresenterBaseDBBase<TravelRouteView>
    {
        [NotNull] private readonly TravelRoute _travelRoute;

        public TravelRoutePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TravelRouteView view,
            [NotNull] TravelRoute route) : base(view, "ThisRoute.Name", route, applicationPresenter)
        {
            _travelRoute = route;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Site> Sites => Sim.Sites.MyItems;

        [NotNull]
        [UsedImplicitly]
        public TravelRoute ThisRoute => _travelRoute;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransportationDeviceCategory> TransportationDeviceCategories => Sim
            .TransportationDeviceCategories.It;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns { get; } = new ObservableCollection<UsedIn>();

        public void Delete()
        {
            Sim.TravelRoutes.DeleteItem(_travelRoute);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            TravelRoutePresenter presenter = obj as TravelRoutePresenter;
            return presenter?.ThisRoute.Equals(_travelRoute) == true;
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

        public static void RefreshUsedIn()
        {
            throw new NotImplementedException();
            //TODO: Fix
            /*
            Simulator s = Sim;
            List<UsedIn> usedIn = T.GetUsedIns(s.Households.MyItems, s.HouseholdTraits.It,
                s.Affordances.It, s.HouseTypes.It, s.DeviceActions.It);
            _usedIns.Clear();
            foreach (UsedIn dui in usedIn)
                _usedIns.Add(dui);*/
        }

        public void CopyRoute()
        {
            TravelRoute newRoute = ThisRoute.MakeACopy(Sim);
            ApplicationPresenter.OpenItem(newRoute);
        }
    }
}