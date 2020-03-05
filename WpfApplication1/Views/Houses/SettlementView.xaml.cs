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

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Houses;

#endregion

namespace LoadProfileGenerator.Views.Houses {
    /// <summary>
    ///     Interaktionslogik f�r SettlementView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class SettlementView {
        public SettlementView()
        {
            InitializeComponent();
        }

        [NotNull]
        private SettlementPresenter Presenter => (SettlementPresenter) DataContext;

        private void BtnAddHousehold_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (TxtHouseholdCount.Text.Length == 0) {
                return;
            }

            if (Presenter.SelectedCalcObjectType == CalcObjectType.ModularHousehold &&
                CmbModularHouseholds.SelectedItem == null) {
                return;
            }

            if (Presenter.SelectedCalcObjectType == CalcObjectType.House && CmbHouses.SelectedItem == null) {
                return;
            }

            var count = Utili.ConvertToIntWithMessage(TxtHouseholdCount.Text);
            switch (Presenter.SelectedCalcObjectType) {
                case CalcObjectType.ModularHousehold:
                    var mhh = (ModularHousehold) CmbModularHouseholds.SelectedItem;
                    Presenter.AddCalcObject(mhh, count);
                    break;
                case CalcObjectType.House:
                    var house = (House) CmbHouses.SelectedItem;
                    Presenter.AddCalcObject(house, count);
                    break;
                case CalcObjectType.Settlement: throw new LPGException("Nonsensical calc object type! This is a bug.");
                default: throw new LPGException("unknown Calc Object Type! This is a bug.");
            }

            LstHouseholds.ResizeColummns();
        }

        private void BtnRefreshAgeStatistics([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshAgeEntries();

        private void BtnRefreshLivingPatternStatistics([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshLivingPatternEntries();
        }

        private void BtnRefreshTagStatistics([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshTagEntries();

        private void BtnRemoveLocation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseholds.SelectedItem == null) {
                return;
            }

            var shh = (SettlementHH) LstHouseholds.SelectedItem;
            Presenter.RemoveHousehold(shh);
            LstHouseholds.ResizeColummns();
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CmbCalcObjectType_SelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbCalcObjectType.SelectedItem == null) {
                return;
            }

            Presenter.SelectedCalcObjectType = (CalcObjectType) CmbCalcObjectType.SelectedItem;
        }

        private void CopySettlementClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.MakeCopy();

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisSettlement.HeaderString, Presenter.Delete);

        private void LstHouseholds_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstHouseholds.SelectedItem == null) {
                return;
            }

            var shh = (SettlementHH) LstHouseholds.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(shh.CalcObject);
        }

        private void BtnExportJson([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            if (Presenter.ThisSettlement.OutputDirectory == null) {
                Logger.Error("No output directory was selected");
                return;
            }

            try {
                Presenter.ExportCalculationJson(Presenter.ThisSettlement.OutputDirectory);
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        private void LstSelectedOptions_MouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            if (LstSelected.SelectedItem == null)
            {
                return;
            }

            var selected = (string)LstSelected.SelectedItem;
            var o = CalcOptionHelper.CalcOptionDictionary.FirstOrDefault(x => x.Value == selected).Key;
            Presenter.RemoveOption(o);
        }

        private void OptionRemove_OnClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            if (LstSelected.SelectedItem == null)
            {
                return;
            }

            var selected = (string)LstSelected.SelectedItem;
            var o = CalcOptionHelper.CalcOptionDictionary.FirstOrDefault(x => x.Value == selected).Key;
            Presenter.RemoveOption(o);
        }

        private void OptionAdd_OnClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {

                if (LstNotSelected.SelectedItem == null)
                {
                    return;
                }

                var selected = (string)LstNotSelected.SelectedItem;
                var o = CalcOptionHelper.CalcOptionDictionary.FirstOrDefault(x => x.Value == selected).Key;
                Presenter.AddOption(o);

        }


        private void ApplyOptionDefaultClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            if (CmbOptionPresets.SelectedItem == null) {
                return;
            }

            var ofd = (System.Collections.Generic.KeyValuePair<Automation.OutputFileDefault, string>)CmbOptionPresets.SelectedItem;
            Presenter.ApplyOptionDefault(ofd.Key);
        }

        private void LstNotSelectedOptions_MouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            if (LstNotSelected.SelectedItem == null)
            {
                return;
            }

            var selected = (string)LstNotSelected.SelectedItem;
            var o = CalcOptionHelper.CalcOptionDictionary.FirstOrDefault(x => x.Value == selected).Key;
            Presenter.AddOption(o);
        }

        private void BtnResetLoadtypeSelection([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Presenter.ResetLoadtypeSelection();
        }
    }
}