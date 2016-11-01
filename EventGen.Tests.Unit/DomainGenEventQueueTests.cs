using NUnit.Framework;
using System;
using System.Linq;

namespace EventGen.Tests.Unit
{
    [TestFixture]
    public class DomainGenEventQueueTests
    {
        private GenEventQueue eventQueue;
        private Guid clientID;

        [SetUp]
        public void Setup()
        {
            eventQueue = new DomainGenEventQueue();
            clientID = Guid.NewGuid();
        }

        [Test]
        public void EnqueueEvent()
        {
            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(clientID, genEvent);

            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void EnqueueSourceAndMessage()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(clientID, source, message);

            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent.Source, Is.EqualTo(source));
            Assert.That(queuedEvent.Message, Is.EqualTo(message));
            Assert.That(queuedEvent.When, Is.EqualTo(DateTime.Now).Within(1).Seconds);
        }

        [Test]
        public void EnqueueSecondEvent()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(clientID, source, message);

            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(clientID, genEvent);

            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent.Source, Is.EqualTo(source));
            Assert.That(queuedEvent.Message, Is.EqualTo(message));
            Assert.That(queuedEvent.When, Is.EqualTo(DateTime.Now).Within(1).Seconds);

            queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void DequeueEvent()
        {
            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(Guid.NewGuid(), new GenEvent());
            eventQueue.Enqueue(clientID, genEvent);
            eventQueue.Enqueue(Guid.NewGuid(), new GenEvent());

            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void DequeueNoEvent()
        {
            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent, Is.Null);
        }

        [Test]
        public void DequeueAllEvents()
        {
            for (var i = 0; i < 10; i++)
            {
                eventQueue.Enqueue(Guid.NewGuid(), new GenEvent());
                eventQueue.Enqueue(clientID, $"source {i}", $"message {i}");
                eventQueue.Enqueue(Guid.NewGuid(), new GenEvent());
            }

            var events = eventQueue.DequeueAll(clientID).ToArray();

            for (var i = 0; i < 10; i++)
            {
                Assert.That(events[i].Message, Is.EqualTo($"message {i}"));
                Assert.That(events[i].Source, Is.EqualTo($"source {i}"));
            }
        }

        [Test]
        public void DequeueNoEvents()
        {
            var events = eventQueue.DequeueAll(clientID);
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void ContainsEvents()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(clientID, source, message);

            var containsEvents = eventQueue.ContainsEvents(clientID);
            Assert.That(containsEvents, Is.True);
        }

        [Test]
        public void ContainsNoEvents()
        {
            var containsEvents = eventQueue.ContainsEvents(clientID);
            Assert.That(containsEvents, Is.False);
        }

        [Test]
        public void ContainsNoEventsForClientID()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(Guid.NewGuid(), source, message);

            var containsEvents = eventQueue.ContainsEvents(clientID);
            Assert.That(containsEvents, Is.False);
        }
    }
}
