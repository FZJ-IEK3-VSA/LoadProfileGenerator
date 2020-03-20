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
using System.Threading;
using System.Windows;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;
using Microsoft.Win32;

namespace LoadProfileGenerator.Views.SpecialViews {
    /// <summary>
    ///     Interaktionslogik f�r ImportView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class ImportView {
        public ImportView() {
            InitializeComponent();
        }

        [NotNull]
        private ImportPresenter Presenter => (ImportPresenter)DataContext;

        private void BtnBrowseForFile([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var ofd = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                Filter = "DB3 Files (*.Db3)|*.db3|All files (*.*)|*.*"
            };
            var result = ofd.ShowDialog();
            if (result == true) {
                var filename = ofd.FileName;
                Presenter.FileName = filename;
            }
        }

        private void BtnGetPreview([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var pbw = new ProgressbarWindow("Loading..", "Generating Preview",
                Presenter.ApplicationPresenter.CurrentCategories?.Count??0);
            pbw.Show();
            var filename = Presenter.FileName;
            var dm = Presenter.ThisMerger;
            var t = new Thread(() => {
                try {
                    dm.RunFindItems(filename, pbw.UpdateValue);
                    Logger.Get().SafeExecute(pbw.Close);
                    Logger.Get().SafeExecute(() => LstItemsToImport.ResizeColummns());
                }
                catch (Exception ex) {
                    Logger.Exception(ex);
                    MessageWindowHandler.Mw.ShowDebugMessage(ex);
                }
            });
            t.Start();
        }

        private void BtnImportNow([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var pbw = new ProgressbarWindow("Importing...", "Importing items...",
                Presenter.ThisMerger.ItemsToImport.Count);
            pbw.Show();
            Presenter.ThisMerger.DisplayMessageBox = true;
            var dm = Presenter.ThisMerger;
            var t = new Thread(() => {
                dm.RunImport(pbw.UpdateValue);
                Logger.Get().SafeExecute(pbw.Close);
                var oldPath = Shell.DB3Path;
                if (oldPath == null) {
                    throw new LPGException("Old path was null");
                }
                Logger.Get().SafeExecute(() => {
                    Presenter.ApplicationPresenter.Shell.CloseDB();
                    Presenter.ApplicationPresenter.Shell.OpenDatabase(oldPath);
                });
            });
            t.Start();
        }

        private void BtnSelectAll([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            foreach (var item in Presenter.ThisMerger.ItemsToImport) {
                item.Import = true;
            }
        }

        private void BtnSelectNone([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            foreach (var item in Presenter.ThisMerger.ItemsToImport) {
                item.Import = false;
            }
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);
    }
}