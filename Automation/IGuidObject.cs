using JetBrains.Annotations;

namespace Automation {
    public interface IGuidObject
    {
        [NotNull]
        string Guid { get; }
    }
}