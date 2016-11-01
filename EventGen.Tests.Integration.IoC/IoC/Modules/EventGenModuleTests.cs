using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EventGen.Tests.Integration.IoC.IoC.Modules
{
    [TestFixture]
    public class EventGenModuleTests : IntegrationTests
    {
        private bool shouldSleep;
        private List<GenEvent> events;
        private Guid clientID;
        private Guid secondClientID;
        private List<GenEvent> secondEvents;

        [SetUp]
        public void Setup()
        {
            shouldSleep = false;
            events = new List<GenEvent>();
            clientID = Guid.NewGuid();
            secondEvents = new List<GenEvent>();
            secondClientID = Guid.NewGuid();
        }

        [TearDown]
        public void TearDown()
        {
            var queue = GetNewInstanceOf<GenEventQueue>();
            queue.DequeueAll(clientID);
            queue.DequeueAll(secondClientID);
        }

        [Test]
        public void GenEventQueueIsInjected()
        {
            var queue = GetNewInstanceOf<GenEventQueue>();
            Assert.That(queue, Is.Not.Null);
            Assert.That(queue, Is.InstanceOf<DomainGenEventQueue>());
        }

        [Test]
        public void GenEventQueueIsInjectedAsSingleton()
        {
            var first = GetNewInstanceOf<GenEventQueue>();
            var second = GetNewInstanceOf<GenEventQueue>();
            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void GenEventQueueUsesQueueAsSingleton()
        {
            var first = GetNewInstanceOf<GenEventQueue>();
            var message = Guid.NewGuid().ToString();
            var enqueueTime = DateTime.Now;

            first.Enqueue(clientID, "EventGen integration tests", message);

            var second = GetNewInstanceOf<GenEventQueue>();
            var genEvent = second.Dequeue(clientID);

            Assert.That(genEvent.Source, Is.EqualTo("EventGen integration tests"));
            Assert.That(genEvent.Message, Is.EqualTo(message));
            Assert.That(genEvent.When, Is.EqualTo(enqueueTime).Within(1).Seconds);
        }

        [Test]
        public void GenEventQueueUsesQueueAsSingletonBetweenThreads()
        {
            shouldSleep = true;

            var first = new Thread(FirstThreadAction);
            var second = new Thread(SecondThreadAction);

            first.Start();
            Thread.Sleep(50);

            second.Start();
            Thread.Sleep(50);

            var eventQueue = GetNewInstanceOf<GenEventQueue>();
            var events = eventQueue.DequeueAll(clientID);
            Assert.That(events.Count, Is.EqualTo(6));

            var orderedEvents = events.OrderBy(e => e.When).ToArray();
            Assert.That(orderedEvents[0].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[0].Message, Is.EqualTo("logged a message"));
            Assert.That(orderedEvents[1].Source, Is.EqualTo("second thread"));
            Assert.That(orderedEvents[1].Message, Is.EqualTo("logged an event"));
            Assert.That(orderedEvents[2].Source, Is.EqualTo("second thread"));
            Assert.That(orderedEvents[2].Message, Is.EqualTo("logged a message"));
            Assert.That(orderedEvents[3].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[3].Message, Is.EqualTo("logged an event"));
            Assert.That(orderedEvents[4].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[4].Message, Is.EqualTo("logged a first-thread message"));
            Assert.That(orderedEvents[5].Source, Is.EqualTo("second thread"));
            Assert.That(orderedEvents[5].Message, Is.EqualTo("logged a second-thread message"));
        }

        [Test]
        public void GenEventQueueOrdersEventsChonologicallyBetweenThreads()
        {
            var first = new Thread(FirstThreadAction);
            var second = new Thread(SecondThreadAction);

            first.Start();
            Thread.Sleep(10);

            second.Start();
            Thread.Sleep(1000);

            var eventQueue = GetNewInstanceOf<GenEventQueue>();
            var events = eventQueue.DequeueAll(clientID).ToArray();
            Assert.That(events.Count, Is.EqualTo(6));

            var orderedEvents = events.OrderBy(e => e.When).ToArray();

            for (var i = 0; i < events.Length; i++)
                Assert.That(events[i], Is.EqualTo(orderedEvents[i]), $"index {i}, {events[i].Source} - {events[i].Message} vs {orderedEvents[i].Source} - {orderedEvents[i].Message}");
        }

        [Test]
        public void GenEventQueueEnqueuesAndDequeuesBetweenThreads()
        {
            shouldSleep = true;

            var first = new Thread(FirstThreadAction);
            var firstDequeue = new Thread(FirstDequeueThreadAction);

            first.Start();
            Thread.Sleep(10);

            firstDequeue.Start();
            Thread.Sleep(100);

            Assert.That(events.Count, Is.EqualTo(3));
        }

        [Test]
        public void GenEventQueueEnqueuesAndDequeuesOnlyClientIDBetweenThreads()
        {
            shouldSleep = true;

            var first = new Thread(FirstThreadAction);
            var firstDequeue = new Thread(FirstDequeueThreadAction);
            var second = new Thread(SecondThreadActionWithSecondClientID);
            var secondDequeue = new Thread(SecondDequeueThreadAction);

            first.Start();
            second.Start();
            Thread.Sleep(10);

            secondDequeue.Start();
            firstDequeue.Start();
            Thread.Sleep(100);

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(secondEvents.Count, Is.EqualTo(3));

            Assert.That(events.Select(e => e.Source), Is.All.EqualTo("first thread"));
            Assert.That(secondEvents.Select(e => e.Source), Is.All.EqualTo("second thread"));
        }

        private void FirstThreadAction()
        {
            var eventQueue = GetNewInstanceOf<GenEventQueue>();
            eventQueue.Enqueue(clientID, "first thread", "logged a message");

            if (shouldSleep)
                Thread.Sleep(60);

            var genEvent = new GenEvent("first thread", "logged an event");
            eventQueue.Enqueue(clientID, genEvent);

            eventQueue.Enqueue(clientID, "first thread", "logged a first-thread message");
        }

        private void SecondThreadAction()
        {
            EnqueueSecondThread(clientID);
        }

        private void EnqueueSecondThread(Guid targetClientID)
        {
            var eventQueue = GetNewInstanceOf<GenEventQueue>();

            var genEvent = new GenEvent("second thread", "logged an event");
            eventQueue.Enqueue(targetClientID, genEvent);

            eventQueue.Enqueue(targetClientID, "second thread", "logged a message");

            if (shouldSleep)
                Thread.Sleep(30);

            eventQueue.Enqueue(targetClientID, "second thread", "logged a second-thread message");
        }

        private void SecondThreadActionWithSecondClientID()
        {
            EnqueueSecondThread(secondClientID);
        }

        private void FirstDequeueThreadAction()
        {
            PopulateEvents(events, clientID);
        }

        private void PopulateEvents(List<GenEvent> targetEvents, Guid clientID)
        {
            var eventQueue = GetNewInstanceOf<GenEventQueue>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < 100)
            {
                var genEvent = eventQueue.Dequeue(clientID);

                if (genEvent != null)
                    targetEvents.Add(genEvent);

                if (shouldSleep)
                    Thread.Sleep(10);
            }
        }

        private void SecondDequeueThreadAction()
        {
            PopulateEvents(secondEvents, secondClientID);
        }
    }
}
