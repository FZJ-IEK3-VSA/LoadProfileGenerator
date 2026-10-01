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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r DeviceTaggingSetView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceTaggingSetView {
        public DeviceTaggingSetView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private DeviceTaggingSetPresenter Presenter => (DeviceTaggingSetPresenter) DataContext;

        private void BtnAddLoadTypeClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbLoadTypes.SelectedItem == null) {
                Logger.Warning("Please select a load type first.");
                return;
            }

            VLoadType loadType = (VLoadType) CmbLoadTypes.SelectedItem;
            Presenter.ThisDeviceTaggingSet.AddLoadType(loadType);
            Presenter.ThisDeviceTaggingSet.SaveToDB();
        }

        private void BtnAddRefValueClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbRefValues.SelectedItem == null) {
                return;
            }

            if (string.IsNullOrEmpty(TxtRefPersonCount.Text)) {
                return;
            }

            if (string.IsNullOrEmpty(TxtRefValue.Text)) {
                return;
            }

            if (CmbRefLoadTypes.SelectedItem == null) {
                return;
            }

            var success = double.TryParse(TxtRefValue.Text, out var refval);
            if (!success) {
                Logger.Error("Reference Value: Could not parse " + TxtRefValue.Text);
            }

            success = int.TryParse(TxtRefPersonCount.Text, out var personcount);
            if (!success) {
                Logger.Error("Person Count: Could not parse " + TxtRefValue.Text);
            }

            var tag = (DeviceTag) CmbRefValues.SelectedItem;
            var lt = (VLoadType) CmbRefLoadTypes.SelectedItem;
            Presenter.AddReferenceValue(tag, personcount, refval, lt);
            Logger.Info("New item added with the tag " + tag.Name);
            LstTags.ResizeColummns();
        }

        private void BtnAddTaggingClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (TxtTagName.Text.Length > 0) {
                Presenter.AddTag(TxtTagName.Text);
            }

            LstTags.ResizeColummns();
        }

        private void BtnFindMissingClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var result = Presenter.ThisDeviceTaggingSet.GetNextMissingReference();
            if (result != null) {
                CmbRefValues.SelectedItem = result.Tag;
                TxtRefPersonCount.Text = result.PersonCount.ToString(CultureInfo.CurrentCulture);
                TxtRefValue.Text = string.Empty;
            }
            else {
                Logger.Info("Nothing missing.");
            }
        }

        private void BtnRefreshClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.ThisDeviceTaggingSet.RefreshDevices(Presenter.Sim.RealDevices.Items);
            LstEntries.ResizeColummns();
        }

        private void BtnRefreshRefStatisticsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.RefreshReferenceStatistic();

        private void BtnRefreshStatisticsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshStatistics();

        private void BtnRemoveLoadTypeClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstLoadTypes.SelectedItem == null) {
                Logger.Warning("Please select a load type to remove first.");
                return;
            }

            DeviceTaggingSetLoadType loadType = (DeviceTaggingSetLoadType) LstLoadTypes.SelectedItem;
            Presenter.ThisDeviceTaggingSet.DeleteLoadType(loadType);
        }

        private void BtnRemoveRefValueClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstReferences.SelectedItem == null) {
                return;
            }

            var dtr = (DeviceTaggingReference) LstReferences.SelectedItem;
            Presenter.ThisDeviceTaggingSet.DeleteReference(dtr);
        }

        private void BtnRemoveTagClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstTags.SelectedItem is DeviceTag tag) {
                Presenter.ThisDeviceTaggingSet.DeleteTag(tag);
            }
        }

        private void BtnResortClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Resort();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisDeviceTaggingSet.HeaderString, Presenter.Delete);

        private void LstReferences_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (LstReferences.SelectedItem != null) {
                var dtr = (DeviceTaggingReference) LstReferences.SelectedItem;
                CmbRefValues.SelectedItem = dtr.Tag;
                TxtRefPersonCount.Text = dtr.PersonCount.ToString(CultureInfo.CurrentCulture);
                TxtRefValue.Text = dtr.ReferenceValue.ToString(CultureInfo.CurrentCulture);
            }
        }
    }
}