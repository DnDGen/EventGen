using System;
using System.Collections.Generic;

namespace EventGen
{
    public interface GenEventQueue
    {
        void Enqueue(GenEvent genEvent);
        void Enqueue(string source, string message);
        GenEvent DequeueForCurrentThread();
        GenEvent Dequeue(Guid clientID);
        IEnumerable<GenEvent> DequeueAllForCurrentThread();
        IEnumerable<GenEvent> DequeueAll(Guid clientID);
        bool CurrentThreadContainsEvents();
        bool ContainsEvents(Guid clientID);
        void ClearCurrentThread();
        void Clear(Guid clientID);
    }
}
