using System.Collections.ObjectModel;
using Database.Tables.BasicElements;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Transportation {
    public class TransportationDeviceSetPresenter : PresenterBaseDBBase<TransportationDeviceSetView> {
        [NotNull] private readonly TransportationDeviceSet _transporatationDeviceSet;

        public TransportationDeviceSetPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TransportationDeviceSetView view,
            [NotNull] TransportationDeviceSet set) : base(view, "ThisSet.Name", set, applicationPresenter)
        {
            _transporatationDeviceSet = set;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransportationDevice> AllTransportationDevices => Sim.TransportationDevices.Items;

        [NotNull]
        [UsedImplicitly]
        public TransportationDeviceSet ThisSet => _transporatationDeviceSet;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns { get; } = new ObservableCollection<UsedIn>();

        public void Delete()
        {
            Sim.TransportationDeviceSets.DeleteItem(_transporatationDeviceSet);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            TransportationDeviceSetPresenter presenter = obj as TransportationDeviceSetPresenter;
            return presenter?.ThisSet.Equals(_transporatationDeviceSet) == true;
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