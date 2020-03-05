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

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

#endregion

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r PersonView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TemplatePersonView {
        public TemplatePersonView() {
            InitializeComponent();
        }

        [NotNull]
        private TemplatePersonPresenter Presenter => (TemplatePersonPresenter)DataContext;

        private void BtnAddTrait([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (CmbTraits.SelectedItem == null) {
                return;
            }
            var t = (HouseholdTrait) CmbTraits.SelectedItem;
            Presenter.ThisTemplate.AddTrait(t);
            Presenter.RefreshTree();
        }

        private void BtnRemoveTrait([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (Presenter.SelectedItem == null) {
                return;
            }
            if (Presenter.SelectedItem is TemplatePersonTrait tpt)
            {
                Presenter.ThisTemplate.DeleteTraitFromDB(tpt);
                Presenter.RefreshTree();
            }
        }

        private void ClearCHHClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            Presenter.ThisTemplate.BaseHousehold = null;
            Presenter.ThisTemplate.SaveToDB();
        }

        private void ClearPerson_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            Presenter.ThisTemplate.BasePerson = null;
            Presenter.ThisTemplate.SaveToDB();
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisTemplate.HeaderString, Presenter.Delete);

        private void TreeTraitsSelectedItemChanged([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e) => Presenter
            .SelectedItem = e.NewValue;
    }
}