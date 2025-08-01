using Automation.ResultFiles;

namespace Common.SQLResultLogging
{
    public record DatabaseEntry(string Filename, HouseholdKey Key);

    public record DatabaseList(string HouseholdKey, long? ID, string Filename);

    public record FieldDefinition(string Name, string Type);

    public record FileEntry(string Filename, bool DescriptionTableWritten);
}
