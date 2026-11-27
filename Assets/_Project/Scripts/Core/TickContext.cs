namespace Colony.Core
{
    /// <summary>
    /// What every system is told on every tick. The values describe the moment at the
    /// end of the tick.
    /// </summary>
    public readonly struct TickContext
    {
        public TickContext(long tickNumber, float deltaSeconds, int dayIndex, double dayProgress)
        {
            TickNumber = tickNumber;
            DeltaSeconds = deltaSeconds;
            DayIndex = dayIndex;
            DayProgress = dayProgress;
        }

        /// <summary>Number of this tick: 1 for the first tick of the game.</summary>
        public long TickNumber { get; }

        /// <summary>
        /// Simulated seconds this tick covers (always the fixed tick length). Multiply
        /// per-second rates by it.
        /// </summary>
        public float DeltaSeconds { get; }

        /// <summary>Number of whole days that have passed: 0 during the first day.</summary>
        public int DayIndex { get; }

        /// <summary>How far the current day has progressed: 0 at its start, just below 1 at its end.</summary>
        public double DayProgress { get; }
    }
}
