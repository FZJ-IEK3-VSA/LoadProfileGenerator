using Automation;

namespace MassSimulation
{
    internal class MassSimTargetReference
    {
        public readonly string Id;
        public readonly JsonReference Reference;

        public MassSimTargetReference(string id, JsonReference reference)
        {
            Id = id;
            this.Reference = reference;
        }
    }
}
