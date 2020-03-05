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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Automation.ResultFiles;
using Common;
using Database;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Presenters {
    public abstract class PresenterBaseWithAppPresenter<T> : PresenterBase<T> where T : class {
        [NotNull] private readonly ApplicationPresenter _applicationPresenter;
        protected PresenterBaseWithAppPresenter([NotNull] T view, [NotNull] string tabHeaderPath,
                                       [NotNull] ApplicationPresenter applicationPresenter):base(view,tabHeaderPath)
        {
            if (!Config.IsInUnitTesting && view == null)
            {
                throw new LPGException("View was null");
            }

            _applicationPresenter = applicationPresenter ?? throw new LPGException("Applicationpresenter was null");
        }

        [NotNull]
        public ApplicationPresenter ApplicationPresenter => _applicationPresenter;

        [NotNull]
        public  Simulator Sim => _applicationPresenter.Simulator ?? throw new LPGException("Sim was null");
    }

    public abstract class PresenterBase<T> : Notifier where T : class {

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        protected PresenterBase([NotNull] T view, [NotNull] string tabHeaderPath)
        {
            if (view == null && !Config.IsInUnitTesting) {
                throw new LPGException("View was null");
            }

            TabHeaderPath = tabHeaderPath;
            View = view;
        }


        [NotNull]
        public string TabHeaderPath { get; }

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public bool UnloadingStarted { get; set; }

        [NotNull]
        public T View { get; }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public void AskDeleteQuestion([NotNull] string headerstring, [NotNull] Action delete)
        {
            var s = "Are you sure you want to delete the element:" + Environment.NewLine + headerstring + "?";
            var dr = MessageWindows.ShowYesNoMessage(s, "Delete?");
            if (dr == MessageBoxResult.Yes) {
                delete();
            }
        }

        public abstract void Close(bool saveToDB, bool removeLast = false);

        [UsedImplicitly]
        public void ScrollTo([CanBeNull]int? value)
        {
            if (value != null) {
                dynamic myVar = View;

                Type typeOfDynamic = myVar.GetType();
                var properties = typeOfDynamic.GetProperties();
                var fields = typeOfDynamic.GetFields();
                var names = properties.Select(x => x.Name).ToList();
                names.AddRange(fields.Select(x => x.Name));
                var exist = names.Contains("ScViewer");
                if (exist) {
                    ScrollViewer sc = myVar.ScViewer;
                    sc.ScrollToVerticalOffset((double) value);
                }
            }
        }
    }
}