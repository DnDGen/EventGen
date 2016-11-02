using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace EventGen.Tests.Unit
{
    [TestFixture]
    public class DomainGenEventQueueTests
    {
        private GenEventQueue eventQueue;
        private Mock<ClientIDManager> mockClientIDManager;
        private Guid clientID;

        [SetUp]
        public void Setup()
        {
            mockClientIDManager = new Mock<ClientIDManager>();
            eventQueue = new DomainGenEventQueue(mockClientIDManager.Object);
            clientID = Guid.NewGuid();

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
        }

        [Test]
        public void EnqueueEvent()
        {
            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(genEvent);

            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void EnqueueSourceAndMessage()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(source, message);

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

            eventQueue.Enqueue(source, message);

            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(genEvent);

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

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(Guid.NewGuid());
            eventQueue.Enqueue(new GenEvent());

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(genEvent);

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(Guid.NewGuid());
            eventQueue.Enqueue(new GenEvent());

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
                mockClientIDManager.Setup(m => m.GetClientID()).Returns(Guid.NewGuid());
                eventQueue.Enqueue(new GenEvent());

                mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
                eventQueue.Enqueue($"source {i}", $"message {i}");

                mockClientIDManager.Setup(m => m.GetClientID()).Returns(Guid.NewGuid());
                eventQueue.Enqueue(new GenEvent());
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

            eventQueue.Enqueue(source, message);

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

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(Guid.NewGuid());
            eventQueue.Enqueue(source, message);

            var containsEvents = eventQueue.ContainsEvents(clientID);
            Assert.That(containsEvents, Is.False);
        }
    }
}
