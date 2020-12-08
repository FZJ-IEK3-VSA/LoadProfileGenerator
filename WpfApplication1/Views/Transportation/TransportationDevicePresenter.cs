using System.Collections.ObjectModel;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Transportation {
    public class TransportationDevicePresenter : PresenterBaseDBBase<TransportationDeviceView> {
        [JetBrains.Annotations.NotNull] private readonly TransportationDevice _transportationDevice;

        public TransportationDevicePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] TransportationDeviceView view,
            [JetBrains.Annotations.NotNull] TransportationDevice transportationDevice) : base(view, "ThisTransportationDevice.Name", transportationDevice, applicationPresenter)
        {
            _transportationDevice = transportationDevice;
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> AllLoadTypes => Sim.LoadTypes.Items;

        [JetBrains.Annotations.NotNull]
        public TransportationDevice ThisTransportationDevice => _transportationDevice;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TransportationDeviceCategory> TransportationDeviceCategories => Sim.TransportationDeviceCategories.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns { get; } = new ObservableCollection<UsedIn>();

        public void Delete()
        {
            Sim.TransportationDevices.DeleteItem(_transportationDevice);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            TransportationDevicePresenter presenter = obj as TransportationDevicePresenter;
            return presenter?.ThisTransportationDevice.Equals(_transportationDevice) == true;
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