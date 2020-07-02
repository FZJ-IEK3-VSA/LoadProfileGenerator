//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c)  Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by Noah Pflugradt.”
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public enum SelectionSource {
        All,
        ModularHousehold
    }

    public class DeviceSelectionPresenter : PresenterBaseDBBase<DeviceSelectionView> {
        [NotNull] private readonly DeviceSelection _deviceSelection;

        [CanBeNull] private RealDevice _selectedDevice;

        [CanBeNull] private DeviceAction _selectedDeviceAction;

        [CanBeNull] private DeviceActionGroup _selectedDeviceActionGroup;

        [CanBeNull] private DeviceCategory _selectedDeviceCategory;

        [CanBeNull] private ModularHousehold _selectedModularHousehold;

        private SelectionSource _selectionSource;

        public DeviceSelectionPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] DeviceSelectionView view,
            [NotNull] DeviceSelection deviceSelection) : base(view, "ThisDeviceSelection.HeaderString", deviceSelection,
            applicationPresenter)
        {
            _deviceSelection = deviceSelection;
            RefreshDeviceCategoriesAndGroups();
        }

        [NotNull]
        [UsedImplicitly]
        public string DeviceActionEnergyEnergy {
            get {
                if (SelectedDeviceAction == null) {
                    return string.Empty;
                }
                var s = "Weighted Device Energy Use: " + SelectedDeviceAction.CalculateWeightedEnergyUse();
                var lts =
                    SelectedDeviceAction.CalculateAverageEnergyUse(null, Sim.DeviceActions.Items, null, 1, 1);
                foreach (var lt in lts) {
                    s += "; " + lt.Item1.Name + ": " + lt.Item2.ToString("N2", CultureInfo.CurrentCulture) + " " +
                         lt.Item1.UnitOfSum;
                }
                return s;
            }
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceActionGroup> DeviceActionGroups { get; private set; }

        [ItemCanBeNull]
        [UsedImplicitly]
        [CanBeNull]
        public ObservableCollection<DeviceAction> DeviceActionsInGroup {
            get {
                if (_selectedDeviceActionGroup == null) {
                    return null;
                }
                return new ObservableCollection<DeviceAction>(
                    _selectedDeviceActionGroup.GetDeviceActions(Sim.DeviceActions.Items));
            }
        }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> DeviceCategories { get; private set; }

        [UsedImplicitly]
        [CanBeNull]
        [ItemNotNull]
        public ObservableCollection<RealDevice> DevicesInCategory => _selectedDeviceCategory?.SubDevices;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> ModularHouseholds => Sim.ModularHouseholds.Items;

        [CanBeNull]
        [UsedImplicitly]
        public RealDevice SelectedDevice {
            get => _selectedDevice;
            set {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceAction SelectedDeviceAction {
            get => _selectedDeviceAction;
            set {
                _selectedDeviceAction = value;
                OnPropertyChanged(nameof(SelectedDeviceAction));
                OnPropertyChanged(nameof(DeviceActionEnergyEnergy));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceActionGroup SelectedDeviceActionGroup {
            get => _selectedDeviceActionGroup;
            set {
                _selectedDeviceActionGroup = value;
                OnPropertyChanged(nameof(SelectedDeviceActionGroup));
                OnPropertyChanged(nameof(DeviceActionsInGroup));
                if (value == null) {
                    return;
                }
                if (_selectedDeviceActionGroup == null) {
                    throw new LPGException("_selected device action group was null");
                }

                var actions =
                    _selectedDeviceActionGroup.GetDeviceActions(Sim.DeviceActions.Items);
                if (actions.Count > 0) {
                    SelectedDeviceAction = actions[0];
                }
                else {
                    SelectedDeviceAction = null;
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public DeviceCategory SelectedDeviceCategory {
            get => _selectedDeviceCategory;
            set {
                _selectedDeviceCategory = value;
                OnPropertyChanged(nameof(SelectedDeviceCategory));
                OnPropertyChanged(nameof(DevicesInCategory));
                if (_selectedDeviceCategory != null && _selectedDeviceCategory.SubDevices.Count > 0) {
                    SelectedDevice = _selectedDeviceCategory.SubDevices[0];
                }
                else {
                    SelectedDevice = null;
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public ModularHousehold SelectedModularHousehold {
            get => _selectedModularHousehold;
            set {
                _selectedModularHousehold = value;
                OnPropertyChanged(nameof(SelectedModularHousehold));
                RefreshDeviceCategoriesAndGroups();
            }
        }

        [UsedImplicitly]
        public SelectionSource SelectionSource {
            get => _selectionSource;
            set {
                _selectionSource = value;
                OnPropertyChanged(nameof(SelectionSource));
                RefreshDeviceCategoriesAndGroups();
            }
        }

        [NotNull]
        public DeviceSelection ThisDeviceSelection => _deviceSelection;

        public void AddAction([NotNull] DeviceActionGroup dag, [NotNull] DeviceAction da)
        {
            ThisDeviceSelection.AddAction(dag, da);
            RefreshDeviceCategoriesAndGroups();
        }

        public void AddItem([NotNull] DeviceCategory dc, [NotNull] RealDevice rd)
        {
            ThisDeviceSelection.AddItem(dc, rd);
            RefreshDeviceCategoriesAndGroups();
        }

        public void Delete()
        {
            Sim.DeviceSelections.DeleteItem(ThisDeviceSelection);
            Close(false);
        }

        public void DeleteAction([NotNull] DeviceSelectionDeviceAction dsi)
        {
            _deviceSelection.DeleteActionFromDB(dsi);
        }

        public void DeleteItem([NotNull] DeviceSelectionItem item)
        {
            _deviceSelection.DeleteItemFromDB(item);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as DeviceSelectionPresenter;
            return presenter?.ThisDeviceSelection.Equals(_deviceSelection) == true;
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

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        private void RefreshDeviceCategoriesAndGroups()
        {
            if (_selectedModularHousehold == null && _selectionSource == SelectionSource.ModularHousehold) {
                return;
            }
            switch (_selectionSource) {
                case SelectionSource.ModularHousehold: {
                    var mydcs = new ObservableCollection<DeviceCategory>();
                    List<DeviceCategory> deviceCategories;
                    switch (SelectionSource) {
                        case SelectionSource.ModularHousehold:
                            if (_selectedModularHousehold == null) {
                                throw new LPGException("_selectedModularHousehold != null");
                            }
                            deviceCategories = _selectedModularHousehold.CollectDeviceCategories();
                            break;
                        default: throw new LPGException("Unknown Selection Source. This is a bug. Please report!");
                    }
                    foreach (var category in deviceCategories) {
                        var dsi =
                            ThisDeviceSelection.Items.FirstOrDefault(item => item.DeviceCategory == category);
                        if (dsi == null ) {
                            mydcs.Add(category);
                        }
                    }
                    mydcs.Sort();
                    DeviceCategories = mydcs;

                    var myGroups = new ObservableCollection<DeviceActionGroup>();
                    List<DeviceActionGroup> deviceActionGroups;
                    switch (SelectionSource) {
                        case SelectionSource.ModularHousehold:
                            deviceActionGroups = _selectedModularHousehold.CollectDeviceActionGroups();
                            break;
                        default: throw new LPGException("Unknown Selection Source. This is a bug. Please report!");
                    }
                    foreach (var group in deviceActionGroups) {
                        var dsi =
                            ThisDeviceSelection.Actions.FirstOrDefault(item => item.DeviceActionGroup == group);
                        if (dsi == null) {
                            myGroups.Add(group);
                        }
                    }
                    myGroups.Sort();
                    DeviceActionGroups = myGroups;
                }
                    break;
                case SelectionSource.All:
                    DeviceCategories = Sim.DeviceCategories.Items;
                    DeviceActionGroups = Sim.DeviceActionGroups.Items;
                    break;
                default: throw new LPGException("Forgotten Selection Source. Please report.");
            }
            OnPropertyChanged(nameof(DeviceCategories));
            OnPropertyChanged(nameof(DeviceActionGroups));
            if (DeviceCategories?.Count > 0) {
                SelectedDeviceCategory = DeviceCategories[0];
            }
            if (DeviceActionGroups?.Count > 0) {
                SelectedDeviceActionGroup = DeviceActionGroups[0];
            }
        }
    }
}