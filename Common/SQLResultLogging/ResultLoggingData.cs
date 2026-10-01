using Automation.ResultFiles;

namespace Common.SQLResultLogging
{
    /// <summary>
    /// Identifies a database created by the result logging service. This can be an SQL database file, or
    /// a directory of JSON files.
    /// </summary>
    /// <param name="Filename">the path of the file or directory</param>
    /// <param name="Key">the corresponding household key of the database</param>
    public record DatabaseEntry(string Filename, HouseholdKey Key);

    /// <summary>
    /// Defines a column for a table when using the SQLResultLoggingService.
    /// </summary>
    /// <param name="Name">column name</param>
    /// <param name="Type">SQL data type of the column</param>
    public record FieldDefinition(string Name, string Type);

    public record FileEntry(string Filename, bool DescriptionTableWritten);
}
