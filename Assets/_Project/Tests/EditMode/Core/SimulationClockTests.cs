using System;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class SimulationClockTests
    {
        private const double TICK_SECONDS = 0.1;
        private const double HALF_TICK_SECONDS = 0.05;
        private const double TOLERANCE_SECONDS = 0.000001;

        private SimulationClock _clock;

        [SetUp]
        public void CreateClock()
        {
            _clock = new SimulationClock(TICK_SECONDS);
        }

        [TestCase(0.0)]
        [TestCase(-0.1)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Constructor_WithInvalidTickLength_Throws(double tickSeconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationClock(tickSeconds));
        }

        [Test]
        public void Constructor_StartsAtTickZeroRunningAtNormalSpeed()
        {
            Assert.That(_clock.TickCount, Is.EqualTo(0));
            Assert.That(_clock.IsPaused, Is.False);
            Assert.That(_clock.Speed, Is.EqualTo(1.0));
        }

        [Test]
        public void Advance_ByLessThanATick_CountsNoTick()
        {
            _clock.Advance(HALF_TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Advance_ByTwoHalfTicks_CountsOneTick()
        {
            _clock.Advance(HALF_TICK_SECONDS);
            _clock.Advance(HALF_TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Advance_ByExactlyOneTick_CountsOneTick()
        {
            _clock.Advance(TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Advance_ByZero_CountsNoTick()
        {
            _clock.Advance(0.0);

            Assert.That(_clock.TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Advance_ByAFrameLongerThanTheCap_CountsOnlyTheCap()
        {
            _clock.Advance(5.0);

            Assert.That(_clock.TickCount, Is.EqualTo(2));
        }

        [Test]
        public void Advance_AfterACappedFrame_KeepsTheHalfTickLeftOver()
        {
            _clock.Advance(SimulationClock.MAX_FRAME_SECONDS);

            _clock.Advance(HALF_TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(3));
        }

        [Test]
        public void Advance_AtDoubleSpeed_CountsTwiceTheTicks()
        {
            _clock.SetSpeed(2.0);

            _clock.Advance(TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(2));
        }

        [Test]
        public void Advance_AtHalfSpeed_CountsHalfTheTicks()
        {
            _clock.SetSpeed(0.5);

            _clock.Advance(TICK_SECONDS + TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Advance_AtDoubleSpeedByALongFrame_AppliesTheCapBeforeTheSpeed()
        {
            _clock.SetSpeed(2.0);

            _clock.Advance(5.0);

            Assert.That(_clock.TickCount, Is.EqualTo(5));
        }

        [Test]
        public void Advance_WhilePaused_CountsNoTick()
        {
            _clock.Pause();

            _clock.Advance(TICK_SECONDS + TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Advance_AfterResume_ContinuesFromThePartialTickBeforeThePause()
        {
            _clock.Advance(HALF_TICK_SECONDS);
            _clock.Pause();
            _clock.Advance(TICK_SECONDS);
            _clock.Resume();

            _clock.Advance(HALF_TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(1));
        }

        [TestCase(-0.01)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Advance_ByAnInvalidFrameTime_Throws(double frameSeconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _clock.Advance(frameSeconds));
        }

        [TestCase(0.0)]
        [TestCase(-1.0)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void SetSpeed_ToAnInvalidValue_Throws(double speed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _clock.SetSpeed(speed));
        }

        [Test]
        public void Pause_ThenResume_ReportsEachState()
        {
            _clock.Pause();
            Assert.That(_clock.IsPaused, Is.True);

            _clock.Resume();
            Assert.That(_clock.IsPaused, Is.False);
        }

        [Test]
        public void ElapsedSeconds_AfterTenTicks_IsOneSecond()
        {
            _clock.AddTicks(10);

            Assert.That(_clock.ElapsedSeconds, Is.EqualTo(1.0).Within(TOLERANCE_SECONDS));
        }

        [Test]
        public void AddTicks_WhilePaused_StillCountsTheTicks()
        {
            _clock.Pause();

            _clock.AddTicks(7);

            Assert.That(_clock.TickCount, Is.EqualTo(7));
        }

        [Test]
        public void AddTicks_WithANegativeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _clock.AddTicks(-1));
        }

        [Test]
        public void Restore_ToASavedTickCount_SetsTheCount()
        {
            _clock.Restore(28800);

            Assert.That(_clock.TickCount, Is.EqualTo(28800));
        }

        [Test]
        public void Restore_WithAPartialTickPending_ForgetsThePartialTick()
        {
            _clock.Advance(HALF_TICK_SECONDS);
            _clock.Restore(100);

            _clock.Advance(HALF_TICK_SECONDS);

            Assert.That(_clock.TickCount, Is.EqualTo(100));
        }

        [Test]
        public void Restore_ToANegativeTickCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _clock.Restore(-1));
        }
    }
}
