using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Houses;

namespace LoadProfileGenerator.Views.Houses
{
    /// <summary>
    ///     Interaktionslogik für SettlementTemplate.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
#pragma warning disable S110 // Inheritance tree of classes should not be too deep
    public partial class SettlementTemplateView
    {
#pragma warning restore S110 // Inheritance tree of classes should not be too deep
        public SettlementTemplateView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private SettlementTemplatePresenter Presenter => (SettlementTemplatePresenter)DataContext;

        private void BtnAddAllTemplate([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AddAllTemplates();

        private void BtnAddHouseSize([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.HSDMinimum == 0 || Presenter.HSDMaximum == 0 ||
                Math.Abs(Presenter.HSDPercent) < 0.000001)
            {
                return;
            }
            Presenter.ThisTemplate.AddHouseSize(Presenter.HSDMinimum, Presenter.HSDMaximum, Presenter.HSDPercent);
        }

        private void BtnAddHouseType([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.HTSelection == null)
            {
                return;
            }
            Presenter.ThisTemplate.AddHouseType(Presenter.HTSelection);
        }

        private void BtnAddManyHouseTypes([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AddManyHousetypes();

        private void BtnAddPercentage([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (!TxtMinNumberOfPersons.CheckTextBox("No minimum number of persons entered!"))
            {
                return;
            }
            if (!TxtMaxNumberOfPersons.CheckTextBox("No maximum number of persons entered!"))
            {
                return;
            }
            if (!TxtHouseholdsPercent.CheckTextBox("No percentage entered"))
            {
                return;
            }
            if (!CmbEnergyIntensity.CheckCombobox("No energy intensity selected."))
            {
                return;
            }
            if (Presenter.AllTags.All(x => !x.IsChecked))
            {
                return;
            }
            var percent = Presenter.HHDPercent;
            var hhd = Presenter.ThisTemplate.AddHouseholdDistribution(Presenter.HHDMinPersons,
                Presenter.HHDMaxPersons, percent, Presenter.HHDIntensity.EnergyIntensityType);
            foreach (var entry in Presenter.AllTags)
            {
                if (entry.IsChecked)
                {
                    hhd.AddTag(entry.Tag);
                }
            }
        }

        private void BtnAddTemplate([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.HouseholdTemplateSelection == null)
            {
                return;
            }

            Presenter.ThisTemplate.AddHouseholdTemplate(Presenter.HouseholdTemplateSelection);
        }

        private void BtnAddTraitLimit([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.LimitHouseholdTrait == null)
            {
                return;
            }
            Presenter.ThisTemplate.AddTraitLimit(Presenter.LimitHouseholdTrait, Presenter.LimitMaximum);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void BtnCreateSettlement([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            try
            {
                Presenter.CreateSettlement();
            }
            catch (DataIntegrityException dix)
            {
                MessageWindowHandler.Mw.ShowDataIntegrityMessage(dix);
            }
            catch (Exception ex)
            {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        private void BtnDeleteAllHouses([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.DeleteGeneratedHouses();

        private void BtnDeleteAllSettlements([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.DeleteGeneratedSettlements();

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void BtnMakePreview([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var pbw =
                new ProgressbarWindow("Making preview", "Making Preview and creating households", 1);
            pbw.Show();
            var stp = Presenter;

            void MakePreview()
            {
                try {
                    stp.GeneratePreview();
                    Logger.Get().SafeExecuteWithWait(() => LstHouseEntries.ResizeColummns());
                    if (pbw.IsVisible) {
                        Logger.Get().SafeExecuteWithWait(pbw.Close);
                    }
                }
                catch (DataIntegrityException dix) {
                    Logger.Error(dix.Message);
                    MessageWindowHandler.Mw.ShowDataIntegrityMessage(dix);
                }
                catch (Exception ex) {
                    Logger.Exception(ex);
                    MessageWindowHandler.Mw.ShowDebugMessage(ex);
                }
            }

            var t = new Thread(() => MakePreview());
            t.Start();
        }

        private void BtnRefreshHouses([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshGeneratedHouses();

        private void BtnRefreshSettlements([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshGeneratedSettlements();

        private void BtnRemoveHouseSize([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseSizes.SelectedItem == null)
            {
                return;
            }
            var sths = (STHouseSize)LstHouseSizes.SelectedItem;
            Presenter.ThisTemplate.DeleteHouseSize(sths);
        }

        private void BtnRemoveHouseType([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseTypes.SelectedItem == null)
            {
                return;
            }
            var stht = (STHouseType)LstHouseTypes.SelectedItem;
            Presenter.ThisTemplate.DeleteHouseType(stht);
        }

        private void BtnRemovePercentage([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseholdDistribution.SelectedItem == null)
            {
                return;
            }
            var sthd = (STHouseholdDistribution)LstHouseholdDistribution.SelectedItem;
            Presenter.ThisTemplate.DeleteHouseholdDistribution(sthd);
        }

        private void BtnRemoveTemplate([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseholdTemplates.SelectedItem == null)
            {
                Logger.Error("No template was selected.");
                return;
            }
            var stht = (STHouseholdTemplate)LstHouseholdTemplates.SelectedItem;

            Presenter.ThisTemplate.DeleteHouseholdTemplate(stht);
        }

        private void BtnRemoveTraitLimit([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTraitLimits.SelectedItem == null)
            {
                return;
            }
            var stl = (STTraitLimit)LstTraitLimits.SelectedItem;
            Presenter.ThisTemplate.DeleteTraitLimit(stl);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AskDeleteQuestion(
            Presenter.ThisTemplate.HeaderString, Presenter.Delete);

        private void LstHhdOnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstHouseholdDistribution.SelectedItem == null)
            {
                return;
            }
            var sthhd = (STHouseholdDistribution)LstHouseholdDistribution.SelectedItem;
            Presenter.HHDMinPersons = sthhd.MinimumNumber;
            Presenter.HHDMaxPersons = sthhd.MaximumNumber;
            Presenter.HHDPercent = sthhd.PercentOfHouseholds;
            Presenter.HHDIntensity = Presenter.Eic.GetAllDisplayElement(sthhd.EnergyIntensity);
            foreach (var tagEntry in Presenter.AllTags)
            {
                tagEntry.IsChecked = false;
            }
            foreach (var stHouseholdDistributionTag in sthhd.Tags)
            {
                foreach (var entry in Presenter.AllTags)
                {
                    if (entry.Tag == stHouseholdDistributionTag.Tag)
                    {
                        entry.IsChecked = true;
                    }
                }
            }
        }

        private void LstHouseholdTemplatesOnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstHouseholdTemplates.SelectedItem == null)
            {
                Presenter.HouseholdTemplateSelection = null;
                return;
            }
            var stht = (STHouseholdTemplate)LstHouseholdTemplates.SelectedItem;
            Presenter.HouseholdTemplateSelection = stht.HouseholdTemplate;
        }

        private void LstHouseSizesOnClick([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstHouseSizes.SelectedItem == null)
            {
                return;
            }
            var sthhd = (STHouseSize)LstHouseSizes.SelectedItem;
            Presenter.HSDMinimum = sthhd.MinimumHouseSize;
            Presenter.HSDMaximum = sthhd.MaximumHouseSize;
            Presenter.HSDPercent = sthhd.Percentage;
        }

        private void LstHousetypes_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstHouseTypes.SelectedItem == null)
            {
                return;
            }

            var st = (STHouseType)LstHouseTypes.SelectedItem;
            Presenter.HTSelection = st.HouseType;
        }

        private void LstTraitLimitsSelectionChange([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstTraitLimits.SelectedItem == null)
            {
                return;
            }
            var stl = (STTraitLimit)LstTraitLimits.SelectedItem;
            Presenter.LimitHouseholdTrait = stl.Trait;
            Presenter.LimitMaximum = stl.Maximum;
        }

        private void MakeCopy_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.MakeCopy();

        private void BtnAddTransportationDeviceSet(object sender, RoutedEventArgs e)
        {
            if (Presenter.TransportationDeviceSetSelection == null)
            {
                return;
            }
            Presenter.ThisTemplate.AddTransportationDeviceSet(Presenter.TransportationDeviceSetSelection);
        }

        private void BtnRemoveTransportationDeviceSet(object sender, RoutedEventArgs e)
        {
            if (LstTransportationDeviceSets.SelectedItem == null)
            {
                return;
            }
            var sths = (STTransportationDeviceSet)LstTransportationDeviceSets.SelectedItem;
            Presenter.ThisTemplate.DeleteTransportationDeviceSet(sths);
        }

        private void BtnAddAllTransportationDeviceSets(object sender, RoutedEventArgs e)
        {
            foreach (var deviceSet in Presenter.TransportationDeviceSets) {
                Presenter.ThisTemplate.AddTransportationDeviceSet(deviceSet);
            }
        }

        private void BtnAddChargingStationSet(object sender, RoutedEventArgs e)
        {
            if (Presenter.ChargingStationSelection == null)
            {
                return;
            }
            Presenter.ThisTemplate.AddChargingStationSet(Presenter.ChargingStationSelection);
        }

        private void BtnRemoveChargingStationSet(object sender, RoutedEventArgs e)
        {
            if (LstChargingStationSets.SelectedItem == null)
            {
                return;
            }
            var sths = (STChargingStationSet)LstChargingStationSets.SelectedItem;
            Presenter.ThisTemplate.DeleteChargingStation(sths);
        }

        private void BtnAddAllChargingStationSets(object sender, RoutedEventArgs e)
        {
            foreach (var chargingStationSet in Presenter.ChargingStationSets)
            {
                Presenter.ThisTemplate.AddChargingStationSet(chargingStationSet);
            }
        }

        private void BtnAddTravelRouteSet(object sender, RoutedEventArgs e)
        {

            if (Presenter.TravelRouteSetSelection == null)
            {
                return;
            }
            Presenter.ThisTemplate.AddTravelRouteSet(Presenter.TravelRouteSetSelection);
        }

        private void BtnRemoveTravelRouteSet(object sender, RoutedEventArgs e)
        {
            if (LstTravelRouteSets.SelectedItem == null)
            {
                return;
            }
            var sths = (STTravelRouteSet)LstTravelRouteSets.SelectedItem;
            Presenter.ThisTemplate.DeleteTravelRouteSet(sths);
        }


        private void BtnAddAllTravelRouteSets(object sender, RoutedEventArgs e)
        {
            foreach (var travelRouteSet in Presenter.TravelRouteSets)
            {
                Presenter.ThisTemplate.AddTravelRouteSet(travelRouteSet);
            }
        }

        private void LstGeneratedHouses_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstGeneratedHouses.SelectedItem == null) {
                return;
            }

            var house = (House)LstGeneratedHouses.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(house);
        }
    }
}