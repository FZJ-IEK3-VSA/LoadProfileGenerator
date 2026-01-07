using Automation.ResultFiles;
using System;
using System.Collections.Generic;

namespace Common.SQLResultLogging
{
    /// <summary>
    /// Interface for the result logging service. The result logging service is used throughout the main simulation to
    /// store results in a tabular format, e.g., general information on a household, or data on every action performed in a household.
    /// Data can be logged for a house in general, or for a specific household, using a HouseholdKey to identify the correct context.
    /// </summary>
    public interface IResultLoggingService
    {
        /// <summary>
        /// Deletes the given entries from the specified table. The entries to delete must be specified as full
        /// objects, the same way they are specified when adding them.
        /// </summary>
        /// <param name="entries">the entries to delete</param>
        /// <param name="tableName">the table to delete the entries from</param>
        /// <param name="householdKey">the relevant household key</param>
        void DeleteEntries(IEnumerable<Dictionary<string, object>> entries, string tableName, HouseholdKey householdKey);

        /// <summary>
        /// Loads a list of all databases managed by the result logging service. There is one database per household, and
        /// additionally one general database for the house.
        /// </summary>
        /// <returns>the list of available databases</returns>
        List<DatabaseEntry> LoadDatabases();

        /// <summary>
        /// Loads the table definitions of all tables for a specific context, i.e. for a single household or for the general
        /// data.
        /// </summary>
        /// <param name="dbKey">the HouseholdKey specifying for which context to get the table definitions</param>
        /// <returns>the list of table definitions for a single database</returns>
        List<ResultTableDefinition> LoadTables(HouseholdKey dbKey);

        /// <summary>
        /// Loads a list of objects stored in a table as JSON.
        /// </summary>
        /// <typeparam name="T">type of the stored objects</typeparam>
        /// <param name="rtd">definition of the table to load objects from</param>
        /// <param name="key">the HouseholdKey identifying the context</param>
        /// <param name="expectedResult">the expected number of objects, for verification</param>
        /// <returns>the list of loaded objects</returns>
        List<T> ReadFromJson<T>(ResultTableDefinition rtd, HouseholdKey key, ExpectedResultCount expectedResult);

        /// <summary>
        /// Loads objects stored in a table as JSON.
        /// </summary>
        /// <typeparam name="T">type of the stored objects</typeparam>
        /// <param name="rtd">definition of the table to load objects from</param>
        /// <param name="key">the HouseholdKey identifying the context</param>
        /// <returns>the loaded objects</returns>
        IEnumerable<T> ReadFromJsonAsEnumerable<T>(ResultTableDefinition rtd, HouseholdKey key);

        /// <summary>
        /// Saves a single object in dictionary format as a new row in a table.
        /// </summary>
        /// <param name="values">the object properties as dictionary</param>
        /// <param name="tableName">the table to save the object in</param>
        /// <param name="householdKey">the HouseholdKey identifying the context</param>
        void SaveDictionaryToDatabaseNewConnection(Dictionary<string, object> values, string tableName, HouseholdKey householdKey);

        /// <summary>
        /// Saves a list of objects in dictionary format as new rows in a table.
        /// </summary>
        /// <param name="values">the properties as one dictionary per object</param>
        /// <param name="tableName">the table to save the objects in</param>
        /// <param name="householdKey">the HouseholdKey identifying the context</param>
        void SaveDictionaryToDatabaseNewConnection(List<Dictionary<string, object>> values, string tableName, HouseholdKey householdKey);

        /// <summary>
        /// Saves an object in the form of a SaveableEntry. This does not store the data as JSON, but stores each object property
        /// in an individual column.
        /// </summary>
        /// <remarks>As of now, there is no general method to load such an entry</remarks>
        /// <param name="entry">the entry to save</param>
        void SaveResultEntry(SaveableEntry entry);

        Dictionary<HouseholdKey, FileEntry> FilenameByHouseholdKey { get; } // TODO: only used for testing
        void MakeTableForListOfFields(List<FieldDefinition> fields, HouseholdKey householdKey, string tableName); // TODO: only used for testing
    }

    public static class ResultLoggingFactory
    {
        /// <summary>
        /// Creates the result logging service instance to used. Chooses the correct type
        /// depending on the config.
        /// </summary>
        /// <param name="basePath">path of the base directory to log results to</param>
        /// <returns>the result logging service instance to use</returns>
        /// <exception cref="NotImplementedException">if an unknown service type was specified in the config</exception>
        public static IResultLoggingService CreateResultLoggingService(string basePath)
        {
            return Config.ResultLogger switch
            {
                ResultLoggerType.SQL => new SqlResultLoggingService(basePath),
                ResultLoggerType.JSON => new JsonResultLoggingService(basePath),
                _ => throw new NotImplementedException($"Missing case for result logger type {Config.ResultLogger}"),
            };
        }
    }

    /// <summary>
    /// Defines the types of possible ResultLoggingServices.
    /// </summary>
    public enum ResultLoggerType
    {
        SQL,
        JSON
    }
}
