using Automation.ResultFiles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Text.Json;

namespace Common.SQLResultLogging
{
    public class JsonResultLoggingService(string basePath) : IResultLoggingService
    {
        private readonly string basePath = basePath;

        public Dictionary<HouseholdKey, FileEntry> FilenameByHouseholdKey { get; } //  TODO: dummy for interface; remove

        /// <summary>
        /// Returns the directory in which the data for the specified household key is stored.
        /// </summary>
        /// <param name="key">the household key for which to get the directory</param>
        /// <returns>the directory path</returns>
        private string GetDirectory(HouseholdKey key) => Path.Combine(basePath, $"Results.{key}");

        private string GetFilePath(HouseholdKey key, string tableName) => Path.Combine(GetDirectory(key), tableName + ".json");

        private string InitJsonFile(HouseholdKey key, string tableName)
        {
            string filepath = GetFilePath(key, tableName);
            if (File.Exists(filepath))
            {
                // file exists already
                return filepath;
            }

            // file does not exist yet, create and initialize it
            FileInfo fileInfo = new(filepath);
            var directory = fileInfo.Directory;
            bool dirExistedAlready = directory.Exists;
            directory.Create();

            // init the file by writing the JSON array start
            CreateEmptyDataFile(filepath);

            // the file needs to be created before registering a new directory to avoid recursion with duplicate file creation
            if (!dirExistedAlready)
            {
                // register the directory for the specified HouseholdKey in the list of databases
                AddDBListEntryForDirectory(key, directory.FullName);
            }

            SqlResultLoggingService.AddResultFileEntry(this, key, fileInfo, tableName);
            return filepath;
        }

        private static void CreateEmptyDataFile(string filepath)
        {
            File.WriteAllText(filepath, "[]");
        }

        private void AddDBListEntryForDirectory(HouseholdKey key, string directoryPath)
        {
            // save the path to the new database directory in the General database
            var row = new Dictionary<string, object>
            {
                ["HouseholdKey"] = key.Key,
                ["Filename"] = directoryPath
            };
            SaveToFile([new DatabaseEntry(directoryPath, key)], Constants.DatabaseListTableName, Constants.GeneralHouseholdKey);
        }

        /// <summary>
        /// Serializes the passed items to JSON and combines them with newlines and commas in
        /// a stringbuilder. This stringbuilder should be directly passed to the filestream
        /// instead of converting to a string to avoid exceeding the maximum string length.
        /// </summary>
        /// <typeparam name="T">item type</typeparam>
        /// <param name="data">the data to serialize</param>
        /// <returns>a stringbuilder with all serialized items concatenated</returns>
        private static StringBuilder CreateJsonEntries<T>(IEnumerable<T> data)
        {
            //var jsonString = JsonSerializer.Serialize(data);
            var jsonStrings = data.Select(d => "\n" + JsonConvert.SerializeObject(d));
            StringBuilder sb = new();
            sb.AppendJoin(',', jsonStrings);
            return sb;
        }

        private static void AddToJsonFile<T>(string filepath, IEnumerable<T> data)
        {
            using var stream = new FileStream(filepath, FileMode.OpenOrCreate);

            // check if the last character is a closing bracket
            stream.Seek(-1, SeekOrigin.End);
            if ((char)stream.ReadByte() != ']')
                throw new LPGException($"Unexpected JSON format in result file {filepath}");

            // check the character before that to determine whether a comma at the start is needed
            stream.Seek(-2, SeekOrigin.End);
            bool firstValue = (char)stream.ReadByte() == '[';

            // serialize the items and concatenate them to valid JSON
            var stringBuilder = CreateJsonEntries(data);
            if (firstValue)
                stringBuilder.Insert(0, ",");
            stringBuilder.Append(']');

            // apppend the new entries, overwriting the previous closing bracket
            using var writer = new StreamWriter(stream);
            writer.Write(stringBuilder);
        }

        private IEnumerable<T> LoadItemsFromFile<T>(HouseholdKey key, string tableName)
        {
            string filepath = GetFilePath(key, tableName);
            string jsonString = File.ReadAllText(filepath);

            //return AutomationUtili.ParseJsonFile<IEnumerable<T>>(filepath);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
        }

        public IEnumerable<T> ReadFromJsonAsEnumerable<T>(ResultTableDefinition rtd, HouseholdKey key)
        {
            // load all items
            var items = LoadItemsFromFile<Dictionary<string, object>>(key, rtd.TableName);
            // deserialize the JSON strings contained in the Json column
            //return items.Select(item => ParseJsonColumn<T>(item[Constants.JsonColumnName]));
            return items.Select(item => JsonConvert.DeserializeObject<T>((string)item[Constants.JsonColumnName]));
        }

        public List<T> ReadFromJson<T>(ResultTableDefinition rtd, HouseholdKey key, ExpectedResultCount expectedResult)
        {
            List<T> results = [.. ReadFromJsonAsEnumerable<T>(rtd, key)];
            SqlResultLoggingService.CheckResultCount(expectedResult, results.Count);
            return results;
        }

        ///// <summary>
        ///// Parse the JSON column of of a data entry with System.Text.Json.
        ///// </summary>
        ///// <typeparam name="T">type of the object to parse</typeparam>
        ///// <param name="content">JsonElement (parsed from JSON without a target type)</param>
        ///// <returns>the parsed object</returns>
        //private static T ParseJsonColumn<T>(object content)
        //{
        //    JsonElement element = (JsonElement)content;
        //    string jsonString = element.GetString();
        //    return AutomationUtili.ParseJsonString<T>(jsonString);
        //}

        public void SaveToFile<T>(IEnumerable<T> values, string tableName, HouseholdKey householdKey)
        {
            // create the file if it does not exist yet
            string filepath = InitJsonFile(householdKey, tableName);

            // add the data to the file
            AddToJsonFile(filepath, values);
        }

        public void SaveResultEntry(SaveableEntry entry)
        {
            if (!CheckifTableExists(entry.ResultTableDefinition.TableName, entry.HouseholdKey))
            {
                // add the new table to the list of tables
                string filepath = InitJsonFile(entry.HouseholdKey, Constants.TableDescriptionTableName);
                AddToJsonFile(filepath, [entry.ResultTableDefinition]);
            }
            SaveToFile(entry.RowEntries, entry.ResultTableDefinition.TableName, entry.HouseholdKey);
        }

        public void SaveDictionaryToDatabaseNewConnection(List<Dictionary<string, object>> values, string tableName, HouseholdKey householdKey)
            => SaveToFile(values, tableName, householdKey);

        public void SaveDictionaryToDatabaseNewConnection(Dictionary<string, object> values, string tableName, HouseholdKey householdKey)
            => SaveDictionaryToDatabaseNewConnection([values], tableName, householdKey);

        public bool CheckifTableExists(string tableName, HouseholdKey key)
        {
            return File.Exists(GetFilePath(key, tableName));
        }

        public List<DatabaseEntry> LoadDatabases()
        {
            return [.. LoadItemsFromFile<DatabaseEntry>(Constants.GeneralHouseholdKey, Constants.DatabaseListTableName)];
        }

        public List<ResultTableDefinition> LoadTables(HouseholdKey dbKey)
        {
            return [.. LoadItemsFromFile<ResultTableDefinition>(dbKey, Constants.TableDescriptionTableName)];
        }


        public void DeleteEntries(IEnumerable<Dictionary<string, object>> toDelete, string tableName, HouseholdKey householdKey)
        {
            // load all entries from the file to delete items from
            var items = LoadItemsFromFile<Dictionary<string, object>>(householdKey, tableName);
            // build a dict mapping JSON strings to deserialized items
            var itemDict = items.ToDictionary(JsonConvert.SerializeObject);
            // serialize the entries to delete for comparison
            var jsonStringsToDelete = toDelete.Select(JsonConvert.SerializeObject).ToHashSet();

            // get all entries whose JSON strings are not in the set of entries to delete
            var keptItems = itemDict.Where(kvp => !jsonStringsToDelete.Contains(kvp.Key)).Select(kvp => kvp.Value);

            // clear the file but keep it to avoid duplicat result file entries
            var filepath = GetFilePath(householdKey, tableName);
            CreateEmptyDataFile(filepath);

            // save the remaining entries
            SaveToFile(keptItems, tableName, householdKey);
        }

        public void MakeTableForListOfFields(List<FieldDefinition> fields, HouseholdKey key, string tableName) => InitJsonFile(key, tableName);
    }
}
