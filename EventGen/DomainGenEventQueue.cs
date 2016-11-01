using System;
using System.Collections.Generic;
using System.Linq;

namespace EventGen
{
    internal class DomainGenEventQueue : GenEventQueue
    {
        private readonly Dictionary<Guid, Queue<GenEvent>> queues;

        public DomainGenEventQueue()
        {
            queues = new Dictionary<Guid, Queue<GenEvent>>();
        }

        public bool ContainsEvents(Guid clientID)
        {
            var queue = GetQueue(clientID);
            return queues[clientID].Any();
        }

        public GenEvent Dequeue(Guid clientID)
        {
            if (ContainsEvents(clientID) == false)
                return null;

            var queue = GetQueue(clientID);
            return queue.Dequeue();
        }

        private Queue<GenEvent> GetQueue(Guid clientID)
        {
            if (queues.ContainsKey(clientID) == false)
                queues[clientID] = new Queue<GenEvent>();

            return queues[clientID];
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

        public void Enqueue(Guid clientID, GenEvent genEvent)
        {
            var queue = GetQueue(clientID);
            queue.Enqueue(genEvent);
        }

        public void Enqueue(Guid clientID, string source, string message)
        {
            var genEvent = new GenEvent(source, message);
            Enqueue(clientID, genEvent);
        }
    }
}
