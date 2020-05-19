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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Automation.ResultFiles;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    public class ResultPresenter : PresenterBaseWithAppPresenter<ResultView> {
        [NotNull]
        public string ResultPath { get; }
        //private readonly CalculationResult _calculationResult;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ResultFileEntry> _filteredResultFiles =
            new ObservableCollection<ResultFileEntry>();

        [NotNull] private readonly string _householdname;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ResultFileEntry> _resultFiles;

        [CanBeNull] private string _resultFilter;

        public ResultPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] ResultView view, [NotNull] string householdname,
                               [NotNull] string resultPath) : base(view, "HeaderString", applicationPresenter)
        {
            ResultPath = resultPath;
            _householdname = householdname;
            SqlResultLoggingService srls = new SqlResultLoggingService(resultPath);
            ResultFileEntryLogger rfel = new ResultFileEntryLogger(srls);
            var rfes = rfel.Load();
            _resultFiles = new ObservableCollection<ResultFileEntry>(rfes);
            foreach (var entry in _resultFiles) {
                _filteredResultFiles.Add(entry);
            }

            ResultPath = resultPath;
        }

        //[UsedImplicitly]public DateTime Endtime => _calculationResult.CalcEndTime;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ResultFileEntry> FilteredResultFiles => _filteredResultFiles;

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Results for " + _householdname;

        [NotNull]
        [UsedImplicitly]
        public string Householdname => _householdname;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ResultFileEntry> ResultFiles => _resultFiles;

        [CanBeNull]
        [UsedImplicitly]
        public string ResultFilter {
            get => _resultFilter;
            set {
                _resultFilter = value;
                _filteredResultFiles.Clear();
                if (_resultFiles == null) {
                    throw new LPGException("Resultfiles was null");
                }
                foreach (var entry in _resultFiles) {
                    if (string.IsNullOrEmpty(_resultFilter) ||
                        entry.Name?.ToLower(CultureInfo.CurrentCulture)
                            .Contains(_resultFilter.ToLower(CultureInfo.CurrentCulture))==true) {
                        _filteredResultFiles.Add(entry);
                    }
                }
                OnPropertyChanged(nameof(ResultFilter));
            }
        }

/*        [UsedImplicitly]
        public DateTime SimEndtime => _calculationResult.SimEndTime;

        [UsedImplicitly]
        public DateTime SimStarttime => _calculationResult.SimStartTime;

        [UsedImplicitly]
        public DateTime Starttime => _calculationResult.CalcStartTime;*/

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals(object obj)
        {
            return obj is ResultPresenter presenter && presenter._householdname.Equals(_householdname) &&
                   presenter.ResultPath.Equals(ResultPath);
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
        public static void StartExcel([NotNull] ResultFileEntry resultFile)
        {
#pragma warning disable S2930 // "IDisposables" should be disposed
#pragma warning disable CC0022 // Should dispose object
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var excel = new Process();
            excel.StartInfo.FileName = resultFile.FullFileName??throw new LPGException("fullfilename was null");
            excel.StartInfo.UseShellExecute = true;
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CC0022 // Should dispose object
#pragma warning restore S2930 // "IDisposables" should be disposed
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
}