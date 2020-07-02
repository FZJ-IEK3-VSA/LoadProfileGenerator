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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Common;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.BasicElements;

#endregion

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class TimeProfilePresenter : PresenterBaseDBBase<TimeProfileView> {
        [NotNull] private readonly CSVImporter _csvImporter;
        [NotNull] private readonly TimeBasedProfile _tp;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIns;

        public TimeProfilePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TimeProfileView view,
            [NotNull] TimeBasedProfile tp)
            : base(view, "ThisProfile.HeaderString", tp, applicationPresenter)
        {
            _tp = tp;
            _csvImporter = new CSVImporter(false);
            _usedIns = new ObservableCollection<UsedIn>();
            TimeProfileTypes.Add(TimeProfileType.Relative);
            TimeProfileTypes.Add(TimeProfileType.Absolute);
            RefreshUsedIn();
        }

        [NotNull]
        public CSVImporter CsvImporter => _csvImporter;

        [NotNull]
        public TimeBasedProfile ThisProfile => _tp;

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeProfileType> TimeProfileTypes { get; } =
            new ObservableCollection<TimeProfileType>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public void AddTimepoint(TimeSpan ts, double value)
        {
            _tp.AddNewTimepoint(ts, value);
            _tp.SaveToDB();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public new void AskDeleteQuestion([NotNull] string headerstring, [NotNull] Action delete)
        {
            var s = "Are you sure you want to delete the element:"+ Environment.NewLine + headerstring + "?";
            var affordances = new Dictionary<Affordance, bool>();
            foreach (var aff in Sim.Affordances.Items) {
                foreach (var affordanceDevice in aff.AffordanceDevices) {
                    if (affordanceDevice.TimeProfile == _tp && !affordances.ContainsKey(aff)) {
                        affordances.Add(aff, true);
                    }
                }
            }
            if (affordances.Count > 0) {
                s = s +
                    Environment.NewLine+ "The following affordances are affected and the affected device entries would be deleted from them:"+ Environment.NewLine;
                foreach (var affordance in affordances.Keys) {
                    s = s + affordance + Environment.NewLine;
                }
            }
            var dr = MessageWindowHandler.Mw.ShowYesNoMessage(s, "Delete?");
            if (dr == LPGMsgBoxResult.Yes) {
                delete();
            }
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _tp.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.Timeprofiles.DeleteItem(_tp);
            Close(false);
        }

        public void DeleteAllDataPoints()
        {
            Logger.Info("Deleting " + ThisProfile.ObservableDatapoints.Count + " datapoints...");
            ThisProfile.DeleteAllTimepoints();
            Logger.Info("Deleted all data points.");
        }

        public override bool Equals(object obj)
        {
            return obj is TimeProfilePresenter presenter && presenter.ThisProfile.Equals(_tp);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }

        public void ImportData([NotNull] Action updateGraph)
        {
            var pbw = new ProgressbarWindow("Importing...",
                "Importing " + _csvImporter.Entries.Count + " datapoints.",
                _csvImporter.Entries.Count + _tp.DatapointsCount);
            pbw.Show();
            var t = new Thread(() => {
                Logger.Info("Importing " + _csvImporter.Entries.Count + " datapoints...");
                var startcount = _tp.DatapointsCount;
                for (var i = 0; i < _csvImporter.Entries.Count; i++) {
                    var ce = _csvImporter.Entries[i];

                    Logger.Get().SafeExecute(() => _tp.AddNewTimepoint(ce.TimeSinceStart, ce.Value, false));
                }
                while (startcount + _csvImporter.Entries.Count != _tp.DatapointsCount) {
                    Thread.Sleep(10);
                }
                _tp.SaveToDB(pbw.UpdateValue);
                lock (_tp.ObservableDatapoints) {
                    Logger.Get().SafeExecute(_tp.ObservableDatapoints.Sort);
                }
                Logger.Get().SafeExecute(pbw.Close);
                Logger.Get().SafeExecute(updateGraph);
            });
            t.Start();
            Logger.Info("Imported all data points.");
        }

        public void RefreshUsedIn()
        {
            var usedIn = _tp.CalculateUsedIns(Sim);
            _usedIns.Clear();
            foreach (var dui in usedIn) {
                _usedIns.Add(dui);
            }
        }

        public void RemoveTimepoint([NotNull] TimeDataPoint tdp)
        {
            _tp.DeleteTimepoint(tdp);
        }
    }
}