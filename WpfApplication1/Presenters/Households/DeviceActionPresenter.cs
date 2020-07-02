using System.Collections.ObjectModel;
using System.Globalization;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class DeviceActionPresenter : PresenterBaseDBBase<DeviceActionView> {
        [NotNull] private readonly DeviceAction _deviceAction;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIns;

        public DeviceActionPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] DeviceActionView view,
            [NotNull] DeviceAction deviceAction) : base(view, "ThisDeviceAction.Name", deviceAction, applicationPresenter)
        {
            _deviceAction = deviceAction;
            _usedIns = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
            RefreshEstimates();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceActionGroup> DeviceActionGroups
            => Sim.DeviceActionGroups.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.Items;

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public ObservableCollection<EstimateEntry> EstimateEntries { get; } = new ObservableCollection<EstimateEntry>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> LoadTypes => Sim.LoadTypes.Items;

        [NotNull]
        public DeviceAction ThisDeviceAction => _deviceAction;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeBasedProfile> Timeprofiles
            => Sim.Timeprofiles.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public void Delete()
        {
            Sim.DeviceActions.DeleteItem(_deviceAction);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as DeviceActionPresenter;
            return presenter?.ThisDeviceAction.Equals(_deviceAction) == true;
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

        public void MakeCopy()
        {
            var newaction = Sim.DeviceActions.CreateNewItem(Sim.ConnectionString);
            newaction.Name = _deviceAction.Name + " (Copy)";
            newaction.Description = _deviceAction.Description;
            newaction.Device = _deviceAction.Device;
            newaction.DeviceActionGroup = _deviceAction.DeviceActionGroup;
            newaction.SaveToDB();
            foreach (var actionProfile in _deviceAction.Profiles) {
                if (actionProfile.Timeprofile != null && actionProfile.VLoadType != null) {
                    newaction.AddDeviceProfile(actionProfile.Timeprofile, actionProfile.TimeOffset,
                        actionProfile.VLoadType,
                        actionProfile.Multiplier);
                }
            }

            newaction.SaveToDB();
            ApplicationPresenter.OpenItem(newaction);
        }

        public void RefreshEstimates()
        {
            EstimateEntries.Clear();
            var estimates = ThisDeviceAction.CalculateAverageEnergyUse(null,
                Sim.DeviceActions.Items, null, 1, 1);
            foreach (var estimate in estimates) {
                var entry = new EstimateEntry(estimate.Item1.PrettyName,
                    estimate.Item2.ToString(CultureInfo.CurrentCulture) + " " + estimate.Item1.UnitOfSum);
                EstimateEntries.Add(entry);
            }
        }

        public void RefreshUsedIn()
        {
            var usedIn = ThisDeviceAction.CalculateUsedIns(Sim);
            _usedIns.SynchronizeWithList(usedIn);
        }

        public class EstimateEntry {
            public EstimateEntry([NotNull] string name, [NotNull] string estimate)
            {
                Name = name;
                Estimate = estimate;
            }

            [NotNull]
            [UsedImplicitly]
            public string Estimate { get; }

            [NotNull]
            [UsedImplicitly]
            public string Name { get; }
        }
    }
}