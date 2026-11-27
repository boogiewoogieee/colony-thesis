using System;
using System.Collections.Generic;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class GameCalendarTests
    {
        private const long TICKS_PER_DAY = 10;
        private const double TOLERANCE = 0.000001;

        private GameCalendar _calendar;
        private List<int> _startedDayIndexList;

        [SetUp]
        public void CreateCalendar()
        {
            _calendar = new GameCalendar(TICKS_PER_DAY);
            _startedDayIndexList = new List<int>();
            _calendar.OnDayStarted += _startedDayIndexList.Add;
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithLessThanOneTickPerDay_Throws(long ticksPerDay)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameCalendar(ticksPerDay));
        }

        [Test]
        public void Constructor_StartsAtTheBeginningOfTheFirstDay()
        {
            Assert.That(_calendar.DayIndex, Is.EqualTo(0));
            Assert.That(_calendar.DayProgress, Is.EqualTo(0.0));
        }

        [Test]
        public void AdvanceTo_TheLastTickOfTheFirstDay_StaysInTheFirstDay()
        {
            _calendar.AdvanceTo(TICKS_PER_DAY - 1);

            Assert.That(_calendar.DayIndex, Is.EqualTo(0));
            Assert.That(_startedDayIndexList, Is.Empty);
        }

        [Test]
        public void AdvanceTo_TheFirstTickOfTheSecondDay_StartsTheSecondDay()
        {
            _calendar.AdvanceTo(TICKS_PER_DAY);

            Assert.That(_calendar.DayIndex, Is.EqualTo(1));
            Assert.That(_startedDayIndexList, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void AdvanceTo_AcrossSeveralDaysAtOnce_AnnouncesEachDayInOrder()
        {
            _calendar.AdvanceTo(35);

            Assert.That(_startedDayIndexList, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void AdvanceTo_WithinTheSameDay_AnnouncesNothing()
        {
            _calendar.AdvanceTo(12);
            _startedDayIndexList.Clear();

            _calendar.AdvanceTo(19);

            Assert.That(_startedDayIndexList, Is.Empty);
        }

        [Test]
        public void AdvanceTo_TheSameTickAgain_AnnouncesNothing()
        {
            _calendar.AdvanceTo(TICKS_PER_DAY);
            _startedDayIndexList.Clear();

            _calendar.AdvanceTo(TICKS_PER_DAY);

            Assert.That(_startedDayIndexList, Is.Empty);
        }

        [Test]
        public void AdvanceTo_AnEarlierTick_Throws()
        {
            _calendar.AdvanceTo(12);

            Assert.Throws<ArgumentOutOfRangeException>(() => _calendar.AdvanceTo(11));
        }

        [Test]
        public void AdvanceTo_WithNobodyListening_DoesNotThrow()
        {
            GameCalendar calendarWithoutListeners = new GameCalendar(TICKS_PER_DAY);

            Assert.DoesNotThrow(() => calendarWithoutListeners.AdvanceTo(25));
        }

        [TestCase(0, 0.0)]
        [TestCase(5, 0.5)]
        [TestCase(9, 0.9)]
        [TestCase(10, 0.0)]
        [TestCase(25, 0.5)]
        public void DayProgress_AtAGivenTick_IsTheFractionOfTheCurrentDay(long tickCount, double expectedProgress)
        {
            _calendar.AdvanceTo(tickCount);

            Assert.That(_calendar.DayProgress, Is.EqualTo(expectedProgress).Within(TOLERANCE));
        }

        [Test]
        public void Restore_ToASavedTick_SetsTheDayWithoutAnnouncingIt()
        {
            _calendar.Restore(25);

            Assert.That(_calendar.DayIndex, Is.EqualTo(2));
            Assert.That(_calendar.DayProgress, Is.EqualTo(0.5).Within(TOLERANCE));
            Assert.That(_startedDayIndexList, Is.Empty);
        }

        [Test]
        public void Restore_ToAnEarlierTick_IsAllowed()
        {
            _calendar.AdvanceTo(35);

            _calendar.Restore(5);

            Assert.That(_calendar.DayIndex, Is.EqualTo(0));
        }

        [Test]
        public void AdvanceTo_AfterRestore_AnnouncesOnlyTheDaysAfterTheSave()
        {
            _calendar.Restore(25);

            _calendar.AdvanceTo(30);

            Assert.That(_startedDayIndexList, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void Restore_ToANegativeTick_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _calendar.Restore(-1));
        }
    }
}
