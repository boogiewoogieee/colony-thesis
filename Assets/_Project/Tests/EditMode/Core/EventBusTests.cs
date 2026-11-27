using System;
using System.Collections.Generic;
using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class EventBusTests
    {
        private EventBus _eventBus;
        private List<string> _receivedList;

        [SetUp]
        public void CreateEventBus()
        {
            _eventBus = new EventBus();
            _receivedList = new List<string>();
        }

        [Test]
        public void Publish_WithOneSubscriber_DeliversTheMessage()
        {
            _eventBus.Subscribe<DomeActivated>(RecordDomeForFirstListener);

            _eventBus.Publish(new DomeActivated(radiusCells: 12));

            Assert.That(_receivedList, Is.EqualTo(new[] { "first:12" }));
        }

        [Test]
        public void Publish_WithTwoSubscribers_DeliversToBothInSubscriptionOrder()
        {
            _eventBus.Subscribe<DomeActivated>(RecordDomeForFirstListener);
            _eventBus.Subscribe<DomeActivated>(RecordDomeForSecondListener);

            _eventBus.Publish(new DomeActivated(radiusCells: 12));

            Assert.That(_receivedList, Is.EqualTo(new[] { "first:12", "second:12" }));
        }

        [Test]
        public void Publish_OfAnotherMessageType_DoesNotReachTheSubscriber()
        {
            _eventBus.Subscribe<DomeActivated>(RecordDomeForFirstListener);

            _eventBus.Publish(new RainStarted());

            Assert.That(_receivedList, Is.Empty);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _eventBus.Publish(new DomeActivated(radiusCells: 12)));
        }

        [Test]
        public void Publish_AfterUnsubscribe_DoesNotReachTheRemovedHandler()
        {
            _eventBus.Subscribe<DomeActivated>(RecordDomeForFirstListener);
            _eventBus.Subscribe<DomeActivated>(RecordDomeForSecondListener);
            _eventBus.Unsubscribe<DomeActivated>(RecordDomeForFirstListener);

            _eventBus.Publish(new DomeActivated(radiusCells: 12));

            Assert.That(_receivedList, Is.EqualTo(new[] { "second:12" }));
        }

        [Test]
        public void Publish_AfterTheLastSubscriberLeft_DoesNotThrow()
        {
            _eventBus.Subscribe<DomeActivated>(RecordDomeForFirstListener);
            _eventBus.Unsubscribe<DomeActivated>(RecordDomeForFirstListener);

            Assert.DoesNotThrow(() => _eventBus.Publish(new DomeActivated(radiusCells: 12)));
            Assert.That(_receivedList, Is.Empty);
        }

        [Test]
        public void Publish_WhenAHandlerUnsubscribesItself_StillReachesTheOtherHandler()
        {
            _eventBus.Subscribe<DomeActivated>(RecordDomeThenUnsubscribe);
            _eventBus.Subscribe<DomeActivated>(RecordDomeForSecondListener);

            _eventBus.Publish(new DomeActivated(radiusCells: 12));
            _eventBus.Publish(new DomeActivated(radiusCells: 20));

            Assert.That(_receivedList, Is.EqualTo(new[] { "once:12", "second:12", "second:20" }));
        }

        [Test]
        public void Unsubscribe_AHandlerThatNeverSubscribed_DoesNothing()
        {
            Assert.DoesNotThrow(() => _eventBus.Unsubscribe<DomeActivated>(RecordDomeForFirstListener));
        }

        [Test]
        public void Subscribe_WithoutAHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _eventBus.Subscribe<DomeActivated>(null));
        }

        [Test]
        public void Unsubscribe_WithoutAHandler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _eventBus.Unsubscribe<DomeActivated>(null));
        }

        private void RecordDomeForFirstListener(DomeActivated message)
        {
            _receivedList.Add($"first:{message.RadiusCells}");
        }

        private void RecordDomeForSecondListener(DomeActivated message)
        {
            _receivedList.Add($"second:{message.RadiusCells}");
        }

        private void RecordDomeThenUnsubscribe(DomeActivated message)
        {
            _receivedList.Add($"once:{message.RadiusCells}");
            _eventBus.Unsubscribe<DomeActivated>(RecordDomeThenUnsubscribe);
        }

        private readonly struct DomeActivated
        {
            public DomeActivated(int radiusCells)
            {
                RadiusCells = radiusCells;
            }

            public int RadiusCells { get; }
        }

        private readonly struct RainStarted
        {
        }
    }
}
