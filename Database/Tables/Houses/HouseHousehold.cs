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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class HouseHousehold : DBBase {
        public const string TableName = "tblHouseHouseholds";

        [CanBeNull] private readonly ICalcObject _household;

        public HouseHousehold([CanBeNull]int? pID, int houseID, [CanBeNull] ICalcObject household, [NotNull] string connectionString,
            [NotNull] string householdName, StrGuid guid, [CanBeNull] TransportationDeviceSet transportationDeviceSet, [CanBeNull] ChargingStationSet chargingStationSet, [CanBeNull] TravelRouteSet travelRouteSet) : base(householdName, TableName, connectionString, guid)
        {
            ID = pID;
            HouseID = houseID;
            _household = household;
            TransportationDeviceSet = transportationDeviceSet;
            ChargingStationSet = chargingStationSet;
            TravelRouteSet = travelRouteSet;
            TypeDescription = "House Household";
        }
        [CanBeNull]
        public ICalcObject CalcObject => _household;
        [CanBeNull]
        public TransportationDeviceSet TransportationDeviceSet {get; set; }
        [CanBeNull]
        public ChargingStationSet ChargingStationSet { get; set; }
        [CanBeNull]
        public TravelRouteSet TravelRouteSet { get; set; }
        [UsedImplicitly]
        public CalcObjectType CalcObjectType {
            get {
                if (_household != null) {
                    return _household.CalcObjectType;
                }
                return CalcObjectType.ModularHousehold;
            }
        }

        public int HouseID { get; }

        [ItemNotNull]
        [NotNull]
        public List<VacationTimeframe> VacationTimeframes => CalculateVacationTimeframes();

        [NotNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        private static HouseHousehold AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var houseID = dr.GetIntFromLong("HouseID");
            var householdID = dr.GetNullableIntFromLong("HouseholdID", false);

            ICalcObject calcObject = aic.ModularHouseholds.FirstOrDefault(hh1 => hh1.ID == householdID);
            var householdname = string.Empty;
            if (calcObject != null) {
                householdname = calcObject.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            int chargingSetID = dr.GetIntFromLong("ChargingSetID",false,ignoreMissingFields);
            var chargingStation = aic.ChargingStationSets.FirstOrDefault(x => x.ID == chargingSetID);

            int transportationDeviceSetID = dr.GetIntFromLong("TransportationDeviceSetID", false, ignoreMissingFields);
            var transportationDeviceSet = aic.TransportationDeviceSets.FirstOrDefault(x => x.ID == transportationDeviceSetID);

            int travelrouteSetID = dr.GetIntFromLong("TravelRouteSetID",false, ignoreMissingFields);
            var travelrouteSet = aic.TravelRouteSets.FirstOrDefault(x => x.ID == travelrouteSetID);
            return new HouseHousehold(id, houseID, calcObject,
                connectionString, householdname, guid,
                transportationDeviceSet,chargingStation,travelrouteSet);
        }

        [ItemNotNull]
        [NotNull]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        private List<VacationTimeframe> CalculateVacationTimeframes()
        {
            if (_household == null) {
                return new List<VacationTimeframe>();
            }
            switch (_household.CalcObjectType) {
                case CalcObjectType.ModularHousehold:
                    var chh = (ModularHousehold) _household;
                    if (chh.Vacation == null) {
                        throw new LPGException("Vacation was null");
                    }
                    return chh.Vacation.VacationTimeframes();
                default:
                    throw new LPGException("Forgotten Calc Object Type");
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_household == null) {
                message = "Household not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseHousehold> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<ModularHousehold> modularHouseholds,
                                            [ItemNotNull] [NotNull] ObservableCollection<ChargingStationSet> chargingStationSets,
                                            [ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceSet> transportationDeviceSets,
                                            [ItemNotNull] [NotNull] ObservableCollection<TravelRouteSet> travelRouteSets,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(modularHouseholds: modularHouseholds,chargingStationSets:chargingStationSets,
                transportationDeviceSets:transportationDeviceSets,travelRouteSets:travelRouteSets);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("HouseID", HouseID);
            if (_household != null) {
                cmd.AddParameter("HouseholdID", _household.IntID);
                cmd.AddParameter("CalcObjectType", (int) _household.CalcObjectType);
            }
            if(ChargingStationSet != null)
            {
                cmd.AddParameter("ChargingSetID",ChargingStationSet.IntID);
            }
            if (TransportationDeviceSet != null)
            {
                cmd.AddParameter("TransportationDeviceSetID", TransportationDeviceSet.IntID);
            }
            if (TravelRouteSet != null)
            {
                cmd.AddParameter("TravelRouteSetID", TravelRouteSet.IntID);
            }
        }

        public override string ToString() => Name;
    }
}