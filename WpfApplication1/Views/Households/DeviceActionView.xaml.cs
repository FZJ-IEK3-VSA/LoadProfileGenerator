using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceActionView {
        public DeviceActionView()
        {
            InitializeComponent();
        }

        [NotNull]
        private DeviceActionPresenter Presenter => (DeviceActionPresenter) DataContext;

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTimeprofiles.SelectedItem == null) {
                Logger.Error("Please select a time profile first!");
                return;
            }

            if (CmbVLoadtypes.SelectedItem == null) {
                Logger.Error("Please select a load type first!");
                return;
            }

            if (!(CmbTimeprofiles.SelectedItem is TimeBasedProfile tp)) {
                Logger.Error("Time based profile was null");
                return;
            }

            if (!(CmbVLoadtypes.SelectedItem is VLoadType lt)) {
                Logger.Error("Load type was null");
                return;
            }

            var timeoffsetstr = TxtTimeOffset.Text;
            var success = decimal.TryParse(timeoffsetstr, out decimal d);
            if (!success) {
                Logger.Error("Could not convert " + timeoffsetstr);
            }

            var multistring = TxtMultiplier.Text;
            success = double.TryParse(multistring, out double d2);
            if (!success) {
                Logger.Error("Could not convert " + multistring);
            }

            Presenter.ThisDeviceAction.AddDeviceProfile(tp, d, lt, d2);
        }

        private void BtnBackToDevice_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            if (Presenter.ThisDeviceAction.Device == null) {
                Logger.Info("No Device selected at device action");
                return;
            }
            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            if (!(ui.Item is Affordance aff)) {
                return;
            }

            var da = Presenter.ThisDeviceAction;
            var affprofs = aff.AffordanceDevices.Where(x => x.Device == da).ToList();
            foreach (var affordanceDevice in affprofs) {
                foreach (var deviceActionProfile in da.Profiles) {
                    if (da.Device == null)
                    {
                        Logger.Info("Device was null");
                        return;
                    }
                    aff.AddDeviceProfile(da.Device, deviceActionProfile.Timeprofile, affordanceDevice.TimeOffset,
                        Presenter.Sim.RealDevices.It,
                        Presenter.Sim.DeviceCategories.It, deviceActionProfile.VLoadType,
                        affordanceDevice.Probability);
                }

                aff.DeleteDeviceFromDB(affordanceDevice);
            }

            Presenter.RefreshUsedIn();
        }

        private void BtnFixElectricalDevice_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var da = Presenter.ThisDeviceAction;
            var elec =
                da.Profiles.FirstOrDefault(x => x.VLoadType?.Name.ToUpperInvariant().Contains("ELECTRICITY") == true);
            if (elec == null) {
                Logger.Error("Could not find the electric load type. Please fix.");
                return;
            }

            var app =
                da.Profiles.FirstOrDefault(x => x.VLoadType?.Name.ToUpperInvariant().Contains("APPARENT") == true);
            var reac =
                da.Profiles.FirstOrDefault(x => x.VLoadType?.Name.ToUpperInvariant().Contains("REACTIVE") == true);

            var placeholder = Presenter.Sim.Timeprofiles.FindByName("placeholder", FindMode.Partial);

            // apparent
            var appLoad = Presenter.Sim.LoadTypes.FindByName("apparent", FindMode.Partial);
            if (appLoad == null) {
                Logger.Error("Could not find the apparent load type. Please fix.");
                return;
            }

            if (elec.Timeprofile != null) {
                if (app == null) {
                    da.AddDeviceProfile(elec.Timeprofile, elec.TimeOffset, appLoad, elec.Multiplier);
                }
                else if (app.Timeprofile == placeholder) {
                    da.AddDeviceProfile(elec.Timeprofile, elec.TimeOffset, appLoad, elec.Multiplier);
                }
            }

            // reactive
            var reacLoad = Presenter.Sim.LoadTypes.FindByName("reactive", FindMode.Partial);
            if (reacLoad == null) {
                Logger.Error("Could not find the reactive load type. Please fix.");
                return;
            }

            if (elec.Timeprofile == null) {
                Logger.Error("Could not find the electric load type time profile. Please fix.");
                return;
            }

            if (reac == null || reac.Timeprofile == placeholder) {
                if (elec.Timeprofile.TimeProfileType == TimeProfileType.Relative) {
                    da.AddDeviceProfile(elec.Timeprofile, elec.TimeOffset, reacLoad, elec.Multiplier);
                }
                else {
                    da.AddDeviceProfile(elec.Timeprofile, elec.TimeOffset, reacLoad, elec.Multiplier * 0.32);
                }
            }
        }

        private void BtnRefreshEstimates_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshEstimates();

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUsedIn();

        private void BtnRemoveDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstProfiles.SelectedItem == null) {
                Logger.Error("No profile was selected.");
                return;
            }

            if (!(LstProfiles.SelectedItem is DeviceActionProfile dap)) {
                Logger.Error("No profile was selected.");
                return;
            }

            Presenter.ThisDeviceAction.DeleteProfileFromDB(dap);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisDeviceAction.Name, Presenter.Delete);

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void LstProfiles_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstProfiles.SelectedItem as DeviceActionProfile;
            if (ui?.Timeprofile != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.Timeprofile);
            }
        }

        private void MakeCopy_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.MakeCopy();

        private void OpenDAG_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDeviceActionGroup.SelectedItem == null) {
                return;
            }

            var dag = (DeviceActionGroup) CmbDeviceActionGroup.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(dag);
        }

        private void OpenDevice_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDevices.SelectedItem == null) {
                return;
            }

            var rd = (RealDevice) CmbDevices.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(rd);
        }
    }
}