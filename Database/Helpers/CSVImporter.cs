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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

namespace Database.Helpers {
    public sealed class CSVImporter : INotifyPropertyChanged {
        private readonly bool _considerTime;

        private int _column;

        [CanBeNull] private string _fileName;

        private int _headerLineCount;
        [JetBrains.Annotations.NotNull] private string _previewText;
        private char _separator;
        private int _timeColumn;
        private TimeSpan _timeSpan;

        public CSVImporter(bool considerTime) {
            _column = 1;
            _separator = ';';

            _headerLineCount = 1;
            _previewText = string.Empty;
            _considerTime = considerTime;
            _timeSpan = new TimeSpan(0, 1, 0);
            if (_considerTime) {
                _timeColumn = 0;
                _column = 1;
            }
            Entries = new ObservableCollection<Entry>();
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [UsedImplicitly]
        public int Column {
            get => _column + 1;
            set {
                if (value == int.MinValue) {
                    throw new ArgumentOutOfRangeException(nameof(Column), "input must be greater than Int32.MinValue");
                }
                _column = value - 1;
                RefreshEntries();
                OnPropertyChanged(nameof(Column));
            }
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Entry> Entries { get; }

        [CanBeNull]
        public string FileName {
            [UsedImplicitly] get => _fileName;
            set {
                _fileName = value;
                RefreshEntries();
                OnPropertyChanged(nameof(FileName));
            }
        }

        [UsedImplicitly]
        public int HeaderLineCount {
            get => _headerLineCount;
            set {
                _headerLineCount = value;
                RefreshEntries();
                OnPropertyChanged(nameof(HeaderLineCount));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string PreviewText {
            get => _previewText;
            set {
                _previewText = value;
                OnPropertyChanged(nameof(PreviewText));
            }
        }

        [UsedImplicitly]
        public char Separator {
            get => _separator;
            set {
                _separator = value;
                RefreshEntries();
                OnPropertyChanged(nameof(Separator));
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [UsedImplicitly]
        public int TimeColumn {
            get => _timeColumn + 1;
            set {
                if (value == int.MinValue) {
                    throw new ArgumentOutOfRangeException(nameof(TimeColumn),
                        "input must be greater than Int32.MinValue");
                }
                _timeColumn = value - 1;
                RefreshEntries();
                OnPropertyChanged(nameof(TimeColumn));
            }
        }

        [UsedImplicitly]
        public TimeSpan TimeSpan {
            get => _timeSpan;
            set {
                _timeSpan = value;
                OnPropertyChanged(nameof(TimeSpan));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotNull]
        private Entry MakeEntry(int linecount, [ItemNotNull] [JetBrains.Annotations.NotNull] string[] strarr, ref TimeSpan ts) {
            var e = new Entry
            {
                EntryNumber = linecount
            };
            var number1 = strarr[_column];
            if (number1.Contains(".") && !number1.Contains(",") &&
                CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator == ",") {
                number1 = number1.Replace(".", ",");
            }
            var success = double.TryParse(number1, out var d);
            if (!success) {
                success = double.TryParse(strarr[_column], out d);
            }
            if (!success) {
                d = 0;
            }
            e.Value = d;
            if (_considerTime) {
                var successdt = DateTime.TryParse(strarr[_timeColumn], CultureInfo.CurrentCulture, DateTimeStyles.None,
                    out var dt);
                if (!successdt) {
                    dt = DateTime.Now;
                }
                e.Time = dt;
            }
            else {
                e.TimeSinceStart = ts;
                ts = ts.Add(_timeSpan);
            }

            return e;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([JetBrains.Annotations.NotNull] string propertyName) {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void RefreshEntries() {
            Logger.Info("Generating preview for time point import...");
            Entries.Clear();
            if (_column < 0 || _column > 1000) {
                Logger.Warning("Aborted preview. Column invalid.");
                return;
            }
            if (_headerLineCount < 0 || _headerLineCount > 1000) {
                Logger.Warning("Aborted preview. Header Line Count invalid.");
                return;
            }
            if (Separator == 0) {
                Logger.Warning("Aborted preview. No Delimiter");
                return;
            }
            if (string.IsNullOrEmpty(_fileName)) {
                Logger.Warning("Aborted preview. No Filename.");
                return;
            }
            if (_considerTime) {
                if (_timeColumn < 0 || _timeColumn > 1000) {
                    Logger.Warning("Aborted preview. Timecolumn invalid.");
                    return;
                }
            }
            try {
                using (var sr = new StreamReader(_fileName)) {
                    var previewTemp = new StringBuilder();
                    const char cr = (char) 13;
                    const char lf = (char) 10;
                    var linecount = 0;
                    for (var i = 0; i < HeaderLineCount; i++) {
                        var a = sr.ReadLine();
                        if (previewTemp.Length < 1000) {
                            previewTemp.Append(a + cr + lf);
                        }
                    }
                    var ts = new TimeSpan(0);
                    while (!sr.EndOfStream && linecount < 1000000) {
                        var line = sr.ReadLine();
                        if (line == null) {
                            throw new LPGException("Line was null");
                        }
                        if (previewTemp.Length < 1000) {
                            previewTemp.Append(line + cr + lf);
                        }
                        var strarr = line.Split(_separator);
                        var maxcolumn = _column; // consider max column
                        if (_considerTime && _timeColumn > _column) {
                            maxcolumn = _timeColumn;
                        }
                        if (strarr.Length > maxcolumn) {
                            var e = MakeEntry(linecount, strarr, ref ts);
                            Entries.Add(e);
                        }
                        linecount++;
                    }
                    // cause notify to fire
                    PreviewText = previewTemp.ToString();
                }
            }
            catch (Exception e) {
                Logger.Error("Error:" + e.Message);
                Logger.Exception(e);
            }
        }

        #region Nested type: Entry

        public class Entry {
            [UsedImplicitly]
            public int EntryNumber { get; set; }

            public DateTime Time { get; set; }
            public TimeSpan TimeSinceStart { get; set; }

            [JetBrains.Annotations.NotNull]
            [UsedImplicitly]
            public string TimeSpanString => TimeSinceStart.ToString();

            [JetBrains.Annotations.NotNull]
            [UsedImplicitly]
            public string TimeString => Time.ToString(CultureInfo.CurrentCulture);

            public double Value { get; set; }
        }

        #endregion
    }
}