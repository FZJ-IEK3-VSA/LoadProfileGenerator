using JetBrains.Annotations;

namespace Automation {
    public interface IGuidObject
    {
        [NotNull]
        StrGuid Guid { get; }
    }
}