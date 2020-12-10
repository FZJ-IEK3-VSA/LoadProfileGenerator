using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Common;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Transportation {
    public class ChargingStationSetPresenter : PresenterBaseDBBase<ChargingStationSetView> {
        [CanBeNull] private ModularHousehold _selectedModularHousehold;
        [CanBeNull] private TravelRouteSet _selectedTravelRouteSet;

        [CanBeNull] private Site _selectedSite;

        public ChargingStationSetPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] ChargingStationSetView view,
                                       [NotNull] ChargingStationSet routeSet) : base(view, "ThisChargingStationSet.Name", routeSet,
            applicationPresenter)
        {
            ThisChargingStationSet = routeSet;
            _selectedModularHousehold = Sim.ModularHouseholds[0];
            _selectedTravelRouteSet = Sim.TravelRouteSets[0];
            RefreshRelevantSites();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> AllHouseholds =>
            Sim.ModularHouseholds.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TravelRouteSet> AllTravelRouteSets => Sim.TravelRouteSets.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Site> RelevantSites { get; } =
            new ObservableCollection<Site>();

        [CanBeNull]
        [UsedImplicitly]
        public ModularHousehold SelectedModularHousehold {
            get => _selectedModularHousehold;
            set {
                if (Equals(value, _selectedModularHousehold)) {
                    return;
                }

                _selectedModularHousehold = value;
                OnPropertyChanged(nameof(SelectedModularHousehold));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Site SelectedSite
        {
            get => _selectedSite;
            set
            {
                if (Equals(value, _selectedSite))
                {
                    return;
                }

                _selectedSite = value;
                OnPropertyChanged(nameof(SelectedSite));
                RefreshRelevantSites();
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TravelRouteSet SelectedTravelRouteSet
        {
            get => _selectedTravelRouteSet;
            set
            {
                if (Equals(value, _selectedTravelRouteSet))
                {
                    return;
                }

                _selectedTravelRouteSet = value;
                OnPropertyChanged(nameof(SelectedTravelRouteSet));
                RefreshRelevantSites();
            }
        }

        public void RefreshRelevantSites()
        {
            if (SelectedModularHousehold == null) {
                return;
            }

            if (SelectedTravelRouteSet == null) {
                return;
            }

            var sites = SelectedTravelRouteSet.TravelRoutes.Select(x => x.TravelRoute.SiteA).ToList();
            var sitesB = SelectedTravelRouteSet.TravelRoutes.Select(x => x.TravelRoute.SiteB).ToList();
            sites.AddRange(sitesB);
            sites = sites.Distinct().ToList();
            var filteredSites = new List<Site>();
            var householdLocs = SelectedModularHousehold.CollectLocations();
            foreach (var site in sites) {
                var locs = site.Locations.Select(x => x.Location).ToList();
                if (locs.Any(x => householdLocs.Contains(x))) {
                    filteredSites.Add(site);
                }
            }
            RelevantSites.SynchronizeWithList(filteredSites);
        }

        [NotNull]
        [UsedImplicitly]
        public ChargingStationSet ThisChargingStationSet { get; }

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> LoadTypes => Sim.LoadTypes.Items;
        [NotNull]
        [ItemNotNull]
        public ObservableCollection<TransportationDeviceCategory> TransportationDeviceCategories => Sim.TransportationDeviceCategories.Items;

        public void Delete()
        {
            Sim.ChargingStationSets.DeleteItem(ThisChargingStationSet);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as ChargingStationSetPresenter;
            return presenter?.ThisChargingStationSet.Equals(ThisChargingStationSet) == true;
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
    }
}