namespace MassSimulation
{
    /// <summary>
    /// Wrapper exception that can be used to add relevant information about the part of the simulation that caused
    /// an exception.
    /// </summary>
    internal class CitySimWrapperException(Exception ex, int worker, MassSimulationTarget? target = null, int timestep = -1)
        : Exception(BuildExceptionMessage(ex, worker, target, timestep), ex)
    {
        public int Worker { get; } = worker;
        public MassSimulationTarget? Target { get; } = target;
        public int Timestep { get; } = timestep;

        private static string BuildExceptionMessage(Exception ex, int worker, MassSimulationTarget? target, int timestep)
        {
            var targetString = target is not null ? $" from target '{target.Id}'" : "";
            var timestepString = timestep != -1 ? $" in timestep {timestep}" : "";
            return $"Exception on worker {worker}{targetString}{timestepString}: {ex.Message}";
        }
    }
}
