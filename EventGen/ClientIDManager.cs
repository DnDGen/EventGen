using System;

namespace EventGen
{
    public interface ClientIDManager
    {
        void SetClientID(Guid clientID);
        Guid GetClientID();
    }
}
