using System;
using System.Collections.Generic;
using System.Threading;

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
            if (clientIDs.ContainsKey(Thread.CurrentThread.ManagedThreadId) == false)
                throw new InvalidOperationException("No Client ID has been set for this thread.");

            return clientIDs[Thread.CurrentThread.ManagedThreadId];
        }

        public void SetClientID(Guid clientID)
        {
            clientIDs[Thread.CurrentThread.ManagedThreadId] = clientID;
        }
    }
}
