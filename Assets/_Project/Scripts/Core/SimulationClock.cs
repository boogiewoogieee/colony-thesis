using System;

namespace Colony.Core
{
    /// <summary>
    /// Turns variable frame time into a whole number of fixed simulation ticks.
    /// The simulation only ever advances in steps of <see cref="TickSeconds"/>,
    /// so results do not depend on frame rate.
    /// </summary>
    public sealed class SimulationClock
    {
        /// <summary>Longest real frame accepted, so a hitch never triggers a burst of catch-up ticks.</summary>
        public const float MaxFrameSeconds = 0.25f;

        // Tolerance, in ticks, for float rounding in frame times (e.g. 0.06f + 0.06f + 0.08f
        // must count as two 0.1 s ticks, not 1.9999999).
        private const double TickEpsilon = 1e-6;

        private readonly double _tickSeconds;
        private double _accumulatedSeconds;
        private float _speed = 1f;

        public SimulationClock(float tickSeconds)
        {
            if (tickSeconds <= 0f || float.IsNaN(tickSeconds) || float.IsInfinity(tickSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(tickSeconds), tickSeconds, "Tick length must be a positive number.");
            }

            TickSeconds = tickSeconds;

            // Convert via decimal so 0.1f becomes exactly 0.1 instead of 0.100000001490116.
            _tickSeconds = (double)(decimal)tickSeconds;
        }

        /// <summary>Simulated seconds per tick.</summary>
        public float TickSeconds { get; }

        /// <summary>Simulated seconds per tick, at double precision.</summary>
        public double TickSecondsPrecise => _tickSeconds;

        /// <summary>Game speed multiplier. 0 behaves like a pause.</summary>
        public float Speed
        {
            get => _speed;
            set
            {
                if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Speed must be zero or a positive number.");
                }

                _speed = value;
            }
        }

        public bool IsPaused { get; set; }

        /// <summary>Total ticks simulated since the start of the game.</summary>
        public long TickCount { get; private set; }

        /// <summary>Total simulated play time in seconds.</summary>
        public double ElapsedSeconds => TickCount * _tickSeconds;

        /// <summary>
        /// Adds real frame time and returns how many ticks the simulation must run this frame.
        /// </summary>
        public int Advance(float realDeltaSeconds)
        {
            if (IsPaused || _speed <= 0f || realDeltaSeconds <= 0f || float.IsNaN(realDeltaSeconds))
            {
                return 0;
            }

            _accumulatedSeconds += Math.Min(realDeltaSeconds, MaxFrameSeconds) * (double)_speed;

            var ticks = (int)Math.Floor(_accumulatedSeconds / _tickSeconds + TickEpsilon);
            if (ticks <= 0)
            {
                return 0;
            }

            _accumulatedSeconds = Math.Max(0d, _accumulatedSeconds - ticks * _tickSeconds);
            TickCount += ticks;
            return ticks;
        }

        /// <summary>Sets the clock to a saved tick count, dropping any partial tick.</summary>
        public void Restore(long tickCount)
        {
            if (tickCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tickCount), tickCount, "Tick count cannot be negative.");
            }

            TickCount = tickCount;
            _accumulatedSeconds = 0d;
        }
    }
}
