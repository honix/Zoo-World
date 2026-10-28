namespace ZooWorld.Core.Time
{
    /// <summary>
    /// Simulation clock. The game reads Unity's fixed time step;
    /// tests and the headless simulation drive time manually.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>Duration of one simulation step, in seconds.</summary>
        float DeltaTime { get; }
    }
}
