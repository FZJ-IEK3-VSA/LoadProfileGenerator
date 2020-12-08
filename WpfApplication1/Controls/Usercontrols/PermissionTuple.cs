using Database.Helpers;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Controls.Usercontrols
{
    public class PermissionTuple
    {
        public PermissionTuple(PermissionMode permissionMode, [JetBrains.Annotations.NotNull] string name)
        {
            PermissionMode = permissionMode;
            Name = name;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string Name { get; set; }

        [UsedImplicitly]
        public PermissionMode PermissionMode { get; set; }

        [NotNull]
        public override string ToString() => Name;
    }
}