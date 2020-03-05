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
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Model {
    /// <summary>
    ///     Interaktionslogik f�r GlobalTree.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class GlobalTree {
        public GlobalTree()
        {
            InitializeComponent();
        }

        [NotNull]
        private ApplicationPresenter Presenter => (ApplicationPresenter) DataContext;

        public void SelectItem([CanBeNull] object toSelect, bool expand) => GlobalTreeA.SelectItem(toSelect, expand);

        private void MyTreeViewMouseDoubleClick([CanBeNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            if (sender is TreeView ti) {
                Presenter.OpenItem(ti.SelectedItem);
                e.Handled = false;
            }
        }

        private void TreeView1SelectedItemChanged([CanBeNull] object sender,
            [CanBeNull] RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView ti) {
                Presenter.OpenItem(ti.SelectedItem);
                if (e != null) {
                    e.Handled = false;
                }
            }
        }

        private void TxtSearchBox_OnTextChanged([CanBeNull] object sender, [CanBeNull] TextChangedEventArgs e)
            => Presenter.FilterString = TxtSearchBox.Text;
    }
}