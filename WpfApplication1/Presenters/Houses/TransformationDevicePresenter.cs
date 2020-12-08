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
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

#endregion

namespace LoadProfileGenerator.Presenters.Houses {
    public class TransformationDevicePresenter : PresenterBaseDBBase<TransformationDeviceView> {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<string> _factorTypes = new ObservableCollection<string>();

        [JetBrains.Annotations.NotNull] private readonly TransformationDevice _trafoDevice;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<UsedIn> _usedIn = new ObservableCollection<UsedIn>();
        private double _exampleConversionFactor1;

        [CanBeNull] private VLoadType _selectedOutputLoadtype;

        public TransformationDevicePresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] TransformationDeviceView view,
            [JetBrains.Annotations.NotNull] TransformationDevice trafoDevice) : base(view, "ThisTrafo.HeaderString", trafoDevice,
            applicationPresenter)
        {
            _trafoDevice = trafoDevice;

            _factorTypes.Add("FixedFactor");
            _factorTypes.Add("Interpolated");
            _factorTypes.Add("FixedValue");
            ExampleConversionFactor = 1;
            ConversionExampleQuantity = 1;
            ConversionExampleTimespan = new TimeSpan(0, 1, 0);
            RefreshConversionHelper();
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public List<string> ConditionTypes { get; } = new List<string>();

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
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyStorage> EnergyStorages
            => Sim.EnergyStorages.Items;

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
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> FactorTypes => _factorTypes;

        [CanBeNull]
        [UsedImplicitly]
        public VLoadType SelectedOutputLoadtype {
            get => _selectedOutputLoadtype;
            set {
                _selectedOutputLoadtype = value;
                OnPropertyChanged(nameof(SelectedOutputLoadtype));
            }
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public TransformationDevice ThisTrafo => _trafoDevice;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIn => _usedIn;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> VLoadTypes => Sim.LoadTypes.Items;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<Variable> Variables => Sim.Variables.Items;


        public void AddOutputLoadType([JetBrains.Annotations.NotNull] VLoadType selectedLoadType, double factor, TransformationFactorType factorType)
        {
            ThisTrafo.AddOutTransformationDeviceLoadType(selectedLoadType, factor, factorType);
        }

        public void Delete()
        {
            Sim.TransformationDevices.DeleteItem(_trafoDevice);
            Close(false);
        }

        public override bool Equals(object obj)
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

        public void RemoveOutputDeviceLoadType([JetBrains.Annotations.NotNull] TransformationDeviceLoadType tdlt)
        {
            ThisTrafo.DeleteTransformationLoadtypeFromDB(tdlt);
        }

        public void MakeACopy()
        {
            var newht = TransformationDevice.ImportFromItem(ThisTrafo, Sim);
            newht.Name = newht.Name + " (Copy)";
            newht.SaveToDB();
            Sim.TransformationDevices.Items.Add(newht);
            ApplicationPresenter.OpenItem(newht);
        }
    }
}