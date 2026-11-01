using System;
using System.Collections.Generic;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class GameCalendarTests
    {
        [Test]
        public void AdvanceTo_WithinFirstDay_StaysOnDayZero()
        {
            var calendar = new GameCalendar(100d);

            calendar.AdvanceTo(25d);

            Assert.That(calendar.Day, Is.EqualTo(0));
            Assert.That(calendar.TimeOfDay01, Is.EqualTo(0.25d).Within(1e-9));
        }

        [Test]
        public void AdvanceTo_CrossingSeveralDays_RaisesEachDayInOrder()
        {
            var calendar = new GameCalendar(100d);
            var started = new List<int>();
            calendar.DayStarted += started.Add;

            calendar.AdvanceTo(350d);

            Assert.That(started, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(calendar.Day, Is.EqualTo(3));
            Assert.That(calendar.TimeOfDay01, Is.EqualTo(0.5d).Within(1e-9));
        }

        [Test]
        public void AdvanceTo_ExactlyOnBoundary_StartsNewDay()
        {
            var calendar = new GameCalendar(100d);
            var started = new List<int>();
            calendar.DayStarted += started.Add;

            calendar.AdvanceTo(100d);

            Assert.That(started, Is.EqualTo(new[] { 1 }));
            Assert.That(calendar.TimeOfDay01, Is.EqualTo(0d).Within(1e-9));
        }

        [Test]
        public void AdvanceTo_Backwards_Throws()
        {
            var calendar = new GameCalendar(100d);
            calendar.AdvanceTo(50d);

            Assert.Throws<ArgumentOutOfRangeException>(() => calendar.AdvanceTo(10d));
        }

        [Test]
        public void Restore_SetsDayWithoutRaisingEvents()
        {
            var calendar = new GameCalendar(100d);
            var raised = false;
            calendar.DayStarted += _ => raised = true;

            calendar.Restore(1234d);

            Assert.That(calendar.Day, Is.EqualTo(12));
            Assert.That(raised, Is.False);
        }

        [Test]
        public void Constructor_NonPositiveDayLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameCalendar(0d));
        }
    }
}
