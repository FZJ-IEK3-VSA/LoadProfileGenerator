using Automation.ResultFiles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.SQLResultLogging
{
    /// <summary>
    /// An alternative result logging service that stores all data in directories of JSON files. For every household, one
    /// directory of JSON files is created, plus one directory for the general house data.
    /// The JSON files are continually updated, so that they always contain valid JSON, including the final closing bracket.
    /// The output files of this result logger are slightly larger than those of the <see cref="SqlResultLoggingService"/>, but
    /// this service does not have any issues with database locking in case of large-scale parallelization (e.g., for the city
    /// simulation).
    /// </summary>
    /// <param name="basePath">the path of the base directory to log results to</param>
    public class JsonResultLoggingService(string basePath) : IResultLoggingService
    {
        /// <summary>
        /// base directory for storing results
        /// </summary>
        private readonly string basePath = basePath;

        /// <summary>
        /// Returns the directory in which the data for the specified household key is stored.
        /// </summary>
        /// <param name="key">the household key for which to get the directory</param>
        /// <returns>the directory path</returns>
        private string GetDirectory(HouseholdKey key) => Path.Combine(basePath, $"Results.{key}");

        /// <summary>
        /// Returns the path of the JSON file containing the specified data (corresponding to a table in the SQLResultLoggingService).
        /// </summary>
        /// <param name="key">the household key for which to get the file</param>
        /// <param name="tableName">the table name</param>
        /// <returns>the path of the JSON file</returns>
        private string GetFilePath(HouseholdKey key, string tableName) => Path.Combine(GetDirectory(key), tableName + ".json");

        /// <summary>
        /// Checks whether a JSON file already exists, and if not initializes it so data can be added.
        /// </summary>
        /// <param name="key">the household key</param>
        /// <param name="tableName">the table name</param>
        /// <returns>the path of the initialized JSON file</returns>
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

            // init the file by writing an empty JSON array
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

        /// <summary>
        /// Writes an empty JSON array to the file, creating it if it does not exist.
        /// </summary>
        /// <param name="filepath">the path of the file</param>
        private static void CreateEmptyDataFile(string filepath)
        {
            File.WriteAllText(filepath, "[]");
        }

        /// <summary>
        /// Adds the specified directory to the list of databases.
        /// In the JsonResultLoggingService, a directory of JSON files represents a database.
        /// </summary>
        /// <param name="key">the household key</param>
        /// <param name="directoryPath">the path of the directory</param>
        private void AddDBListEntryForDirectory(HouseholdKey key, string directoryPath)
        {
            // save the path to the new database directory in the General database
            var entry = new DatabaseEntry(directoryPath, key);
            SaveToFile([entry], Constants.DatabaseListTableName, Constants.GeneralHouseholdKey);
        }

        /// <summary>
        /// Limit for the string builder size before it is written to file
        /// to avoid exceeding the maximum string length.
        /// </summary>
        private const int StringBufferLimit = 100_000_000;

        /// <summary>
        /// Serializes the data and appends the JSON string to the stream, adding commas and a closing bracket
        /// so the file ends up containing one valid JSON array of objects.
        /// </summary>
        /// <typeparam name="T">the type of object to write to the stream</typeparam>
        /// <param name="stream">the stream to write the serialized objects to</param>
        /// <param name="data">the objects to serialize</param>
        /// <param name="addInitialComma">if true, adds an initial comma before the first value; this is necessary
        /// if the file already contains data and not just an empty array</param>
        private static void WriteEntriesToFile<T>(Stream stream, IEnumerable<T> data, bool addInitialComma)
        {
            using var writer = new StreamWriter(stream);
            var builder = new StringBuilder();
            if (addInitialComma)
                builder.Append(',');

            foreach (var item in data)
            {
                // serialize one item at a time
                string jsonString = "\n" + JsonConvert.SerializeObject(item) + ",";
                if (builder.Length + jsonString.Length > StringBufferLimit)
                {
                    // adding the new item to the buffer would exceed the limit, so write the buffer to file before
                    writer.Write(builder);
                    builder.Clear();
                }
                builder.Append(jsonString);
            }

            // remove the trailing comma
            if (data.Any())
                builder.Length--;

            // add the final closing bracket and write the remaining buffer to file
            builder.Append(']');
            writer.Write(builder);
        }

        /// <summary>
        /// Adds objects to a JSON file. Handles commas and brackets to always generate valid JSON.
        /// </summary>
        /// <typeparam name="T">the type of object to save as JSON</typeparam>
        /// <param name="filepath">the path of the file to add data to</param>
        /// <param name="data">the data objects to append to the file</param>
        /// <exception cref="LPGException">if a JSON file has unexpected content; this should never
        /// happen and indicates an internal error in this class</exception>
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

            WriteEntriesToFile(stream, data, !firstValue);
        }

        /// <summary>
        /// Loads all objects from the specified table, for the specified household key.
        /// Only works with objects that were directly serialized to JSON, objects saved via
        /// SaveableEntry objects cannot be loaded as of now.
        /// </summary>
        /// <typeparam name="T">the type of the objects to load</typeparam>
        /// <param name="key">the household key</param>
        /// <param name="tableName">the table from which objects will be loaded</param>
        /// <returns>the loaded and deserialized objects</returns>
        private IEnumerable<T> LoadItemsFromFile<T>(HouseholdKey key, string tableName)
        {
            string filepath = GetFilePath(key, tableName);
            return StreamItemsFromJsonArrayFile<T>(filepath);
        }

        /// <summary>
        /// Opens a result JSON file and positions the reader on the opening bracket of the top-level array.
        /// </summary>
        /// <param name="filepath">the path of the JSON file</param>
        /// <returns>the JSON reader, positioned on the StartArray token; must be disposed by the caller</returns>
        /// <exception cref="LPGException">if the file does not start with a JSON array</exception>
        private static JsonTextReader OpenJsonArrayReader(string filepath)
        {
            var filereader = new StreamReader(filepath);
            var jsonreader = new JsonTextReader(filereader)
            {
                // keep date-like strings as strings
                DateParseHandling = DateParseHandling.None
            };
            if (!jsonreader.Read() || jsonreader.TokenType != JsonToken.StartArray)
            {
                filereader.Dispose();
                throw new LPGException($"Unexpected JSON format in result file {filepath}: expected an array");
            }
            return jsonreader;
        }

        /// <summary>
        /// Streams all objects from a JSON file containing a top-level array, deserializing one object at a time.
        /// Only one object is held in memory at a time to reduce RAM usage.
        /// </summary>
        /// <typeparam name="T">the type of the objects to deserialize</typeparam>
        /// <param name="filepath">the path of the JSON file</param>
        /// <returns>the deserialized objects, one at a time</returns>
        /// <exception cref="LPGException">if an element of the array could not be deserialized</exception>
        private static IEnumerable<T> StreamItemsFromJsonArrayFile<T>(string filepath)
        {
            using var jsonreader = OpenJsonArrayReader(filepath);
            var serializer = new JsonSerializer();
            while (jsonreader.Read() && jsonreader.TokenType != JsonToken.EndArray)
            {
                var item = serializer.Deserialize<T>(jsonreader) ?? throw new LPGException($"Result file {filepath} contains an invalid entry.");
                yield return item;
            }
        }

        /// <summary>
        /// Directly streams the content of the Json column of every row in a result table file, without
        /// loading the other columns or more than one row at a time.
        /// This avoids creating a dictionary for every row and is therefore more memory-efficient than StreamItemsFromFile.
        /// </summary>
        /// <param name="filepath">the path of the JSON file</param>
        /// <returns>the JSON strings stored in the Json column, one per row</returns>
        /// <exception cref="LPGException">if a row is malformed or has no Json column</exception>
        private static IEnumerable<string> StreamJsonColumnFromFile(string filepath)
        {
            using var jsonreader = OpenJsonArrayReader(filepath);
            // read the rows (i.e. JSON array elements) one at a time
            while (jsonreader.Read() && jsonreader.TokenType != JsonToken.EndArray)
            {
                if (jsonreader.TokenType != JsonToken.StartObject)
                {
                    throw new LPGException($"Unexpected JSON format in result file {filepath}: expected an object");
                }

                // read all columns (i.e. properties) of the row, looking for the "Json" column
                string? json = null;
                while (jsonreader.Read() && jsonreader.TokenType != JsonToken.EndObject)
                {
                    if (jsonreader.TokenType != JsonToken.PropertyName)
                    {
                        throw new LPGException($"Unexpected JSON format in result file {filepath}: expected a property name");
                    }

                    if ((string?)jsonreader.Value == Constants.JsonColumnName)
                    {
                        // the property value follows the property name token
                        jsonreader.Read();
                        json = jsonreader.Value as string;
                    }
                    else
                    {
                        // skip the value of any other column
                        jsonreader.Skip();
                    }
                }

                yield return json ?? throw new LPGException($"Row without {Constants.JsonColumnName} column in result file {filepath}");
            }
        }

        public IEnumerable<T> ReadFromJsonAsEnumerable<T>(ResultTableDefinition rtd, HouseholdKey key)
        {
            string filepath = GetFilePath(key, rtd.TableName);
            if (!File.Exists(filepath))
            {
                // no entry was ever logged to this file - return an empty collection
                return [];
            }

            // stream the items of the JSON array and deserialize the JSON strings contained in the Json column one at a time
            return StreamJsonColumnFromFile(filepath)
                .Select(json => JsonConvert.DeserializeObject<T>(json) ?? throw new LPGException($"Invalid entry in result file {filepath}"));
        }

        public List<T> ReadFromJson<T>(ResultTableDefinition rtd, HouseholdKey key, ExpectedResultCount expectedResult)
        {
            List<T> results = [.. ReadFromJsonAsEnumerable<T>(rtd, key)];
            SqlResultLoggingService.CheckResultCount(expectedResult, results.Count);
            return results;
        }

        ///// <summary>
        ///// Parse the JSON column of a data entry with System.Text.Json.
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

            // save the actual data
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
            if (!CheckifTableExists(Constants.TableDescriptionTableName, dbKey))
            {
                // there are no tables for this household key - return an empty collection
                return [];
            }
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
