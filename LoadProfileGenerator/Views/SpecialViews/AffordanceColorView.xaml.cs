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
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;
using Button = System.Windows.Controls.Button;

namespace LoadProfileGenerator.Views.SpecialViews {
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class AffordanceColorView {
        public AffordanceColorView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private AffordanceColorPresenter Presenter => (AffordanceColorPresenter) DataContext;

        private void BtnCheckDiffCheckerClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Logger.Info("Starting duplicate color check...");

            for (var index = 0; index < LstAffordances.Items.Count - 1; index++) {
                var aff = (Affordance) LstAffordances.Items[index];
                var aff2 = (Affordance) LstAffordances.Items[index + 1];
                var diff = CalcDiff(aff, aff2);
                if (aff != aff2 && diff > 1 && diff < 64) {
                    MessageWindowHandler.Mw.ShowInfoMessage(
                        "Color gap of " + diff + " between affordances:" + Environment.NewLine + aff.Name +
                        Environment.NewLine + aff2.Name,
                        "Color Gap");
                    foreach (var item in LstAffordances.Items) {
                        if (item == aff) {
                            LstAffordances.SelectedItem = item;
                        }
                    }

                    Logger.Info("Color gap check finished.");
                    return;
                }
            }

            MessageWindowHandler.Mw.ShowInfoMessage("No color gap found.", "Color Gap Check");
            Logger.Info("Color gap check finished.");
        }

        private void BtnDuplicateCheckClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Logger.Info("Starting duplicate color check...");
            foreach (var aff in Presenter.Affordances) {
                foreach (var aff2 in Presenter.Affordances) {
                    if (aff != aff2 && aff.Red == aff2.Red && aff.Green == aff2.Green && aff.Blue == aff2.Blue) {
                        MessageWindowHandler.Mw.ShowInfoMessage(
                            "Duplicate colors found in these two affordances:" + Environment.NewLine + aff.Name +
                            Environment.NewLine + aff2.Name,
                            "Duplicate colors");
                        foreach (var item in LstAffordances.Items) {
                            if (item == aff) {
                                LstAffordances.SelectedItem = item;
                            }
                        }

                        Logger.Info("Duplicate color check finished.");
                        return;
                    }
                }
            }

            MessageWindowHandler.Mw.ShowInfoMessage("No duplicate colors found.", "Duplicate colors");
            Logger.Info("Duplicate color check finished.");
        }

        private void BtnPickColorClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (!(sender is Button button)) {
                throw new LPGException("Could not convert the button");
            }

            var aff = (Affordance) button.DataContext;
            Color c2 = Color.Aqua;
            //using (var cd = new ColorDialog()) {
            //    var mediaColor = aff.CarpetPlotColor;

            //    var c = Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
            //    cd.Color = c;
            //    cd.ShowDialog();
            //    c2 = cd.Color;
            //}
            //TODO: make a color selector
            aff.CarpetPlotColor = new ColorRGB(c2.A, c2.R, c2.G, c2.B);
            aff.SaveToDB();
        }

        private void BtnResortClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshSort();

        private static int CalcDiff([JetBrains.Annotations.NotNull] Affordance a1, [JetBrains.Annotations.NotNull] Affordance a2)
        {
            var count = Math.Abs(a1.Red - a2.Red);
            count += Math.Abs(a1.Blue - a2.Blue);
            count += Math.Abs(a1.Green - a2.Green);
            return count;
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void LstAffordances_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstAffordances.SelectedItem is Affordance aff) {
                Presenter.ApplicationPresenter.OpenItem(aff);
            }
        }
    }
}