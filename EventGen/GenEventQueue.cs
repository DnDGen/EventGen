using System;
using System.Collections.Generic;

namespace EventGen
{
    public interface GenEventQueue
    {
        void Enqueue(Guid clientID, GenEvent genEvent);
        void Enqueue(Guid clientID, string source, string message);
        GenEvent Dequeue(Guid clientID);
        IEnumerable<GenEvent> DequeueAll(Guid clientID);
        bool ContainsEvents(Guid clientID);
    }
}
