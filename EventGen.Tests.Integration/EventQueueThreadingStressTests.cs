using Ninject;
using NUnit.Framework;

namespace EventGen.Tests.Integration
{
    [TestFixture]
    public class EventQueueThreadingStressTests : StressTests
    {
        [Inject]
        public ClientIDManager ClientIDManager { get; set; }
        [Inject]
        public GenEventQueue EventQueue { get; set; }

        private EventQueueThreadingTests threadingTests;

        [SetUp]
        public void Setup()
        {
            threadingTests = new EventQueueThreadingTests();
            threadingTests.ClientIDManager = ClientIDManager;
            threadingTests.EventQueue = EventQueue;
        }

        [Test]
        public void StressGenEventQueueUsesQueueAsSingletonBetweenThreads()
        {
            Stress(threadingTests.Setup, threadingTests.GenEventQueueUsesQueueAsSingletonBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressGenEventQueueOrdersEventsChonologicallyBetweenThreads()
        {
            Stress(threadingTests.Setup, threadingTests.GenEventQueueOrdersEventsChonologicallyBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressGenEventQueueEnqueuesAndDequeuesBetweenThreads()
        {
            Stress(threadingTests.Setup, threadingTests.GenEventQueueEnqueuesAndDequeuesBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressGenEventQueueEnqueuesAndDequeuesOnlyClientIDBetweenThreads()
        {
            Stress(threadingTests.Setup, threadingTests.GenEventQueueEnqueuesAndDequeuesOnlyClientIDBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressEnqueueAndDequeueIsThreadSafe()
        {
            Stress(threadingTests.Setup, threadingTests.EnqueueAndDequeueIsThreadSafe, threadingTests.TearDown);
        }

        [Test]
        public void StressEnqueueAndDequeueAllIsThreadSafe()
        {
            Stress(threadingTests.Setup, threadingTests.EnqueueAndDequeueAllIsThreadSafe, threadingTests.TearDown);
        }

        [Test]
        public void StressDequeueAllHasFiniteRun()
        {
            Stress(threadingTests.Setup, threadingTests.DequeueAllHasFiniteRun, threadingTests.TearDown);
        }

        [Test]
        public void StressClearAfterDone()
        {
            Stress(threadingTests.Setup, threadingTests.ClearAfterDone, threadingTests.TearDown);
        }

        [Test]
        public void StressClearAfterDoneDequeueingWithEventsRemaining()
        {
            Stress(threadingTests.Setup, threadingTests.ClearAfterDoneDequeueingWithEventsRemaining, threadingTests.TearDown);
        }
    }
}
