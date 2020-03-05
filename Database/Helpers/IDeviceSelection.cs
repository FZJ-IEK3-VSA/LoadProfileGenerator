using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Helpers {
    public interface IDeviceSelection {
        [CanBeNull]
        IAssignableDevice Device { get; }
        [CanBeNull]
        VLoadType LoadType { get; }
        double Probability { get; }
        [CanBeNull]
        TimeBasedProfile TimeProfile { get; }
    }

    public interface IDeviceSelectionWithVariable : IDeviceSelection {
        [CanBeNull]
        Variable Variable { get; }
        VariableCondition VariableCondition { get; }
    }
}