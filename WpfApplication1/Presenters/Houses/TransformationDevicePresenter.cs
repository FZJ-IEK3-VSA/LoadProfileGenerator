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
using Automation.ResultFiles;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

#endregion

namespace LoadProfileGenerator.Presenters.Houses {
    public class TransformationDevicePresenter : PresenterBaseDBBase<TransformationDeviceView> {
        [NotNull] private readonly Dictionary<TransformationConditionType, string> _conditionTypeToString =
            new Dictionary<TransformationConditionType, string>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<string> _factorTypes = new ObservableCollection<string>();

        [NotNull] private readonly Dictionary<string, TransformationConditionType> _stringToConditionType =
            new Dictionary<string, TransformationConditionType>();

        [NotNull] private readonly TransformationDevice _trafoDevice;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIn = new ObservableCollection<UsedIn>();
        private double _exampleConversionFactor1;

        [CanBeNull] private VLoadType _selectedOutputLoadtype;

        public TransformationDevicePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] TransformationDeviceView view,
            [NotNull] TransformationDevice trafoDevice) : base(view, "ThisTrafo.HeaderString", trafoDevice,
            applicationPresenter)
        {
            _trafoDevice = trafoDevice;

            foreach (TransformationConditionType type in Enum.GetValues(typeof(TransformationConditionType))) {
                switch (type) {
                    case TransformationConditionType.MinMaxValue:
                        _conditionTypeToString.Add(TransformationConditionType.MinMaxValue,
                            "Between a minimum and maximum value");
                        break;
                    case TransformationConditionType.StorageContent:
                        _conditionTypeToString.Add(TransformationConditionType.StorageContent,
                            "The storage content has to be between these values.");
                        break;
                    default:
                        throw new LPGException("Unkown Transformation Condition Type");
                }
            }
            foreach (var keyValuePair in _conditionTypeToString) {
                StringToConditionType.Add(keyValuePair.Value, keyValuePair.Key);
                ConditionTypes.Add(keyValuePair.Value);
            }
            SelectedConditionTypeStr = ConditionTypes[0];
            foreach (var condition in _trafoDevice.Conditions) {
                condition.SetNameDictionary(_conditionTypeToString);
            }

            _factorTypes.Add("Fixed");
            _factorTypes.Add("Interpolated");
            ExampleConversionFactor = 1;
            ConversionExampleQuantity = 1;
            ConversionExampleTimespan = new TimeSpan(0, 1, 0);
            RefreshConversionHelper();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public List<string> ConditionTypes { get; } = new List<string>();

        [NotNull]
        [UsedImplicitly]
        public Dictionary<TransformationConditionType, string> ConditionTypeToString => _conditionTypeToString;

        [CanBeNull]
        [UsedImplicitly]
        public string ConversionExample1 { get; private set; }

        [CanBeNull]
        [UsedImplicitly]
        public string ConversionExample2 { get; private set; }

        [CanBeNull]
        [UsedImplicitly]
        public string ConversionExample3 { get; private set; }

        [UsedImplicitly]
        public double ConversionExampleQuantity { get; set; }

        [UsedImplicitly]
        public TimeSpan ConversionExampleTimespan { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType ConversionOutLoadType { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyStorage> EnergyStorages
            => Sim.EnergyStorages.MyItems;

        [UsedImplicitly]
        public double ExampleConversionFactor {
            get => _exampleConversionFactor1;
            set {
                if (value.Equals(_exampleConversionFactor1)) {
                    return;
                }
                _exampleConversionFactor1 = value;
                OnPropertyChanged(nameof(ExampleConversionFactor));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> FactorTypes => _factorTypes;

        [UsedImplicitly]
        public TransformationConditionType SelectedConditionType { get; private set; }

        [NotNull]
        [UsedImplicitly]
        public string SelectedConditionTypeStr {
            get => _conditionTypeToString[SelectedConditionType];
            set {
                SelectedConditionType = _stringToConditionType[value];
                OnPropertyChanged(nameof(SelectedConditionType));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType SelectedOutputLoadtype {
            get => _selectedOutputLoadtype;
            set {
                _selectedOutputLoadtype = value;
                OnPropertyChanged(nameof(SelectedOutputLoadtype));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public Dictionary<string, TransformationConditionType> StringToConditionType => _stringToConditionType;

        [NotNull]
        [UsedImplicitly]
        public TransformationDevice ThisTrafo => _trafoDevice;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIn => _usedIn;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> VLoadTypes => Sim.LoadTypes.MyItems;

        public void AddOutputLoadType([NotNull] VLoadType selectedLoadType, double factor, TransformationFactorType factorType)
        {
            ThisTrafo.AddOutTransformationDeviceLoadType(selectedLoadType, factor, factorType);
        }

        public void Delete()
        {
            Sim.TransformationDevices.DeleteItem(_trafoDevice);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            var presenter = obj as TransformationDevicePresenter;
            return presenter?.ThisTrafo.Equals(_trafoDevice) == true;
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

        public void RefreshConversionHelper()
        {
            var srcLoadtype = ThisTrafo.LoadTypeIn;
            if (srcLoadtype == null) {
                return;
            }
            var dstLoadType = ConversionOutLoadType;
            if (dstLoadType == null) {
                return;
            }
            var v = ConversionExampleQuantity;
            var r = ConversionExampleQuantity * ExampleConversionFactor;
            ConversionExample1 = "Converting " + v + " " + srcLoadtype.UnitOfPower + " yields " + r + " " +
                                 dstLoadType.UnitOfPower;
            ConversionExample2 = "Converting " + v + " " + srcLoadtype.UnitOfPower + " for " +
                                 ConversionExampleTimespan +
                                 " yields " +
                                 r * dstLoadType.ConversionFaktorPowerToSum * ConversionExampleTimespan.TotalSeconds +
                                 " " +
                                 dstLoadType.UnitOfSum;
            ConversionExample3 = "Converting " + v + " " + srcLoadtype.UnitOfSum + " yields " +
                                 r * dstLoadType.ConversionFaktorPowerToSum / srcLoadtype.ConversionFaktorPowerToSum +
                                 " " +
                                 dstLoadType.UnitOfSum;
            OnPropertyChanged(nameof(ConversionExample1));
            OnPropertyChanged(nameof(ConversionExample2));
            OnPropertyChanged(nameof(ConversionExample3));
        }

        public void RefreshUsedIn()
        {
            var usedIn = _trafoDevice.CalculateUsedIns(Sim);
            _usedIn.Clear();
            foreach (var p in usedIn) {
                _usedIn.Add(p);
            }
        }

        public void RemoveOutputDeviceLoadType([NotNull] TransformationDeviceLoadType tdlt)
        {
            ThisTrafo.DeleteTransformationLoadtypeFromDB(tdlt);
        }
    }
}