using JetBrains.Annotations;

namespace Automation.ResultFiles {
    public class PersonInformation {
        //TODO: remove this
        // needed for xml deserialize
        [UsedImplicitly]
        // ReSharper disable once NotNullMemberIsNotInitialized
#pragma warning disable 8618
        public PersonInformation()
#pragma warning restore 8618
        {
        }

        public PersonInformation([JetBrains.Annotations.NotNull] string name, StrGuid guid, [JetBrains.Annotations.NotNull] string traitTag)
        {
            Name = name;

            Guid = guid;
            TraitTag = traitTag;
        }

        [UsedImplicitly]
        public StrGuid Guid { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string? Name { get; set; }
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string? TraitTag { get; set; }
    }
}