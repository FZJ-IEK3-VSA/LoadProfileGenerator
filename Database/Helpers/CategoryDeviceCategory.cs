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
using Common;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

#endregion

namespace Database.Helpers {
    public class CategoryDeviceCategory : CategoryDBBase<DeviceCategory> {
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<DeviceCategory> _deviceCategoriesRoot =
            new ObservableCollection<DeviceCategory>();

        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public CategoryDeviceCategory() : base("Device categories")
        {
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> DeviceCategoriesRoot {
            get {
                RefreshRootCategories();
                return _deviceCategoriesRoot;
            }
        }

        [JetBrains.Annotations.NotNull]
        public DeviceCategory DeviceCategoryNone { get; set; }

        [UsedImplicitly]
        //dynamically called
        public new void CheckForDuplicateNames(bool savetoDB) {
            base.CheckForDuplicateNames(savetoDB);
            var repeat = true;
            while (repeat) {
                var hs = new HashSet<string>();
                DeviceCategory itemToChange = null;
                foreach (var item in Items) {
                    if (hs.Contains(item.ShortName)) {
                        itemToChange = item;
                        break;
                    }
                    hs.Add(item.ShortName);
                }
                if (itemToChange != null) {
                    var oldname = itemToChange.ShortName;
                    var i = 1;
                    while (i < 100 && IsNameTaken(itemToChange.ShortName + " " + i)) {
                        i++;
                    }

                    itemToChange.ShortName = itemToChange.ShortName + " " + i;
                    Logger.Info("Changed a name from " + oldname + " to " + itemToChange.ShortName);
                    if (savetoDB) {
                        itemToChange.SaveToDB();
                    }
                }
                else {
                    repeat = false;
                }
            }
        }

        public void RefreshRootCategories() {
            var newRoot = new List<DeviceCategory>();
            foreach (var category in Items) {
                if (category.IsRootCategory) {
                    newRoot.Add(category);
                }
            }
            foreach (var deviceCategory in newRoot) {
                if (!_deviceCategoriesRoot.Contains(deviceCategory)) {
                    _deviceCategoriesRoot.Add(deviceCategory);
                }
            }
            var items2Remove = new List<DeviceCategory>();
            foreach (var category in _deviceCategoriesRoot) {
                if (!newRoot.Contains(category)) {
                    items2Remove.Add(category);
                }
            }
            foreach (var deviceCategory in items2Remove) {
                _deviceCategoriesRoot.Remove(deviceCategory);
            }
        }
    }
}