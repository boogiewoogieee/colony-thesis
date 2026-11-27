using System;

namespace Colony.Core
{
    /// <summary>
    /// Turns the varying length of frames into a whole number of fixed simulation ticks.
    /// The simulation only ever advances in steps of <see cref="TickSeconds"/>, so the
    /// results do not depend on the frame rate.
    /// </summary>
    public sealed class SimulationClock
    {
        /// <summary>
        /// Longest frame that is counted. After a long hitch the game continues from where
        /// it was instead of running a burst of catch-up ticks.
        /// </summary>
        public const double MAX_FRAME_SECONDS = 0.25;

        private const double NORMAL_SPEED = 1.0;

        // Time that has passed but does not yet fill a whole tick, as a fraction of a tick.
        // Kept in ticks rather than seconds so that taking whole ticks out of it is exact.
        private double _unspentTicks;

        public SimulationClock(double tickSeconds)
        {
            if (!IsPositiveFiniteNumber(tickSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tickSeconds),
                    tickSeconds,
                    "The tick length must be a positive number of seconds.");
            }

            TickSeconds = tickSeconds;
            Speed = NORMAL_SPEED;
        }

        /// <summary>Simulated seconds covered by one tick.</summary>
        public double TickSeconds { get; }

        /// <summary>Number of ticks simulated since the start of the game.</summary>
        public long TickCount { get; private set; }

        /// <summary>Simulated play time since the start of the game.</summary>
        public double ElapsedSeconds => TickCount * TickSeconds;

        /// <summary>How many times faster than normal the game runs. 1 is normal speed.</summary>
        public double Speed { get; private set; }

        public bool IsPaused { get; private set; }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void SetSpeed(double speed)
        {
            if (!IsPositiveFiniteNumber(speed))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(speed),
                    speed,
                    "The speed must be a positive number. Use Pause to stop the game.");
            }

            Speed = speed;
        }

        /// <summary>
        /// Adds the time of one frame and counts the ticks that became due.
        /// Does nothing while paused.
        /// </summary>
        internal void Advance(double frameSeconds)
        {
            if (!IsZeroOrPositiveFiniteNumber(frameSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(frameSeconds),
                    frameSeconds,
                    "The frame time must be zero or a positive number of seconds.");
            }

            if (IsPaused)
            {
                return;
            }

            double countedSeconds = Math.Min(frameSeconds, MAX_FRAME_SECONDS) * Speed;
            _unspentTicks += countedSeconds / TickSeconds;

            double dueTicks = Math.Floor(_unspentTicks);
            _unspentTicks -= dueTicks;
            TickCount += (long)dueTicks;
        }

        /// <summary>Counts an exact number of ticks, whatever the pause state and speed.</summary>
        internal void AddTicks(int tickCount)
        {
            if (tickCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tickCount),
                    tickCount,
                    "The number of ticks to add cannot be negative.");
            }

            TickCount += tickCount;
        }

        /// <summary>Sets the clock to a saved tick count and forgets any partial tick.</summary>
        internal void Restore(long tickCount)
        {
            if (tickCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tickCount),
                    tickCount,
                    "The tick count cannot be negative.");
            }

            TickCount = tickCount;
            _unspentTicks = 0.0;
        }

        private static bool IsPositiveFiniteNumber(double value)
        {
            return value > 0.0 && !double.IsInfinity(value);
        }

        private static bool IsZeroOrPositiveFiniteNumber(double value)
        {
            return value >= 0.0 && !double.IsInfinity(value);
        }
    }
}
