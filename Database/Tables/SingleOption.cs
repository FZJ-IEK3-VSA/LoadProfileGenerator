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
using Automation;
using Automation.ResultFiles;
using Database.Database;
using JetBrains.Annotations;

#endregion

namespace Database.Tables {
    public class SingleOption : DBBase {
        public const string TableName = "tblOptions";
        private static bool _savingOk = true;
        private CalcOption _option;
        private bool _settingValue;

        public SingleOption(CalcOption option, bool settingValue, [NotNull] string connectionString, [NotNull] string guid,
                            [CanBeNull] int? pID = null)
            : base(option.ToString(), GeneralConfig.TableName, connectionString, guid)
        {
            _settingValue = settingValue;
            ID = pID;
            _option = option;
        }

        [UsedImplicitly]
        public CalcOption Option {
            get => _option;
            set {
                if (_option == value) {
                    return;
                }
                NeedsUpdate = true;
                _option = value;
                SaveToDB();
            }
        }

        public bool SettingValue {
            get => _settingValue;
            set {
                if (_settingValue == value) {
                    return;
                }
                NeedsUpdate = true;
                _settingValue = value;
                SaveToDB();
            }
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        [NotNull]
        public static Dictionary<CalcOption, SingleOption>
            LoadFromDatabase([NotNull] string connectionString, bool ignoreMissing)
        {
            if (ignoreMissing) {
                if (!DoesTableExist(connectionString, TableName)) {
                    _savingOk = false;
                    return new Dictionary<CalcOption, SingleOption>();
                }
            }
            var items2Delete = new List<int>();
            var settings = new Dictionary<CalcOption, SingleOption>();
            using (var con = new Connection(connectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    using (var dr = cmd.GetTableReader(TableName)) {
                        while (dr.Read()) {
                            var id = dr.GetIntFromLong("ID", false, ignoreMissing);
                            if (Enum.IsDefined(typeof(CalcOption), id)) {
                                var option = (CalcOption) id;
                                var value = dr.GetBool("Value");
                                var guid = GetGuid(dr, true);
                                var o = new SingleOption(option, value, connectionString,guid, id);

                                settings.Add(option, o);
                            }
                            else {
                                items2Delete.Add(id);
                            }
                        }
                    }
                }
            }
            foreach (var i in items2Delete) {
                DeleteByID(i, TableName, connectionString);
            }
            return settings;
        }

        public override void SaveToDB()
        {
            if (!_savingOk) {
                return;
            }
            if (ID != null) {
                if (NeedsUpdate) {
                    using (var con = new Connection(ConnectionString)) {
                        using (var cmd = new Command(con)) {
                            con.Open();
                            cmd.AddParameter("myval", _settingValue);
                            cmd.AddParameter("myid", ID);
                            cmd.AddParameter("guid", Guid);
                            cmd.ExecuteNonQuery("UPDATE " + TableName + " SET Value = @myval, Guid=@guid WHERE ID = @myid");
                            NeedsUpdate = false;
                        }
                    }
                }
            }
            else {
                using (var con = new Connection(ConnectionString)) {
                    con.Open();
                    using (var cmd = new Command(con)) {
                        cmd.AddParameter("myval", _settingValue);
                        cmd.AddParameter("myid", (int) _option);
                        cmd.AddParameter("guid", Guid);
                        cmd.ExecuteNonQuery("INSERT INTO " + TableName + " (ID, Value, Guid) VALUES (@myid, @myval, @guid)");
                        ID = (int) _option;
                        NeedsUpdate = false;
                    }
                }
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            throw new LPGException("Not Implemented");
        }

        public override string ToString() => Name;

        public void EnableNeedsUpdate()
        {
            NeedsUpdate = true;
        }
    }
}