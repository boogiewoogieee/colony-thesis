using Colony.Core;
using NUnit.Framework;

namespace Colony.Tests.Core
{
    public sealed class EventBusTests
    {
        private struct Ping
        {
            public int Value;
        }

        private struct Pong
        {
        }

        [Test]
        public void Publish_DeliversToEverySubscriberOfThatType()
        {
            var bus = new EventBus();
            var sum = 0;
            bus.Subscribe<Ping>(e => sum += e.Value);
            bus.Subscribe<Ping>(e => sum += e.Value * 10);

            bus.Publish(new Ping { Value = 2 });

            Assert.That(sum, Is.EqualTo(22));
        }

        [Test]
        public void Publish_DoesNotDeliverToOtherTypes()
        {
            var bus = new EventBus();
            var pongs = 0;
            bus.Subscribe<Pong>(_ => pongs++);

            bus.Publish(new Ping { Value = 1 });

            Assert.That(pongs, Is.EqualTo(0));
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            var count = 0;
            void Handler(Ping _) => count++;
            bus.Subscribe<Ping>(Handler);

            bus.Unsubscribe<Ping>(Handler);
            bus.Publish(new Ping());

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNothing()
        {
            var bus = new EventBus();

            Assert.DoesNotThrow(() => bus.Publish(new Ping()));
        }
    }
}
