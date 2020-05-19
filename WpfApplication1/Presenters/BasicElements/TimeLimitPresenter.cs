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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Views.BasicElements;

#endregion

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class TimeLimitPresenter : PresenterBaseDBBase<TimeLimitView> {
        [NotNull] private readonly TimeLimitView _dtv;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<PermissionLinePresenter> _plps =
            new ObservableCollection<PermissionLinePresenter>();

        [CanBeNull] private BitmapImage _bitmapAllPermittedTime;

        [CanBeNull] private BitmapImage _bitmapSinglePermittedTime;

        [CanBeNull] private ModularHousehold _household;

        private double _imageHeight;
        [NotNull] private GeographicLocation _previewGeographicLocation;
        [NotNull] private TemperatureProfile _previewTemperatureProfile;

        public TimeLimitPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TimeLimitView view, [NotNull] TimeLimit timeLimit)
            : base(view, "ThisTimeLimit.HeaderString", timeLimit, applicationPresenter)
        {
            _dtv = view;
            ThisTimeLimit = timeLimit;

            _previewGeographicLocation = Sim.GeographicLocations.MyItems[0];
            _previewTemperatureProfile = Sim.TemperatureProfiles.MyItems[0];
            if (Sim.ModularHouseholds.It.Count > 0) {
                _household = Sim.ModularHouseholds.MyItems[0];
            }
            MakeCalculatingImage();
            if (ThisTimeLimit.RootEntry == null) {
                ThisTimeLimit.AddTimeLimitEntry(null, Sim.DateBasedProfiles.MyItems);
            }
            RefreshAllPermissionLines();

            view.SetOneRow(ThisTimeLimit.RootEntry);
            RefreshUsedIn();
        }

        [CanBeNull]
        [UsedImplicitly]
        public BitmapImage BitmapAllPermittedTime {
            get => _bitmapAllPermittedTime;
            set {
                _bitmapAllPermittedTime = value;
                OnPropertyChanged(nameof(BitmapAllPermittedTime));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public BitmapImage BitmapSinglePermittedTime {
            get => _bitmapSinglePermittedTime;
            set {
                _bitmapSinglePermittedTime = value;
                OnPropertyChanged(nameof(BitmapSinglePermittedTime));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<GeographicLocation> GeographicLocations
            => Sim.GeographicLocations.MyItems;

        public double ImageHeight {
            get => _imageHeight;
            set {
                _imageHeight = value;
                OnPropertyChanged(nameof(ImageHeight));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> ModularHouseholds
            => Sim.ModularHouseholds.MyItems;

        [NotNull]
        [UsedImplicitly]
        public GeographicLocation PreviewGeographicLocation {
            get => _previewGeographicLocation;
            set {
                _previewGeographicLocation = value;
                OnPropertyChanged(nameof(PreviewGeographicLocation));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public ModularHousehold PreviewHousehold {
            get => _household;
            set {
                _household = value;
                OnPropertyChanged(nameof(PreviewHousehold));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public TemperatureProfile PreviewTemperatureProfile {
            get => _previewTemperatureProfile;
            set {
                _previewTemperatureProfile = value;
                OnPropertyChanged(nameof(PreviewTemperatureProfile));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TimeLimit SelectedTimeLimit { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TemperatureProfile> TemperaturProfiles
            => Sim.TemperatureProfiles.MyItems;
        [NotNull]
        public TimeLimit ThisTimeLimit { get; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeLimit> TimeLimits => Sim.TimeLimits.MyItems;

        [CanBeNull]
        [UsedImplicitly]
        public BitmapImage TmpCalcImage { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIn { get; } = new ObservableCollection<UsedIn>();

        public void AddSubentry([NotNull] TimeLimitEntry parentEntry)
        {
            ThisTimeLimit.AddTimeLimitEntry(parentEntry, parentEntry, Sim.DateBasedProfiles.MyItems);
            RefreshAllPermissionLines();
            _dtv.SetOneRow(ThisTimeLimit.RootEntry);
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                ThisTimeLimit.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void CreateCopy()
        {
            var newtimelimit = Sim.TimeLimits.CreateNewItem(Sim.ConnectionString);
            newtimelimit.Name = ThisTimeLimit.Name + " (Copy)";

            newtimelimit.ImportFromOtherTimeLimit(ThisTimeLimit, Sim.DateBasedProfiles.It);
            ApplicationPresenter.OpenItem(newtimelimit);
        }

        public void Delete()
        {
            Sim.TimeLimits.DeleteItem(ThisTimeLimit);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var limitPresenter = obj as TimeLimitPresenter;
            return limitPresenter?.ThisTimeLimit.Equals(ThisTimeLimit) == true;
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

        public void ImportTimeLimit()
        {
            if (SelectedTimeLimit == null) {
                return;
            }
            ThisTimeLimit.ImportFromOtherTimeLimit(SelectedTimeLimit, Sim.DateBasedProfiles.MyItems);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void MakeCalculatingImage()
        {
#pragma warning disable S2930 // "IDisposables" should be disposed
            var mybm = new Bitmap(366, 25);
            var g = Graphics.FromImage(mybm);
            var drawFont = new Font("Arial", 10);
            var drawBrush = new SolidBrush(Color.Black);
            g.DrawString("Calculating...", drawFont, drawBrush, 0, 0);
            BitmapImage bi;
            var ms = new MemoryStream();
#pragma warning restore S2930 // "IDisposables" should be disposed
            {
                mybm.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                bi = new BitmapImage();
                bi.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
            }
            bi.EndInit();
            TmpCalcImage = bi;
            TmpCalcImage.Freeze();
            BitmapAllPermittedTime = TmpCalcImage;
            BitmapSinglePermittedTime = TmpCalcImage;
        }

        private void RefreshAllPermissionLines()
        {
            int lineidx = 0;
            foreach (var boolEntry in ThisTimeLimit.TimeLimitEntries) {
                var found = false;
                foreach (var line in _dtv.PermissionLines) {
                    if (line.TimeLimitEntry == boolEntry) {
                        found = true;
                    }
                }
                if (!found) {
                    var permissionLine = new PermissionLine();
                    var plp = new PermissionLinePresenter(Sim, permissionLine, boolEntry,
                        TimeLimitEntry.GetLevel(boolEntry) * 10, true);
                    permissionLine.DataContext = plp;
                    permissionLine.AddClicked += _dtv.PermissionLineOnAddClicked;
                    permissionLine.RemoveClicked += _dtv.PermissionLineOnRemoveClicked;
                    permissionLine.ShowPreviewClicked += _dtv.PermissionLineOnShowPreviewClicked;
                    _dtv.PermissionLines.Add(permissionLine);
                    _dtv.BoolGrid.Children.Add(permissionLine);
                    Grid.SetRow(permissionLine,lineidx++);
                    _plps.Add(plp);
                    plp.SetAllOnProperty();
                }
            }
            var items2Delete = new List<PermissionLine>();
            foreach (var line in _dtv.PermissionLines) {
                var found = false;
                foreach (var boolEntry in ThisTimeLimit.TimeLimitEntries) {
                    if (line.TimeLimitEntry == boolEntry) {
                        found = true;
                    }
                }
                if (!found) {
                    items2Delete.Add(line);
                }
            }
            foreach (var permissionLine in items2Delete) {
                _dtv.PermissionLines.Remove(permissionLine);
                _dtv.BoolGrid.Children.Remove(permissionLine);
            }
        }

        public void RefreshUsedIn()
        {
            var usedIn = ThisTimeLimit.CalculateUsedIns(Sim);
            UsedIn.Clear();
            foreach (var p in usedIn) {
                UsedIn.Add(p);
            }
        }

        public void RemoveSubentry([NotNull] TimeLimitEntry timeLimitEntry)
        {
            ThisTimeLimit.DeleteTimeLimitEntryFromDB(timeLimitEntry);
            RefreshAllPermissionLines();
            _dtv.SetOneRow(ThisTimeLimit.RootEntry);
        }

        public void SetExpanderWidth(double actualWidth)
        {
            foreach (var plp in _plps) {
                plp.ExpanderSize = actualWidth;
            }
        }
    }
}