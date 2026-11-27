using System;

namespace Colony.Core
{
    /// <summary>
    /// Turns the number of simulated ticks into in-game days. Days are counted in whole
    /// ticks, so a day boundary always falls exactly on a tick.
    /// </summary>
    public sealed class GameCalendar
    {
        private const long MIN_TICKS_PER_DAY = 1;

        private long _tickCount;

        public GameCalendar(long ticksPerDay)
        {
            if (ticksPerDay < MIN_TICKS_PER_DAY)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ticksPerDay),
                    ticksPerDay,
                    "A day must last at least one tick.");
            }

            TicksPerDay = ticksPerDay;
        }

        /// <summary>
        /// Raised once for every day that begins, in order, with the index of the new day.
        /// Daily rules (upkeep, grass growth) subscribe here.
        /// </summary>
        public event Action<int> OnDayStarted;

        public long TicksPerDay { get; }

        /// <summary>Number of whole days that have passed: 0 during the first day.</summary>
        public int DayIndex => checked((int)(_tickCount / TicksPerDay));

        /// <summary>How far the current day has progressed: 0 at its start, just below 1 at its end.</summary>
        public double DayProgress => (double)(_tickCount % TicksPerDay) / TicksPerDay;

        /// <summary>
        /// Moves the calendar forward to the given tick count and announces every day that began.
        /// </summary>
        internal void AdvanceTo(long tickCount)
        {
            if (tickCount < _tickCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tickCount),
                    tickCount,
                    $"The calendar is at tick {_tickCount} and cannot move backwards. Use Restore when loading.");
            }

            int previousDayIndex = DayIndex;
            _tickCount = tickCount;

            for (int dayIndex = previousDayIndex + 1; dayIndex <= DayIndex; dayIndex++)
            {
                OnDayStarted?.Invoke(dayIndex);
            }
        }

        /// <summary>
        /// Sets the calendar to a saved tick count without announcing days: the days in a
        /// save file have already happened.
        /// </summary>
        internal void Restore(long tickCount)
        {
            if (tickCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tickCount),
                    tickCount,
                    "The tick count cannot be negative.");
            }

            _tickCount = tickCount;
        }
    }
}
