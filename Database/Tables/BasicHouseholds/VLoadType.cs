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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Tables.BasicHouseholds {
    public class VLoadType : DBBaseElement {
        public const string TableName = "tblLoadTypes";
        [NotNull] private string _description;
        private double _exampleOfPower;
        private double _exampleOfSum;
        private double _loadTypeWeight;
        private LoadTypePriority _priority;
        private bool _showInCharts;
        private TimeSpan _timeSpanForSum;
        [NotNull] private string _unitOfPower;
        [NotNull] private string _unitOfSum;

        public VLoadType([NotNull] string pName, [NotNull] string description, [NotNull] string unitOfPower, [NotNull] string unitOfSum, double exampleOfPower,
            double exampleOfSum, TimeSpan timeSpanForSum, double loadTypeWeight, [NotNull] string connectionString,
            LoadTypePriority priority, bool showInCharts, [NotNull] string guid, [CanBeNull]int? pID = null)
            : base(pName, TableName, connectionString, guid)
        {
            if (pID == 0 || pID == -1) {
                throw new LPGException("Loadtype ID should never be 0 or negative!");
            }
            ID = pID;
            TypeDescription = "Loadtype";
            _description = description;
            _unitOfPower = unitOfPower;
            _unitOfSum = unitOfSum;
            _exampleOfPower = exampleOfPower;
            _exampleOfSum = exampleOfSum;
            _timeSpanForSum = timeSpanForSum;
            _loadTypeWeight = loadTypeWeight;
            _priority = priority;
            _showInCharts = showInCharts;
        }

        [UsedImplicitly]
        public double ConversionFaktorPowerToSum {
            get {
                var timeInSeconds = _timeSpanForSum.TotalSeconds;
                // 1000 W * 3600 s = 1 kWh => Faktor 1/3600000
                var factor = _exampleOfSum / (_exampleOfPower * timeInSeconds);
                return factor;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        public double ExampleOfPower {
            get => _exampleOfPower;
            set => SetValueWithNotify(value, ref _exampleOfPower, nameof(ExampleOfPower));
        }

        public double ExampleOfSum {
            get => _exampleOfSum;
            set => SetValueWithNotify(value, ref _exampleOfSum, nameof(ExampleOfSum));
        }

        [UsedImplicitly]
        public double LoadTypeWeight {
            get => _loadTypeWeight;
            set => SetValueWithNotify(value, ref _loadTypeWeight, nameof(LoadTypeWeight));
        }

        [UsedImplicitly]
        public LoadTypePriority Priority {
            get => _priority;
            set => SetValueWithNotify(value, ref _priority, nameof(Priority));
        }

        [UsedImplicitly]
        public bool ShowInCharts {
            get => _showInCharts;
            set => SetValueWithNotify(value, ref _showInCharts, nameof(ShowInCharts));
        }

        public TimeSpan TimeSpanForSum {
            get => _timeSpanForSum;
            set => SetValueWithNotify(value, ref _timeSpanForSum, nameof(TimeSpanForSum));
        }

        [NotNull]
        public string UnitOfPower {
            get => _unitOfPower;
            set => SetValueWithNotify(value, ref _unitOfPower, nameof(UnitOfPower));
        }

        [NotNull]
        public string UnitOfSum {
            get => _unitOfSum;
            set => SetValueWithNotify(value, ref _unitOfSum, nameof(UnitOfSum));
        }

        [NotNull]
        private static VLoadType AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name");
            var id = (int) dr.GetLong("ID");
            var description = dr.GetString("Description");
            var unitOfPower = dr.GetString("UnitOfPower");
            var unitOfSum = dr.GetString("UnitOfSum");
            var exampleOfPower = dr.GetDouble("ExampleOfPower");
            var exampleOfSum = dr.GetDouble("ExampleOfSum");
            var loadTypeWeight = dr.GetDouble("LoadTypeWeight", false);
            var priority = (LoadTypePriority) dr.GetIntFromLongOrInt("Priority", false, ignoreMissingFields);
            var timeSpanForSumInSeconds = dr.GetTimeSpan("TimeSpanForSum");
            var showInCharts = dr.GetBool("ShowInCharts", false, true, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new VLoadType(name, description, unitOfPower, unitOfSum, exampleOfPower, exampleOfSum,
                timeSpanForSumInSeconds, loadTypeWeight, connectionString, priority, showInCharts,guid, id);
        }

        public double ConvertPowerValueWithTime(double value, TimeSpan ts) => value * ts.TotalSeconds *
                                                                              ConversionFaktorPowerToSum;

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([CanBeNull] Func<string, bool> isNameTaken, [NotNull] string connectionString)
        {
            string name = "New Load Type";
            if (isNameTaken != null) {
                name =FindNewName(isNameTaken, "New Load Type ");
            }

            var vLoadType = new VLoadType(name
                , "(no description)", "Watt", "kWh", 1000, 1,
                new TimeSpan(1, 0, 0), 1, connectionString, LoadTypePriority.Mandatory, true,
                System.Guid.NewGuid().ToString());
            return vLoadType;
        }

        [ItemNotNull]
        [NotNull]
        public override List<UsedIn> CalculateUsedIns([NotNull] Simulator sim)
        {
            var usedIns = new List<UsedIn>();
            foreach (var device in sim.RealDevices.It) {
                if (device.Loads.Any(x => x.LoadType == this)) {
                    usedIns.Add(new UsedIn(device, "Device"));
                }
            }
            foreach (var trafo in sim.TransformationDevices.It) {
                if (trafo.LoadTypeIn == this || trafo.LoadTypesOut.Any(x => x.VLoadType == this)) {
                    usedIns.Add(new UsedIn(trafo, "Transformation Device"));
                }
            }

            foreach (var aff in sim.Affordances.It) {
                foreach (AffordanceDevice device in aff.AffordanceDevices) {
                    if (device.LoadType == this) {
                        usedIns.Add(new UsedIn(aff,aff.TypeDescription));
                    }
                }
            }
            return usedIns;
        }

        [NotNull]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dstSim")]
        [UsedImplicitly]
        public static VLoadType ImportFromItem([NotNull] VLoadType toImport, [NotNull] Simulator dstSim)
        {
            var vlt = new VLoadType(toImport.Name, toImport.Description, toImport.UnitOfPower, toImport.UnitOfSum,
                toImport.ExampleOfPower, toImport.ExampleOfSum, toImport.TimeSpanForSum, toImport.LoadTypeWeight,
                dstSim.ConnectionString, toImport.Priority, toImport.ShowInCharts, toImport.Guid);
            vlt.SaveToDB();
            return vlt;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([NotNull] [ItemNotNull] ObservableCollection<VLoadType> result, [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("Description", _description);
            cmd.AddParameter("UnitOfPower", _unitOfPower);
            cmd.AddParameter("UnitOfSum", _unitOfSum);
            cmd.AddParameter("ExampleOfPower", _exampleOfPower);
            cmd.AddParameter("ExampleOfSum", _exampleOfSum);
            cmd.AddParameter("TimeSpanForSum", _timeSpanForSum);
            cmd.AddParameter("LoadTypeWeight", _loadTypeWeight);
            cmd.AddParameter("ShowInCharts", _showInCharts);
            cmd.AddParameter("Priority", _priority);
        }

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((VLoadType)toImport,dstSim);
    }
}