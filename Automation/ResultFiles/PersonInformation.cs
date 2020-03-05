using JetBrains.Annotations;

namespace Automation.ResultFiles {
    public class PersonInformation {
        //TODO: remove this
        // needed for xml deserialize
        [UsedImplicitly]
        // ReSharper disable once NotNullMemberIsNotInitialized
        public PersonInformation()
        {
        }

        public PersonInformation([NotNull] string name, [NotNull] string guid, [NotNull] string traitTag)
        {
            Name = name;

            Guid = guid;
            TraitTag = traitTag;
        }

        [NotNull]
        [UsedImplicitly]
        public string Guid { get; set; }

        // needed for xml deserialize
        [UsedImplicitly]
        [NotNull]
        public string Name { get; set; }
        [UsedImplicitly]
        [NotNull]
        public string TraitTag { get; set; }
    }
}