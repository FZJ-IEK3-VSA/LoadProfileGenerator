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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

#endregion

namespace Database.Database
{
#pragma warning disable S2696 // Instance members should not write to "static" fields
    public sealed class DataReader : IDisposable
    {
        private static long _totalReads;
        [JetBrains.Annotations.NotNull] [ItemNotNull] private readonly SQLiteDataReader _dr;

        public DataReader([JetBrains.Annotations.NotNull] [ItemNotNull] SQLiteDataReader dr) => _dr = dr;

        [CanBeNull]
        public object this[[JetBrains.Annotations.NotNull] string colkey] => _dr[colkey];

        public static long TotalReads
        {
            get => _totalReads;
            set => _totalReads = value;
        }

        public void Dispose() => Close();

        public bool CheckForField([JetBrains.Annotations.NotNull] string fieldName)
        {
            for (var i = 0; i < _dr.FieldCount; i++)
            {
                if (string.Equals(_dr.GetName(i), fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void Close()
        {
            _dr.Close();
            _dr.Dispose();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool")]
        public bool GetBool([JetBrains.Annotations.NotNull] string fieldName, bool failWithException = true, bool defaultvalue = false,
            bool ignoreMissingFields = false)
        {
            _totalReads++;

            if (failWithException)
            {
                return (bool)_dr[fieldName];
            }
            if (ignoreMissingFields)
            {
                if (!CheckForField(fieldName))
                {
                    return defaultvalue;
                }
            }
            if (_dr[fieldName] != DBNull.Value)
            {
                return (bool)_dr[fieldName];
            }
            return defaultvalue;
        }

        /// <summary>
        ///     Reads a datetime field from the DB
        /// </summary>
        /// <param name="fieldName">the db field</param>
        /// <param name="failWithException">should it fail immidiately</param>
        /// <param name="myDefaultValue">default value</param>
        /// <param name="checkForFieldFirst">first check or just fail</param>
        /// <param name="failMissingFieldWithException">fail if field is missing</param>
        /// <returns>db value</returns>
        /// <exception cref="LPGException">Field is missing.</exception>
        public DateTime GetDateTime([JetBrains.Annotations.NotNull] string fieldName, bool failWithException = true,[CanBeNull] DateTime? myDefaultValue = null,
            bool checkForFieldFirst = false, bool failMissingFieldWithException = false)
        {
            _totalReads++;
            var defaultvalue = myDefaultValue;
            if (failWithException)
            {
                return (DateTime)_dr[fieldName];
            }
            if (defaultvalue == null)
            {
                defaultvalue = new DateTime(2000, 1, 1);
            }
            if (checkForFieldFirst)
            {
                if (!CheckForField(fieldName))
                {
                    if (failMissingFieldWithException)
                    {
                        throw new LPGException("Field " + fieldName + " is missing.");
                    }
                    return (DateTime)defaultvalue;
                }
            }
            if (_dr[fieldName] != DBNull.Value)
            {
                return (DateTime)_dr[fieldName];
            }
            return (DateTime)defaultvalue;
        }

        public decimal GetDecimal([JetBrains.Annotations.NotNull] string fieldName, bool failWithException = true, decimal defaultValue = 0m,
            bool ignoreMissingField = false)
        {
            _totalReads++;
            if (failWithException)
            {
                return (decimal)_dr[fieldName];
            }
            if (ignoreMissingField)
            {
                if (!CheckForField(fieldName))
                {
                    return defaultValue;
                }
            }
            if (_dr[fieldName] != DBNull.Value)
            {
                return (decimal)_dr[fieldName];
            }
            return defaultValue;
        }

        public double GetDouble([JetBrains.Annotations.NotNull] string fieldName, bool failWithException = true, double defaultvalue = 0,
            bool ignoreMissingFields = false)
        {
            _totalReads++;
            if (failWithException)
            {
                return (double)_dr[fieldName];
            }
            if (ignoreMissingFields)
            {
                if (!CheckForField(fieldName))
                {
                    return defaultvalue;
                }
            }
            if (_dr[fieldName] != DBNull.Value)
            {
                return (double)_dr[fieldName];
            }
            return defaultvalue;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        public int GetInt([JetBrains.Annotations.NotNull] string fieldName, bool faildirectlyWithException = true, int defaultvalue = -1,
            bool ignoreMissingFields = false)
        {
            _totalReads++;
            if (faildirectlyWithException)
            {
                return (int)_dr[fieldName];
            }
            if (ignoreMissingFields)
            {
                if (!CheckForField(fieldName))
                {
                    return defaultvalue;
                }
            }
            if (_dr[fieldName] != DBNull.Value)
            {
                return (int)_dr[fieldName];
            }
            return defaultvalue;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public int GetIntFromLong([JetBrains.Annotations.NotNull] string fieldName, bool faildirectlyWithException = true,
            bool ignoreMissingField = false, int defaultValue = 0)
        {
            var result = GetNullableIntFromLongOrInt(fieldName, faildirectlyWithException, ignoreMissingField);
            if (result != null)
            {
                return (int)result;
            }
            return defaultValue;
        }

        /// <summary>
        ///     Reads an integer from the db, no matter if the field is long or int
        ///     needed because from version changes some fields used to be int but now they are long
        /// </summary>
        /// <param name="fieldName">fieldName</param>
        /// <param name="faildirectlyWithException">faildirectlyWithException</param>
        /// <param name="ignoreMissingField">ignoreMissingField</param>
        /// <param name="defaultValue">defaultValue</param>
        /// <returns>db value</returns>
        /// <exception cref="LPGException">Condition.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        public int GetIntFromLongOrInt([JetBrains.Annotations.NotNull] string fieldName, bool faildirectlyWithException = true,
            bool ignoreMissingField = false, int defaultValue = 0)
        {
            _totalReads++;
            if (ignoreMissingField && !CheckForField(fieldName))
            {
                return defaultValue;
            }
            if (_dr[fieldName] is long)
            {
                var result = GetNullableIntFromLong(fieldName, faildirectlyWithException, ignoreMissingField);
                if (result != null)
                {
                    return (int)result;
                }
            }
            if (_dr[fieldName] is int)
            {
                var result = GetNullableInt(fieldName, faildirectlyWithException, ignoreMissingField);
                if (result != null)
                {
                    return (int)result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        ///     Reads a long from the database
        /// </summary>
        /// <param name="fieldName">fieldName</param>
        /// <param name="failWithException">afailWithException</param>
        /// <param name="defaultvalue">a defaultvalue</param>
        /// <param name="checkForFieldFirst">checkForFieldFirst</param>
        /// <param name="failMissingFieldWithException">failMissingFieldWithException</param>
        /// <returns>db value</returns>
        /// <exception cref="LPGException">Condition.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetLong([JetBrains.Annotations.NotNull] string fieldName, bool failWithException = true, int defaultvalue = -1,
            bool checkForFieldFirst = false, bool failMissingFieldWithException = false)
        {
            _totalReads++;
            if (failWithException)
            {
                return (long)_dr[fieldName];
            }
            if (checkForFieldFirst && !CheckForField(fieldName))
            {
                if (failMissingFieldWithException)
                {
                    throw new LPGException("Field " + fieldName + " is missing.");
                }
                return defaultvalue;
            }

            if (_dr[fieldName] != DBNull.Value)
            {
                return (long)_dr[fieldName];
            }
            return defaultvalue;
        }

        /// <summary>
        ///     gets a nullable int from the database
        /// </summary>
        /// <param name="fieldName">fieldName</param>
        /// <param name="faildirectlyWithException">faildirectlyWithException</param>
        /// <param name="checkForFieldFirst">checkForFieldFirst</param>
        /// <param name="failMissingFieldWithException">failMissingFieldWithException</param>
        /// <returns>db value</returns>
        /// <exception cref="LPGException">Condition.</exception>
        [CanBeNull]
        private int? GetNullableInt([JetBrains.Annotations.NotNull] string fieldName, bool faildirectlyWithException = true,
            bool checkForFieldFirst = false, bool failMissingFieldWithException = false)
        {
            _totalReads++;
            object o = _dr[fieldName];
            if (!(o is int) &&o is DBNull)
            {
                Logger.Error("GetNullableInt Error: The real type is:" + o.GetType());
            }
            if (faildirectlyWithException)
            {
                return (int?)o;
            }
            if (checkForFieldFirst)
            {
                if (!CheckForField(fieldName))
                {
                    if (failMissingFieldWithException)
                    {
                        throw new LPGException("Field " + fieldName + " is missing.");
                    }
                    return null;
                }
            }
            if (o != DBNull.Value)
            {
                return (int?)o;
            }
            return null;
        }
        [CanBeNull]
        public int? GetNullableIntFromLong([JetBrains.Annotations.NotNull] string fieldName, bool faildirectlyWithException = true,
            bool ignoreMissingField = false)
        {
            _totalReads++;

            if (!ignoreMissingField && !(_dr[fieldName] is long) && !(_dr[fieldName] is DBNull))
            {
                Logger.Error("GetNullableIntFromLong: the real type is:" + _dr[fieldName].GetType());
            }
            if (faildirectlyWithException)
            {
                return (int?)(long?)_dr[fieldName];
            }
            if (ignoreMissingField)
            {
                if (!CheckForField(fieldName))
                {
                    return null;
                }
            }
            object o = _dr[fieldName];
            if (o != DBNull.Value)
            {
                return (int?)(long?)o;
            }
            return null;
        }

        /// <summary>
        ///     Gets a nullable int from the database from either a long or int field for compatiblity reasons
        /// </summary>
        /// <param name="fieldName">fieldName</param>
        /// <param name="faildirectlyWithException">faildirectlyWithException</param>
        /// <param name="ignoreMissingField">ignoreMissingField</param>
        /// <param name="defaultvalue">defaultvalue</param>
        /// <returns>db value</returns>
        /// <exception cref="LPGException">Condition.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int")]
        [CanBeNull]
        public int? GetNullableIntFromLongOrInt([JetBrains.Annotations.NotNull] string fieldName, bool faildirectlyWithException = true,
            bool ignoreMissingField = false, [CanBeNull]int? defaultvalue = null)
        {
            _totalReads++;
            if (ignoreMissingField && !CheckForField(fieldName))
            {
                return defaultvalue;
            }
            object o = _dr[fieldName];

            if (o is long)
            {
                return GetNullableIntFromLong(fieldName, faildirectlyWithException, ignoreMissingField);
            }
            if (o is int)
            {
                return GetNullableInt(fieldName, faildirectlyWithException, ignoreMissingField);
            }
            if (o is DBNull)
            {
                return null;
            }
            throw new LPGException("Unknown type when reading field " + fieldName + ": " + _dr[fieldName].GetType());
        }
        [JetBrains.Annotations.NotNull]
        public string GetString([JetBrains.Annotations.NotNull] string fieldName,  [JetBrains.Annotations.NotNull] string defaultvalue)
        {
            string s = GetString(fieldName, true, defaultvalue);
            return s;
        }

        [JetBrains.Annotations.NotNull]
        public string GetString([JetBrains.Annotations.NotNull] string fieldName, bool failAtNullWithException = true,[JetBrains.Annotations.NotNull] string defaultvalue = "",
            bool ignoreMissingFields = false)
        {
            _totalReads++;
            if (failAtNullWithException)
            {
                return (string)_dr[fieldName];
            }
            if (ignoreMissingFields)
            {
                if (!CheckForField(fieldName))
                {
                    return defaultvalue;
                }
            }
            object o = _dr[fieldName];
            if (o != DBNull.Value)
            {
                return (string)o;
            }
            return defaultvalue;
        }

        /// <summary>
        ///     Gets a timespan from the database
        /// </summary>
        /// <param name="fieldName">fieldName</param>
        /// <param name="failWithException">failWithException</param>
        /// <param name="defaultvalue">defaultvalue</param>
        /// <param name="checkForFieldFirst">checkForFieldFirst</param>
        /// <param name="failMissingFieldWithException">failMissingFieldWithException</param>
        /// <returns>db value</returns>
        /// <exception cref="LPGException">Condition.</exception>
        public TimeSpan GetTimeSpan([JetBrains.Annotations.NotNull] string fieldName, bool failWithException = true,[CanBeNull] TimeSpan? defaultvalue = null,
            bool checkForFieldFirst = false, bool failMissingFieldWithException = false)
        {
            _totalReads++;
            if (failWithException)
            {
                var tmpTimeSpan = (DateTime)_dr[fieldName] - Config.DummyTime;
                return tmpTimeSpan;
            }
            TimeSpan convertedDefaultTimeSpan;
            if (defaultvalue == null)
            {
                convertedDefaultTimeSpan = new TimeSpan(0, 0, 0);
            }
            else
            {
                convertedDefaultTimeSpan = (TimeSpan)defaultvalue;
            }
            if (checkForFieldFirst)
            {
                if (!CheckForField(fieldName))
                {
                    if (failMissingFieldWithException)
                    {
                        throw new LPGException("Field " + fieldName + " is missing.");
                    }
                    return convertedDefaultTimeSpan;
                }
            }
            if (_dr[fieldName] != DBNull.Value)
            {
                var tmpTimeSpan = (DateTime)_dr[fieldName] - Config.DummyTime;
                return tmpTimeSpan;
            }
            return convertedDefaultTimeSpan;
        }

        public bool Read() => _dr.Read();
    }
#pragma warning restore S2696 // Instance members should not write to "static" fields
}