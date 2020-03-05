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
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Controls.Usercontrols
{
    /// <summary>
    ///     Interaktionslogik f�r PermissionLine.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class PermissionLine
    {
        public PermissionLine()
        {
            InitializeComponent();
        }

        [NotNull]
        public TimeLimitEntry TimeLimitEntry => Presenter.Entry;
        [NotNull]
        public PermissionLinePresenter Presenter => (PermissionLinePresenter)DataContext;

        public event EventHandler AddClicked;

        private void ComboBox_SelectionChanged([CanBeNull] object sender, [CanBeNull] SelectionChangedEventArgs e)
            =>
                Logger.Debug(
                        "PermissionLine: " + ActualWidth + " Overgrid: " + OverGrid.ActualWidth + " GridWidth: " +
                        MainGrid.ActualWidth);

        private void HandleAddClicked([NotNull]object sender, [NotNull] EventArgs e)
        {
            AddClickedEventArgs aca = new AddClickedEventArgs(Presenter.Entry);
            OnAddClicked(aca);
            Presenter.SetAllOnProperty();
        }

        private void HandleRemoveClick([CanBeNull]object sender, [CanBeNull] RoutedEventArgs e)
        {
            AddClickedEventArgs aca = new AddClickedEventArgs(Presenter.Entry);
            OnRemoveClicked(aca);
            Presenter.SetAllOnProperty();
        }

        private void HandleShowPreviewClick([CanBeNull]object sender, [CanBeNull] RoutedEventArgs e)
        {
            AddClickedEventArgs aca = new AddClickedEventArgs(Presenter.Entry);
            OnShowPreviewClicked(aca);
        }

        protected virtual void OnAddClicked([CanBeNull]EventArgs e)
        {
            EventHandler handler = AddClicked;
            handler?.Invoke(this, e);
        }

        protected virtual void OnRemoveClicked([CanBeNull]EventArgs e)
        {
            EventHandler handler = RemoveClicked;
            handler?.Invoke(this, e);
        }

        protected virtual void OnShowPreviewClicked([CanBeNull]EventArgs e)
        {
            EventHandler handler = ShowPreviewClicked;
            handler?.Invoke(this, e);
        }

        public event EventHandler RemoveClicked;
        public event EventHandler ShowPreviewClicked;
    }

    public class AddClickedEventArgs : EventArgs
    {
        public AddClickedEventArgs([NotNull] TimeLimitEntry tle) => TimeLimitEntry = tle;

        [NotNull]
        public TimeLimitEntry TimeLimitEntry { get;  }
    }
}