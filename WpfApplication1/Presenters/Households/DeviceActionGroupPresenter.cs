using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    internal class DeviceActionGroupPresenter : PresenterBaseDBBase<DeviceActionGroupView> {
        [JetBrains.Annotations.NotNull] private readonly DeviceActionGroup _deviceActionGroup;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<UsedIn> _usedIns;

        public DeviceActionGroupPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] DeviceActionGroupView view,
            [JetBrains.Annotations.NotNull] DeviceActionGroup deviceActionGroup)
            : base(view, "ThisDeviceActionGroup.Name", deviceActionGroup, applicationPresenter)
        {
            _deviceActionGroup = deviceActionGroup;
            _usedIns = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
        }

        [JetBrains.Annotations.NotNull]
        public DeviceActionGroup ThisDeviceActionGroup => _deviceActionGroup;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _deviceActionGroup.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.DeviceActionGroups.DeleteItem(_deviceActionGroup);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            return obj is DeviceActionGroupPresenter presenter && presenter.ThisDeviceActionGroup.Equals(_deviceActionGroup);
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
            var s = Sim;
            var usedIn = _deviceActionGroup.CalculateUsedIns(s);
            _usedIns.Clear();
            foreach (var dui in usedIn) {
                _usedIns.Add(dui);
            }
        }
    }
}