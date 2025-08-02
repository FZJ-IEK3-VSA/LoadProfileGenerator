using Automation.ResultFiles;
using Newtonsoft.Json;

namespace Common.SQLResultLogging
{
    public record DatabaseEntry(string Filename, HouseholdKey Key)
    {
        [JsonConstructor]
        public DatabaseEntry(string Filename, string HouseholdKey) : this(Filename, new HouseholdKey(HouseholdKey))
        { }
    }

    public record DatabaseList(string HouseholdKey, long? ID, string Filename);

    public record FieldDefinition(string Name, string Type);

    public record FileEntry(string Filename, bool DescriptionTableWritten);
}
