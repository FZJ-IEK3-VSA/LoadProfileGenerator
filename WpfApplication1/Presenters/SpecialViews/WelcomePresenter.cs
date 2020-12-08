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

using System.Windows;
using Common;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    public class WelcomePresenter : PresenterBaseWithAppPresenter<WelcomeView> {
        [JetBrains.Annotations.NotNull] private readonly ApplicationPresenter _applicationPresenter;

        public WelcomePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] WelcomeView view)
            : base(view, "WelcomeHeader", applicationPresenter) => _applicationPresenter = applicationPresenter;

        [CanBeNull]
        [UsedImplicitly]
        public string WarningString => Config.WarningString;
        [UsedImplicitly]
        public Visibility WarningVisibility {
            get {
                if (Config.WarningString?.Length > 0) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string WelcomeHeader { get; } = "Welcome";

        public override void Close(bool saveToDB, bool removeLast = false) {
            _applicationPresenter.CloseTab(this, removeLast);
        }

        public void Refresh() {
            OnPropertyChanged(nameof(WarningString));
        }
    }
}