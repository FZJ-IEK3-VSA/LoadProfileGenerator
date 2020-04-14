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
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Houses;

namespace LoadProfileGenerator.Views.Houses {
    /// <summary>
    ///     Interaktionslogik f�r TransformationDeviceView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
#pragma warning disable S110 // Inheritance tree of classes should not be too deep
    public partial class TransformationDeviceView {
        public TransformationDeviceView()
        {
            InitializeComponent();
        }

        [NotNull]
        private TransformationDevicePresenter Presenter => (TransformationDevicePresenter) DataContext;

        private void BtnAddCondition_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbVariable.SelectedItem == null) {
                Logger.Warning("Please select a variable first.");
                return;
            }
            var variable = (Variable)CmbVariable.SelectedItem;
            var success = double.TryParse(TxtCondMinValue.Text, out var minValue);
            if (!success) {
                Logger.Info("Could not convert " + TxtCondMinValue.Text + " to double");
            }

            success = double.TryParse(TxtCondMaxValue.Text, out var maxValue);
            if (!success) {
                Logger.Info("Could not convert " + TxtCondMaxValue.Text + " to double");
            }

            Presenter.ThisTrafo.AddTransformationDeviceCondition(variable,  minValue, maxValue);
        }

        private void BtnAddFactor_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (TxtInputValue.Text.Length == 0) {
                return;
            }

            if (TxtMatchingFactor.Text.Length == 0) {
                return;
            }

            var success = double.TryParse(TxtInputValue.Text, out var input);
            if (!success) {
                return;
            }

            success = double.TryParse(TxtMatchingFactor.Text, out var factor);
            if (!success) {
                return;
            }

            Presenter.ThisTrafo.AddDataPoint(input, factor);
        }

        private void BtnAddLoadtype_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbOutputLoadtype.SelectedItem == null) {
                Logger.Error("You need to select an output load type!");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtFactor.Text)) {
                Logger.Error("You need to enter a factor!");
                return;
            }

            if (CmbFactorType.SelectedItem == null) {
                return;
            }

            var selectedLoadType = (VLoadType) CmbOutputLoadtype.SelectedItem;
            var success = double.TryParse(TxtFactor.Text, out var factor);
            if (!success) {
                Logger.Error("Could not convert " + TxtFactor.Text + " to double");
            }

            TransformationFactorType ft;
            if (CmbFactorType.SelectedItem.ToString() == "Fixed") {
                ft = TransformationFactorType.Fixed;
            }
            else {
                ft = TransformationFactorType.Interpolated;
            }

            Presenter.AddOutputLoadType(selectedLoadType, factor, ft);
        }

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUsedIn();

        private void BtnRemoveCondition_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstConditions.SelectedItem == null) {
                Logger.Error("No condition selected.");
                return;
            }

            if (!(LstConditions.SelectedItem is TransformationDeviceCondition condition)) {
                Logger.Error("No condition selected.");
                return;
            }

            Presenter.ThisTrafo.DeleteTransformationDeviceCondition(condition);
        }

        private void BtnRemoveFactor_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstFactors.SelectedItem is TransformationFactorDatapoint dp) {
                Presenter.ThisTrafo.DeleteFactorDataPoint(dp);
            }
        }

        private void BtnRemoveLoadtype_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstOutputLoadtypes.SelectedItem == null) {
                Logger.Error("You need to select an output load type!");
                return;
            }

            var tdlt = (TransformationDeviceLoadType) LstOutputLoadtypes.SelectedItem;
            Presenter.RemoveOutputDeviceLoadType(tdlt);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CmbConversionInputLoadtype_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbConversionOutputLoadtype.SelectedItem != null) {
                Presenter.ConversionOutLoadType = (VLoadType) CmbConversionOutputLoadtype.SelectedItem;
            }

            Presenter.RefreshConversionHelper();
        }

        private void CmbOutputLoadtype_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbOutputLoadtype.SelectedItem == null) {
                return;
            }

            Presenter.SelectedOutputLoadtype = CmbOutputLoadtype.SelectedItem as VLoadType;
        }

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisTrafo.HeaderString, Presenter.Delete);

        private void LstUses_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var u = (UsedIn) LstUses.SelectedItem;
            if (u?.Item != null) {
                Presenter.ApplicationPresenter.OpenItem(u.Item);
            }
        }

        private void TxtConversionFactor_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtConversionFactor.Text)) {
                var success = double.TryParse(TxtConversionFactor.Text, out double factor);
                if (success) {
                    Presenter.ExampleConversionFactor = factor;
                }
            }

            Presenter.RefreshConversionHelper();
        }

        private void TxtExampleQuantity_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtExampleQuantity.Text)) {
                var success = double.TryParse(TxtExampleQuantity.Text, out var factor);
                if (success) {
                    Presenter.ConversionExampleQuantity = factor;
                }
            }

            Presenter.RefreshConversionHelper();
        }

        private void TxtExampleTimespan_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtExampleQuantity.Text)) {
                var success = TimeSpan.TryParse(TxtExampleTimespan.Text, out TimeSpan factor);
                if (success) {
                    Presenter.ConversionExampleTimespan = factor;
                }
            }

            Presenter.RefreshConversionHelper();
        }
#pragma warning restore S110 // Inheritance tree of classes should not be too deep
    }
}