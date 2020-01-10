using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DnDGen.EventGen.Tests.Integration
{
    [TestFixture]
    public class EventQueueThreadingTests : IntegrationTests
    {
        [Inject]
        public ClientIDManager ClientIDManager { get; set; }
        [Inject]
        public GenEventQueue EventQueue { get; set; }

        private const int PopulateTimeoutInMilliseconds = 250;

        private bool shouldSleep;
        private List<GenEvent> events;
        private Guid clientID;
        private Guid secondClientID;
        private List<GenEvent> secondEvents;

        [SetUp]
        public void Setup()
        {
            shouldSleep = false;
            clientID = Guid.NewGuid();
            secondClientID = Guid.NewGuid();
            CreateEventLists();

            ClientIDManager.SetClientID(clientID);
        }

        public void CreateEventLists()
        {
            events = new List<GenEvent>();
            secondEvents = new List<GenEvent>();
        }

        [TearDown]
        public void TearDown()
        {
            CleanQueues();
        }

        private void CleanQueues()
        {
            EventQueue.Clear(clientID);
            EventQueue.Clear(secondClientID);
        }

        [Test]
        public void GenEventQueueUsesQueueAsSingletonBetweenThreads()
        {
            shouldSleep = true;

            var firstTask = new Task(FirstThreadAction);
            var secondTask = new Task(SecondThreadAction);

            firstTask.Start();
            Thread.Sleep(10); //HACK: Adding this sleep here, as when they begin right after one another, there are odd overlaps when trying to enqueue the events

            secondTask.Start();

            WaitOn(firstTask, secondTask);

            Assert.That(firstTask.IsCompleted, Is.True);
            Assert.That(firstTask.IsFaulted, Is.False, "First task: " + firstTask.Exception?.ToString());
            Assert.That(firstTask.IsCanceled, Is.False);
            Assert.That(secondTask.IsCompleted, Is.True);
            Assert.That(secondTask.IsFaulted, Is.False, "Second task: " + secondTask.Exception?.ToString());
            Assert.That(secondTask.IsCanceled, Is.False);

            var events = EventQueue.DequeueAll(clientID);
            var orderedEvents = events.OrderBy(e => e.Source).ThenBy(e => e.When).ToArray();
            var summaries = orderedEvents.Select(e => $"{e.Source}: {e.Message}");
            var summary = string.Join("; ", summaries);
            Assert.That(events.Count, Is.EqualTo(6), summary);

            Assert.That(orderedEvents[0].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[0].Message, Is.EqualTo("logged a message (1)"));
            Assert.That(orderedEvents[1].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[1].Message, Is.EqualTo("logged an event (2)"));
            Assert.That(orderedEvents[2].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[2].Message, Is.EqualTo("logged a first-thread message (3)"));
            Assert.That(orderedEvents[3].Source, Is.EqualTo("second thread"));
            Assert.That(orderedEvents[3].Message, Is.EqualTo("logged an event (1)"));
            Assert.That(orderedEvents[4].Source, Is.EqualTo("second thread"));
            Assert.That(orderedEvents[4].Message, Is.EqualTo("logged a message (2)"));
            Assert.That(orderedEvents[5].Source, Is.EqualTo("second thread"));
            Assert.That(orderedEvents[5].Message, Is.EqualTo("logged a second-thread message (3)"));
        }

        [Test]
        public void GenEventQueueOrdersEventsChonologicallyBetweenThreads()
        {
            shouldSleep = true;

            var firstTask = new Task(FirstThreadAction);
            var secondTask = new Task(SecondThreadAction);

            firstTask.Start();
            Thread.Sleep(10); //HACK: Adding this sleep here, as when they begin right after one another, there are odd overlaps when trying to enqueue the events

            secondTask.Start();

            WaitOn(firstTask, secondTask);

            Assert.That(firstTask.IsCompleted, Is.True);
            Assert.That(firstTask.IsFaulted, Is.False, firstTask.Exception?.ToString());
            Assert.That(firstTask.IsCanceled, Is.False);
            Assert.That(secondTask.IsCompleted, Is.True);
            Assert.That(secondTask.IsFaulted, Is.False, secondTask.Exception?.ToString());
            Assert.That(secondTask.IsCanceled, Is.False);

            var events = EventQueue.DequeueAll(clientID).ToArray();
            Assert.That(events.Count, Is.EqualTo(6));

            var orderedEvents = events.OrderBy(e => e.When).ToArray();

            for (var i = 0; i < events.Length; i++)
                Assert.That(events[i], Is.EqualTo(orderedEvents[i]), $"index {i}, [{events[i].Source} - {events[i].Message}] vs [{orderedEvents[i].Source} - {orderedEvents[i].Message}]");
        }

        [Test]
        public void GenEventQueueEnqueuesAndDequeuesBetweenThreads()
        {
            shouldSleep = true;

            var enqueueTask = new Task(FirstThreadAction);
            var dequeueTask = new Task(FirstDequeueThreadAction);

            enqueueTask.Start();
            Thread.Sleep(10); //HACK: Adding this sleep here, as when they begin right after one another, there are odd overlaps when trying to enqueue the events

            dequeueTask.Start();

            WaitOn(enqueueTask, dequeueTask);

            Assert.That(enqueueTask.IsCompleted, Is.True, "Enqueue task should be completed");
            Assert.That(enqueueTask.IsFaulted, Is.False, enqueueTask.Exception?.ToString());
            Assert.That(enqueueTask.IsCanceled, Is.False, "Enqueue task should not be canceled");
            Assert.That(dequeueTask.IsCompleted, Is.True, "Dequeue task should be completed");
            Assert.That(dequeueTask.IsFaulted, Is.False, dequeueTask.Exception?.ToString());
            Assert.That(dequeueTask.IsCanceled, Is.False, "Dequeue task should not be canceled");

            Assert.That(events.Count, Is.EqualTo(3));

            var orderedEvents = events.OrderBy(e => e.Source).ThenBy(e => e.When).ToArray();
            Assert.That(orderedEvents[0].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[0].Message, Is.EqualTo("logged a message (1)"));
            Assert.That(orderedEvents[1].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[1].Message, Is.EqualTo("logged an event (2)"));
            Assert.That(orderedEvents[2].Source, Is.EqualTo("first thread"));
            Assert.That(orderedEvents[2].Message, Is.EqualTo("logged a first-thread message (3)"));
        }

        [Test]
        public void GenEventQueueEnqueuesAndDequeuesOnlyClientIDBetweenThreads()
        {
            shouldSleep = true;

            var firstEnqueueTask = new Task(FirstThreadAction);
            var firstDequeueTask = new Task(FirstDequeueThreadAction);
            var secondEnqueueTask = new Task(SecondThreadActionWithSecondClientID);
            var secondDequeueTask = new Task(SecondDequeueThreadAction);

            firstEnqueueTask.Start();
            Thread.Sleep(10); //HACK: Adding this sleep here, as when they begin right after one another, there are odd overlaps when trying to enqueue the events

            secondEnqueueTask.Start();
            Thread.Sleep(10); //HACK: Adding this sleep here, as when they begin right after one another, there are odd overlaps when trying to enqueue the events

            firstDequeueTask.Start();
            Thread.Sleep(10); //HACK: Adding this sleep here, as when they begin right after one another, there are odd overlaps when trying to enqueue the events

            secondDequeueTask.Start();

            WaitOn(firstEnqueueTask, firstDequeueTask, secondEnqueueTask, secondDequeueTask);

            Assert.That(firstEnqueueTask.IsCompleted, Is.True);
            Assert.That(firstEnqueueTask.IsFaulted, Is.False, firstEnqueueTask.Exception?.ToString());
            Assert.That(firstEnqueueTask.IsCanceled, Is.False);
            Assert.That(firstDequeueTask.IsCompleted, Is.True);
            Assert.That(firstDequeueTask.IsFaulted, Is.False, firstDequeueTask.Exception?.ToString());
            Assert.That(firstDequeueTask.IsCanceled, Is.False);
            Assert.That(secondEnqueueTask.IsCompleted, Is.True);
            Assert.That(secondEnqueueTask.IsFaulted, Is.False, secondEnqueueTask.Exception?.ToString());
            Assert.That(secondEnqueueTask.IsCanceled, Is.False);
            Assert.That(secondDequeueTask.IsCompleted, Is.True);
            Assert.That(secondDequeueTask.IsFaulted, Is.False, secondDequeueTask.Exception?.ToString());
            Assert.That(secondDequeueTask.IsCanceled, Is.False);

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(secondEvents.Count, Is.EqualTo(3));

            Assert.That(events.Select(e => e.Source), Is.All.EqualTo("first thread"));
            Assert.That(secondEvents.Select(e => e.Source), Is.All.EqualTo("second thread"));
        }

        private void WaitOn(params Task[] tasks)
        {
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException exception)
            {
                var message = exception.ToString();
                message += "\n" + exception.InnerException.StackTrace;

                Assert.Fail(message);
            }
        }

        [Test]
        public void EnqueueAndDequeueIsThreadSafe()
        {
            var enqueueTask = new Task(CreateEvents);
            enqueueTask.Start();

            var events = new List<GenEvent>();

            var attempts = 0;
            while (!enqueueTask.IsCompleted)
            {
                attempts++;
                var newEvent = EventQueue.Dequeue(clientID);

                if (newEvent != null)
                    events.Add(newEvent);
            }

            WaitOn(enqueueTask);

            Assert.That(enqueueTask.IsCompleted, Is.True, "Should be completed");
            Assert.That(enqueueTask.IsFaulted, Is.False, enqueueTask.Exception?.ToString());
            Assert.That(enqueueTask.IsCanceled, Is.False, "Should not be canceled");

            Assert.That(attempts, Is.Positive);
            Assert.That(events, Is.Not.Empty);
            Assert.That(events, Is.All.Not.Null);
            Assert.That(events.Count, Is.AtLeast(100));
        }

        [Test]
        public void EnqueueAndDequeueAllIsThreadSafe()
        {
            var enqueueTask = new Task(CreateEvents);
            enqueueTask.Start();

            var events = new List<GenEvent>();

            var attempts = 0;
            while (!enqueueTask.IsCompleted)
            {
                attempts++;
                var newEvents = EventQueue.DequeueAll(clientID);
                events.AddRange(newEvents);
            }

            WaitOn(enqueueTask);

            Assert.That(enqueueTask.IsCompleted, Is.True, "Should be completed");
            Assert.That(enqueueTask.IsFaulted, Is.False, enqueueTask.Exception?.ToString());
            Assert.That(enqueueTask.IsCanceled, Is.False, "Should not be canceled");

            Assert.That(attempts, Is.Positive);
            Assert.That(events, Is.Not.Empty);
            Assert.That(events, Is.All.Not.Null);
            Assert.That(events.Count, Is.AtLeast(100));
        }

        [Test]
        public void DequeueAllHasFiniteRun()
        {
            var enqueueTask = new Task(CreateEvents);
            enqueueTask.Start();

            Thread.Sleep(PopulateTimeoutInMilliseconds / 2);

            var earlyEvents = EventQueue.DequeueAll(clientID);

            WaitOn(enqueueTask);

            Assert.That(enqueueTask.IsCompleted, Is.True, "Should be completed");
            Assert.That(enqueueTask.IsFaulted, Is.False, enqueueTask.Exception?.ToString());
            Assert.That(enqueueTask.IsCanceled, Is.False, "Should not be canceled");

            Assert.That(earlyEvents, Is.Not.Empty.And.All.Not.Null);

            var laterEvents = EventQueue.DequeueAll(clientID);
            Assert.That(laterEvents, Is.Not.Empty.And.All.Not.Null);

            var duplicates = laterEvents.Intersect(earlyEvents);
            Assert.That(duplicates, Is.Empty, "Duplicates should be empty");
        }

        [Test]
        public void ClearAfterDone()
        {
            var enqueueTask = new Task(CreateEvents);
            enqueueTask.Start();

            WaitOn(enqueueTask);

            Assert.That(enqueueTask.IsCompleted, Is.True, "Should be completed");
            Assert.That(enqueueTask.IsFaulted, Is.False, enqueueTask.Exception?.ToString());
            Assert.That(enqueueTask.IsCanceled, Is.False, "Should not be canceled");

            EventQueue.Clear(clientID);

            var laterEvents = EventQueue.DequeueAll(clientID);
            Assert.That(laterEvents, Is.Empty, "Later events should be empty");
        }

        [Test]
        public void ClearAfterDoneDequeueingWithEventsRemaining()
        {
            var enqueueTask = new Task(CreateEvents);
            enqueueTask.Start();

            Thread.Sleep(PopulateTimeoutInMilliseconds / 2);

            var earlyEvents = EventQueue.DequeueAll(clientID);

            WaitOn(enqueueTask);

            Assert.That(enqueueTask.IsCompleted, Is.True, "Should be completed");
            Assert.That(enqueueTask.IsFaulted, Is.False, enqueueTask.Exception?.ToString());
            Assert.That(enqueueTask.IsCanceled, Is.False, "Should not be canceled");

            Assert.That(earlyEvents, Is.Not.Empty.And.All.Not.Null);

            EventQueue.Clear(clientID);

            var laterEvents = EventQueue.DequeueAll(clientID);
            Assert.That(laterEvents, Is.Empty);
        }

        private void FirstThreadAction()
        {
            ClientIDManager.SetClientID(clientID);

            EventQueue.Enqueue("first thread", "logged a message (1)");

            if (shouldSleep)
                Thread.Sleep(60);

            var genEvent = new GenEvent("first thread", "logged an event (2)");
            EventQueue.Enqueue(genEvent);

            EventQueue.Enqueue("first thread", "logged a first-thread message (3)");
        }

        private void SecondThreadAction()
        {
            EnqueueSecondThread(clientID);
        }

        private void EnqueueSecondThread(Guid targetClientID)
        {
            ClientIDManager.SetClientID(targetClientID);

            var genEvent = new GenEvent("second thread", "logged an event (1)");
            EventQueue.Enqueue(genEvent);

            EventQueue.Enqueue("second thread", "logged a message (2)");

            if (shouldSleep)
                Thread.Sleep(30);

            EventQueue.Enqueue("second thread", "logged a second-thread message (3)");
        }

        private void SecondThreadActionWithSecondClientID()
        {
            EnqueueSecondThread(secondClientID);
        }

        private void FirstDequeueThreadAction()
        {
            DequeueEvents(events, clientID);
        }

        private void DequeueEvents(List<GenEvent> targetEvents, Guid clientID)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < PopulateTimeoutInMilliseconds)
            {
                var genEvent = EventQueue.Dequeue(clientID);

                if (genEvent != null)
                    targetEvents.Add(genEvent);

                if (shouldSleep)
                    Thread.Sleep(10);
            }
        }

        private void SecondDequeueThreadAction()
        {
            DequeueEvents(secondEvents, secondClientID);
        }

        private void CreateEvents()
        {
            ClientIDManager.SetClientID(clientID);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < PopulateTimeoutInMilliseconds)
            {
                EventQueue.Enqueue("EventGen", $"logged a message at {stopwatch.Elapsed}");
            }
        }
    }
}
