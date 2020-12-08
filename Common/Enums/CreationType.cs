using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Enums {
    public enum CreationType {
        ManuallyCreated,
        TemplateCreated
    }

    public static class CreationTypeHelper {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [JetBrains.Annotations.NotNull]
        public static  Dictionary<CreationType, string> CreationTypeDictionary { get; }=
            new Dictionary<CreationType, string> {
                {CreationType.ManuallyCreated, "Manually created"},
                {CreationType.TemplateCreated, "Created by a template"}
            };
    }
}