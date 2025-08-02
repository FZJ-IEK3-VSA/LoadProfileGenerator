using Automation.ResultFiles;
using System.Collections.Generic;

namespace Common.SQLResultLogging
{
    public interface IResultLoggingService
    {
        bool CheckifTableExits(string tableName);
        void DeleteEntries(List<Dictionary<string, object>> entries, string tableName, HouseholdKey householdKey);
        void DeleteEntry(Dictionary<string, object> entry, string tableName, HouseholdKey householdKey);
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
            //return new JsonResultLoggingService(basePath);
            return new SqlResultLoggingService(basePath);
        }
    }
}
