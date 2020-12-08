//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.Households;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r HouseholdTraitView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class HouseholdTraitView {
        public HouseholdTraitView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private HouseholdTraitPresenter Presenter => (HouseholdTraitPresenter)DataContext;

        private void AddNewTagClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var ib = new InputBox();

            ib.ShowDialog();
            if (ib.IsOk) {
                var sim = Presenter.Sim;
                var tag = sim.TraitTags.FindFirstByName(ib.Result);
                if (tag != null) {
                    Logger.Error("This tag already existed");
                }
                else {
                    tag = sim.TraitTags.CreateNewItem(sim.ConnectionString);
                    tag.Name = ib.Result;
                    tag.SaveToDB();
                }

                Presenter.ThisHouseholdTrait.AddTag(tag);
                HHTTags.ResizeColummns();
            }
        }

        private void BtnAddLocationClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbLocations.SelectedItem == null) {
                return;
            }

            Presenter.AddLocation((Location)CmbLocations.SelectedItem);
        }

        private void BtnAddNewAffordanceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbNewAffAffordance.SelectedItem == null) {
                return;
            }

            if (CmbNewAffLocation.SelectedItem == null) {
                return;
            }

            Presenter.AddAffordanceToLocation();
            var hhl = (HHTLocation)CmbNewAffLocation.SelectedItem;
            TreeNewAffordances.SelectItem(hhl, true);
        }

        private void BtnAddNormalDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDesires.SelectedItem == null) {
                Logger.Error("Please select a desire first!");
                return;
            }

            if (CmbHealthStatus.SelectedItem == null) {
                Logger.Error("Please select a health status first!");
                return;
            }

            if (CmbPermittedGender.SelectedItem == null) {
                Logger.Error("Please select a gender first!");
                return;
            }

            var d = (Desire)CmbDesires.SelectedItem;
            var healthString = (string)CmbHealthStatus.SelectedItem;
            var weight = Utili.ConvertToDecimalWithMessage(TxtWeight.Text);
            var decayTime = Utili.ConvertToDecimalWithMessage(TxtDecayRate.Text);
            var threshold = Utili.ConvertToDecimalWithMessage(TxtThreshold.Text);
            var minAge = Utili.ConvertToIntWithMessage(TxtMinAge.Text);
            var maxAge = Utili.ConvertToIntWithMessage(TxtMaxAge.Text);
            threshold /= 100;
            var gender = (PermittedGender)CmbPermittedGender.SelectedItem;
            Presenter.AddDesire(d, decayTime, healthString, threshold, weight, minAge, maxAge, gender);
        }

        private void BtnAddTagClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTags.SelectedItem != null) {
                var tag = (TraitTag)CmbTags.SelectedItem;
                Presenter.AddTag(tag);

                HHTTags.ResizeColummns();
            }
        }

        private void BtnAddTraitClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTraits.SelectedItem == null) {
                Logger.Error("Please select a trait first!");
                return;
            }

            var trait = (HouseholdTrait)CmbTraits.SelectedItem;
            Presenter.ThisHouseholdTrait.AddTrait(trait);
            LstTraits.ResizeColummns();
        }

        private void BtnImportHouseholdTraitClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedImportHouseholdTrait == null) {
                return;
            }

            if (MessageWindowHandler.Mw.ShowYesNoMessage(
                    "This will import everything from the household trait " + Presenter.SelectedImportHouseholdTrait.Name +
                    ". If this household trait isn't empty, this will most likely result in quite a mess. Are you sure?", "Sure?") ==
                LPGMsgBoxResult.No) {
                return;
            }

            Presenter.ImportHousehold();
            MessageWindowHandler.Mw.ShowInfoMessage("Import is finished!", "Load Profile Generator");
        }

        private void BtnLookForCorrectDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedDesire == null) {
                Logger.Error("No desire selected!");
                return;
            }

            var desires = new List<Desire>();
            foreach (var desire in Presenter.Sim.Desires.Items) {
                if (desire.Name.Contains(Presenter.SelectedDesire.Name) && desire.Name.Contains("/")) {
                    desires.Add(desire);
                }
            }

            if (desires.Count > 1) {
                desires.Sort((x, y) => x.Name.Length.CompareTo(y.Name.Length));
                var list = string.Empty;
                foreach (var desire in desires) {
                    list += desire.PrettyName + Environment.NewLine;
                }

                Logger.Info("Found the following:" + list);
                Presenter.SelectedDesire = desires[0];
            }
            else if (desires.Count == 1) {
                Presenter.SelectedDesire = desires[0];
            }
            else {
                Logger.Info("No desire found!");
            }

            LstHealthyDesires.ResizeColummns();
        }

        private void BtnRefreshAffordanceForAddingClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshRelevantAffordancesForAdding();

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUses();

        private void BtnRemoveLocationClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (HHTLocations.SelectedItem == null) {
                return;
            }

            Presenter.RemoveLocation((HHTLocation)HHTLocations.SelectedItem);
        }

        private void BtnRemoveNewAffordanceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (TreeNewAffordances.SelectedItem == null) {
                Logger.Info("No item was selected.");
                return;
            }

            if (TreeNewAffordances.SelectedItem is HHTAffordance hhta) {
                Logger.Info("No affordance was selected.");
                Presenter.ThisHouseholdTrait.DeleteAffordanceFromDB(hhta);
            }
        }

        private void BtnRemoveNormalDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHealthyDesires.SelectedItem == null) {
                Logger.Error("Please select a desire first!");
                return;
            }

            var hhtDesire = (HHTDesire)LstHealthyDesires.SelectedItem;
            Presenter.RemoveDesire(hhtDesire);
        }

        private void BtnRemoveTagClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (HHTTags.SelectedItem != null) {
                var tag = (HHTTag)HHTTags.SelectedItem;
                Presenter.ThisHouseholdTrait.DeleteHHTTag(tag);
            }
        }

        private void BtnRemoveTraitClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTraits.SelectedItem == null) {
                return;
            }

            var trait = (HHTTrait)LstTraits.SelectedItem;
            Presenter.ThisHouseholdTrait.DeleteHHTTrait(trait);
        }

        private void BtnShowOtherPossibleDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.ShowOtherPossibleDesires();

        private void BtnSwapLocationClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (HHTLocations.SelectedItem == null) {
                return;
            }

            if (CmbLocations.SelectedItem == null) {
                return;
            }

            var newlocation = (Location)CmbLocations.SelectedItem;
            var hhtLocation = (HHTLocation)HHTLocations.SelectedItem;
            var newhhtl = Presenter.AddLocation(newlocation);
            foreach (var affordance in hhtLocation.AffordanceLocations) {
                if (affordance.Affordance != null) {
                    Presenter.ThisHouseholdTrait.AddAffordanceToLocation(newhhtl, affordance.Affordance, affordance.TimeLimit, affordance.Weight,
                        Presenter.StartMinusTime, Presenter.StartPlusTime, Presenter.EndMinusTime, Presenter.EndPlusTime);
                }
            }

            Presenter.RemoveLocation(hhtLocation);
            var olddevs = Presenter.ThisHouseholdTrait.Autodevs.ToList();
            foreach (var autodev in olddevs) {
                if (autodev.Location == hhtLocation.Location && autodev.Device != null) {
                    Presenter.ThisHouseholdTrait.AddAutomousDevice(autodev.Device, autodev.TimeProfile, autodev.TimeStandardDeviation,
                        autodev.LoadType, autodev.TimeLimit, newlocation, autodev.VariableValue, autodev.VariableCondition, autodev.Variable);
                    Presenter.ThisHouseholdTrait.DeleteHHTAutonomousDeviceFromDB(autodev);
                }
            }
        }

        private void ButtonBase_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbReplacementTrait.SelectedItem == null) {
                return;
            }

            var sim = Presenter.Sim;
            var uses = Presenter.ThisHouseholdTrait.CalculateUsedIns(sim);
            var replacement = (HouseholdTrait)CmbReplacementTrait.SelectedItem;
            var count = 0;
            foreach (var usedIn in uses) {
                if (usedIn.Item is ModularHousehold chh) {
                    for (var index = 0; index < chh.Traits.Count; index++) {
                        var trait = chh.Traits[index];
                        if (trait.HouseholdTrait == Presenter.ThisHouseholdTrait) {
                            chh.AddTrait(replacement, trait.AssignType, trait.DstPerson);
                            chh.DeleteTraitFromDB(trait);
                            index = 0;
                            count++;
                        }
                    }
                }
            }

            Logger.Info("Replaced " + count + " items.");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool CheckCombobox([CanBeNull] object box, [JetBrains.Annotations.NotNull] string s)
        {
            if (box == null) {
                Logger.Error("Please select a " + s + " to continue");
                return false;
            }

            return true;
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CmbClassification_OnLostFocus([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshClassifications();

        private void CmbTags_OnKeyUp([CanBeNull] object sender, [CanBeNull] KeyEventArgs e)
        {
            if (e == null) {
                return;
            }

            if (e.Key == Key.Enter && CmbTags.SelectedItem != null) {
                var tag = (TraitTag)CmbTags.SelectedItem;
                Presenter.AddTag(tag);
                HHTTags.ResizeColummns();
            }
        }

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(Presenter.ThisHouseholdTrait.HeaderString, Presenter.Delete);

        private void DeviceSelectorControl_OnOnAddedDevice([CanBeNull] object sender, [CanBeNull] DeviceSelectorControl.DeviceAddedEventArgs e)
        {
            if (!CheckCombobox(AutoDevs.AssignableDevice, "Device")) {
                return;
            }

            if (!CheckCombobox(AutoDevs.TimeLimit, "Time Limit")) {
                return;
            }

            if (!CheckCombobox(AutoDevs.Location, "Location")) {
                return;
            }

            if (AutoDevs.SelectedDeviceType == AssignableDeviceType.Device || AutoDevs.SelectedDeviceType == AssignableDeviceType.DeviceCategory) {
                if (!CheckCombobox(AutoDevs.TimeBasedProfile, "TimeBasedProfile")) {
                    return;
                }

                if (!CheckCombobox(AutoDevs.LoadType, "LoadType")) {
                    return;
                }
            }

            var adev = AutoDevs.AssignableDevice;
            if (adev == null) {
                throw new LPGException("Bug: adev should never be null");
            }

            var tp = AutoDevs.TimeBasedProfile;
            var timeStandardDeviation = AutoDevs.TimeDeviation;
            var vlt = AutoDevs.LoadType;
            var loc = (Location)AutoDevs.Location;
            var timeLimit = AutoDevs.TimeLimit;
            if (timeLimit == null) {
                Logger.Error("Time Limit was null, not adding");
                return;
            }

            var variableValue = AutoDevs.VariableValue;
            var tc = AutoDevs.VariableCondition;
            var variable = AutoDevs.SelectedVariable;
            Presenter.AddAutoDev(adev, tp, timeStandardDeviation, vlt, timeLimit, loc, variableValue, tc, variable);
        }

        private void EstimateButton_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.ThisHouseholdTrait.CalculateEstimatedTimes();

        private void HHTTags_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (HHTTags.SelectedItem == null) {
                return;
            }

            var tag = (HHTTag)HHTTags.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(tag.Tag);
        }

        private void LstHealthyDesires_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstHealthyDesires.SelectedItem == null) {
                return;
            }

            var hhtDesire = (HHTDesire)LstHealthyDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(hhtDesire.Desire);
        }

        private void LstHealthyDesires_OnSelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstHealthyDesires.SelectedItem == null) {
                return;
            }

            var hhtd = (HHTDesire)LstHealthyDesires.SelectedItem;
            Presenter.SelectedDecayRate = hhtd.DecayTime;
            Presenter.SelectedDesire = hhtd.Desire;
            Presenter.SelectedHealthStatus = hhtd.HealthStatus;
            Presenter.SelectedMinAge = hhtd.MinAge;
            Presenter.SelectedMaxAge = hhtd.MaxAge;
            Presenter.SelectedThreshold = hhtd.Threshold;
            Presenter.SelectedWeight = hhtd.Weight;
            Presenter.SelectedGender = hhtd.Gender;
        }

        private void LstTraitUsedBy_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstTraitUsedBy.SelectedItem == null) {
                return;
            }

            var o = LstTraitUsedBy.SelectedItem;
            var ui = (UsedIn)o;

            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void MakeTraitCopyOnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.CreateNewTrait();

        private void TreeNewAffordances_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            try {
                var a = new Action(() => Presenter.ApplicationPresenter.OpenItem(Presenter.SelectedAffordance));
                Dispatcher.BeginInvoke(a);
            }
            catch (Exception x) {
                Logger.Exception(x);
            }
        }

        private void TreeNewAffordancesSelectedItemChanged([CanBeNull] object sender, [CanBeNull] RoutedPropertyChangedEventArgs<object> e)
        {
            if (e == null) {
                return;
            }

            if (e.NewValue is HHTAffordance aff) {
                Presenter.SelectedAffordance = aff.Affordance;
                Presenter.SelectedAffLocation = aff.HHTLocation;
                string s = "Desires of the selected affordance: ";
                if (aff.Affordance != null) {
                    foreach (AffordanceDesire affdes in aff.Affordance.AffordanceDesires) {
                        s += affdes.Desire.PrettyName + ", ";
                    }

                    s = s.Substring(0, s.Length - 2);
                }

                Presenter.CurrentAffordanceDesireString = s;
                Presenter.AffordanceWeight = aff.Weight;
                Presenter.StartMinusTime = aff.StartMinusMinutes;
                Presenter.StartPlusTime = aff.StartPlusMinutes;

                Presenter.EndMinusTime = aff.EndMinusMinutes;
                Presenter.EndPlusTime = aff.EndPlusMinutes;
                if (aff.TimeLimit != null) {
                    Presenter.OverwriteTimeLimitOnAffordance = true;
                    Presenter.SelectedTimeLimit = aff.TimeLimit;
                }
                else {
                    Presenter.OverwriteTimeLimitOnAffordance = false;
                    if (aff.Affordance != null) {
                        Presenter.SelectedTimeLimit = aff.Affordance.TimeLimit;
                    }
                }
            }

            if (e.NewValue is HHTLocation hhl) {
                Presenter.SelectedAffLocation = hhl;
            }
        }

#pragma warning disable CC0068 // Unused Method
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void AutoDevs_Loaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            AutoDevs.Simulator = Presenter.ApplicationPresenter.Simulator;
            AutoDevs.OpenItem = Presenter.ApplicationPresenter.OpenItem;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void AutoDevs_OnOnRemovedDevice([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] DeviceSelectorControl.DeviceRemovedEventArgs e)
        {
            if (!(e.ItemToRemove is HHTAutonomousDevice ad)) {
                Logger.Error("Could not remove autonomous device.");
                return;
            }

            Presenter.RemoveAutoDev(ad);
        }
#pragma warning restore CC0068 // Unused Method
        private void BtnAddLivingPatternTagClick(object sender, RoutedEventArgs e)
        {
            if (CmbLivingPatternTags.SelectedItem != null)
            {
                var tag = (LivingPatternTag)CmbLivingPatternTags.SelectedItem;
                Presenter.AddLivingPatternTag(tag);

                HHTLivingPatternTags.ResizeColummns();
            }
        }

        private void BtnRemoveLivingPatternTagClick(object sender, RoutedEventArgs e)
        {
            if (HHTLivingPatternTags.SelectedItem != null)
            {
                var tag = (HHTLivingPatternTag)HHTLivingPatternTags.SelectedItem;
                Presenter.ThisHouseholdTrait.DeleteHHTLivingPatternTag(tag);
            }
        }

        private void HHTLivingPatternTags_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HHTLivingPatternTags.SelectedItem == null)
            {
                return;
            }

            var tag = (HHTLivingPatternTag)HHTLivingPatternTags.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(tag.Tag);
        }
    }
}