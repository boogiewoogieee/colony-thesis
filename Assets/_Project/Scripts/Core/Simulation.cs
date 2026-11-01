using System;
using System.Collections.Generic;

namespace Colony.Core
{
    /// <summary>Information passed to every system on every tick.</summary>
    public readonly struct TickContext
    {
        public TickContext(long tick, float deltaSeconds, double elapsedSeconds, int day, double timeOfDay01)
        {
            Tick = tick;
            DeltaSeconds = deltaSeconds;
            ElapsedSeconds = elapsedSeconds;
            Day = day;
            TimeOfDay01 = timeOfDay01;
        }

        /// <summary>Index of this tick, starting at 1 for the first tick simulated.</summary>
        public long Tick { get; }

        /// <summary>Simulated seconds covered by this tick (the fixed tick length).</summary>
        public float DeltaSeconds { get; }

        /// <summary>Total simulated play time at the end of this tick.</summary>
        public double ElapsedSeconds { get; }

        public int Day { get; }

        public double TimeOfDay01 { get; }
    }

    /// <summary>A piece of game logic advanced by the simulation at a fixed rate.</summary>
    public interface ISimulationSystem
    {
        void Tick(in TickContext context);
    }

    /// <summary>
    /// Owns the clock, calendar and event bus, and ticks every registered system in
    /// registration order. Plain C#: the scene only calls <see cref="Update"/> each frame.
    /// </summary>
    public sealed class Simulation
    {
        /// <summary>Default tick length: 10 ticks per simulated second.</summary>
        public const float DefaultTickSeconds = 0.1f;

        /// <summary>
        /// Default day length: 48 minutes of play. Follows from grass growing 20% per day
        /// and reaching full size in 4 hours (GDD section 11); still to be confirmed.
        /// </summary>
        public const double DefaultDayLengthSeconds = 48d * 60d;

        private readonly List<ISimulationSystem> _systems = new List<ISimulationSystem>();

        public Simulation(float tickSeconds = DefaultTickSeconds, double dayLengthSeconds = DefaultDayLengthSeconds)
        {
            Clock = new SimulationClock(tickSeconds);
            Calendar = new GameCalendar(dayLengthSeconds);
            Events = new EventBus();
        }

        public SimulationClock Clock { get; }

        public GameCalendar Calendar { get; }

        public EventBus Events { get; }

        public IReadOnlyList<ISimulationSystem> Systems => _systems;

        /// <summary>Adds a system. Systems tick in the order they were added.</summary>
        public void AddSystem(ISimulationSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (_systems.Contains(system))
            {
                throw new InvalidOperationException($"{system.GetType().Name} is already registered.");
            }

            _systems.Add(system);
        }

        /// <summary>Call once per frame with real (unscaled) frame time. Returns the ticks run.</summary>
        public int Update(float realDeltaSeconds)
        {
            var ticks = Clock.Advance(realDeltaSeconds);
            RunTicks(Clock.TickCount - ticks, ticks);
            return ticks;
        }

        /// <summary>Runs an exact number of ticks regardless of pause and speed. For tests and tools.</summary>
        public void Step(int ticks)
        {
            if (ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticks), ticks, "Tick count cannot be negative.");
            }

            var firstTick = Clock.TickCount;
            Clock.Restore(firstTick + ticks);
            RunTicks(firstTick, ticks);
        }

        /// <summary>Restores clock and calendar from a save. Does not run any ticks.</summary>
        public void Restore(long tickCount)
        {
            Clock.Restore(tickCount);
            Calendar.Restore(Clock.ElapsedSeconds);
        }

        private void RunTicks(long ticksBefore, int count)
        {
            for (var i = 1; i <= count; i++)
            {
                var tick = ticksBefore + i;
                var elapsed = tick * Clock.TickSecondsPrecise;
                Calendar.AdvanceTo(elapsed);

                var context = new TickContext(tick, Clock.TickSeconds, elapsed, Calendar.Day, Calendar.TimeOfDay01);
                for (var s = 0; s < _systems.Count; s++)
                {
                    _systems[s].Tick(in context);
                }
            }
        }
    }
}
