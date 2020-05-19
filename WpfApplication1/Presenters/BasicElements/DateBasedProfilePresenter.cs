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
using System.Threading;
using Common;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.BasicElements;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class DateBasedProfilePresenter : PresenterBaseDBBase<DateBasedProfileView> {
        [NotNull] private readonly CSVImporter _csvImporter;
        [NotNull] private readonly DateBasedProfile _dbp;

        public DateBasedProfilePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] DateBasedProfileView view,
            [NotNull] DateBasedProfile tp) : base(view, "ThisProfile.HeaderString", tp, applicationPresenter) {
            _dbp = tp;
            _csvImporter = new CSVImporter(true);
        }

        [NotNull]
        public CSVImporter CsvImporter => _csvImporter;

        [NotNull]
        public DateBasedProfile ThisProfile => _dbp;

        [NotNull]
        public DateProfileDataPoint AddDataPoint(DateTime time, double value) {
            DateProfileDataPoint dp= _dbp.AddNewDatePoint(time, value);
            _dbp.SaveToDB();
            return dp;
        }

        public override void Close(bool saveToDB, bool removeLast = false) {
            if (saveToDB) {
                _dbp.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete() {
           Sim.DateBasedProfiles.DeleteItem(_dbp);
            Close(false);
        }

        public void DeleteAllDataPoints() {
            Logger.Info("Deleting " + ThisProfile.Datapoints.Count + " all datapoints...");
            ThisProfile.DeleteAllTimepoints();
            Logger.Info("Deleted all data points.");
        }

        public override bool Equals(object obj) {
            return obj is DateBasedProfilePresenter presenter && presenter.ThisProfile.Equals(_dbp);
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

        public void ImportData() {
            Logger.Info("Importing " + _csvImporter.Entries.Count + " datapoints...");
            for (var i = 0; i < _csvImporter.Entries.Count; i++) {
                var ce = _csvImporter.Entries[i];
                var date = ce.Time;
                _dbp.AddNewDatePoint(date, ce.Value, false);
            }
            _dbp.Datapoints.Sort();
            var pbw = new ProgressbarWindow("Importing...",
                "Importing " + _csvImporter.Entries.Count + " entries.", _csvImporter.Entries.Count);
            pbw.Show();
            var t = new Thread(() => {
                _dbp.SaveToDB(pbw.UpdateValue);
                Logger.Get().SafeExecute(pbw.Close);
            });
            t.Start();

            Logger.Info("Imported all data points.");
        }

        public void RemoveTimepoint([NotNull] DateProfileDataPoint tdp) {
            _dbp.DeleteDatePoint(tdp);
        }
    }
}