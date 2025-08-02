using Automation.ResultFiles;
using System;
using System.Collections.Generic;

namespace Common.SQLResultLogging
{
    public interface IResultLoggingService
    {
        void DeleteEntries(IEnumerable<Dictionary<string, object>> entries, string tableName, HouseholdKey householdKey);
        List<DatabaseEntry> LoadDatabases();
        List<ResultTableDefinition> LoadTables(HouseholdKey dbKey);
        List<T> ReadFromJson<T>(ResultTableDefinition rtd, HouseholdKey key, ExpectedResultCount expectedResult);
        IEnumerable<T> ReadFromJsonAsEnumerable<T>(ResultTableDefinition rtd, HouseholdKey key);
        void SaveDictionaryToDatabaseNewConnection(Dictionary<string, object> values, string tableName, HouseholdKey householdKey);
        void SaveDictionaryToDatabaseNewConnection(List<Dictionary<string, object>> values, string tableName, HouseholdKey householdKey);
        void SaveResultEntry(SaveableEntry entry);

        Dictionary<HouseholdKey, FileEntry> FilenameByHouseholdKey { get; } // TODO: only for testing
        void MakeTableForListOfFields(List<FieldDefinition> fields, HouseholdKey householdKey, string tableName); // TODO: only for testing
    }

    public static class ResultLoggingFactory
    {
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
