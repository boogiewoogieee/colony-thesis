using System;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class SimulationClockTests
    {
        [Test]
        public void Advance_ExactTickLength_RunsOneTick()
        {
            var clock = new SimulationClock(0.1f);

            Assert.That(clock.Advance(0.1f), Is.EqualTo(1));
            Assert.That(clock.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Advance_PartialTicks_AccumulateAcrossFrames()
        {
            var clock = new SimulationClock(0.1f);

            Assert.That(clock.Advance(0.06f), Is.EqualTo(0));
            Assert.That(clock.Advance(0.06f), Is.EqualTo(1));
            Assert.That(clock.Advance(0.08f), Is.EqualTo(1));
            Assert.That(clock.TickCount, Is.EqualTo(2));
        }

        [Test]
        public void Advance_ManySmallFrames_DoesNotDriftFromRealTime()
        {
            var clock = new SimulationClock(0.1f);

            for (var i = 0; i < 600; i++)
            {
                clock.Advance(1f / 60f);
            }

            Assert.That(clock.TickCount, Is.InRange(99, 100));
        }

        [Test]
        public void Advance_WhenPaused_RunsNoTicks()
        {
            var clock = new SimulationClock(0.1f) { IsPaused = true };

            Assert.That(clock.Advance(1f), Is.EqualTo(0));
            Assert.That(clock.TickCount, Is.EqualTo(0));
        }

        [Test]
        public void Advance_DoubleSpeed_RunsTwiceAsManyTicks()
        {
            var clock = new SimulationClock(0.1f) { Speed = 2f };

            Assert.That(clock.Advance(0.2f), Is.EqualTo(4));
        }

        [Test]
        public void Advance_LongHitch_IsCappedToMaxFrame()
        {
            var clock = new SimulationClock(0.05f);

            var ticks = clock.Advance(10f);

            Assert.That(ticks, Is.EqualTo((int)Math.Round(SimulationClock.MaxFrameSeconds / 0.05f)));
        }

        [Test]
        public void Restore_SetsTickCountAndDropsPartialTick()
        {
            var clock = new SimulationClock(0.1f);
            clock.Advance(0.05f);

            clock.Restore(42);

            Assert.That(clock.TickCount, Is.EqualTo(42));
            Assert.That(clock.Advance(0.05f), Is.EqualTo(0));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Constructor_InvalidTickLength_Throws(float tickSeconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationClock(tickSeconds));
        }

        [Test]
        public void Speed_Negative_Throws()
        {
            var clock = new SimulationClock(0.1f);

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Speed = -1f);
        }
    }
}
