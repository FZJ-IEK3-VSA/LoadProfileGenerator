using Automation.ResultFiles;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Common.SQLResultLogging
{
    public interface IResultLoggingService
    {
        Dictionary<HouseholdKey, FileEntry> FilenameByHouseholdKey { get; }

        bool CheckifTableExits(string tableName);
        void DeleteEntries([ItemNotNull, NotNull] List<Dictionary<string, object>> entries, [NotNull] string tableName, HouseholdKey householdKey);
        void DeleteEntry(Dictionary<string, object> entry, [NotNull] string tableName, HouseholdKey householdKey);
        List<DatabaseEntry> LoadDatabases();
        List<ResultTableDefinition> LoadTables([NotNull] HouseholdKey dbKey);
        void MakeTableForListOfFields([ItemNotNull, NotNull] List<FieldDefinition> fields, [NotNull] HouseholdKey householdKey, [NotNull] string tableName);
        List<T> ReadFromJson<T>([NotNull] ResultTableDefinition rtd, [NotNull] HouseholdKey key, ExpectedResultCount expectedResult);
        IEnumerable<T> ReadFromJsonAsEnumerable<T>([NotNull] ResultTableDefinition rtd, [NotNull] HouseholdKey key);
        void SaveDictionaryToDatabaseNewConnection([NotNull] Dictionary<string, object> values, [NotNull] string tableName, [NotNull] HouseholdKey householdKey);
        void SaveDictionaryToDatabaseNewConnection([ItemNotNull, NotNull] List<Dictionary<string, object>> values, [NotNull] string tableName, [NotNull] HouseholdKey householdKey);
        void SaveResultEntry([NotNull] SaveableEntry entry);
    }

    public static class ResultLoggingFactory
    {
        public static IResultLoggingService CreateResultLoggingService(string basePath)
        {
            return new SqlResultLoggingService(basePath);
        }
    }
}