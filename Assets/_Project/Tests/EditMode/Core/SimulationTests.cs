using System.Collections.Generic;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class SimulationTests
    {
        private sealed class RecordingSystem : ISimulationSystem
        {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingSystem(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public List<TickContext> Contexts { get; } = new List<TickContext>();

            public void Tick(in TickContext context)
            {
                Contexts.Add(context);
                _log?.Add(_name);
            }
        }

        [Test]
        public void Update_RunsSystemsOncePerTick()
        {
            var simulation = new Simulation(0.1f, 100d);
            var system = new RecordingSystem("a", null);
            simulation.AddSystem(system);

            var ticks = simulation.Update(0.25f);

            Assert.That(ticks, Is.EqualTo(2));
            Assert.That(system.Contexts.Count, Is.EqualTo(2));
            Assert.That(system.Contexts[0].Tick, Is.EqualTo(1));
            Assert.That(system.Contexts[1].Tick, Is.EqualTo(2));
            Assert.That(system.Contexts[1].ElapsedSeconds, Is.EqualTo(0.2d).Within(1e-6));
        }

        [Test]
        public void Update_TicksSystemsInRegistrationOrder()
        {
            var log = new List<string>();
            var simulation = new Simulation(0.1f, 100d);
            simulation.AddSystem(new RecordingSystem("first", log));
            simulation.AddSystem(new RecordingSystem("second", log));

            simulation.Step(1);

            Assert.That(log, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Step_AcrossDayBoundary_UpdatesCalendarBeforeSystemsRun()
        {
            var simulation = new Simulation(1f, 10d);
            var system = new RecordingSystem("a", null);
            simulation.AddSystem(system);
            var days = new List<int>();
            simulation.Calendar.DayStarted += days.Add;

            simulation.Step(10);

            Assert.That(days, Is.EqualTo(new[] { 1 }));
            Assert.That(system.Contexts[9].Day, Is.EqualTo(1));
            Assert.That(system.Contexts[8].Day, Is.EqualTo(0));
        }

        [Test]
        public void Step_IgnoresPause()
        {
            var simulation = new Simulation(0.1f, 100d);
            simulation.Clock.IsPaused = true;

            simulation.Step(5);

            Assert.That(simulation.Clock.TickCount, Is.EqualTo(5));
        }

        [Test]
        public void Restore_SetsClockAndCalendarWithoutTicking()
        {
            var simulation = new Simulation(1f, 10d);
            var system = new RecordingSystem("a", null);
            simulation.AddSystem(system);

            simulation.Restore(35);

            Assert.That(simulation.Clock.TickCount, Is.EqualTo(35));
            Assert.That(simulation.Calendar.Day, Is.EqualTo(3));
            Assert.That(system.Contexts, Is.Empty);
        }

        [Test]
        public void AddSystem_Twice_Throws()
        {
            var simulation = new Simulation();
            var system = new RecordingSystem("a", null);
            simulation.AddSystem(system);

            Assert.Throws<System.InvalidOperationException>(() => simulation.AddSystem(system));
        }
    }
}
