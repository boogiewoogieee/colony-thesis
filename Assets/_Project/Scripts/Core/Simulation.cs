using System;
using System.Collections.Generic;

namespace Colony.Core
{
    /// <summary>
    /// The game's rules in motion: owns the clock, the calendar and the event bus, and
    /// ticks every registered system in the order it was added. Plain C#; the scene only
    /// calls <see cref="Update"/> once per frame.
    /// </summary>
    public sealed class Simulation
    {
        /// <summary>Default tick length: 10 ticks per simulated second.</summary>
        public const double DEFAULT_TICK_SECONDS = 0.1;

        /// <summary>
        /// Default day length: 48 minutes of play. Follows from grass growing 20% per day
        /// and reaching full size in 4 hours (GDD section 11); still to be confirmed.
        /// </summary>
        public const double DEFAULT_DAY_LENGTH_SECONDS = 2880.0;

        private readonly List<ISimulationSystem> _systemList = new List<ISimulationSystem>();

        public Simulation(
            double tickSeconds = DEFAULT_TICK_SECONDS,
            double dayLengthSeconds = DEFAULT_DAY_LENGTH_SECONDS)
        {
            Clock = new SimulationClock(tickSeconds);
            Calendar = new GameCalendar(GetTicksPerDay(dayLengthSeconds, tickSeconds));
            EventBus = new EventBus();
        }

        public SimulationClock Clock { get; }

        public GameCalendar Calendar { get; }

        public EventBus EventBus { get; }

        /// <summary>Adds a system. Systems are ticked in the order they were added.</summary>
        public void AddSystem(ISimulationSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (_systemList.Contains(system))
            {
                throw new ArgumentException(
                    $"{system.GetType().Name} has already been added to the simulation.",
                    nameof(system));
            }

            _systemList.Add(system);
        }

        /// <summary>
        /// Call once per frame with the real time the frame took. Runs the ticks that
        /// became due, which may be none.
        /// </summary>
        public void Update(double frameSeconds)
        {
            long previousTickCount = Clock.TickCount;
            Clock.Advance(frameSeconds);
            RunTicksAfter(previousTickCount);
        }

        /// <summary>
        /// Runs an exact number of ticks, whatever the pause state and speed. For tests.
        /// </summary>
        public void Step(int tickCount)
        {
            long previousTickCount = Clock.TickCount;
            Clock.AddTicks(tickCount);
            RunTicksAfter(previousTickCount);
        }

        /// <summary>
        /// Sets the time to a saved tick count when a game is loaded. Runs no ticks and
        /// announces no days.
        /// </summary>
        public void Restore(long tickCount)
        {
            Clock.Restore(tickCount);
            Calendar.Restore(tickCount);
        }

        private static long GetTicksPerDay(double dayLengthSeconds, double tickSeconds)
        {
            if (!LastsAtLeastOneTick(dayLengthSeconds, tickSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dayLengthSeconds),
                    dayLengthSeconds,
                    $"A day must last at least one tick ({tickSeconds} s).");
            }

            return (long)Math.Round(dayLengthSeconds / tickSeconds);
        }

        private static bool LastsAtLeastOneTick(double dayLengthSeconds, double tickSeconds)
        {
            return dayLengthSeconds >= tickSeconds && !double.IsInfinity(dayLengthSeconds);
        }

        private void RunTicksAfter(long previousTickCount)
        {
            float deltaSeconds = (float)Clock.TickSeconds;
            for (long tickNumber = previousTickCount + 1; tickNumber <= Clock.TickCount; tickNumber++)
            {
                // The calendar moves first, so daily rules run before the systems on the
                // first tick of a new day.
                Calendar.AdvanceTo(tickNumber);
                TickContext context = new TickContext(
                    tickNumber,
                    deltaSeconds,
                    Calendar.DayIndex,
                    Calendar.DayProgress);
                TickSystems(in context);
            }
        }

        private void TickSystems(in TickContext context)
        {
            for (int i = 0; i < _systemList.Count; i++)
            {
                _systemList[i].Tick(in context);
            }
        }
    }
}
