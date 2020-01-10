using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace DnDGen.EventGen.Tests.Unit
{
    [TestFixture]
    public class DomainGenEventQueueTests
    {
        private GenEventQueue eventQueue;
        private Mock<ClientIDManager> mockClientIDManager;
        private Guid clientID;
        private Guid currentThreadClientId;

        [SetUp]
        public void Setup()
        {
            mockClientIDManager = new Mock<ClientIDManager>();
            eventQueue = new DomainGenEventQueue(mockClientIDManager.Object);
            clientID = Guid.NewGuid();
            currentThreadClientId = Guid.NewGuid();

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
        }

        [Test]
        public void EnqueueEvent()
        {
            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(genEvent);

            var queuedEvent = eventQueue.Dequeue(currentThreadClientId);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void EnqueueSourceAndMessage()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(source, message);

            var queuedEvent = eventQueue.Dequeue(currentThreadClientId);
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

            var queuedEvent = eventQueue.Dequeue(currentThreadClientId);
            Assert.That(queuedEvent.Source, Is.EqualTo(source));
            Assert.That(queuedEvent.Message, Is.EqualTo(message));
            Assert.That(queuedEvent.When, Is.EqualTo(DateTime.Now).Within(1).Seconds);

            queuedEvent = eventQueue.Dequeue(currentThreadClientId);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void DequeueEvent()
        {
            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
            eventQueue.Enqueue(new GenEvent());

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(genEvent);

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
            eventQueue.Enqueue(new GenEvent());

            var queuedEvent = eventQueue.Dequeue(clientID);
            Assert.That(queuedEvent, Is.EqualTo(genEvent));
        }

        [Test]
        public void DequeueEventForCurrentThread()
        {
            var genEvent = new GenEvent();
            genEvent.Message = Guid.NewGuid().ToString();
            genEvent.Source = Guid.NewGuid().ToString();

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(new GenEvent());

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
            eventQueue.Enqueue(genEvent);

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(new GenEvent());

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);

            var queuedEvent = eventQueue.DequeueForCurrentThread();
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
                mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
                eventQueue.Enqueue(new GenEvent());

                mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
                eventQueue.Enqueue($"source {i}", $"message {i}");

                mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
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
        public void DequeueAllEventsForCurrentThread()
        {
            for (var i = 0; i < 10; i++)
            {
                mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
                eventQueue.Enqueue(new GenEvent());

                mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
                eventQueue.Enqueue($"source {i}", $"message {i}");

                mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
                eventQueue.Enqueue(new GenEvent());
            }

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(currentThreadClientId);
            var events = eventQueue.DequeueAllForCurrentThread().ToArray();

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

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(source, message);

            var containsEvents = eventQueue.ContainsEvents(clientID);
            Assert.That(containsEvents, Is.True);
        }

        [Test]
        public void CurrentThreadContainsEvents()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(source, message);

            var containsEvents = eventQueue.CurrentThreadContainsEvents();
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

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(source, message);

            var containsEvents = eventQueue.ContainsEvents(currentThreadClientId);
            Assert.That(containsEvents, Is.False);
        }

        [Test]
        public void ClearsEventsForCurrentThread()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            eventQueue.Enqueue(source, message);

            eventQueue.ClearCurrentThread();

            var events = eventQueue.DequeueAllForCurrentThread();
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void ClearsEvents()
        {
            var message = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();

            mockClientIDManager.Setup(m => m.GetClientID()).Returns(clientID);
            eventQueue.Enqueue(source, message);

            eventQueue.Clear(clientID);

            var events = eventQueue.DequeueAll(clientID);
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void ClearsEventsForNonexistentClientId()
        {
            var wrongClientID = Guid.NewGuid();
            eventQueue.Clear(wrongClientID);

            var events = eventQueue.DequeueAll(wrongClientID);
            Assert.That(events, Is.Empty);
        }
    }
}
