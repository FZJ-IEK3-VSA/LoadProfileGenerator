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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    ///     Interaktionslogik f�r HouseView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class HouseView {
        public HouseView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private HousePresenter Presenter => (HousePresenter) DataContext;

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        private void BtnAddHousehold_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedCalcObjectType == CalcObjectType.ModularHousehold &&
                CmbModularHouseholds.SelectedItem == null) {
                return;
            }

            if (CmbModularHouseholds.SelectedItem == null) {
                Logger.Warning("Please select something to add first");
                return;
            }

            if (Presenter.SelectedChargingStationSet == null) {
                Logger.Warning("Select a Charging Station Set.");
                return;
            }

            if (Presenter.SelectedTransportationDeviceSet == null) {
                Logger.Warning("Select a transportation device set");
                return;
            }

            if (Presenter.SelectedTravelRouteSet == null) {
                Logger.Warning("Select a travel route set.");
                return;
            }
            switch (Presenter.SelectedCalcObjectType) {
                case CalcObjectType.ModularHousehold: {
                    var hh = (ModularHousehold) CmbModularHouseholds.SelectedItem;

                    Presenter.AddCalcObject(hh,Presenter.SelectedChargingStationSet,
                        Presenter.SelectedTransportationDeviceSet,
                        Presenter.SelectedTravelRouteSet);
                }
                    break;
                default: throw new LPGException("Forgotten CalcObjectType");
            }
        }

        private void BtnRemoveLocation_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHouseholds.SelectedItem == null) {
                return;
            }

            var hhh = (HouseHousehold) LstHouseholds.SelectedItem;
            Presenter.RemoveHousehold(hhh);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void CmbCalcObjectType_OnSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbCalcObjectType.SelectedItem == null) {
                return;
            }

            Presenter.SelectedCalcObjectType = ((KeyValuePair<CalcObjectType, string>) CmbCalcObjectType.SelectedItem)
                .Key;
        }

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisHouse.HeaderString, Presenter.Delete);

        private void LstHouseholds_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstHouseholds.SelectedItem == null) {
                return;
            }

            var hhh = (HouseHousehold) LstHouseholds.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(hhh.CalcObject);
        }
    }
}