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
using System.Data.SQLite;

#endregion

namespace Database.Database
{
    /// <summary>
    /// Manages a database connection. A <see cref="SharedConnectionScope"/> can be used to share a single connection across
    /// multiple operations, as long as the scope is active.
    /// </summary>
    public sealed class Connection : IDisposable
    {
        public static int ConnectionCount { get; private set; }
        [JetBrains.Annotations.NotNull] private readonly SQLiteConnection _sqlcon;

        /// <summary>
        /// Whether this object only wraps the shared connection of an active SharedConnectionScope,
        /// which must not be opened or closed here.
        /// </summary>
        private readonly bool isShared;

        /// <summary>
        /// The shared connection of the currently active SharedConnectionScope, if any. Not thread-safe.
        /// </summary>
        private static SQLiteConnection sharedConnection;

        public Connection(string connectionString)
        {
            if (sharedConnection != null && sharedConnection.ConnectionString == connectionString)
            {
                // reuse the shared connection instead of opening a new one
                _sqlcon = sharedConnection;
                isShared = true;
                return;
            }
            _sqlcon = new SQLiteConnection(connectionString);
            ConnectionCount++;
        }

        [JetBrains.Annotations.NotNull]
        public SQLiteConnection Sqlcon => _sqlcon;

        public void Dispose() => Close();

        private void Close()
        {
            if (isShared)
            {
                // the shared connection is closed by its scope
                return;
            }
            _sqlcon.Close();
            _sqlcon.Dispose();
            ConnectionCount--;
        }

        public void Open()
        {
            if (!isShared)
            {
                _sqlcon.Open();
            }
        }

        [JetBrains.Annotations.NotNull]
        public SQLiteTransaction BeginTransaction()
        {
            return _sqlcon.BeginTransaction();
        }

        /// <summary>
        /// Opens a single shared connection with one transaction for the specified connection string. While the returned
        /// scope is active, all Connection objects created for this connection string reuse the shared connection instead
        /// of opening their own one. Transactions started on the shared connection in the meantime are nested into the
        /// scope transaction. Commit() needs to be called before disposing the scope; otherwise, all changes are rolled
        /// back. Not thread-safe; scopes cannot be nested.
        /// </summary>
        /// <param name="connectionString">the connection string of the database to open</param>
        /// <returns>the scope object, which must be disposed to close the shared connection</returns>
        public static SharedConnectionScope BeginSharedConnection(string connectionString) => new(connectionString);

        /// <summary>
        /// Manages a single shared database connection.
        /// </summary>
        public sealed class SharedConnectionScope : IDisposable
        {
            private readonly SQLiteConnection connection;
            private readonly SQLiteTransaction transaction;

            internal SharedConnectionScope(string connectionString)
            {
                if (sharedConnection != null)
                {
                    throw new InvalidOperationException("A shared database connection is already active. Nested scopes are not supported.");
                }
                // create the shared connection object and start one transaction for all operations
                connection = new SQLiteConnection(connectionString);
                connection.Open();
                transaction = connection.BeginTransaction();
                sharedConnection = connection;
            }

            /// <summary>
            /// Commits all changes made through the shared connection.
            /// </summary>
            public void Commit() => transaction.Commit();

            public void Dispose()
            {
                // disposing a transaction that was not committed rolls it back
                transaction.Dispose();
                connection.Close();
                connection.Dispose();
                sharedConnection = null;
            }
        }
    }
}