using System;

namespace Colony.Core
{
    /// <summary>
    /// Maps simulated play time onto in-game days. Day 0 is the first day.
    /// Raises <see cref="DayStarted"/> once for every day boundary crossed, in order.
    /// </summary>
    public sealed class GameCalendar
    {
        private double _elapsedSeconds;

        public GameCalendar(double dayLengthSeconds)
        {
            if (dayLengthSeconds <= 0d || double.IsNaN(dayLengthSeconds) || double.IsInfinity(dayLengthSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(dayLengthSeconds), dayLengthSeconds, "Day length must be a positive number.");
            }

            DayLengthSeconds = dayLengthSeconds;
        }

        /// <summary>Raised with the index of each new day as play time reaches it.</summary>
        public event Action<int> DayStarted;

        public double DayLengthSeconds { get; }

        /// <summary>Index of the current day, starting at 0.</summary>
        public int Day { get; private set; }

        /// <summary>Progress through the current day, from 0 (start) to just below 1 (end).</summary>
        public double TimeOfDay01 { get; private set; }

        /// <summary>
        /// Moves the calendar forward to the given play time. Time never goes backwards here;
        /// use <see cref="Restore"/> when loading a save.
        /// </summary>
        public void AdvanceTo(double elapsedSeconds)
        {
            if (elapsedSeconds < _elapsedSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), elapsedSeconds, "Play time cannot move backwards. Use Restore when loading.");
            }

            var previousDay = Day;
            Set(elapsedSeconds);

            for (var day = previousDay + 1; day <= Day; day++)
            {
                DayStarted?.Invoke(day);
            }
        }

        /// <summary>Sets the calendar to a saved play time without raising day events.</summary>
        public void Restore(double elapsedSeconds)
        {
            if (elapsedSeconds < 0d || double.IsNaN(elapsedSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), elapsedSeconds, "Play time cannot be negative.");
            }

            Set(elapsedSeconds);
        }

        private void Set(double elapsedSeconds)
        {
            _elapsedSeconds = elapsedSeconds;
            var days = elapsedSeconds / DayLengthSeconds;
            Day = (int)Math.Floor(days);
            TimeOfDay01 = days - Day;
        }
    }
}
