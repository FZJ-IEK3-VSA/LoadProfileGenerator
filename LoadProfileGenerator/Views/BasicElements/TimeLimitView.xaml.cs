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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Automation.ResultFiles;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Controls.Usercontrols;
using LoadProfileGenerator.Presenters.BasicElements;
using Image = System.Windows.Controls.Image;

#endregion

namespace LoadProfileGenerator.Views.BasicElements {
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TimeLimitView {

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht")]
        public static Bitmap MakeBitmapFromBitArray([ItemNotNull] [JetBrains.Annotations.NotNull] BitArray br)
        {
            var rightnow = DateTime.Now;
            var c = new GregorianCalendar(GregorianCalendarTypes.USEnglish);
            var year = rightnow.Year;
            var daysinyear = c.GetDaysInYear(year);

            var mybm = new Bitmap(daysinyear, 25);
            {
                var g = Graphics.FromImage(mybm);

                using var b = new SolidBrush(Color.DarkBlue);
                g.FillRectangle(b, 0, 0, daysinyear, 24);
                var dts = new DateTime[daysinyear * 24];
                dts[0] = new DateTime(year, 1, 1);
                for (var i = 1; i < daysinyear * 24; i++)
                {
                    dts[i] = dts[i - 1] + new TimeSpan(1, 0, 0);
                }
                for (var day = 0; day < daysinyear; day++)
                {
                    for (var hour = 0; hour < 24; hour++)
                    {
                        var starthour = new DateTime(year, 1, 1, hour, 0, 0);
                        starthour = starthour.AddDays(day);
                        var endhour = starthour.AddHours(1);
                        var foundanyactive = false;
                        for (var i = 0; i < dts.Length; i++)
                        {
                            if (dts[i] >= starthour && dts[i] < endhour && br[i])
                            {
                                foundanyactive = true;
                                i = dts.Length;
                            }
                        }
                        if (foundanyactive)
                        {
                            var x = day;
                            var y = hour;
                            mybm.SetPixel(x, y, Color.White);
                        }
                    }
                }
                return mybm;
            }
        }

        [JetBrains.Annotations.NotNull] private readonly object _pictureLock = new object();

        [CanBeNull] private BitmapImage _calcImage;

        private int _concurrentlyRunningPreviewCount;

        [JetBrains.Annotations.NotNull] private GeographicLocation _geographicLocation;

        [CanBeNull] private ModularHousehold _household;

        [JetBrains.Annotations.NotNull] private TimeLimitEntry _rootEntry;

        private int _rowForPermissionLine;

        // these are for thread safety
        [JetBrains.Annotations.NotNull] private TemperatureProfile _selectedTemperatureProfile;

        // this is needed because presenter is in a different thread...
        [CanBeNull] private TimeLimitEntry _timeLimitEntry;

        public TimeLimitView()
        {
            InitializeComponent();
        }

        [CanBeNull]
        private TimeLimitPresenter LimitPresenter => DataContext as TimeLimitPresenter;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<PermissionLine> PermissionLines { get; } =
            new ObservableCollection<PermissionLine>();

        public void PermissionLineOnAddClicked([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            if (e == null) {
                return;
            }

            var aca = (AddClickedEventArgs) e;

            LimitPresenter.AddSubentry(aca.TimeLimitEntry);
        }

        public void PermissionLineOnRemoveClicked([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            if (e == null) {
                return;
            }

            var aca = (AddClickedEventArgs) e;
            if (aca.TimeLimitEntry == LimitPresenter.ThisTimeLimit.RootEntry) {
                return;
            }

            LimitPresenter.RemoveSubentry(aca.TimeLimitEntry);
            foreach (var line in PermissionLines) {
                line.Presenter.SetAllOnProperty();
            }
        }

        public void PermissionLineOnShowPreviewClicked([CanBeNull] object sender, [CanBeNull] EventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            if (e == null) {
                return;
            }

            var aca = (AddClickedEventArgs) e;
            _timeLimitEntry = aca.TimeLimitEntry;
            if (ImageShort.ActualHeight > LimitPresenter.ImageHeight &&
                Math.Abs(ImageShort.ActualHeight) > 0.00001) {
                LimitPresenter.ImageHeight = ImageShort.ActualHeight;
            }

            MakeNewImages();
        }

        public void SetOneRow([CanBeNull] TimeLimitEntry dtbe)
        {
            if (dtbe == null) {
                return;
            }

            if (LimitPresenter?.ThisTimeLimit.RootEntry == null) {
                return;
            }

            if (dtbe == LimitPresenter.ThisTimeLimit.RootEntry) {
                _rowForPermissionLine = 0;
            }

            var foundEntry = false;
            foreach (var permissionLine in PermissionLines) {
                if (permissionLine.TimeLimitEntry == dtbe) {
                    Grid.SetRow(permissionLine, _rowForPermissionLine);
                    _rowForPermissionLine++;
                    permissionLine.Presenter.LeftMargin = TimeLimitEntry.GetLevel(dtbe) * 10;
                    foundEntry = true;
                }
            }

            if (!foundEntry) {
                throw new LPGException("there is a row missing in the display");
            }

            foreach (var timeLimitEntry in dtbe.Subentries) {
                SetOneRow(timeLimitEntry);
            }

            if (BoolGrid.RowDefinitions.Count < _rowForPermissionLine) {
                BoolGrid.RowDefinitions.Add(new RowDefinition());
            }
        }

        private void BtnCreateCopy([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            LimitPresenter.CreateCopy();
        }

        private void BtnImportTimeLimitClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            LimitPresenter.ImportTimeLimit();
            MessageWindowHandler.Mw.ShowInfoMessage("Import finished.", "Success");
            LimitPresenter.Close(true);
            LimitPresenter.ApplicationPresenter.OpenItem(LimitPresenter.ThisTimeLimit);
        }

        private void BtnRefreshPersons_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            LimitPresenter.RefreshUsedIn();
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            LimitPresenter.Close(true);
        }

        private void CmbGeographicLocationSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbPreviewGeographicLocation.SelectedItem == null) {
                return;
            }

            _geographicLocation = (GeographicLocation) CmbPreviewGeographicLocation.SelectedItem;
            MakeNewImages();
        }

        private void CmbPreviewHouseholdSelectionChanged([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbPreviewHousehold.SelectedItem == null) {
                return;
            }

            _household = (ModularHousehold) CmbPreviewHousehold.SelectedItem;
            MakeNewImages();
        }

        private void CmbTemperatureProfilesSelectionChanged1([CanBeNull] object sender,
            [CanBeNull] SelectionChangedEventArgs e)
        {
            if (CmbPreviewTemperatureProfiles.SelectedItem == null) {
                return;
            }

            _selectedTemperatureProfile = (TemperatureProfile) CmbPreviewTemperatureProfiles.SelectedItem;
            MakeNewImages();
        }

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LimitPresenter != null) {
                LimitPresenter.AskDeleteQuestion(
                    LimitPresenter.ThisTimeLimit.HeaderString, LimitPresenter.Delete);
            }
        }

        private void Expander_SizeChanged([CanBeNull] object sender, [CanBeNull] SizeChangedEventArgs e)
        {
            if (LimitPresenter != null) {
                LimitPresenter.SetExpanderWidth(
                    NewPermissionExpander.ActualWidth);
            }
        }

        private void LstUsedIn_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LimitPresenter == null) {
                return;
            }

            var u = (UsedIn) LstUsedIn.SelectedItem;
            if (u?.Item != null) {
                LimitPresenter.ApplicationPresenter.OpenItem(u.Item);
            }
        }

        private void MakeNewImages()
        {
            if (LimitPresenter == null) {
                return;
            }

            _calcImage = LimitPresenter.TmpCalcImage;
            _selectedTemperatureProfile = LimitPresenter.PreviewTemperatureProfile;
            _geographicLocation = LimitPresenter.PreviewGeographicLocation;
            if (LimitPresenter.ThisTimeLimit.RootEntry == null)
            {
                throw new LPGException("root entry was null");
            }
            _rootEntry = LimitPresenter.ThisTimeLimit.RootEntry;
            _household = LimitPresenter.PreviewHousehold;
            TimeLimitPresenter p = null;
            if (Dispatcher == null || Thread.CurrentThread == Dispatcher.Thread) {
                p = LimitPresenter;
            }

            var t = new Thread(() => UpdatePictures(p));
            t.Start();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void UpdatePictures([CanBeNull] TimeLimitPresenter p)
        {
            if (p == null) {
                return;
            }

            if (_household?.Vacation == null) {
                return;
            }

            if (_timeLimitEntry == null) {
                return;
            }

            lock (_pictureLock) {
                _concurrentlyRunningPreviewCount++;
                if (_concurrentlyRunningPreviewCount > 1) {
                    throw new LPGException("this should never happen!");
                }

                Action<Image, BitmapImage> setsingleimage = (_, bitmapImage) => {
                    p.BitmapSinglePermittedTime = bitmapImage;
                    Logger.Debug("Setting the new image with " + bitmapImage.Width + " pixels width.");
                };
                Action<Image, BitmapImage> setMergedImage = (_, bitmapImage) => {
                    p.BitmapAllPermittedTime = bitmapImage;
                    Logger.Debug("Setting the new image with " + bitmapImage.Width + " pixels width.");
                };
                Dispatcher?.Invoke(DispatcherPriority.Normal, setsingleimage, ImageShort, _calcImage);
                Dispatcher?.Invoke(DispatcherPriority.Normal, setMergedImage, ImageShort, _calcImage);
                var r = new Random();
                List<VacationTimeframe> vacationTimeframes = _household.Vacation.VacationTimeframes();
                var previewKey = "TimeLimitView" + DateTime.Now.ToLongTimeString();
                var br = _timeLimitEntry.GetOneYearHourArray(_selectedTemperatureProfile, _geographicLocation, r,
                    vacationTimeframes, previewKey, out _);
                var bmp = MakeBitmapFromBitArray(br);
                BitmapImage totalImage;
#pragma warning disable S2930 // "IDisposables" should be disposed
                var ms = new MemoryStream();
#pragma warning restore S2930 // "IDisposables" should be disposed
                {
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    var singleImage = new BitmapImage();
                    singleImage.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    singleImage.StreamSource = ms;
                    singleImage.EndInit();
                    singleImage.Freeze();
                    if (singleImage.IsDownloading) {
                        Logger.Info("image is downloading");
                    }

                    ImageShort.Dispatcher?.Invoke(DispatcherPriority.Normal, setsingleimage, ImageShort, singleImage);
                }
                ms = new MemoryStream();
                {
                    var br3 = _rootEntry.GetOneYearHourArray(_selectedTemperatureProfile, _geographicLocation, r,
                        vacationTimeframes, previewKey, out _);
                    var bmp2 = MakeBitmapFromBitArray(br3);
                    bmp2.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    totalImage = new BitmapImage();
                    totalImage.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    totalImage.StreamSource = ms;
                }
                totalImage.EndInit();
                totalImage.Freeze();
                ImageLong.Dispatcher?.Invoke(DispatcherPriority.Normal, setMergedImage, ImageLong, totalImage);

                _concurrentlyRunningPreviewCount--;
            }
        }
    }
}