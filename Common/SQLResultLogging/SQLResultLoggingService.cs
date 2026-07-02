using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Threading;

namespace Common.SQLResultLogging
{
    /*public interface ITypeDescriber {
        [JetBrains.Annotations.NotNull]
        HouseholdKey HouseholdKey { get; }

        [UsedImplicitly]
        int ID { get; set; }

        [JetBrains.Annotations.NotNull]
        string GetTypeDescription();
    }*/

    /// <summary>
    /// The original result logging service that stores data in sqlite database files. There is one database file
    /// for every different HouseholdKey, so one per household and additionally one general database for the house.
    /// When using many SQLResultLoggingServices in parallel, e.g. in a city simulation, errors with database locking
    /// can occur. In this case, use the JsonResultLoggingService instead.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class SqlResultLoggingService : IResultLoggingService
    {
        private readonly string _basePath;

        private readonly Dictionary<HouseholdKey, List<string>> _createdTablesPerHousehold = [];

        private readonly Dictionary<HouseholdKey, FileEntry> _filenameByHouseholdKey = [];

        private bool _isFileNameDictLoaded;

        public bool DoesTableExist(HouseholdKey key, [JetBrains.Annotations.NotNull] string tableName)
        {
            string constr = GetConnectionString(Constants.GeneralHouseholdKey, false);
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name LIKE'" +
                                                             tableName + "'"))
                {
                    cmd.Connection = conn;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var s = dr.GetString(0);
                            if (string.Equals(s, tableName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
        }

        public SqlResultLoggingService([JetBrains.Annotations.NotNull] string basePath)
        {
            _basePath = basePath;
            if (_basePath.Contains(".sqlite"))
            {
                throw new LPGException("need to put in the path, not a filename");
            }
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<HouseholdKey, FileEntry> FilenameByHouseholdKey => _filenameByHouseholdKey;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<DatabaseEntry> LoadDatabases()
        {
            List<DatabaseEntry> td = new List<DatabaseEntry>();
            const string sql = $"SELECT * FROM {Constants.DatabaseListTableName}";

            string constr = GetConnectionString(Constants.GeneralHouseholdKey);
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string keyStr = reader["HouseholdKey"].ToString() ?? "";
                        HouseholdKey key = new HouseholdKey(keyStr);
                        string filename = reader["Filename"].ToString() ?? "";
                        DatabaseEntry fe = new DatabaseEntry(filename, key);
                        td.Add(fe);
                    }
                }
            }

            return td;
        }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<ResultTableDefinition> LoadTables([JetBrains.Annotations.NotNull] HouseholdKey dbKey)
        {
            List<ResultTableDefinition> td = new List<ResultTableDefinition>();
            if (!FilenameByHouseholdKey.ContainsKey(dbKey))
            {
                return td;
            }
            const string sql = $"SELECT * FROM {Constants.TableDescriptionTableName}";

            string constr = GetConnectionString(dbKey);
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string tableName = reader["TableName"].ToString() ?? "no table name";
                        string description = reader["Description"].ToString() ?? "no description";
                        int resultTableid = (int)(long)reader["ResultTableID"];
                        CalcOption enablingOption = (CalcOption)(long)reader["EnablingOption"];
                        ResultTableDefinition fe = new ResultTableDefinition(tableName, (ResultTableID)resultTableid, description, enablingOption);
                        td.Add(fe);
                    }
                }
            }

            return td;
        }

        //[JetBrains.Annotations.NotNull]
        //private string MainFilename { get; set; }

        public void MakeTableForListOfFields([JetBrains.Annotations.NotNull] [ItemNotNull]
                                             List<FieldDefinition> fields,
                                             [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                             [JetBrains.Annotations.NotNull] string tableName)
        {
            if (fields.Count == 0)
            {
                throw new LPGException("No fields defined for database");
            }

            string sql = "CREATE TABLE " + tableName + "(";
            foreach (var field in fields)
            {
                sql += field.Name + " " + field.Type + ",";
            }

            sql = sql.Substring(0, sql.Length - 1) + ");";
            string conStr = GetConnectionString(householdKey, true);
            using (SQLiteConnection conn = new SQLiteConnection(conStr)
            )
            {
                AttemptToOpenDBConnection(conn);
                var command = conn.CreateCommand();
                command.CommandText = sql;
                var result = command.ExecuteNonQuery();
                if (result != 0)
                {
                    throw new LPGException("Creating the table " + tableName + " failed.");
                }
            }
        }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public IEnumerable<T> ReadFromJsonAsEnumerable<T>([JetBrains.Annotations.NotNull] ResultTableDefinition rtd, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            if (!_isFileNameDictLoaded)
            {
                LoadFileNameDict();
            }

            string sql = $"SELECT {Constants.JsonColumnName} FROM " + rtd.TableName;
            if (!FilenameByHouseholdKey.ContainsKey(key))
            {
                throw new LPGException("Missing sql file for household key " + key);
            }

            string constr = GetConnectionString(key);
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    /*
                    List<string> tables = new List<string>();
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
                    using (var reader2 = cmd.ExecuteReader()) {
                        while (reader2.Read())
                        {
                            tables.Add(reader2[0].ToString());
                        }
                    }*/

                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string s = reader[0].ToString() ?? "";
                        T re = JsonConvert.DeserializeObject<T>(s);
                        if (re is null)
                        {
                            throw new LPGException("object was null");
                        }

                        yield return re;
                    }
                }
            }
        }


        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<T> ReadFromJson<T>([JetBrains.Annotations.NotNull] ResultTableDefinition rtd, [JetBrains.Annotations.NotNull] HouseholdKey key,
                                       ExpectedResultCount expectedResult)
        {
            if (!_isFileNameDictLoaded)
            {
                LoadFileNameDict();
            }

            string sql = $"SELECT {Constants.JsonColumnName} FROM " + rtd.TableName;

            string constr = GetConnectionString(key);
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    /*
                    List<string> tables = new List<string>();
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
                    using (var reader2 = cmd.ExecuteReader()) {
                        while (reader2.Read())
                        {
                            tables.Add(reader2[0].ToString());
                        }
                    }*/
                    List<T> resultsObjects = new List<T>();
                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string s = reader[0].ToString() ?? "";
                        T re = JsonConvert.DeserializeObject<T>(s);
                        resultsObjects.Add(re);
                    }

                    CheckResultCount(expectedResult, resultsObjects.Count);
                    return resultsObjects;
                }
            }
        }

        /// <summary>
        /// Checks whether the expected number of results was found, throwing an
        /// exception if not.
        /// </summary>
        /// <param name="expectedResult">the expected number of results</param>
        /// <param name="resultsObjectCount">the number of results found</param>
        /// <exception cref="DataIntegrityException">if the encountered number does not match the expectation</exception>
        /// <exception cref="ArgumentOutOfRangeException">if an enum value was missing</exception>
        public static void CheckResultCount(ExpectedResultCount expectedResult, int resultsObjectCount)
        {
            switch (expectedResult)
            {
                case ExpectedResultCount.One:
                    if (resultsObjectCount != 1)
                    {
                        throw new DataIntegrityException("Not exactly one result");
                    }

                    break;
                case ExpectedResultCount.Many:
                    if (resultsObjectCount < 2)
                    {
                        throw new DataIntegrityException("Not many results");
                    }

                    break;
                case ExpectedResultCount.OneOrMore:
                    if (resultsObjectCount < 1)
                    {
                        throw new DataIntegrityException("Not one or more results");
                    }

                    break;
                case ExpectedResultCount.AnyNumber:
                    break; // any number is allowed
                default:
                    throw new ArgumentOutOfRangeException(nameof(expectedResult), expectedResult, null);
            }
        }

        public void SaveDictionaryToDatabaseNewConnection([JetBrains.Annotations.NotNull] Dictionary<string, object> values,
                                                          [JetBrains.Annotations.NotNull] string tableName,
                                                          [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
            => SaveDictionaryToDatabaseNewConnection([values], tableName, householdKey);

        public void SaveDictionaryToDatabaseNewConnection([ItemNotNull] [JetBrains.Annotations.NotNull]
                                                          List<Dictionary<string, object>> values,
                                                          [JetBrains.Annotations.NotNull] string tableName,
                                                          [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            string sql = "Insert into " + tableName + "(";
            string fields = "";
            string parameters = "";
            foreach (KeyValuePair<string, object> pair in values[0])
            {
                fields += pair.Key + ",";
                parameters += "@" + pair.Key + ",";
            }

            fields = fields.Substring(0, fields.Length - 1);
            parameters = parameters.Substring(0, parameters.Length - 1);
            sql += fields + ") VALUES (" + parameters + ")";
            string conStr = GetConnectionString(householdKey, true, true);
            using SQLiteConnection conn = new(conStr);
            AttemptToOpenDBConnection(conn);
            using (var transaction = conn.BeginTransaction())
            {
                var command = conn.CreateCommand();
                command.CommandText = sql;
                foreach (Dictionary<string, object> row in values)
                {
                    if (row.Count != values[0].Count)
                    {
                        throw new LPGException("Incorrect number of columns");
                    }

                    command.Parameters.Clear();
                    foreach (KeyValuePair<string, object> pair in row)
                    {
                        string parameter = "@" + pair.Key;
                        command.Parameters.AddWithValue(parameter, pair.Value);
                    }

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Attempts to open a database. If that fails, retries a fixed number of times
        /// before aborting.
        /// </summary>
        /// <param name="conn">the Database connection to open</param>
        /// <exception cref="LPGException">if the maximum number of attempts failed</exception>
        private static void AttemptToOpenDBConnection(SQLiteConnection conn)
        {
            bool successful = false;
            int failures = 0;
            while (!successful)
            {
                try
                {
                    conn.Open();
                    successful = true;
                }
                catch (SQLiteException e)
                {
                    // opening the DB failed, e.g., because the "database is locked"
                    failures++;
                    if (failures > Constants.MaxDbOpenAttempts)
                        throw new LPGException($"Could not open the database in {Constants.MaxDbOpenAttempts} attempts", e);

                    // wait a bit before trying again
                    Thread.Sleep(1000);
                }
            }
            if (failures > 0)
                Logger.Info($"Opening database succeeded on {failures + 1}. attempt");
        }

        public void SaveResultEntry([JetBrains.Annotations.NotNull] SaveableEntry entry)
        {
            entry.IntegrityCheck();
            string conStr = GetConnectionString(entry.HouseholdKey, true);
            using SQLiteConnection conn = new(conStr);
            AttemptToOpenDBConnection(conn);
            if (!CheckIfTableExists(entry.ResultTableDefinition.TableName, entry.HouseholdKey))
            {
                CreateNewTable(entry, conn);
            }

            SaveDictionaryToDatabase(entry.RowEntries, entry.ResultTableDefinition.TableName, conn);
        }

        /*
public void SaveToDatabase<T>([JetBrains.Annotations.NotNull] [ItemNotNull] List<T> items) where T : ITypeDescriber
{
   Dictionary<HouseholdKey, List<T>> itemsByKey = new Dictionary<HouseholdKey, List<T>>();
   foreach (T item in items) {
       HouseholdKey key = item.HouseholdKey;
       if (!itemsByKey.ContainsKey(key)) {
           itemsByKey.Add(key, new List<T>());
       }

       itemsByKey[key].Add(item);
   }

   foreach (KeyValuePair<HouseholdKey, List<T>> pair in itemsByKey) {
       var filteredItems = items.Where(x => x.HouseholdKey == pair.Key).ToList();
       SaveableEntry se = new SaveableEntry(pair.Key, typeof(T).Name, filteredItems[0].GetTypeDescription());
       var properties = typeof(T).GetProperties();
       var fprops = properties.Where(x => !IgnoreThisField(x.Name)).ToList();
       foreach (var prop in fprops) {
           se.AddField(prop.Name, prop.PropertyType);
       }

       foreach (T item in filteredItems) {
           RowBuilder rb = new RowBuilder();
           foreach (var prop in fprops) {
               rb.Add(prop.Name, prop.GetValue(item));
           }

           se.AddRow(rb.ToDictionary());
       }

       SaveResultEntry(se);
   }
}*/

        /// <summary>
        /// Build an SQLite connection string for the specified database file
        /// </summary>
        /// <param name="filename">the file path of the database to open</param>
        /// <param name="walMode">if true, sets journal mode to WAL</param>
        /// <returns>the connection string that can be used to open the database</returns>
        private static string MakeConnectionString(string filename, bool walMode=false)
        {
            string additionalParams = walMode ? ";Synchronous=OFF;Journal Mode=WAL;" : "";
            return $"Data Source={filename};Version=3{additionalParams}";
        }

        /// <summary>
        /// Gets the connection string for opening a specific database file. Makes sure the file exists.
        /// </summary>
        /// <param name="key">the household key of the database to open</param>
        /// <param name="writing">whether the database should be openend for writing</param>
        /// <param name="walMode">if true, sets journal mode to WAL in the connection string</param>
        /// <returns>the connection string that can be used to open the database</returns>
        private string GetConnectionString(HouseholdKey key, bool writing = false, bool walMode = false)
        {
            string databaseFile = InitDatabaseFile(key, writing);
            return MakeConnectionString(databaseFile, walMode);
        }

        /// <summary>
        /// Checks if a specific database file already exists, and if not, creates and initializes it if it is
        /// opened for writing.
        /// </summary>
        /// <param name="key">the household key of the database to check</param>
        /// <param name="writing">whether the database will be openend for writing</param>
        /// <returns>the file path of the database</returns>
        /// <exception cref="LPGException">if the database should be opened for reading, but does not exist</exception>
        [JetBrains.Annotations.NotNull]
        private string InitDatabaseFile([JetBrains.Annotations.NotNull] HouseholdKey key, bool writing = false)
        {
            if (FilenameByHouseholdKey.TryGetValue(key, out FileEntry? value))
            {
                // database file exists and file path is already cached
                return value.Filename;
            }

            // determine the file path of the database file based on the household key
            string dbPath = Path.Combine(_basePath, "Results." + key + ".sqlite");
            FileInfo fi = new(dbPath);
            if (fi.Exists && fi.Length > 1000)
            {
                // file already exists and is not empty, so assume it is a valid database file
                return dbPath;
            }

            if (!writing)
            {
                // the database should be opened for reading, but it does not exist
                throw new LPGException($"Database file for household key {key} does not exist: {dbPath}");
            }
            if (fi.FullName.Length > 260)
            {
                throw new LPGException($"Filename length > 260. This is a Windows limitation: {fi.FullName}");
            }

            // create the result directory if it does not exist yet
            fi.Directory.Create();

            // cache the file path of the new database file in advance
            FilenameByHouseholdKey.Add(key, new FileEntry(dbPath, true));

            // create and initialize the new database file
            string connectionString = MakeConnectionString(fi.FullName, true);
            using (SQLiteConnection dbcon = new SQLiteConnection(connectionString))
            {
                AttemptToOpenDBConnection(dbcon);
                {
                    FieldDefinition fd1 = new FieldDefinition("TableName", "Text");
                    FieldDefinition fd2 = new FieldDefinition("Description", "Text");
                    FieldDefinition fd3 = new FieldDefinition("ResultTableID", "Integer");
                    FieldDefinition fd4 = new FieldDefinition("EnablingOption", "Integer");
                    List<FieldDefinition> fields = new List<FieldDefinition> {
                        fd1,
                        fd2,
                        fd3,
                        fd4
                    };
                    MakeTableForListOfFields(fields, dbcon, Constants.TableDescriptionTableName);
                }

                bool isMainDatabase = key == Constants.GeneralHouseholdKey;
                if (isMainDatabase)
                {
                    //MainFilename = newName;
                    {
                        FieldDefinition fd1 = new FieldDefinition("Filename", "Text");
                        //FieldDefinition fd3 = new FieldDefinition("ID", "INTEGER");
                        FieldDefinition fd2 = new FieldDefinition("HouseholdKey", "Text");
                        List<FieldDefinition> fields = new List<FieldDefinition> {
                            fd1,
                            fd2
                        };
                        //fields.Add(fd3);
                        MakeTableForListOfFields(fields, dbcon, Constants.DatabaseListTableName);
                    }
                }
            }
            AddResultFileEntry(this, key, fi);
            var row = RowBuilder.Start("HouseholdKey", key.Key).Add("Filename", dbPath).ToDictionary();
            SaveDictionaryToDatabaseNewConnection(row, "DatabaseList", Constants.GeneralHouseholdKey);
            return dbPath;
        }

        /// <summary>
        /// Adds an entry for a new database file with the ResultFileEntryLogger.
        /// </summary>
        /// <param name="service">the result logging service to use for the ResultFileEntryLogger</param>
        /// <param name="key">the household key of the file</param>
        /// <param name="fi">the FileInfo object of the file</param>
        public static void AddResultFileEntry(IResultLoggingService service, HouseholdKey key, FileInfo fi, string? fileIndex = null)
        {
            ResultFileEntry rfe = new("Database", fi, false, ResultFileID.SqliteResultFiles, key.Key, fileIndex, CalcOption.BasicOverview);
            ResultFileEntryLogger rfel = new(service);
            rfel.Run(Constants.GeneralHouseholdKey, rfe);
        }

        private void LoadFileNameDict()
        {
            const string sql = "SELECT * FROM DatabaseList";
            string constr = GetConnectionString(Constants.GeneralHouseholdKey, false);
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string keyStr = reader["HouseholdKey"].ToString() ?? "";
                        HouseholdKey key = new HouseholdKey(keyStr);
                        string filename = reader["Filename"].ToString() ?? "";
                        FileEntry fe = new FileEntry(filename, true);
                        FilenameByHouseholdKey.TryAdd(key, fe);
                    }
                }
            }

            _isFileNameDictLoaded = true;
        }

        /// <summary>
        /// Deletes a list of entries from a database table
        /// </summary>
        /// <param name="entries">A list of dictionaries, one for each entry to delete. Each dictionary contains field values of the entry to delete.</param>
        /// <param name="tableName">The name of the table to delete entries from</param>
        /// <param name="householdKey">The HouseholdKey matching the entries</param>
        public void DeleteEntries([JetBrains.Annotations.NotNull][ItemNotNull] IEnumerable<Dictionary<string, object>> entries,
                                   [JetBrains.Annotations.NotNull] string tableName, HouseholdKey householdKey)
        {
            if (entries.Count() == 0)
            {
                // nothing to do
                return;
            }

            // open the SQLite database connection
            string conStr = GetConnectionString(householdKey, true);
            using SQLiteConnection conn = new(conStr);
            AttemptToOpenDBConnection(conn);

            // prepare the sql command without the specific conditions
            string sqlBase = "DELETE FROM " + tableName + " WHERE ";
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = conn.CreateCommand())
                {
                    foreach (Dictionary<string, object> row in entries)
                    {
                        // get an enumerable of "field=@field" strings and concatenate them with AND in between
                        var conditions = row.Select(pair => pair.Key + " = @" + pair.Key);
                        string conditionString = string.Join(" AND ", conditions);
                        // combine base and conditions to full command
                        command.CommandText = sqlBase + conditionString;
                        // add all parameter values
                        command.Parameters.Clear();
                        foreach (KeyValuePair<string, object> pair in row)
                        {
                            string parameter = "@" + pair.Key;
                            command.Parameters.AddWithValue(parameter, pair.Value);
                        }
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        private static void MakeTableForListOfFields([JetBrains.Annotations.NotNull] [ItemNotNull]
                                                     List<FieldDefinition> fields,
                                                     [JetBrains.Annotations.NotNull] SQLiteConnection conn,
                                                     [JetBrains.Annotations.NotNull] string tableName)
        {
            if (fields.Count == 0)
            {
                throw new LPGException("No fields defined for database");
            }

            string sql = "CREATE TABLE " + tableName + "(";
            foreach (var field in fields)
            {
                sql += field.Name + " " + field.Type + ",";
            }

            sql = sql.Substring(0, sql.Length - 1) + ");";
            int result;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = sql;
                result = command.ExecuteNonQuery();
            }

            if (result != 0)
            {
                throw new LPGException("Creating the table " + tableName + " failed.");
            }
        }

        private void CreateNewTable(SaveableEntry entry, SQLiteConnection conn)
        {
            MakeTableForListOfFields(entry.Fields, conn, entry.ResultTableDefinition.TableName);
            Dictionary<string, object> fields = new Dictionary<string, object> {
                        {"TableName", entry.ResultTableDefinition.TableName},
                        {"Description", entry.ResultTableDefinition.Description},
                        {"ResultTableID", entry.ResultTableDefinition.ResultTableID},
                        {"EnablingOption", entry.ResultTableDefinition.EnablingOption}
                    };
            List<Dictionary<string, object>> rows = [fields];
            SaveDictionaryToDatabase(rows, Constants.TableDescriptionTableName, conn);
            if (!_createdTablesPerHousehold.ContainsKey(entry.HouseholdKey))
            {
                _createdTablesPerHousehold.Add(entry.HouseholdKey, new List<string>());
            }

            _createdTablesPerHousehold[entry.HouseholdKey].Add(entry.ResultTableDefinition.TableName);
        }

        //[JetBrains.Annotations.NotNull]
        //public string ReturnMainSqlPath() => _filenameByHouseholdKey[Constants.GeneralHouseholdKey].Filename;

        private static void SaveDictionaryToDatabase([JetBrains.Annotations.NotNull][ItemNotNull] List<Dictionary<string, object>> values,
                                                     [JetBrains.Annotations.NotNull] string tableName,
                                                     [JetBrains.Annotations.NotNull] SQLiteConnection conn)
        {
            if (values.Count == 0)
            {
                return;
            }

            //figure out sql
            var firstrow = values[0];
            string sql = "Insert into " + tableName + "(";
            string fields = "";
            string parameters = "";
            foreach (KeyValuePair<string, object> pair in firstrow)
            {
                fields += pair.Key + ",";
                parameters += "@" + pair.Key + ",";
            }

            fields = fields.Substring(0, fields.Length - 1);
            parameters = parameters.Substring(0, parameters.Length - 1);
            sql += fields + ") VALUES (" + parameters + ")";
            //execute the sql
            using (var transaction = conn.BeginTransaction())
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    foreach (Dictionary<string, object> row in values)
                    {
                        if (row.Count != firstrow.Count)
                        {
                            throw new LPGException("Incorrect number of columns");
                        }

                        command.Parameters.Clear();
                        foreach (KeyValuePair<string, object> pair in row)
                        {
                            string parameter = "@" + pair.Key;
                            command.Parameters.AddWithValue(parameter, pair.Value);
                        }

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        public bool CheckIfTableExists(string tableName, HouseholdKey key)
        {
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tableName + "';";

            string constr = GetConnectionString(key);
            int lines = 0;
            using (SQLiteConnection conn = new SQLiteConnection(constr))
            {
                //;Synchronous=OFF;Journal Mode=WAL;
                AttemptToOpenDBConnection(conn);
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lines++;
                    }
                }
            }
            if (lines > 0)
            {
                return true;
            }

            return false;
        }
    }

    public enum ExpectedResultCount
    {
        One,
        OneOrMore,
        Many,
        AnyNumber
    }

    public enum SqliteDataType
    {
        Text,
        Integer,
        Double,
        Bit,
        DateTime,
        JsonField
    }

    public class SaveableEntry
    {
        public SaveableEntry([JetBrains.Annotations.NotNull] HouseholdKey householdKey, [JetBrains.Annotations.NotNull] ResultTableDefinition resultTableDefinition)
        {
            HouseholdKey = householdKey;
            ResultTableDefinition = resultTableDefinition;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<FieldDefinition> Fields { get; } = new List<FieldDefinition>();

        [JetBrains.Annotations.NotNull]
        public HouseholdKey HouseholdKey { get; }

        [JetBrains.Annotations.NotNull]
        public ResultTableDefinition ResultTableDefinition { get; }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<Dictionary<string, object>> RowEntries { get; } = new List<Dictionary<string, object>>();

        public void AddField([JetBrains.Annotations.NotNull] string name, SqliteDataType datatype)
        {
            Fields.Add(new FieldDefinition(name, datatype.ToString()));
        }

        public void AddRow([JetBrains.Annotations.NotNull] Dictionary<string, object> row)
        {
            RowEntries.Add(row);
        }

        public void IntegrityCheck()
        {
            if (RowEntries.Count > 0 && Fields.Count != RowEntries[0].Count)
            {
                throw new LPGException("Inconsistent number of columns");
            }
        }
    }

    public class RowBuilder
    {
        [JetBrains.Annotations.NotNull]
        public Dictionary<string, object> Row { get; } = new Dictionary<string, object>();

        [JetBrains.Annotations.NotNull]
        public RowBuilder Add([JetBrains.Annotations.NotNull] string name, [CanBeNull] object content)
        {
            Row.Add(name, content);
            return this;
        }

        [JetBrains.Annotations.NotNull]
        public static RowBuilder Start([JetBrains.Annotations.NotNull] string name, [CanBeNull] object content)
        {
            RowBuilder rb = new RowBuilder();
            return rb.Add(name, content);
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<string, object> ToDictionary() => Row;
    }
}