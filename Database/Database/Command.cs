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
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

#endregion

namespace Database.Database {
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public sealed class Command : IDisposable {
        [JetBrains.Annotations.NotNull] private static readonly Dictionary<Connection, int> _opencount = new Dictionary<Connection, int>();
        [JetBrains.Annotations.NotNull] private readonly SQLiteCommand _cmd;

        [JetBrains.Annotations.NotNull] private readonly Connection _sqlcon;

        // list of the parameter names for building the sql strings
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly List<Parameter> _sqlparameter;

        [CanBeNull] private Parameter _idParameter;

        public bool IsParameterSet([JetBrains.Annotations.NotNull] string name)
        {
            if (_sqlparameter.Any(x => String.Equals(x.SqlName, name, StringComparison.InvariantCultureIgnoreCase))) {
                return true;
            }
            return false;
        }
        public Command([JetBrains.Annotations.NotNull] Connection sqlcon)
        {
            _sqlcon = sqlcon;
            _cmd = new SQLiteCommand
            {
                Connection = sqlcon.Sqlcon,
                CommandType = CommandType.Text
            };
            _sqlparameter = new List<Parameter>();
            if (!_opencount.ContainsKey(_sqlcon)) {
                _opencount.Add(_sqlcon, 1);
            }
            else {
                _opencount[_sqlcon]++;
            }
            if (Config.IsInUnitTesting) {
                if (_opencount[_sqlcon] > 1) {
                    throw new LPGException("opening database connection...");
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Security", "CA2100:SQL-Abfragen auf Sicherheitsrisiken �berpr�fen")]
        private string Cmdstring {
            get => _cmd.CommandText;
            set => _cmd.CommandText = value;
        }

        public void Dispose()
        {
            _cmd.Dispose();
            _opencount[_sqlcon]--;
            if (_opencount[_sqlcon] == 0) {
                _opencount.Remove(_sqlcon);
            }
        }

        public void AddParameter([JetBrains.Annotations.NotNull] string parametername, [JetBrains.Annotations.NotNull] object value)
            => AddParameter(parametername, "@" + parametername, value);

        public void AddParameter([JetBrains.Annotations.NotNull] string sqlname, [JetBrains.Annotations.NotNull] string parametername, [JetBrains.Annotations.NotNull] object value, bool isID = false)
        {
            if (value == null) {
                throw new LPGException("Tried to save null to database");
            }
            var pm = new Parameter(sqlname, parametername, value);

            if (isID) {
                _idParameter = pm;
            }
            else {
                _sqlparameter.Add(pm);
            }
            var valtype = value.GetType();
            var typename = valtype.ToString();
            if (typename == "System.TimeSpan") {
                var param1 = new SQLiteParameter(parametername, DbType.DateTime)
                {
                    Value = Config.DummyTime.Add((TimeSpan)value)
                };
                _cmd.Parameters.Add(param1);
            }
            else if (value == DBNull.Value) {
                var param1 = new SQLiteParameter(parametername)
                {
                    Value = DBNull.Value
                };
                _cmd.Parameters.Add(param1);
            }
            else {
                var dstType = GetDstType(typename);
                var param = new SQLiteParameter(parametername, dstType)
                {
                    Value = value
                };
                _cmd.Parameters.Add(param);
            }
        }

        public void DeleteByID([JetBrains.Annotations.NotNull] string tblName, int id)
        {
            Cmdstring = "DELETE FROM " + tblName + " WHERE ID = @id";
            AddParameter("id", id);
            _cmd.ExecuteNonQuery();
            if (Config.ShowDeleteMessages) {
                Logger.Debug("Deleted from " + tblName + " the entry with ID " + id);
            }
        }

        public void DeleteEntireTable([JetBrains.Annotations.NotNull] string tblName)
        {
            Cmdstring = "DELETE FROM " + tblName + "";
            _cmd.ExecuteNonQuery();
        }


        public int ExecuteInsert([JetBrains.Annotations.NotNull] string tblName)
        {
            var cmd = "INSERT INTO " + tblName + " (";
            var sqlnames = string.Empty;
            var parameternames = string.Empty;
            HashSet<string> usedSqlNames = new HashSet<string>();
            foreach (var parameter in _sqlparameter) {
                sqlnames = sqlnames + parameter.SqlName + ", ";
                parameternames = parameternames + parameter.Name + ", ";
                if (usedSqlNames.Contains(parameter.SqlName)) {
                    throw new Exception("Duplicate Sql Parameter");
                }
                usedSqlNames.Add(parameter.SqlName);
            }
            sqlnames = sqlnames.Substring(0, sqlnames.Length - 2);
            parameternames = parameternames.Substring(0, parameternames.Length - 2);
            cmd = cmd + sqlnames + ") VALUES (" + parameternames + ")";
            var count = ExecuteNonQuery(cmd);
            if (count != 1 && Config.IsInUnitTesting) {
                throw new LPGException("Insert seems to have failed?!?");
            }
            return (int) _sqlcon.Sqlcon.LastInsertRowId;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public int ExecuteNonQuery([JetBrains.Annotations.NotNull] string command)
        {
            Cmdstring = command;
            return _cmd.ExecuteNonQuery();
        }

        [JetBrains.Annotations.NotNull]
        public DataReader ExecuteReader([JetBrains.Annotations.NotNull] string command)
        {
            Cmdstring = command;
            return new DataReader(_cmd.ExecuteReader());
        }

        [JetBrains.Annotations.NotNull]
        public object ExecuteScalar([JetBrains.Annotations.NotNull] string command)
        {
            Cmdstring = command;
            return _cmd.ExecuteScalar();
        }

        public void ExecuteUpdate([JetBrains.Annotations.NotNull] string tblName)
        {
            var cmd = "UPDATE " + tblName + " SET ";
            HashSet<string> usedSqlNames = new HashSet<string>();
            foreach (var parameter in _sqlparameter) {
                cmd = cmd + " " + parameter.SqlName + " =  " + parameter.Name + ", ";
                if (usedSqlNames.Contains(parameter.SqlName))
                {
                    throw new Exception("Duplicate Sql Parameter");
                }
                usedSqlNames.Add(parameter.SqlName);
            }
            cmd = cmd.Substring(0, cmd.Length - 2);
            if (_idParameter == null) {
                throw new LPGException("_idparameter was null");
            }
            cmd = cmd + " WHERE " + _idParameter.SqlName + " =  " + _idParameter.Name;
            ExecuteNonQuery(cmd);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static DbType GetDstType([CanBeNull] string typename)
        {
            if (typename == typeof(long).FullName) {
                return DbType.Int64;
            }
            if (typename == typeof(string).FullName) {
                return DbType.String;
            }
            if (typename == typeof(int).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(byte).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(double).FullName) {
                return DbType.Double;
            }
            if (typename == typeof(PermittedGender).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(DateTime).FullName) {
                return DbType.DateTime;
            }
            if (typename == typeof(bool).FullName) {
                return DbType.Boolean;
            }
            if (typename == typeof(decimal).FullName) {
                return DbType.Decimal;
            }
            if (typename == typeof(AssignableDeviceType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(PermissionMode).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(AnyAllTimeLimitCondition).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(HealthStatus).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(ModularHouseholdTrait.ModularHouseholdTraitAssignType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(DayOfWeek).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(TimeType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(TimeProfileType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(EnergyIntensityType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(TraitLimitType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(CalcObjectType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(VariableAction).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(VariableCondition).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(VariableExecutionTime).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(ActionAfterInterruption).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(TraitPriority).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(LoadTypePriority).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(EstimateType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(SpeedUnit).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(TransportationDeviceCategory).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(CreationType).FullName) {
                return DbType.Int32;
            }
            if (typename == typeof(BodilyActivityLevel).FullName)
            {
                return DbType.Int32;
            }
            if (typename == typeof(FlexibilityType).FullName)
            {
                return DbType.Int32;
            }
            const string s = nameof(PermittedGender);
            var s2 = typeof(PermittedGender).FullName;
            throw new LPGException("Type fehlt:" + typename + Environment.NewLine + s + Environment.NewLine + s2);
        }

        [JetBrains.Annotations.NotNull]
        public DataReader GetTableReader([JetBrains.Annotations.NotNull] string tblName)
        {
            Cmdstring = "SELECT * FROM " + tblName;
            return new DataReader(_cmd.ExecuteReader());
        }

        public static void HideDeleteMessages() => Config.ShowDeleteMessages = false;

        public static void PrintOpenConnections()
        {
            Logger.Debug("sqlconnection status:");
            foreach (var pair in _opencount) {
                Logger.Debug("connection:" + pair.Key + " - " + pair.Value);
            }
            Logger.Debug("sqlconnection status finished");
        }

        [JetBrains.Annotations.NotNull]
        public override string ToString() => Cmdstring;
    }
}