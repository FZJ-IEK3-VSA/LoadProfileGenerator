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
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Controls.Usercontrols {
    public class PermissionLinePresenter : Notifier {
        private readonly bool _isMinusVisible;

        [NotNull]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
#pragma warning disable IDE0052 // Remove unread private members
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private readonly PermissionLine _permissionLine;
#pragma warning restore IDE0052 // Remove unread private members
        // needed so that the reference is kept somewhere

        [NotNull] private readonly Simulator _sim;
        private double _expanderSize;
        private double _leftMargin;

        [CanBeNull] private PermissionTuple _selectedPermissionTuple;

        public PermissionLinePresenter([NotNull] Simulator sim, [NotNull] PermissionLine permissionLine,
                                       [NotNull] TimeLimitEntry timeEntry,
                                       double leftMargin, bool isMinusVisible)
        {
            _sim = sim;
            _permissionLine = permissionLine;
            Entry = timeEntry;
            _leftMargin = leftMargin;
            Logger.Info("Margin: " + _leftMargin);
            _isMinusVisible = isMinusVisible;
            PermissionModes = new ObservableCollection<PermissionTuple> {
                new PermissionTuple(PermissionMode.EveryXDay, "Every x days"),
                new PermissionTuple(PermissionMode.EveryWorkday, "Every work day"),
                new PermissionTuple(PermissionMode.EveryXWeeks, "Every x weeks"),
                new PermissionTuple(PermissionMode.EveryXMonths, "Every x months"),
                new PermissionTuple(PermissionMode.Yearly, "Yearly"),
                new PermissionTuple(PermissionMode.EveryDateRange, "In a certain date range"),
                new PermissionTuple(PermissionMode.Temperature, "at certain temperatures"),
                new PermissionTuple(PermissionMode.LightControlled, "At certain light conditions"),
                new PermissionTuple(PermissionMode.ControlledByDateProfile,
                    "At values from a date based profile"),
                new PermissionTuple(PermissionMode.VacationControlled, "vacation controlled"),
                new PermissionTuple(PermissionMode.HolidayControlled, "holiday controlled")
            };
            AndOrList = new ObservableCollection<AnyAllTimeLimitCondition>();
            foreach (var value in Enum.GetValues(typeof(AnyAllTimeLimitCondition))) {
                AndOrList.Add((AnyAllTimeLimitCondition)value);
            }

            SetSelectedPermissionModel(Entry);
        }

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AnyAllTimeLimitCondition> AndOrList { get; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DateBasedProfile> DateBasedProfiles => _sim.DateBasedProfiles.Items;

        [NotNull]
        public TimeLimitEntry Entry { get; }

        [UsedImplicitly]
        public double ExpanderSize {
            get => _expanderSize;
            set {
                if (value > 25) {
                    _expanderSize = value - 25;
                }

                OnPropertyChanged(nameof(ExpanderSize));
                Logger.Debug("ExpanderSize set:" + _expanderSize);
            }
        }

        [UsedImplicitly]
        public Visibility IsMinusVisible {
            get {
                if (_isMinusVisible) {
                    return Visibility.Visible;
                }

                return Visibility.Hidden;
            }
        }

        [UsedImplicitly]
        public double LeftMargin {
            get => _leftMargin;
            set {
                _leftMargin = value;
                Margin = new Thickness(_leftMargin, 0, 0, 0);
                Logger.Info("Margin: " + _leftMargin);
                OnPropertyChanged(nameof(LeftMargin));
                OnPropertyChanged(nameof(Margin));
            }
        }

        [UsedImplicitly]
        public Thickness Margin { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<PermissionTuple> PermissionModes { get; }

        [CanBeNull]
        [UsedImplicitly]
        public PermissionTuple SelectedPermissionTuple {
            get => _selectedPermissionTuple;
            set {
                _selectedPermissionTuple = value;
                if (_selectedPermissionTuple == null) {
                    throw new LPGException("_selectedPermissionTuple is null");
                }

                Entry.RepeaterType = _selectedPermissionTuple.PermissionMode;
                SetAllOnProperty();
            }
        }

        [UsedImplicitly]
        public Visibility ShowAndOrCombo {
            get {
                if (Entry.Subentries.Count > 0) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowConditionCombo {
            get {
                if (Entry.Subentries.Count == 0) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDateProfile => GetVisibilityForPermissionMode(PermissionMode.ControlledByDateProfile);

        [UsedImplicitly]
        public Visibility ShowDateRange => GetVisibilityForPermissionMode(PermissionMode.EveryDateRange);

        [UsedImplicitly]
        public Visibility ShowEveryWorkday => GetVisibilityForPermissionMode(PermissionMode.EveryWorkday);

        [UsedImplicitly]
        public Visibility ShowEveryXDays => GetVisibilityForPermissionMode(PermissionMode.EveryXDay);

        [UsedImplicitly]
        public Visibility ShowEveryXMonths => GetVisibilityForPermissionMode(PermissionMode.EveryXMonths);

        [UsedImplicitly]
        public Visibility ShowEveryXWeeks => GetVisibilityForPermissionMode(PermissionMode.EveryXWeeks);

        [UsedImplicitly]
        public Visibility ShowHolidayControlled => GetVisibilityForPermissionMode(PermissionMode.HolidayControlled);

        [UsedImplicitly]
        public Visibility ShowLightcontrolled => GetVisibilityForPermissionMode(PermissionMode.LightControlled);

        [UsedImplicitly]
        public Visibility ShowTemperature => GetVisibilityForPermissionMode(PermissionMode.Temperature);

        [UsedImplicitly]
        public Visibility ShowVacationControlled => GetVisibilityForPermissionMode(PermissionMode.VacationControlled);

        [UsedImplicitly]
        public Visibility ShowYearly => GetVisibilityForPermissionMode(PermissionMode.Yearly);

        public void SetAllOnProperty()
        {
            OnPropertyChanged(nameof(ShowAndOrCombo));
            OnPropertyChanged(nameof(ShowConditionCombo));
            OnPropertyChanged(nameof(SelectedPermissionTuple));
            OnPropertyChanged(nameof(ShowEveryXDays));
            OnPropertyChanged(nameof(ShowEveryWorkday));
            OnPropertyChanged(nameof(ShowEveryXWeeks));
            OnPropertyChanged(nameof(ShowEveryXMonths));
            OnPropertyChanged(nameof(ShowDateRange));
            OnPropertyChanged(nameof(ShowYearly));
            OnPropertyChanged(nameof(ShowTemperature));
            OnPropertyChanged(nameof(ShowLightcontrolled));
            OnPropertyChanged(nameof(ShowDateProfile));
            OnPropertyChanged(nameof(ShowVacationControlled));
            OnPropertyChanged(nameof(ShowHolidayControlled));
        }

        private Visibility GetVisibilityForPermissionMode(PermissionMode pm)
        {
            if (Entry.Subentries.Count == 0) {
                if (SelectedPermissionTuple?.PermissionMode == pm) {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        private void SetSelectedPermissionModel([NotNull] TimeLimitEntry de)
        {
            foreach (var pm in PermissionModes) {
                if (pm.PermissionMode == de.RepeaterType) {
                    SelectedPermissionTuple = pm;
                    SetAllOnProperty();
                    return;
                }
            }

            throw new LPGException("Unknown Permission mode");
        }
    }
}