using System.Collections.Generic;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database
{
    public interface IHouseholdOrTrait
    {
        [JetBrains.Annotations.NotNull]
        string Name { get; }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        List<Affordance> CollectAffordances(bool onlyRelevant);
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        List<IAssignableDevice> CollectStandbyDevices();
    }
}