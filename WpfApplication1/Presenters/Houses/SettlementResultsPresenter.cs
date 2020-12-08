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
/*
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Common;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

namespace LoadProfileGenerator.Presenters.Houses {
    public class SettlementResultsPresenter : PresenterBase<SettlementResultView> {
        private readonly DateTime _calcEndTime;
        private readonly DateTime _calcStartTime;
        private readonly string _settlementName;

        public SettlementResultsPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] SettlementResultView view,
            string settlementName, ObservableCollection<ResultFileEntry> resultFileEntries) :
            base(view, "HeaderString", applicationPresenter)
        {
            _settlementName = settlementName;
            _results = new ObservableCollection<CalculationResult>(results);
            if (_results.Count == 0) {
                throw new DataIntegrityException("No results were returned!");
            }
            SimStartTime = _results[0].SimStartTime;
            SimEndTime = _results[0].SimEndTime;
            _calcStartTime = _results[0].CalcStartTime;
            _calcEndTime = _results[0].CalcEndTime;
            ResultFiles = resultFileEntries;
            foreach (var calculationResult in results) {
                if (calculationResult != null) {
                    if (calculationResult.CalcStartTime < _calcStartTime) {
                        _calcStartTime = calculationResult.CalcStartTime;
                    }
                    if (calculationResult.CalcEndTime > _calcEndTime) {
                        _calcEndTime = calculationResult.CalcEndTime;
                    }
                }
            }
        }

        [UsedImplicitly]
        public DateTime CalcEndTime => _calcEndTime;

        [UsedImplicitly]
        public DateTime CalcStartTime => _calcStartTime;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string HeaderString => "Results for " + _settlementName;

        [UsedImplicitly]
        public ObservableCollection<ResultFileEntry> ResultFiles { get; }

        [UsedImplicitly]
        public ObservableCollection<CalculationResult> Results => _results;

        [UsedImplicitly]
        public string SettlementName => _settlementName;

        [UsedImplicitly]
        public DateTime SimEndTime { get; }

        [UsedImplicitly]
        public DateTime SimStartTime { get; }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            var presenter = obj as SettlementResultsPresenter;
            return presenter != null && presenter._settlementName.Equals(_settlementName) &&
                   presenter._calcEndTime.Equals(_calcEndTime);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void StartExcel([JetBrains.Annotations.NotNull] ResultFileEntry resultFile)
        {
#pragma warning disable S2930 // "IDisposables" should be disposed
#pragma warning disable CC0022 // Should dispose object
            var excel = new Process();
#pragma warning restore CC0022 // Should dispose object
#pragma warning restore S2930 // "IDisposables" should be disposed
            excel.StartInfo.FileName = resultFile.FullFileName;
            excel.StartInfo.UseShellExecute = true;
            try {
                excel.Start();
            }
            catch (Exception e) {
                Logger.Error("Failed to start:" + e.Message);
                Logger.Exception(e);
            }
        }

        public override string ToString() => HeaderString;
    }
}*/