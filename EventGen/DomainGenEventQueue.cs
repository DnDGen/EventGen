using System;
using System.Collections.Generic;
using System.Linq;

namespace EventGen
{
    internal class DomainGenEventQueue : GenEventQueue
    {
        private readonly Dictionary<Guid, Queue<GenEvent>> queues;
        private readonly ClientIDManager clientIDManager;

        public DomainGenEventQueue(ClientIDManager clientIDManager)
        {
            this.clientIDManager = clientIDManager;
            queues = new Dictionary<Guid, Queue<GenEvent>>();
        }

        public bool ContainsEvents()
        {
            var clientID = clientIDManager.GetClientID();
            return ContainsEvents(clientID);
        }

        public bool ContainsEvents(Guid clientID)
        {
            var queue = GetQueue(clientID);
            return queue.Any();
        }

        public GenEvent Dequeue(Guid clientID)
        {
            if (ContainsEvents(clientID) == false)
                return null;

            var queue = GetQueue(clientID);
            return queue.Dequeue();
        }

        public GenEvent Dequeue()
        {
            var clientID = clientIDManager.GetClientID();
            return Dequeue(clientID);
        }

        private Queue<GenEvent> GetQueue()
        {
            var clientID = clientIDManager.GetClientID();
            return GetQueue(clientID);
        }

        private Queue<GenEvent> GetQueue(Guid clientID)
        {
            if (queues.ContainsKey(clientID) == false)
                queues[clientID] = new Queue<GenEvent>();

            return queues[clientID];
        }

        public IEnumerable<GenEvent> DequeueAll()
        {
            var clientID = clientIDManager.GetClientID();
            return DequeueAll(clientID);
        }

        public IEnumerable<GenEvent> DequeueAll(Guid clientID)
        {
            var queue = GetQueue(clientID);
            var events = new List<GenEvent>();

            while (queue.Any())
            {
                var genEvent = queue.Dequeue();
                events.Add(genEvent);
            }

            return events;
        }

        public void Enqueue(GenEvent genEvent)
        {
            var queue = GetQueue();
            queue.Enqueue(genEvent);
        }

        public void Enqueue(string source, string message)
        {
            var genEvent = new GenEvent(source, message);
            Enqueue(genEvent);
        }
    }
}
