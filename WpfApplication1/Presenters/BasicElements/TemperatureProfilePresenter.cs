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
using LoadProfileGenerator.Views.BasicElements;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class TemperatureProfilePresenter : PresenterBaseDBBase<TemperatureProfileView> {
        [JetBrains.Annotations.NotNull] private readonly ApplicationPresenter _applicationPresenter;
        [JetBrains.Annotations.NotNull] private readonly CSVImporter _csvImporter;
        [JetBrains.Annotations.NotNull] private readonly TemperatureProfile _tp;

        public TemperatureProfilePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] TemperatureProfileView view,
            [JetBrains.Annotations.NotNull] TemperatureProfile tp) : base(view, "ThisProfile.HeaderString", tp, applicationPresenter) {
            _applicationPresenter = applicationPresenter;
            _tp = tp;
            _csvImporter = new CSVImporter(true);
        }

        [JetBrains.Annotations.NotNull]
        public CSVImporter CsvImporter => _csvImporter;

        [JetBrains.Annotations.NotNull]
        public TemperatureProfile ThisProfile => _tp;

        public void AddTemperaturePoint(DateTime time, double value) {
            _tp.AddTemperature(time, value);
            _tp.SaveToDB();
        }

        public override void Close(bool saveToDB, bool removeLast = false) {
            if (saveToDB) {
                _tp.SaveToDB();
            }
            _applicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete() {
            Sim.TemperatureProfiles.DeleteItem(_tp);
            Close(false);
        }

        public void DeleteAllDataPoints() {
            Logger.Info("Deleting " + ThisProfile.TemperatureValues.Count + " temperature datapoints...");
            ThisProfile.DeleteAllTemperatures();
            Logger.Info("Deleted all data points.");
        }

        public override bool Equals(object obj) {
            return obj is TemperatureProfilePresenter presenter && presenter.ThisProfile.Equals(_tp);
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

        public void ImportData() {
            Logger.Info("Importing " + _csvImporter.Entries.Count + " datapoints...");
            for (var i = 0; i < _csvImporter.Entries.Count; i++) {
                var ce = _csvImporter.Entries[i];
                var date = ce.Time;
                _tp.AddTemperature(date, ce.Value, null, false, false);
            }
            _tp.TemperatureValues.Sort();
            _tp.SaveToDB(true);
            var t = new Thread(_tp.SaveToDB);
            t.Start();
            Logger.Info("Imported all data points.");
        }

        public void RemoveTimepoint([JetBrains.Annotations.NotNull] TemperatureValue tdp) {
            _tp.DeleteOneTemperatur(tdp);
        }
    }
}