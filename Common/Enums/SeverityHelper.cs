using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Enums
{
    public static class SeverityHelper
    {
        [JetBrains.Annotations.NotNull]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly IReadOnlyDictionary<Severity, string> SeverityShortName = new Dictionary<Severity, string>
        {
            {Severity.Error, "ERROR"},
            {Severity.ImportantInfo, "IINFO"},
            {Severity.Warning, "WARNG"},
            {Severity.Debug, "DEBUG"},
            {Severity.Information, "INFOR"}
        };
    }
}