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
using Automation;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Helpers {
    public interface IAssignableDevice : IComparable {
        AssignableDeviceType AssignableDeviceType { get; }
        [JetBrains.Annotations.NotNull]
        string ConnectionString { get; }
      //  int? ID { get; }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        [UsedImplicitly]
        int IntID { get; }

        [JetBrains.Annotations.NotNull]
        string Name { get; }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        List<Tuple<VLoadType, double>> CalculateAverageEnergyUse([CanBeNull] VLoadType dstLoadType,
            [ItemNotNull] [CanBeNull] ObservableCollection<DeviceAction> allActions, [CanBeNull] TimeBasedProfile timeProfile, double multiplier,
            double probability);

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        List<RealDevice> GetRealDevices([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> allActions);
        bool IsOrContainsStandbyDevice([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> allActions);
        JsonReference GetJsonReference();
    }
}