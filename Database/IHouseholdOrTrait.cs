using System.Collections.Generic;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database
{
    public interface IHouseholdOrTrait
    {
        [NotNull]
        string Name { get; }
        [ItemNotNull]
        [NotNull]
        List<Affordance> CollectAffordances(bool onlyRelevant);
        [ItemNotNull]
        [NotNull]
        List<IAssignableDevice> CollectStandbyDevices();
    }
}