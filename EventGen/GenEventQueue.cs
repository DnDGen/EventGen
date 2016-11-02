using System;
using System.Collections.Generic;

namespace EventGen
{
    public interface GenEventQueue
    {
        void Enqueue(GenEvent genEvent);
        void Enqueue(string source, string message);
        GenEvent Dequeue();
        GenEvent Dequeue(Guid clientID);
        IEnumerable<GenEvent> DequeueAll();
        IEnumerable<GenEvent> DequeueAll(Guid clientID);
        bool ContainsEvents();
        bool ContainsEvents(Guid clientID);
    }
}
