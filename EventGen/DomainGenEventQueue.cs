using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventGen
{
    internal class DomainGenEventQueue : GenEventQueue
    {
        private readonly Dictionary<Guid, ConcurrentQueue<GenEvent>> queues;
        private readonly ClientIDManager clientIDManager;

        public DomainGenEventQueue(ClientIDManager clientIDManager)
        {
            this.clientIDManager = clientIDManager;
            queues = new Dictionary<Guid, ConcurrentQueue<GenEvent>>();
        }

        public bool CurrentThreadContainsEvents()
        {
            var clientID = clientIDManager.GetClientID();
            return ContainsEvents(clientID);
        }

        public bool ContainsEvents(Guid clientID)
        {
            var queue = GetQueueForDequeue(clientID);
            return !queue.IsEmpty;
        }

        public GenEvent Dequeue(Guid clientID)
        {
            if (!ContainsEvents(clientID))
                return null;

            var queue = GetQueueForDequeue(clientID);
            var dequeuedEvent = new GenEvent();
            var isSuccessful = false;

            do isSuccessful = queue.TryDequeue(out dequeuedEvent);
            while (!DequeueSuccessful(isSuccessful, dequeuedEvent) && ContainsEvents(clientID));

            if (!DequeueSuccessful(isSuccessful, dequeuedEvent))
                return null;

            return dequeuedEvent;
        }

        private bool DequeueSuccessful(bool successful, GenEvent dequeuedEvent)
        {
            return successful && !string.IsNullOrEmpty(dequeuedEvent.Source);
        }

        public GenEvent DequeueForCurrentThread()
        {
            var clientID = clientIDManager.GetClientID();
            return Dequeue(clientID);
        }

        private ConcurrentQueue<GenEvent> GetQueueForEnqueueForCurrentThread()
        {
            var clientID = clientIDManager.GetClientID();
            return GetQueueForEnqueue(clientID);
        }

        private ConcurrentQueue<GenEvent> GetQueueForEnqueue(Guid clientID)
        {
            if (!QueueExists(clientID))
                queues[clientID] = new ConcurrentQueue<GenEvent>();

            return queues[clientID];
        }
        private ConcurrentQueue<GenEvent> GetQueueForDequeue(Guid clientID)
        {
            if (!QueueExists(clientID))
                return new ConcurrentQueue<GenEvent>();

            return queues[clientID];
        }

        public IEnumerable<GenEvent> DequeueAllForCurrentThread()
        {
            var clientID = clientIDManager.GetClientID();
            return DequeueAll(clientID);
        }

        private bool QueueExists(Guid clientID)
        {
            return queues.ContainsKey(clientID);
        }

        public IEnumerable<GenEvent> DequeueAll(Guid clientID)
        {
            var events = new List<GenEvent>();
            var queue = GetQueueForDequeue(clientID);
            var snapshotCount = queue.Count;
            var attempts = 0;

            while (ContainsEvents(clientID) && events.Count < snapshotCount && attempts++ < snapshotCount)
            {
                var genEvent = Dequeue(clientID);

                if (genEvent == null)
                    break;

                events.Add(genEvent);
            }

            return events;
        }

        public void Enqueue(GenEvent genEvent)
        {
            var queue = GetQueueForEnqueueForCurrentThread();

            queue.Enqueue(genEvent);
        }

        public void Enqueue(string source, string message)
        {
            var genEvent = new GenEvent(source, message);
            Enqueue(genEvent);
        }

        public void ClearCurrentThread()
        {
            var clientID = clientIDManager.GetClientID();
            Clear(clientID);
        }

        public void Clear(Guid clientID)
        {
            if (QueueExists(clientID))
                queues.Remove(clientID);
        }
    }
}
