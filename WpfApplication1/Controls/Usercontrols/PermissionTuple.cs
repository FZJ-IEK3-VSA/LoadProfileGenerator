using Database.Helpers;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Controls.Usercontrols
{
    public class PermissionTuple
    {
        public PermissionTuple(PermissionMode permissionMode, [NotNull] string name)
        {
            PermissionMode = permissionMode;
            Name = name;
        }

        [NotNull]
        [UsedImplicitly]
        public string Name { get; set; }

        [UsedImplicitly]
        public PermissionMode PermissionMode { get; set; }

        public override string ToString() => Name;
    }
}