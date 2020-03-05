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

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Forms;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;
using Button = System.Windows.Controls.Button;
using Color = System.Drawing.Color;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r AffordanceTaggingSetView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class AffordanceTaggingSetView {
        public AffordanceTaggingSetView() {
            InitializeComponent();
        }

        [NotNull]
        private AffordanceTaggingSetPresenter Presenter => (AffordanceTaggingSetPresenter)DataContext;

        private void BtnAddAffordancesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            Presenter.ThisAffordanceTaggingSet.RefreshAffordances(
                Presenter.Sim.Affordances.MyItems);
            LstEntries.ResizeColummns();
        }

        private void BtnAddAllAffordancesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var mbr =
                MessageWindows.ShowYesNoMessage(
                    "This will create one tag for every affordance. This is only useful as basis for a household plan. Are you sure?",
                    "Sure?");
            if (mbr == MessageBoxResult.Yes) {
                Presenter.AddAllAffordances();
            }
        }

        private void BtnAddRefEntry([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (CmbRefTag.SelectedItem == null) {
                return;
            }
            if (CmbRefGender.SelectedItem == null) {
                return;
            }
            if (string.IsNullOrEmpty(TxtRefMaxAge.Text)) {
                return;
            }
            if (string.IsNullOrEmpty(TxtRefMinAge.Text)) {
                return;
            }
            if (string.IsNullOrEmpty(TxtRefPercentage.Text)) {
                return;
            }
            var tag = (AffordanceTag) CmbRefTag.SelectedItem;
            var gender = (PermittedGender) CmbRefGender.SelectedItem;
            var success = int.TryParse(TxtRefMinAge.Text, out int minAge);
            if (!success) {
                Logger.Error("Minimum age was not parseable:" + TxtRefMinAge.Text);
            }
            success = int.TryParse(TxtRefMaxAge.Text, out int maxAge);
            if (!success) {
                Logger.Error("Maximum age was not parseable:" + TxtRefMaxAge.Text);
            }
            success = double.TryParse(TxtRefPercentage.Text, out double percentage);
            if (!success) {
                Logger.Error("Percentage was not parseable:" + TxtRefPercentage.Text);
            }
            Presenter.ThisAffordanceTaggingSet.AddTagReference(tag, gender, minAge, maxAge, percentage / 100);
            Presenter.RefreshRefStatistics();
        }

        private void BtnAddTaggingClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (TxtTagName.Text.Length > 0) {
                Presenter.AddTag(TxtTagName.Text);
            }
            LstTags.ResizeColummns();
        }

        private void BtnPickColorClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (!(sender is Button button))
            {
                throw new LPGException("Could not convert the button");
            }
            var aff = (AffordanceTag) button.DataContext;
            Color c2;
            using (var cd = new ColorDialog()) {
                var mediaColor = aff.CarpetPlotColor;

                var c = Color.FromArgb( mediaColor.R, mediaColor.G, mediaColor.B);
                cd.Color = c;
                cd.ShowDialog();
                c2 = cd.Color;
            }
            var rescolor = System.Windows.Media.Color.FromArgb(c2.A, c2.R, c2.G, c2.B);
            var cp = new ColorRGB(rescolor.R, rescolor.G, rescolor.B);
            aff.CarpetPlotColor = cp;
            aff.SaveToDB();
        }

        private void BtnRefreshStatistics_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshRefStatistics();

        private void BtnRefreshStatisticsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshStatistics();

        private void BtnRemoveOldAffordances([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var mbr =
                MessageWindows.ShowYesNoMessage(
                    "This will remove all tags that don't match an affordance. This is only useful as basis for a household plan. Are you sure?",
                    "Sure?");
            if (mbr == MessageBoxResult.Yes) {
                Presenter.RemoveOldAffordanceTags();
            }
        }

        private void BtnRemoveTagClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstTags.SelectedItem is AffordanceTag tag)
            {
                Presenter.ThisAffordanceTaggingSet.DeleteTag(tag);
            }
        }

        private void BtnRemoveTagReferenceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstReferences.SelectedItem == null) {
                return;
            }
            var atr = (AffordanceTagReference) LstReferences.SelectedItem;
            Presenter.ThisAffordanceTaggingSet.DeleteTagReference(atr);
            Presenter.RefreshRefStatistics();
        }

        private void BtnResortClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.ThisAffordanceTaggingSet.ResortEntries();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisAffordanceTaggingSet.HeaderString, Presenter.Delete);

        private void BtnAddLoadTypeClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbLoadTypes.SelectedItem == null) {
                Logger.Warning("Please select a load type first.");
                return;
            }
            VLoadType loadType = (VLoadType) CmbLoadTypes.SelectedItem;
            Presenter.ThisAffordanceTaggingSet.AddNewLoadType(loadType);
            Presenter.ThisAffordanceTaggingSet.SaveToDB();
        }

        private void BtnRemoveLoadTypeClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstLoadTypes.SelectedItem == null)
            {
                Logger.Warning("Please select a load type to remove first.");
                return;
            }
            AffordanceTaggingSetLoadType loadType = (AffordanceTaggingSetLoadType)LstLoadTypes.SelectedItem;
            Presenter.ThisAffordanceTaggingSet.DeleteLoadType(loadType);
        }
    }
}