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
using System.Windows.Input;
using Common.Enums;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;

#endregion

namespace LoadProfileGenerator.Views.SpecialViews {
    /// <summary>
    ///     Interaktionslogik f�r SettingsView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class SettingsView {
        public SettingsView()
        {
            InitializeComponent();
            CmbShowSettling.Items.Add("true");
            CmbShowSettling.Items.Add("false");
        }

        [JetBrains.Annotations.NotNull]
        private SettingPresenter Presenter => (SettingPresenter) DataContext;

        private void AddSelected()
        {
            if (LstNotSelected.SelectedItem == null) {
                return;
            }

            var selected = (string) LstNotSelected.SelectedItem;
            var o = CalcOptionHelper.CalcOptionDictionary.FirstOrDefault(x => x.Value == selected).Key;
            Presenter.AddOption(o);
        }

        private void ApplyOptionDefaultClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.ApplyOptionDefault();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void LstNotSelectedOptions_MouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e) => AddSelected();

        private void
            LstSelectedOptions_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e) =>
            RemoveSelected();

        private void NewRandomSeed_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var r = new Random();
            Presenter.ThisConfig.RandomSeed = r.Next();
        }

        private void OptionAdd_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => AddSelected();

        private void OptionRemove_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => RemoveSelected();

        private void RemoveSelected()
        {
            if (LstSelected.SelectedItem == null) {
                return;
            }

            var selected = (string) LstSelected.SelectedItem;
            var o = CalcOptionHelper.CalcOptionDictionary.FirstOrDefault(x => x.Value == selected).Key;
            Presenter.RemoveOption(o);
        }
    }
}