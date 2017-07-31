using Ninject;
using NUnit.Framework;

namespace EventGen.Tests.Integration.Stress
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
            stressor.Stress(threadingTests.Setup, threadingTests.GenEventQueueUsesQueueAsSingletonBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressGenEventQueueOrdersEventsChonologicallyBetweenThreads()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.GenEventQueueOrdersEventsChonologicallyBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressGenEventQueueEnqueuesAndDequeuesBetweenThreads()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.GenEventQueueEnqueuesAndDequeuesBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressGenEventQueueEnqueuesAndDequeuesOnlyClientIDBetweenThreads()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.GenEventQueueEnqueuesAndDequeuesOnlyClientIDBetweenThreads, threadingTests.TearDown);
        }

        [Test]
        public void StressEnqueueAndDequeueIsThreadSafe()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.EnqueueAndDequeueIsThreadSafe, threadingTests.TearDown);
        }

        [Test]
        public void StressEnqueueAndDequeueAllIsThreadSafe()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.EnqueueAndDequeueAllIsThreadSafe, threadingTests.TearDown);
        }

        [Test]
        public void StressDequeueAllHasFiniteRun()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.DequeueAllHasFiniteRun, threadingTests.TearDown);
        }

        [Test]
        public void StressClearAfterDone()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.ClearAfterDone, threadingTests.TearDown);
        }

        [Test]
        public void StressClearAfterDoneDequeueingWithEventsRemaining()
        {
            stressor.Stress(threadingTests.Setup, threadingTests.ClearAfterDoneDequeueingWithEventsRemaining, threadingTests.TearDown);
        }
    }
}
