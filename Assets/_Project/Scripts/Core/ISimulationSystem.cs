namespace Colony.Core
{
    /// <summary>
    /// One piece of game logic that the simulation advances at a fixed rate.
    /// </summary>
    public interface ISimulationSystem
    {
        /// <summary>Advances this system by one tick.</summary>
        void Tick(in TickContext context);
    }
}
