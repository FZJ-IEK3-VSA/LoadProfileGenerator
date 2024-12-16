namespace MassSimulation
{
    /// <summary>
    /// Wrapper exception that can be used to add relevant information about the part of the simulation that caused
    /// an exception.
    /// </summary>
    internal class CitySimWrapperException(Exception ex, int worker, string? targetId = null, string? when = null)
        : Exception(BuildExceptionMessage(worker, targetId, when), ex)
    {
        public int Worker { get; } = worker;
        public string? TargetId { get; } = targetId;
        public string? When { get; } = when;

        private static string BuildExceptionMessage(int worker, string? targetId, string? when)
        {
            var targetString = targetId is not null ? $" from target '{targetId}'" : "";
            var whenString = when is not null ? $" during {when}" : "";
            return $"Exception on worker {worker}{targetString}{whenString}.";
        }
    }
}
