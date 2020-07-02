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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Common;
using Database.Tables;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
#pragma warning disable S3897 // Classes that provide "Equals(<T>)" or override "Equals(Object)" should implement "IEquatable<T>"
    public class AffordanceVariablePresenter : PresenterBaseWithAppPresenter<AffordanceVariableView> {
        [NotNull] private readonly ApplicationPresenter _applicationPresenter;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<VariableEntry> _variableEntries =
            new ObservableCollection<VariableEntry>();

        [CanBeNull] private string _selectedFilterType;

        public AffordanceVariablePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] AffordanceVariableView view) :
            base(view, "HeaderString", applicationPresenter)
        {
            _applicationPresenter = applicationPresenter;
            SortBy = new ObservableCollection<string> {"Affordance Name", "Variable Name", "Type"};
            SelectedFilterType = SortBy[0];
            Refresh();
        }

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Variable Overview";

        [CanBeNull]
        [UsedImplicitly]
        public string SelectedFilterType {
            get => _selectedFilterType;
            set {
                if (value == _selectedFilterType) {
                    return;
                }

                _selectedFilterType = value;
                OnPropertyChanged(nameof(SelectedFilterType));
                Refresh();
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> SortBy { get; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VariableEntry> VariableEntries => _variableEntries;

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            _applicationPresenter.CloseTab(this,
                removeLast);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as AffordanceVariablePresenter;
            return presenter?.HeaderString.Equals(HeaderString) == true;
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

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Refresh()
        {
            var entries = new List<VariableEntry>();
            var affordances = Sim.Affordances.Items;
            foreach (var affordance in affordances) {
                foreach (var operation in affordance.ExecutedVariables) {
                    var locationName = operation.Location?.Name;
                    if (locationName == null) {
                        locationName = "no name";
                    }

                    var te = new VariableEntry(affordance, "Aff. Operation", operation.Action.ToString(),
                        operation.Value.ToString(CultureInfo.CurrentCulture), operation.Variable, locationName,
                        operation.LocationMode.ToString(), operation.ExecutionTimeStr);
                    entries.Add(te);
                }

                foreach (var req in affordance.RequiredVariables) {
                    var locationName = req.Location?.Name;
                    if (locationName == null) {
                        locationName = "no name";
                    }

                    var te = new VariableEntry(affordance, "Aff. Requirement", req.ConditionStr,
                        req.Value.ToString(CultureInfo.CurrentCulture), req.Variable, locationName,
                        req.LocationMode.ToString(), string.Empty);
                    entries.Add(te);
                }
            }

            foreach (var subAffordance in Sim.SubAffordances.Items) {
                foreach (var variableOp in subAffordance.SubAffordanceVariableOps) {
                    var locationName = variableOp.Location?.Name;
                    if (locationName == null) {
                        locationName = "no name";
                    }

                    string varname = variableOp.Variable?.Name;
                    if (varname == null) {
                        varname = "no name";
                    }

                    if (variableOp.Variable != null) {
                        var ve = new VariableEntry(subAffordance, "SubAff. Operation", varname,
                            variableOp.Value.ToString(CultureInfo.CurrentCulture), variableOp.Variable, locationName,
                            variableOp.LocationModeStr, variableOp.ExecutionTimeStr);
                        entries.Add(ve);
                    }
                }
            }

            foreach (var houseType in Sim.HouseTypes.Items) {
                foreach (var dev in houseType.HouseDevices) {
                    if (dev.Variable != null) {
                        string desc = dev.Device?.Name;
                        if (desc == null) {
                            desc = string.Empty;
                        }

                        string locName = dev.Location?.PrettyName;
                        if (locName == null) {
                            locName = string.Empty;
                        }

                        var ve = new VariableEntry(houseType, "House Type Device", desc,
                            dev.VariableValue.ToString(CultureInfo.CurrentCulture), dev.Variable, locName,
                            "Current Location", string.Empty);
                        entries.Add(ve);
                    }
                }
            }

            foreach (var hh in Sim.HouseholdTraits.Items) {
                foreach (var dev in hh.Autodevs) {
                    if (dev.Variable != null && dev.Location != null && dev.Device != null) {
                        var ve = new VariableEntry(hh, "Household Trait Device", dev.Device.Name,
                            dev.VariableValue.ToString(CultureInfo.CurrentCulture), dev.Variable,
                            dev.Location.PrettyName,
                            "Current Location", string.Empty);
                        entries.Add(ve);
                    }
                }
            }

            _variableEntries.SynchronizeWithList(entries);
            _variableEntries.Sort(Comparer);
        }

        private int Comparer([NotNull] VariableEntry x, [NotNull] VariableEntry y)
        {
            switch (SelectedFilterType) {
                case "Affordance Name":
                    if (x.Name != y.Name) {
                        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    }

                    if (x.Variable.Name != y.Variable.Name) {
                        return string.Compare(x.Variable.Name, y.Variable.Name, StringComparison.Ordinal);
                    }

                    return string.CompareOrdinal(x.VariableType, y.VariableType);
                case "Variable Name":
                    if (x.Variable.Name != y.Variable.Name) {
                        return string.Compare(x.Variable.Name, y.Variable.Name, StringComparison.Ordinal);
                    }

                    if (x.Name != y.Name) {
                        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    }

                    return string.CompareOrdinal(x.VariableType, y.VariableType);
                case "Type":
                    if (x.VariableType != y.VariableType) {
                        return string.Compare(x.VariableType, y.VariableType, StringComparison.Ordinal);
                    }

                    if (x.Name != y.Name) {
                        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    }

                    return string.Compare(x.Variable.Name, y.Variable.Name, StringComparison.Ordinal);
                default: return 0;
            }
        }

#pragma warning disable S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
        public class VariableEntry : IComparable {
            public VariableEntry([NotNull] DBBase affordance, [NotNull] string variableType, [NotNull] string desc, [NotNull] string value, [NotNull] Variable variable,
                [NotNull] string locationName, [NotNull] string locationMode, [NotNull] string executionTime)
            {
                Affordance = affordance;
                VariableType = variableType;
                Desc = desc;
                Value = value;
                Variable = variable;
                LocationName = locationName;
                LocationMode = locationMode;
                ExecutionTime = executionTime;
            }

            [NotNull]
            [UsedImplicitly]
            public DBBase Affordance { get; }

            [NotNull]
            [UsedImplicitly]
            public string Desc { get; }

            [NotNull]
            [UsedImplicitly]
            public string ExecutionTime { get; }

            [NotNull]
            [UsedImplicitly]
            public string LocationMode { get; }

            [NotNull]
            [UsedImplicitly]
            public string LocationName { get; }

            [NotNull]
            public string Name => Affordance.Name;

            [NotNull]
            [UsedImplicitly]
            public string Value { get; }

            [NotNull]
            public Variable Variable { get; }

            [NotNull]
            public string VariableType { get; }

            public int CompareTo([CanBeNull] object obj)
            {
                if (!(obj is VariableEntry other))
                {
                    return 0;
                }

                return string.Compare(Variable.Name, other.Variable.Name, StringComparison.Ordinal);
            }
#pragma warning restore S1210 // "Equals" and the comparison operators should be overridden when implementing "IComparable"
        }
#pragma warning restore S3897 // Classes that provide "Equals(<T>)" or override "Equals(Object)" should implement "IEquatable<T>"
    }
}