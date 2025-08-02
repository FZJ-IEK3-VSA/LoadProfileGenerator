using Automation.ResultFiles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (!directory.Exists)
            {
                // create the directory for the specified HouseholdKey first
                AddDirForHouseholdKey(key, directory.FullName);
            }

            // init the file by writing the JSON array start
            File.WriteAllText(filepath, "[]");

            SqlResultLoggingService.AddResultFileEntry(this, key, fileInfo);
            return filepath;
        }
        
        private void AddDirForHouseholdKey(HouseholdKey key, string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);

            // save the path to the new database directory in the General database
            var row = new Dictionary<string, object>
            {
                ["HouseholdKey"] = key.Key,
                ["Filename"] = directoryPath
            };
            SaveDictionaryToDatabaseNewConnection(row, Constants.DatabaseListTableName, Constants.GeneralHouseholdKey);
        }
        
        private void AddItemToJsonFile<T>(string filepath, T data)
        {
            using var stream = new FileStream(filepath, FileMode.OpenOrCreate);

            // check if the last character is a closing bracket
            stream.Seek(-1, SeekOrigin.End);
            if ((char)stream.ReadByte() != ']')
                throw new LPGException($"Unexpected JSON format in result file {filepath}");

            // check the character before that to determine whether a comma is needed
            stream.Seek(-2, SeekOrigin.End);
            bool firstValue = (char)stream.ReadByte() == '[';

            //var jsonString = JsonSerializer.Serialize(data);
            var jsonString = JsonConvert.SerializeObject(data);
            var jsonEntry = (firstValue ? "" : ",") + $"\n{jsonString}]";

            // apppend the new entry, overwriting the previous closing bracket
            using var writer = new StreamWriter(stream);
            writer.Write(jsonEntry);
        }
        
        private IEnumerable<T> LoadItemsFromFile<T>(HouseholdKey key, string tableName)
        {
            string filepath = GetFilePath(key, tableName);
            //return AutomationUtili.ParseJsonFile<IEnumerable<T>>(filepath);
            string jsonString = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
        }

        public IEnumerable<T> ReadFromJsonAsEnumerable<T>(ResultTableDefinition rtd, HouseholdKey key)
        {
            // load all items
            var items = LoadItemsFromFile<Dictionary<string, object>>(key, rtd.TableName);
            //return items.Select(item => ParseJsonColumn<T>(item[Constants.JsonColumnName]));
            return items.Select(item => JsonConvert.DeserializeObject<T>((string)item[Constants.JsonColumnName]));
        }

        public List<T> ReadFromJson<T>(ResultTableDefinition rtd, HouseholdKey key, ExpectedResultCount expectedResult)
        {
            List<T> results = [.. ReadFromJsonAsEnumerable<T>(rtd, key)];
            SqlResultLoggingService.CheckResultCount(expectedResult, results.Count);
            return results;
        }

        //private static T ParseJsonColumn<T>(object content)
        //{
        //    JsonElement element = (JsonElement)content;
        //    string jsonString = element.GetString();
        //    return AutomationUtili.ParseJsonString<T>(jsonString);
        //}

        public void SaveDictionaryToDatabaseNewConnection(List<Dictionary<string, object>> values, string tableName, HouseholdKey householdKey)
        {
            // create the file if it does not exist yet
            string filepath = InitJsonFile(householdKey, tableName);

            // adds the data to the file
            foreach (var dict in values)
            {
                AddItemToJsonFile(filepath, dict);
            }

            // TODO: close file if this was the final save
        }

        public void SaveDictionaryToDatabaseNewConnection(Dictionary<string, object> values, string tableName, HouseholdKey householdKey)
            => SaveDictionaryToDatabaseNewConnection([values], tableName, householdKey);

        public void SaveResultEntry(SaveableEntry entry)
        {
            if (!CheckifTableExits(entry.ResultTableDefinition.TableName, entry.HouseholdKey))
            {
                // add the new table to the list of tables
                string filepath = InitJsonFile(entry.HouseholdKey, Constants.TableDescriptionTableName);
                AddItemToJsonFile(filepath, entry.ResultTableDefinition);
            }
            SaveDictionaryToDatabaseNewConnection(entry.RowEntries, entry.ResultTableDefinition.TableName, entry.HouseholdKey);
        }

        public bool CheckifTableExits(string tableName) => CheckifTableExits(tableName, Constants.GeneralHouseholdKey);

        public bool CheckifTableExits(string tableName, HouseholdKey key)
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

        public void DeleteEntries(List<Dictionary<string, object>> entries, string tableName, HouseholdKey householdKey)
        {
            // TODO: difficult with JSON, need to iterate; only needed for DAT-files, maybe avoid this in general?
            //       Alternative: clear file and save content without deleted entries anew
            //throw new NotImplementedException();
        }
        public void DeleteEntry(Dictionary<string, object> entry, string tableName, HouseholdKey householdKey)
            => DeleteEntries([entry], tableName, householdKey);

        public void MakeTableForListOfFields(List<FieldDefinition> fields, HouseholdKey key, string tableName) => InitJsonFile(key, tableName);
    }
}
