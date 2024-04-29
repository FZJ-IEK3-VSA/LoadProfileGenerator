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
using Automation;
using Automation.ResultFiles;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables {
    internal class SingleSetting : DBBase {
        [CanBeNull] private string _settingValue;

        public SingleSetting([JetBrains.Annotations.NotNull] string settingName, [JetBrains.Annotations.NotNull] string settingValue, [JetBrains.Annotations.NotNull] string connectionString,
                             [NotNull] StrGuid guid,
                             [CanBeNull]int? pID = null)
            : base(settingName, GeneralConfig.TableName, connectionString, guid) {
            _settingValue = settingValue;
            ID = pID;
        }

        [JetBrains.Annotations.NotNull]
        public string SettingValue {
            get => _settingValue ??"";
            set {
                if (_settingValue == value) {
                    return;
                }
                NeedsUpdate = true;
                _settingValue = value;
                SaveToDB();
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            message = "";
            return true;
        }

        [JetBrains.Annotations.NotNull]
        public static Dictionary<string, SingleSetting> LoadFromDatabase([JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields) {
            var items2Delete = new List<SingleSetting>();
            var settings = new Dictionary<string, SingleSetting>();
            using (var con = new Connection(connectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    using (var dr = cmd.GetTableReader(GeneralConfig.TableName)) {
                        while (dr.Read()) {
                            var id = (int?) (long?) dr["ID"];
                            string settingName = dr.GetString("SettingName","");
                            string settingValue = dr.GetString("SettingValue", "");
                            var guid = GetGuid(dr, ignoreMissingFields);
                            var s = new SingleSetting(settingName, settingValue,
                                connectionString, guid,id);
                            if (!settings.ContainsKey(s.Name)) {
                                settings.Add(s.Name, s);
                            }
                            else {
                                items2Delete.Add(s);
                            }
                        }
                    }
                }
            }
            foreach (var singleSetting in items2Delete) {
                singleSetting.DeleteFromDB();
            }
            return settings;
        }

        public override void SaveToDB() {
            if (ID != null) {
                if (NeedsUpdate) {
                    using (var con = new Connection(ConnectionString)) {
                        using (var cmd = new Command(con)) {
                            con.Open();
                            cmd.AddParameter("myname", Name);
                            if(_settingValue != null) {
                                cmd.AddParameter("myval", _settingValue);
                            }

                            cmd.AddParameter("myid", ID);
                            cmd.AddParameter("guid", Guid.StrVal);
                            cmd.ExecuteNonQuery(
                                "UPDATE tblSettings SET Settingname = @myname, Settingvalue = @myval, Guid=@guid WHERE ID = @myid");
                            NeedsUpdate = false;
                        }
                    }
                }
            }
            else {
                using (var con = new Connection(ConnectionString)) {
                    con.Open();
                    using (var cmd = new Command(con)) {
                        cmd.AddParameter("myname", Name);
                        if (_settingValue != null) {
                            cmd.AddParameter("myval", _settingValue);
                        }
                        else {
                            cmd.AddParameter("myval", string.Empty);
                        }
                        cmd.AddParameter("guid",Guid.StrVal);
                        cmd.ExecuteNonQuery(
                            "INSERT INTO tblSettings (Settingname, Settingvalue, Guid) VALUES (@myname, @myval, @guid);");
                        NeedsUpdate = false;
                    }
                }
            }
        }

        protected override void SetSqlParameters(Command cmd) {
            throw new LPGException("Not Implemented");
        }

        public override string ToString() => Name;

        public void EnableNeedsUpdate()
        {
            NeedsUpdate = true;
        }
    }
}