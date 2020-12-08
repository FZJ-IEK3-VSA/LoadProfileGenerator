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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Common;
using Common.Enums;
using Common.JSON;
using Database.Tables;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    public class SettingPresenter : PresenterBaseWithAppPresenter<SettingsView> {
        private const string Allfiles = "Create all Files";
        private const string NoFiles = "Create no files";
        private const string Onlysum = "Create no files except the sum";
        private const string OnlySumAndDevice = "Create no files except the sum and device profiles";
        [JetBrains.Annotations.NotNull] private readonly GeneralConfig _config;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<string> _defaultOptions = new ObservableCollection<string>();

        public SettingPresenter([JetBrains.Annotations.NotNull] ApplicationPresenter applicationPresenter, [JetBrains.Annotations.NotNull] SettingsView view)
            : base(view, "HeaderString", applicationPresenter)
        {
            BoolValues.Add("True");
            BoolValues.Add("False");
            _config = Sim.MyGeneralConfig;
            _defaultOptions.Add(Allfiles);
            _defaultOptions.Add(NoFiles);
            _defaultOptions.Add(Onlysum);
            _defaultOptions.Add(OnlySumAndDevice);
            Refresh();
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> BoolValues { get; } = new ObservableCollection<string>();

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string DatabaseString => _config.ConnectionString;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> DefaultOptions => _defaultOptions;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string HeaderString => "Settings";


        [JetBrains.Annotations.NotNull]
        public Dictionary<DeviceProfileHeaderMode, string> DeviceProfileHeaderStyles => DeviceProfileHeaderModeHelper.DeviceProfileHeaderModeDict;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> NotSelectedOptions { get; } = new ObservableCollection<string>();

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public Dictionary<OutputFileDefault, string> OutputFileDefaults
            => OutputFileDefaultHelper.OutputFileDefaultDictionary;

        [UsedImplicitly]
        public OutputFileDefault SelectedOptionDefault { get; set; }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> SelectedOptions { get; } = new ObservableCollection<string>();

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public GeneralConfig ThisConfig => _config;

        public void AddOption(CalcOption calcOption)
        {
            _config.Enable(calcOption);
            Refresh();
        }

        public void ApplyOptionDefault()
        {
            _config.ApplyOptionDefault(SelectedOptionDefault);
            Refresh();
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _config.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public override bool Equals(object obj)
        {
            return obj is SettingPresenter presenter && presenter.ThisConfig.Equals(_config);
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

        private void Refresh()
        {
            {
                var selected = _config.Options.Where(x => x.Value.SettingValue).Select(x => x.Key);
                var strings = new List<string>();
                foreach (var calcOption in selected) {
                    if (CalcOptionHelper.CalcOptionDictionary.ContainsKey(calcOption)) {
                        strings.Add(CalcOptionHelper.CalcOptionDictionary[calcOption]);
                    }
                    else
                    {
                        Logger.Warning("Missing Calc Option in Dictionary: " + calcOption);
                    }
                }
                SelectedOptions.SynchronizeWithList(strings);
            }
            var notselected = _config.Options.Where(x => !x.Value.SettingValue).Select(x => x.Key);
            var stringsNot = new List<string>();
            foreach (var calcOption in notselected) {
                if (CalcOptionHelper.CalcOptionDictionary.ContainsKey(calcOption)) {
                    stringsNot.Add(CalcOptionHelper.CalcOptionDictionary[calcOption]);
                }
                else
                {
                    Logger.Warning("Missing Calc Option in Dictionary: " + calcOption);
                }
            }
            NotSelectedOptions.SynchronizeWithList(stringsNot);
        }

        public void RemoveOption(CalcOption calcOption)
        {
            _config.Disable(calcOption);
            Refresh();
        }
    }
}