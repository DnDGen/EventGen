using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DnDGen.EventGen
{
    internal class ThreadClientIDManager : ClientIDManager
    {
        private Dictionary<int, Guid> clientIDs;

        public ThreadClientIDManager()
        {
            clientIDs = new Dictionary<int, Guid>();
        }

        public Guid GetClientID()
        {
            if (clientIDs.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                return clientIDs[Thread.CurrentThread.ManagedThreadId];

            if (Task.CurrentId.HasValue && clientIDs.ContainsKey(Task.CurrentId.Value))
                return clientIDs[Task.CurrentId.Value];

            throw new InvalidOperationException("No Client ID has been set for this thread.");
        }

        public void SetClientID(Guid clientID)
        {
            clientIDs[Thread.CurrentThread.ManagedThreadId] = clientID;

            if (Task.CurrentId.HasValue)
                clientIDs[Task.CurrentId.Value] = clientID;
        }

        public void SetClientID(Guid clientID, Task task)
        {
            clientIDs[task.Id] = clientID;
        }
    }
}
