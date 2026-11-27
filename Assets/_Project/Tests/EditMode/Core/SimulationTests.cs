using System;
using System.Collections.Generic;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class SimulationTests
    {
        private const double TICK_SECONDS = 0.1;
        private const double DAY_LENGTH_SECONDS = 1.0;
        private const int TICKS_PER_DAY = 10;
        private const double TOLERANCE = 0.000001;

        private Simulation _simulation;
        private List<string> _journal;

        [SetUp]
        public void CreateSimulation()
        {
            _simulation = new Simulation(TICK_SECONDS, DAY_LENGTH_SECONDS);
            _journal = new List<string>();
        }

        [Test]
        public void Constructor_WithDefaults_UsesATenthOfASecondTickAndA48MinuteDay()
        {
            Simulation defaultSimulation = new Simulation();

            Assert.That(defaultSimulation.Clock.TickSeconds, Is.EqualTo(0.1));
            Assert.That(defaultSimulation.Calendar.TicksPerDay, Is.EqualTo(28800));
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        [TestCase(0.05)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Constructor_WithADayShorterThanATickOrInvalid_Throws(double dayLengthSeconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Simulation(TICK_SECONDS, dayLengthSeconds));
        }

        [Test]
        public void Constructor_WithAnInvalidTickLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Simulation(tickSeconds: 0.0, DAY_LENGTH_SECONDS));
        }

        [Test]
        public void Constructor_WithADayThatIsNotAWholeNumberOfTicks_RoundsToTheNearestTick()
        {
            Simulation simulation = new Simulation(TICK_SECONDS, dayLengthSeconds: 1.04);

            Assert.That(simulation.Calendar.TicksPerDay, Is.EqualTo(10));
        }

        [Test]
        public void Step_WithTwoSystems_TicksThemInTheOrderTheyWereAdded()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));
            _simulation.AddSystem(new JournalingSystem("production", _journal));

            _simulation.Step(2);

            Assert.That(
                _journal,
                Is.EqualTo(new[] { "energy:1", "production:1", "energy:2", "production:2" }));
        }

        [Test]
        public void Step_ByZeroTicks_TicksNothing()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));

            _simulation.Step(0);

            Assert.That(_journal, Is.Empty);
        }

        [Test]
        public void Step_ByANegativeNumberOfTicks_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _simulation.Step(-1));
        }

        [Test]
        public void Step_WhilePaused_StillRunsTheTicks()
        {
            _simulation.Clock.Pause();

            _simulation.Step(3);

            Assert.That(_simulation.Clock.TickCount, Is.EqualTo(3));
        }

        [Test]
        public void Step_IntoANewDay_AnnouncesTheDayBeforeTheSystemsTick()
        {
            _simulation.Calendar.OnDayStarted += JournalDayStarted;
            _simulation.AddSystem(new JournalingSystem("energy", _journal));
            _simulation.Step(TICKS_PER_DAY - 1);
            _journal.Clear();

            _simulation.Step(1);

            Assert.That(_journal, Is.EqualTo(new[] { "day:1", "energy:10" }));
        }

        [Test]
        public void Step_PassesTheTickLengthAndCalendarToEachSystem()
        {
            ContextCapturingSystem capturingSystem = new ContextCapturingSystem();
            _simulation.AddSystem(capturingSystem);

            _simulation.Step(TICKS_PER_DAY + 5);

            TickContext lastContext = capturingSystem.LastContext;
            Assert.That(lastContext.TickNumber, Is.EqualTo(15));
            Assert.That(lastContext.DeltaSeconds, Is.EqualTo(0.1f));
            Assert.That(lastContext.DayIndex, Is.EqualTo(1));
            Assert.That(lastContext.DayProgress, Is.EqualTo(0.5).Within(TOLERANCE));
        }

        [Test]
        public void Update_ByOneTickOfFrameTime_TicksTheSystemsOnce()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));

            _simulation.Update(TICK_SECONDS);

            Assert.That(_journal, Is.EqualTo(new[] { "energy:1" }));
        }

        [Test]
        public void Update_ByLessThanATick_TicksNothing()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));

            _simulation.Update(TICK_SECONDS / 2.0);

            Assert.That(_journal, Is.Empty);
        }

        [Test]
        public void Update_WhilePaused_TicksNothing()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));
            _simulation.Clock.Pause();

            _simulation.Update(TICK_SECONDS);

            Assert.That(_journal, Is.Empty);
        }

        [Test]
        public void Update_AtDoubleSpeed_TicksTwiceAsOften()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));
            _simulation.Clock.SetSpeed(2.0);

            _simulation.Update(TICK_SECONDS);

            Assert.That(_journal, Is.EqualTo(new[] { "energy:1", "energy:2" }));
        }

        [Test]
        public void Update_WithTheSameTotalTimeInDifferentFrames_RunsTheSameNumberOfTicks()
        {
            Simulation simulationWithLongFrames = new Simulation(TICK_SECONDS, DAY_LENGTH_SECONDS);
            Simulation simulationWithShortFrames = new Simulation(TICK_SECONDS, DAY_LENGTH_SECONDS);

            for (int i = 0; i < 4; i++)
            {
                simulationWithLongFrames.Update(0.25);
            }

            for (int i = 0; i < 40; i++)
            {
                simulationWithShortFrames.Update(0.025);
            }

            Assert.That(simulationWithLongFrames.Clock.TickCount, Is.EqualTo(10));
            Assert.That(simulationWithShortFrames.Clock.TickCount, Is.EqualTo(10));
        }

        [Test]
        public void Restore_ToASavedTick_SetsClockAndCalendarWithoutTickingOrAnnouncing()
        {
            _simulation.Calendar.OnDayStarted += JournalDayStarted;
            _simulation.AddSystem(new JournalingSystem("energy", _journal));

            _simulation.Restore(25);

            Assert.That(_simulation.Clock.TickCount, Is.EqualTo(25));
            Assert.That(_simulation.Calendar.DayIndex, Is.EqualTo(2));
            Assert.That(_journal, Is.Empty);
        }

        [Test]
        public void Step_AfterRestore_ContinuesFromTheSavedTick()
        {
            _simulation.AddSystem(new JournalingSystem("energy", _journal));
            _simulation.Restore(25);

            _simulation.Step(1);

            Assert.That(_journal, Is.EqualTo(new[] { "energy:26" }));
        }

        [Test]
        public void Restore_ToANegativeTick_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _simulation.Restore(-1));
        }

        [Test]
        public void AddSystem_WithoutASystem_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _simulation.AddSystem(null));
        }

        [Test]
        public void AddSystem_WithTheSameSystemTwice_Throws()
        {
            JournalingSystem system = new JournalingSystem("energy", _journal);
            _simulation.AddSystem(system);

            Assert.Throws<ArgumentException>(() => _simulation.AddSystem(system));
        }

        private void JournalDayStarted(int dayIndex)
        {
            _journal.Add($"day:{dayIndex}");
        }

        private sealed class JournalingSystem : ISimulationSystem
        {
            private readonly string _systemName;
            private readonly List<string> _journal;

            public JournalingSystem(string systemName, List<string> journal)
            {
                _systemName = systemName;
                _journal = journal;
            }

            public void Tick(in TickContext context)
            {
                _journal.Add($"{_systemName}:{context.TickNumber}");
            }
        }

        private sealed class ContextCapturingSystem : ISimulationSystem
        {
            public TickContext LastContext { get; private set; }

            public void Tick(in TickContext context)
            {
                LastContext = context;
            }
        }
    }
}
